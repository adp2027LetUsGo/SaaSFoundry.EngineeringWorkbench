using System;
namespace SaaSFoundry.SDK.AI.Models;

public sealed class AIRequest<TInput>
{
    public TInput Payload { get; }
    public string Intent { get; }
    public TimeSpan MaxTimeout { get; }

    public AIRequest(TInput payload, string intent, TimeSpan maxTimeout)
    {
        Payload = payload;
        Intent = intent ?? string.Empty;
        MaxTimeout = maxTimeout;
    }
}
