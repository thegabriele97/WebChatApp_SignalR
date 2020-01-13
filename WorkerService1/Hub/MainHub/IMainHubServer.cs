using System.Threading.Tasks;
using WorkerService1.Hub.MainHub.Utils;

namespace WorkerService1.Hub.MainHub {
    public interface IMainHubServer {
        Task ShowMessage(string json_data);
        Task ConfirmUsername(string username, bool confirmed);
        Task SendNumberOfActiveUsers(string json_data);
    }
}