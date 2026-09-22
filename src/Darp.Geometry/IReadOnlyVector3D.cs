namespace Darp.Geometry;

/// <summary>Read-only access to shared geometry storage; other aliases may mutate it.</summary>
public interface IReadOnlyVector3D : IReadOnlyVectorXD
{
    double X { get; }
    double Y { get; }
    double Z { get; }
}
