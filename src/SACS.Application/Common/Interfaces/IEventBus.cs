using System.Threading;
using System.Threading.Tasks;

namespace SACS.Application.Common.Interfaces;

public interface IEventBus
{
    Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class;
}
