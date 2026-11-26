namespace SteamAudio;

internal sealed class OperationGate
{
    private int _busy;
    internal Scope Enter()
    {
        Geometry.RayCallbacks.ThrowIfInside();
        if (Interlocked.CompareExchange(ref _busy, 1, 0) != 0)
        {
            throw new InvalidOperationException("Concurrent operations on the same simulator are not supported.");
        }

        return new(this);
    }

    internal readonly ref struct Scope(OperationGate gate)
    {
        public void Dispose() => Volatile.Write(ref gate._busy, 0);
    }
}
