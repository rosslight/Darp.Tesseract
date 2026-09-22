namespace Darp.Geometry;

/// <summary>Read-only access to independently retained geometry storage. Dispose each acquired view.</summary>
public interface IReadOnlyIsometry3D : IReadOnlyMatrixD
{
    IReadOnlyVector3D Translation { get; }
    IReadOnlyMatrix3D RotationMatrix { get; }
    QuaternionD Rotation { get; }
}
