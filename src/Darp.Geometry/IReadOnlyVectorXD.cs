namespace Darp.Geometry;

/// <summary>Read-only access to independently retained geometry storage. Dispose each acquired view.</summary>
public interface IReadOnlyVectorXD : IReadOnlyMatrixD
{
    int Count { get; }
    double this[int index] { get; }
    IReadOnlyVectorXD Slice(int start, int count);
}
