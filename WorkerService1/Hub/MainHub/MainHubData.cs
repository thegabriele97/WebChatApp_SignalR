using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using WorkerService1.Hub.MainHub.Utils;

namespace WorkerService1.Hub.MainHub {
    public static class MainHubData {
        private static readonly Dictionary<string, string> _userList = new Dictionary<string, string>();
        private static readonly Queue<ChatMessage> _userMessages = new Queue<ChatMessage>();
        private static readonly Semaphore _semaphore_messages = new Semaphore(0, 100);
        private static readonly Semaphore _semaphore_mutexAccessUsersList = new Semaphore(1, 1);

        public static bool RegisterUser(string username, string connection_id) {
            
            foreach (string e_username in _userList.Values.ToList<string>()) {
                if (e_username == username) {
                    return false;
                }
            }
            
            _userList.Add(connection_id, username);
            return true;
        }

        public static void RemoveUser(string connection_id) {
            _userList.Remove(connection_id);
        }

        public static Dictionary<string, string> GetUsers() {
            return new Dictionary<string, string>(_userList);
        }

        public static void RegisterMessage(ChatMessage msg) {
            _userMessages.Enqueue(msg);
            _semaphore_messages.Release();
        }

        public static bool TryDequeLastMessage(out ChatMessage item) {

            var bool_ret = _semaphore_messages.WaitOne();
            _semaphore_mutexAccessUsersList.WaitOne(); //Blocking access to Queue by other threads
            item = _userMessages.Dequeue(); //Retrieving object from queue
            _semaphore_mutexAccessUsersList.Release(); //Releasing access to queue by other thread
            
            return bool_ret;
        }
    }
}
