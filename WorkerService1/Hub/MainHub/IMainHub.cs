using System.Threading.Tasks;
using WorkerService1.Hub.Utils;

namespace WorkerService1.Hub.MainHub {
    public interface IMainHub {
        enum MessageType { Normal, Welcome, Error, Info };
        Task ShowMessage(string user, string message, MessageType type = MessageType.Normal);
        Task ConfirmUsername(string username, bool confirmed);
    }
}