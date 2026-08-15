using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.SDK.AI.Models;

namespace SaaSFoundry.SDK.AI;

public interface IAIProvider
{
    Task<AIResponse<TOutput>> InvokeAsync<TInput, TOutput>(AIRequest<TInput> request, CancellationToken cancellationToken);
}
