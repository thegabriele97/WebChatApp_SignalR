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
                MainHubData.GetUsers()[Context.ConnectionId].Username : null;

            if (username != null) {
                await Clients.All
                        .ShowMessage(ChatMessage.CreateAsJSON(MainHubData.ServerUser, username + " left the chat!",
                                                                    ChatMessage.MessageType.Welcome));
            }

            MainHubData.RemoveUser(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task RegisterUser(string username, string ipAddress) {
            ChatUser new_user = new ChatUser(Context.ConnectionId, username, ipAddress);
            var username_registered = MainHubData.RegisterUser(new_user);

            await Clients.Client(Context.ConnectionId)
                    .ConfirmUsername(username, username_registered);

            if (username_registered) {
                await Clients.Client(Context.ConnectionId)
                        .ShowMessage(ChatMessage.CreateAsJSON(MainHubData.ServerUser, "Welcome " + username + "!", 
                                ChatMessage.MessageType.Welcome));

                await Clients.AllExcept(new List<string> { Context.ConnectionId })
                        .ShowMessage(ChatMessage.CreateAsJSON(MainHubData.ServerUser, username + " joined the chat!", 
                                ChatMessage.MessageType.Welcome));
            }
        }

        public async Task SendMessageFromClient(string user, string message) {
            var correct_user = MainHubData.GetUsers()[Context.ConnectionId];

            if (user != correct_user.Username) {
                throw new InvalidOperationException("Client is using an invalid username.");
            }

            if (!MainHubData.RegisterMessage(new ChatMessage(correct_user, message))) {
                await Clients.Client(Context.ConnectionId)
                    .ShowMessage(ChatMessage.CreateAsJSON(MainHubData.ServerUser, "You are kicked.", ChatMessage.MessageType.Error));

                Context.Abort();
            }
        }

        public async Task GetNumberOfActiveUsers() {
            await Clients.Client(Context.ConnectionId)
                    .SendNumberOfActiveUsers(JsonSerializer.Serialize(new { count_users = MainHubData.GetUsers().Count }));
        }
    }
}
