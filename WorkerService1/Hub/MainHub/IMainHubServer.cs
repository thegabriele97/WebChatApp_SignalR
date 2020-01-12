﻿using System.Threading.Tasks;
using WorkerService1.Hub.MainHub.Utils;

namespace WorkerService1.Hub.MainHub {
    public interface IMainHubServer {
        enum MessageType { Normal, Welcome, Error, Info };
        //Task ShowMessage(string user, string message, MessageType type = MessageType.Normal);
        Task ShowMessage(string json_data, MessageType type = MessageType.Normal);
        Task ConfirmUsername(string username, bool confirmed);
    }
}