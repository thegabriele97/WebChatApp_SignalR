using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkerService1.Hub.MainHub {
    interface IMainHubClient {
        public Task OnDisconnectedAsync(Exception exception);
        public Task RegisterUser(string username, string ipAddress);
        public Task SendMessageFromClient(string user, string message);
        public Task GetNumberOfActiveUsers();
    }
}
