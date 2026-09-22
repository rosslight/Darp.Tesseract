namespace Darp.Geometry;

/// <summary>Read-only access to independently retained geometry storage. Dispose each acquired view.</summary>
public interface IReadOnlyQuaternionD : IReadOnlyMatrixD
{
    double X { get; }
    double Y { get; }
    double Z { get; }
    double W { get; }
    IReadOnlyVectorXD Coefficients { get; }
}
