namespace Darp.Geometry;

/// <summary>Read-only access to independently retained geometry storage. Dispose each acquired view.</summary>
public interface IReadOnlyVector3D : IReadOnlyVectorXD
{
    double X { get; }
    double Y { get; }
    double Z { get; }
}
