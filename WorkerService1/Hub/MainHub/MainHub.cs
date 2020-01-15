using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using WorkerService1.Hub.MainHub.Utils;

namespace WorkerService1.Hub.MainHub {
    public class MainHub : Microsoft.AspNetCore.SignalR.Hub<IMainHubServer>, IMainHubClient {

        public async override Task OnDisconnectedAsync(Exception exception) {

            var username = MainHubData.GetUsers().ContainsKey(Context.ConnectionId) ?
                MainHubData.GetUsers()[Context.ConnectionId] : null;

            if (username != null) {
                await Clients.All
                        .ShowMessage(ChatMessage.CreateAsString(ChatMessage.ServerUsername, username + " left the chat!",
                                                                    ChatMessage.MessageType.Welcome));
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
                        .ShowMessage(ChatMessage.CreateAsString(ChatMessage.ServerUsername, "Welcome " + username + "!", 
                                ChatMessage.MessageType.Welcome));

                await Clients.AllExcept(new List<string> { Context.ConnectionId })
                        .ShowMessage(ChatMessage.CreateAsString(ChatMessage.ServerUsername, username + " joined the chat!", 
                                ChatMessage.MessageType.Welcome));
            }
        }

        public async Task SendMessageFromClient(string user, string message) {

            if (user != MainHubData.GetUsers()[Context.ConnectionId]) {
                throw new InvalidOperationException("Client is using an invalid username.");
            }

            MainHubData.RegisterMessage(new ChatMessage(MainHubData.GetUsers()[Context.ConnectionId], message));
        }

        public async Task GetNumberOfActiveUsers() {
            await Clients.Client(Context.ConnectionId)
                    .SendNumberOfActiveUsers(JsonSerializer.Serialize(new { count_users = MainHubData.GetUsers().Count }));
        }
    }
}
