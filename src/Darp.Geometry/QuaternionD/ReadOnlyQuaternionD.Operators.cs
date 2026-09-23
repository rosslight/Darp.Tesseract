namespace Darp.Geometry;

public readonly partial struct ReadOnlyQuaternionD
{
    public static QuaternionD operator *(ReadOnlyQuaternionD a, ReadOnlyQuaternionD b) =>
        GeometryExtensions.Multiply(a, b);

    public static Vector3D operator *(ReadOnlyQuaternionD rotation, ReadOnlyVector3D vector) =>
        GeometryExtensions.Rotate(rotation, vector);

    public static QuaternionD operator -(ReadOnlyQuaternionD value) => new(-value.X, -value.Y, -value.Z, -value.W);
}
