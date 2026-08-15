namespace SaaSFoundry.SDK.AI.Models;

public sealed class AIResponse<TOutput>
{
    public TOutput Output { get; }
    public AIConfidence Confidence { get; }
    public string Explanation { get; }

    public AIResponse(TOutput output, AIConfidence confidence, string explanation)
    {
        Output = output;
        Confidence = confidence;
        Explanation = explanation ?? string.Empty;
    }
}
