using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkerService1.Hub.MainHub.Utils {
    public class ChatMessage {
        public string User { get; set; }
        public string Message { get; set; }

        public ChatMessage(string user, string message) {
            User = user;
            Message = message;
        }
    }
}
