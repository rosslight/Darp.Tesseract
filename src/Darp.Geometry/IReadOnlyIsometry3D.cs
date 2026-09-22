namespace Darp.Geometry;

/// <summary>Read-only access to shared geometry storage; other aliases may mutate it.</summary>
public interface IReadOnlyIsometry3D : IReadOnlyMatrixD
{
    IReadOnlyVector3D Translation { get; }
    IReadOnlyMatrix3D RotationMatrix { get; }
    QuaternionD Rotation { get; }
}
