namespace Darp.Geometry;

/// <summary>Read-only access to shared geometry storage; other aliases may mutate it.</summary>
public interface IReadOnlyQuaternionD : IReadOnlyMatrixD
{
    double X { get; }
    double Y { get; }
    double Z { get; }
    double W { get; }
    IReadOnlyVectorXD Coefficients { get; }
}
