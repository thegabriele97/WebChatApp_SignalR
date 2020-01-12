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
using WorkerService1.Hub.Utils;
using System.Text;
using WorkerService1.Hub;

namespace WorkerService1 {
    public class Worker : BackgroundService {
        private readonly ILogger<Worker> _logger;
        private IHubContext<MainHub, IMainHub> _mainHub;
        
        private Thread _th_ManageNewMessages;
        private static IHubContext<MainHub, IMainHub> _th_mainHub;

        public Worker(ILogger<Worker> logger, IHubContext<MainHub, IMainHub> hubContext) {
            _logger = logger;
            _mainHub = hubContext;
            _th_ManageNewMessages = new Thread(ManageNewMessages);
        }

        public override Task StartAsync(CancellationToken cancellationToken) {
            _th_mainHub = _mainHub;
            _th_ManageNewMessages.Start();

            //Saying to clients that the server is now on
            _logger.LogInformation("Saying to clients that the server is now on");
            _mainHub.Clients.All.ShowMessage("Server", "> Server is now active.");

            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken) {
            _th_ManageNewMessages.Abort();
            return base.StopAsync(cancellationToken);
        }

        private static void ManageNewMessages() { //Thread for managing messages queue. This can be used for filtering and restyling messages
            while (CommonData.MainHub_tryGetMessage()) {
                var msg = CommonData.MainHub_dequeLastMessage();
                _th_mainHub.Clients.All.ShowMessage(msg.User, msg.Message);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            StringBuilder usersBuilder = new StringBuilder();

            while (!stoppingToken.IsCancellationRequested) {
                //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                usersBuilder.Clear();
                CommonData.MainHub_getUsers()
                    .Values.ToList<string>()
                    .ForEach(e => usersBuilder
                                    .Append(e)
                                    .Append(" - "));

                _logger.LogInformation("Clients currently active on the servr: " + usersBuilder.ToString());
                await _mainHub.Clients.All.ShowMessage("Server", "Server is working.", IMainHub.MessageType.Info);

                await Task.Delay(30000, stoppingToken);
            }
        }
    }
}
