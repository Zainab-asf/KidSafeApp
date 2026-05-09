using System.Threading.Channels;

namespace KidSafe.Backend.Services;

/// <summary>
/// Side-effect job queued after AI result is returned to client.
/// Carries everything needed for DB write + notifications.
/// </summary>
public record ModerationSideEffect(
    int    SenderId,
    string SenderName,
    int    ReceiverId,
    string OriginalMessage,
    string MaskedMessage,
    string Label,          // Safe | Watch | Review
    double Score
);

/// <summary>
/// Bounded in-memory channel — single producer (controller), single consumer (worker).
/// Bounded at 1000 so runaway load doesn't eat memory.
/// </summary>
public sealed class ModerationQueue
{
    private readonly Channel<ModerationSideEffect> _channel =
        Channel.CreateBounded<ModerationSideEffect>(
            new BoundedChannelOptions(1000)
            {
                SingleReader = true,
                FullMode     = BoundedChannelFullMode.Wait
            });

    public ValueTask EnqueueAsync(ModerationSideEffect job, CancellationToken ct = default)
        => _channel.Writer.WriteAsync(job, ct);

    public IAsyncEnumerable<ModerationSideEffect> ReadAllAsync(CancellationToken ct)
        => _channel.Reader.ReadAllAsync(ct);
}
