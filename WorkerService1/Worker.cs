using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using WorkerService1;
using WorkerService1.Hub.MainHub;
using WorkerService1.Hub.MainHub.Utils;
using System.Text;

namespace WorkerService1 {
    public class Worker : BackgroundService {
        private readonly ILogger<Worker> _logger;
        private readonly IHubContext<MainHub, IMainHubServer> _mainHub;
        private readonly List<Thread> _th_ManageNewMessages_list;
        private readonly CancellationTokenSource _th_ManageNewMessages_CancTokenSource;

        public Worker(ILogger<Worker> logger, IHubContext<MainHub, IMainHubServer> hubContext) {
            _logger = logger;
            _mainHub = hubContext;
            _th_ManageNewMessages_CancTokenSource = new CancellationTokenSource();
            _th_ManageNewMessages_list = new List<Thread>();

            for (Int16 i = 0; i < 2; i++) {
                _th_ManageNewMessages_list.Add(new Thread(ManageNewMessages) {
                    Name = String.Format("_th_ManageNewMessages#{0}", i)
                });
            }
        }

        public override Task StartAsync(CancellationToken cancellationToken) {

            _th_ManageNewMessages_list.ForEach(th => {
                th.Start(new List<object> { _mainHub, _logger, _th_ManageNewMessages_CancTokenSource.Token });
            });

            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken) {
            _th_ManageNewMessages_CancTokenSource.Cancel(); //request cancellation of threads

            //for each thread, register a fake message to unlock and stop it
            _th_ManageNewMessages_list.ForEach(_ => MainHubData.RegisterMessage(new ChatMessage(null, null)));
            return base.StopAsync(cancellationToken);
        }

        //Thread for managing messages queue. This can be used for filtering and restyling messages
        private static void ManageNewMessages(object raw_args) {
            var hub = CastRawArgs<IHubContext<MainHub, IMainHubServer>>(raw_args, 0);
            var logger = CastRawArgs<ILogger<Worker>>(raw_args, 1);
            var stoppingToken = CastRawArgs<CancellationToken>(raw_args, 2);

            while (MainHubData.TryDequeLastMessage(out ChatMessage newMessage)) {

                if (stoppingToken.IsCancellationRequested) {
                    break; //exit from while loop and terminate thread
                }

                var msg_json = newMessage.ToString();
                logger.LogInformation(String.Format("#Thread {1} => New message: {0}", msg_json, Thread.CurrentThread.Name));
                hub.Clients.All.ShowMessage(msg_json);
            }

            //last thing to do in order to terminate this thread
            logger.LogInformation(String.Format("#Thread {0} => Terminated by TokenCancelletionRequest", Thread.CurrentThread.Name));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            while (!stoppingToken.IsCancellationRequested) {
                //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                _logger.LogInformation("Clients currently active on the server: " + String.Join(", ", MainHubData.GetUsers().Values.ToList()));
                await _mainHub.Clients.All.ShowMessage(ChatMessage.CreateAsJSON(MainHubData.ServerUser, "Server is working.", 
                                                ChatMessage.MessageType.Info));

                await Task.Delay(30000, stoppingToken);
            }
        }

        private static T CastRawArgs<T>(object args, int index = -1) {
            return index < 0 ? (T)args : (T)(((List<object>)args)[index]);
        }
    }
}
