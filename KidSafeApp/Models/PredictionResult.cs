namespace KidSafeApp.Models;

/// <summary>
/// Result from the cyberbullying detection model for a single message.
/// </summary>
public sealed class PredictionResult
{
    /// <summary>
    /// True when the model thinks the message contains bullying / harmful content.
    /// </summary>
    public bool IsBullying { get; init; }

    /// <summary>
    /// Optional human-readable label (e.g. "harassment", "insult").
    /// </summary>
    public string? Label { get; init; }

    /// <summary>
    /// Optional probability score from 0-1.
    /// </summary>
    public float? Score { get; init; }
}