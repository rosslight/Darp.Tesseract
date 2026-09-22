namespace Darp.Geometry;

/// <summary>Keeps matrix storage alive until the last use of an acquired tensor span.</summary>
/// <remarks>Does not pin managed memory or lock against writes. Do not use the span after disposing its lease.</remarks>
public readonly ref struct TensorSpanLease
{
    private readonly MatrixStorage? _storage;

    internal TensorSpanLease(MatrixStorage storage) => _storage = storage;

    /// <summary>Ends the access scope. The matrix and other views remain usable.</summary>
    public void Dispose() => GC.KeepAlive(_storage);
}
