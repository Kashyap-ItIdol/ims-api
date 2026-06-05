using IMS_Application.DTOs;
using System.Threading.Tasks;

namespace IMS_Application.Interfaces
{
    public interface INotificationDispatcher
    {
        Task DispatchAsync(int userId, NewNotificationDto data);
        Task SendNotificationAsync(int userId, string title, string message);
    }
}





