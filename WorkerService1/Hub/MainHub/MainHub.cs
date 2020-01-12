using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkerService1.Hub.MainHub.Utils;

namespace WorkerService1.Hub.MainHub {
    public class MainHub : Microsoft.AspNetCore.SignalR.Hub<IMainHubServer>, IMainHubClient {

        public override Task OnDisconnectedAsync(Exception exception) {
            MainHubData.RemoveUser(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }

        public async Task RegisterUser(string username) {
            var username_registered = MainHubData.RegisterUser(username, Context.ConnectionId);

            await Clients.Client(Context.ConnectionId)
                    .ConfirmUsername(username, username_registered);

            if (username_registered) {
                await Clients.Client(Context.ConnectionId)
                    .ShowMessage("server", "Welcome " + username + "!", IMainHubServer.MessageType.Welcome);
            }
        }

        public async Task SendMessageFromClient(string user, string message) {
            MainHubData.RegisterMessage(new ChatMessage(user, message));
        }
    }
}
