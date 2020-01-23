using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace WorkerService1.Hub.MainHub.Utils {
    public class ChatUser {

        public string ConnectionId { get; }
        public string Username { get; }
        public string IpAddress { get; }
        public bool IsKicked { get; set; }

        public ChatUser(string connectionId, string username, string ipAddress) {
            ConnectionId = connectionId;
            Username = username;
            IpAddress = ipAddress;
            IsKicked = false;
        }

        public override string ToString() {
            return JsonSerializer.Serialize(this);
        }

        public override int GetHashCode() {
            return (ConnectionId.GetHashCode() + Username.GetHashCode()) / 2 + Convert.ToInt32(IsKicked);
        }

        public override bool Equals(object obj) {
            return GetHashCode() == obj.GetHashCode() || ToString() == obj.ToString();
        }

    }
}
