using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace WorkerService1.Hub.MainHub.Utils {
    public class ChatMessage {

        public string User { get; set; }
        public string Message { get; set; }
        public MessageType Type { get; set; }
        public DateTime Date { get; }

        public static readonly string ServerUsername = "Server";
        public enum MessageType { Normal, Welcome, Error, Info };

        public ChatMessage(string user, string message, MessageType type = MessageType.Normal) {
            User = user;
            Message = message;
            Type = type;
            Date = DateTime.Now;
        }

        public override string ToString() {
            return JsonSerializer.Serialize(this);
        }

        public static string CreateAsString (string user, string message, MessageType type = MessageType.Normal) {
            return new ChatMessage(user, message, type).ToString();
        }
    }
}
