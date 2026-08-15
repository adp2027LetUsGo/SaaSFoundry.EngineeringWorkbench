namespace SaaSFoundry.SDK.AI.Models;

public sealed class AIResult<TOutput>
{
    public AIResultStatus Status { get; }
    public AIResponse<TOutput>? Response { get; }
    public string ErrorMessage { get; }

    private AIResult(AIResultStatus status, AIResponse<TOutput>? response, string errorMessage)
    {
        Status = status;
        Response = response;
        ErrorMessage = errorMessage ?? string.Empty;
    }

    public static AIResult<TOutput> Success(AIResponse<TOutput> response) => new(AIResultStatus.Success, response, string.Empty);
    public static AIResult<TOutput> Failure(AIResultStatus status, string message) => new(status, null, message);
}
