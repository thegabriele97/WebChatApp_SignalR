using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkerService1.Hub.Utils;

namespace WorkerService1.Hub.MainHub {
    public class MainHub : Microsoft.AspNetCore.SignalR.Hub<IMainHub> {

        public override Task OnDisconnectedAsync(Exception exception) {
            CommonData.MainHub_removeUser(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }

        public async Task RegisterUser(string username) {
            var username_registered = CommonData.MainHub_registerUser(username, Context.ConnectionId);

            await Clients.Client(Context.ConnectionId)
                    .ConfirmUsername(username, username_registered);

            if (username_registered) {
                await Clients.Client(Context.ConnectionId)
                    .ShowMessage("server", "Welcome " + username + "!", IMainHub.MessageType.Welcome);
            }
        }

        public async Task SendMessageFromClient(string user, string message) {
            CommonData.MainHub_registerMessage(new ChatMessage(user, message));
        }
    }
}
