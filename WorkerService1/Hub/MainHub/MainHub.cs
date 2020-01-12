using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkerService1.Hub.MainHub.Utils;

namespace WorkerService1.Hub.MainHub {
    public class MainHub : Microsoft.AspNetCore.SignalR.Hub<IMainHubServer>, IMainHubClient {

        public async override Task OnDisconnectedAsync(Exception exception) {

            var username = MainHubData.GetUsers().ContainsKey(Context.ConnectionId) ?
                MainHubData.GetUsers()[Context.ConnectionId] : null;

            if (username != null) {
                await Clients.All.ShowMessage(ChatMessage.CreateAsString(ChatMessage.ServerUsername, username + " left the chat!"),
                                                                IMainHubServer.MessageType.Welcome);
            }

            MainHubData.RemoveUser(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task RegisterUser(string username) {
            var username_registered = MainHubData.RegisterUser(username, Context.ConnectionId);

            await Clients.Client(Context.ConnectionId)
                    .ConfirmUsername(username, username_registered);

            if (username_registered) {
                await Clients.Client(Context.ConnectionId)
                    //.ShowMessage("Server", "Welcome " + username + "!", IMainHubServer.MessageType.Welcome);
                        .ShowMessage(ChatMessage.CreateAsString(ChatMessage.ServerUsername, "Welcome " + username + "!"), 
                                IMainHubServer.MessageType.Welcome);

                await Clients.AllExcept(new List<string> { Context.ConnectionId })
                        .ShowMessage(ChatMessage.CreateAsString(ChatMessage.ServerUsername, username + " joined the chat!"), 
                                IMainHubServer.MessageType.Welcome);
            }
        }

        public async Task SendMessageFromClient(string user, string message) {
            MainHubData.RegisterMessage(new ChatMessage(user, message));
        }
    }
}
