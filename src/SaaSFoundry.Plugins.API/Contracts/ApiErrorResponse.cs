using System.Collections.Generic;

namespace SaaSFoundry.Plugins.API.Contracts;

public sealed record ApiErrorResponse
{
    public string Code { get; }
    public string Message { get; }
    public string CorrelationId { get; }
    public IReadOnlyList<string> ValidationErrors { get; }

    public ApiErrorResponse(string code, string message, string correlationId, IReadOnlyList<string>? validationErrors = null)
    {
        Code = code;
        Message = message;
        CorrelationId = correlationId;
        ValidationErrors = validationErrors ?? System.Array.Empty<string>();
    }
}
