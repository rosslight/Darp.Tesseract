namespace Darp.Geometry;

public readonly partial struct Isometry3D
{
    public static Isometry3D operator *(Isometry3D left, ReadOnlyIsometry3D right) =>
        GeometryExtensions.Multiply(left.AsReadOnly(), right);

    public static Vector3D operator *(Isometry3D transform, ReadOnlyVector3D point) =>
        GeometryExtensions.TransformPoint(transform.AsReadOnly(), point);

    public static implicit operator ReadOnlyIsometry3D(Isometry3D value) => value.AsReadOnly();
}
