namespace Darp.Geometry;

public readonly partial struct Isometry3D
{
    public static Isometry3D operator *(Isometry3D left, IReadOnlyIsometry3D right) => left.Multiply(right);
    public static Vector3D operator *(Isometry3D transform, IReadOnlyVector3D point) => transform.TransformPoint(point);
}
