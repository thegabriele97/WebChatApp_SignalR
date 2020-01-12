using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace WorkerService1.Hub.MainHub.Utils {
    public class ChatMessage {
        public string User { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; }

        public static readonly string ServerUsername = "Server";

        public ChatMessage(string user, string message) {
            User = user;
            Message = message;
            Date = DateTime.Now;
        }

        public override string ToString() {
            return JsonSerializer.Serialize(this);
        }

        public static string CreateAsString (string user, string message) {
            return new ChatMessage(user, message).ToString();
        }
    }
}
