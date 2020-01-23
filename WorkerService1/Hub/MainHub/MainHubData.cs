using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using WorkerService1.Hub.MainHub.Utils;

namespace WorkerService1.Hub.MainHub {
    public static class MainHubData {

        private static readonly short MAX_MESSAGE_COUNT = 2;

        private static readonly Dictionary<string, ChatUser> _userList = new Dictionary<string, ChatUser>();
        private static readonly Queue<ChatMessage> _userMessages = new Queue<ChatMessage>();
        private static readonly Semaphore _semaphore_messages = new Semaphore(0, 100);
        private static readonly Semaphore _semaphore_mutexAccessUsersList = new Semaphore(1, 1);

        private static readonly Thread _th_manageMessagesSpam = new Thread(ThManageMessagesSpam);
        private static readonly Dictionary<string, short> _countMessagesForEachUser = new Dictionary<string, short>();

        public static ChatUser ServerUser { get; } = new ChatUser(null, "Server", null);

        static MainHubData() {
            _th_manageMessagesSpam.Start();
        }

        public static bool RegisterUser(ChatUser new_user) {

            foreach (ChatUser entry_user in _userList.Values.ToList()) {
                if (entry_user.Username == new_user.Username) {
                    return false;
                }
            }

            _userList.Add(new_user.ConnectionId, new_user);
            _countMessagesForEachUser[new_user.ConnectionId] = 0;
            return true;
        }

        public static void RemoveUser(string connection_id) {
            _countMessagesForEachUser.Remove(connection_id);
            _userList.Remove(connection_id);
        }

        public static Dictionary<string, ChatUser> GetUsers() {
            return new Dictionary<string, ChatUser>(_userList);
        }

        public static bool RegisterMessage(ChatMessage msg) {

            ++_countMessagesForEachUser[msg.User.ConnectionId];
            if (msg.User.IsKicked) {
                return false;
            }

            _userMessages.Enqueue(msg);
            _semaphore_messages.Release();
            return true;
        }

        public static bool TryDequeLastMessage(out ChatMessage item) {

            var bool_ret = _semaphore_messages.WaitOne();
            _semaphore_mutexAccessUsersList.WaitOne(); //Blocking access to Queue by other threads
            item = _userMessages.Dequeue(); //Retrieving object from queue
            _semaphore_mutexAccessUsersList.Release(); //Releasing access to queue by other thread

            return bool_ret;
        }

        private static void ThManageMessagesSpam() {
            while (Thread.CurrentThread.IsAlive) {

                var copy_dict = new Dictionary<string, short>(_countMessagesForEachUser);
                foreach (KeyValuePair<string, short> entry in copy_dict) {
                    if (entry.Value >= MAX_MESSAGE_COUNT) {
                        _userList[entry.Key].IsKicked = true;
                    } else {
                        _countMessagesForEachUser[entry.Key] -= (short)((_countMessagesForEachUser[entry.Key] > 0) ? 1 : 0);
                    }
                }

                Thread.Sleep(50);
            }
        }
    }
}
