namespace Darp.Geometry;

public readonly partial struct ReadOnlyIsometry3D
{
    public static Isometry3D operator *(ReadOnlyIsometry3D left, ReadOnlyIsometry3D right) =>
        GeometryExtensions.Multiply(left, right);

    public static Vector3D operator *(ReadOnlyIsometry3D transform, ReadOnlyVector3D point) =>
        GeometryExtensions.TransformPoint(transform, point);
}
