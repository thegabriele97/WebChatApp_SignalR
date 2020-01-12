using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkerService1.Hub.MainHub {
    interface IMainHubClient {
        public Task OnDisconnectedAsync(Exception exception);
        public Task RegisterUser(string username);
        public Task SendMessageFromClient(string user, string message);
    }
}
