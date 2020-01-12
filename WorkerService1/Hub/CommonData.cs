using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using WorkerService1.Hub.Utils;

namespace WorkerService1.Hub {
    public static class CommonData {
        private static readonly Dictionary<string, string> _mainhub_userList = new Dictionary<string, string>();
        private static readonly Queue<ChatMessage> _mainhub_userMessages = new Queue<ChatMessage>();
        private static readonly Semaphore _semaphore_messages = new Semaphore(0, 100);

        public static bool MainHub_registerUser(string username, string connection_id) {
            
            foreach (string e_username in _mainhub_userList.Values.ToList<string>()) {
                if (e_username == username) {
                    return false;
                }
            }
            
            _mainhub_userList.Add(connection_id, username);
            return true;
        }

        public static void MainHub_removeUser(string connection_id) {
            _mainhub_userList.Remove(connection_id);
        }

        public static Dictionary<string, string> MainHub_getUsers() {
            return new Dictionary<string, string>(_mainhub_userList);
        }

        public static void MainHub_registerMessage(ChatMessage msg) {
            _mainhub_userMessages.Enqueue(msg);
            _semaphore_messages.Release();
        }

        public static bool MainHub_tryGetMessage() {
            return _semaphore_messages.WaitOne();
        }

        public static ChatMessage MainHub_dequeLastMessage() {
            return _mainhub_userMessages.Dequeue();
        }
    }
}
