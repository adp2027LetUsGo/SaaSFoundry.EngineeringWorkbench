using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SaaSFoundry.SDK.AI.Models;
using SaaSFoundry.SDK.AI.Validation;

namespace SaaSFoundry.SDK.AI.Engine;

public sealed class DefaultAIEngine : IAIEngine
{
    private readonly IServiceProvider _serviceProvider;
    private static readonly ActivitySource ActivitySource = new("SaaSFoundry.SDK.AI.Engine");

    public DefaultAIEngine(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<AIResult<TOutput>> ExecuteAsync<TInput, TOutput>(AIRequest<TInput> request, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("ExecuteAIRequest");
        activity?.SetTag("ai.intent", request.Intent);

        var provider = _serviceProvider.GetService<IAIProvider>();
        if (provider == null)
        {
            return AIResult<TOutput>.Failure(AIResultStatus.Unavailable, "No IAIProvider registered.");
        }

        var validator = _serviceProvider.GetService<IAIValidator<TOutput>>();

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        if (request.MaxTimeout > TimeSpan.Zero)
        {
            cts.CancelAfter(request.MaxTimeout);
        }

        try
        {
            var response = await provider.InvokeAsync<TInput, TOutput>(request, cts.Token);
            
            if (validator != null)
            {
                var validationResult = validator.Validate(response.Output);
                if (!validationResult.IsValid)
                {
                    activity?.SetTag("ai.error", "ValidationFailed");
                    return AIResult<TOutput>.Failure(AIResultStatus.ValidationFailed, validationResult.ErrorMessage);
                }
            }

            return AIResult<TOutput>.Success(response);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested && cts.IsCancellationRequested)
        {
            activity?.SetTag("ai.error", "Timeout");
            return AIResult<TOutput>.Failure(AIResultStatus.Timeout, $"AI operation timed out after {request.MaxTimeout}");
        }
        catch (Exception ex)
        {
            activity?.SetTag("ai.error", "ProviderError");
            activity?.SetTag("ai.error_message", ex.Message);
            return AIResult<TOutput>.Failure(AIResultStatus.ProviderError, ex.Message);
        }
    }
}
