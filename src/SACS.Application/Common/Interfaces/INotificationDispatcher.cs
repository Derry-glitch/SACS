using System.Threading;
using System.Threading.Tasks;

namespace SACS.Application.Common.Interfaces;

public interface INotificationDispatcher
{
    Task DispatchAsync(long userId, NotificationMessage message, CancellationToken cancellationToken = default);
}
