namespace Darp.Geometry;

// Counts explicitly retained views, not C# variable aliases. The final release
// returns transferred storage to its owner; mapped memory remains caller-owned.
internal sealed class MatrixStorage(Memory<double> memory, IDisposable? owner)
{
    private int _references = 1;
    internal Memory<double> Memory = memory;
    private IDisposable? _owner = owner;

    internal void Retain()
    {
        int count = Volatile.Read(ref _references);
        while (count != 0)
        {
            int observed = Interlocked.CompareExchange(ref _references, checked(count + 1), count);
            if (observed == count) return;
            count = observed;
        }
        throw new ObjectDisposedException(nameof(MatrixStorage));
    }

    internal void Release()
    {
        if (Interlocked.Decrement(ref _references) != 0) return;
        var owner = _owner;
        _owner = null;
        Memory = default;
        owner?.Dispose();
    }
}
