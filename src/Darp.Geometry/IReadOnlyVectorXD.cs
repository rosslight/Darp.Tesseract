namespace Darp.Geometry;

/// <summary>Read-only access to shared geometry storage; other aliases may mutate it.</summary>
public interface IReadOnlyVectorXD : IReadOnlyMatrixD
{
    int Count { get; }
    double this[int index] { get; }
    IReadOnlyVectorXD Slice(int start, int count);
}
