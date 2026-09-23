namespace Darp.Geometry;

public readonly partial struct ReadOnlyVector3D
{
    public static Vector3D operator +(ReadOnlyVector3D a, ReadOnlyVector3D b) => GeometryExtensions.Add(a, b);
    public static Vector3D operator -(ReadOnlyVector3D a, ReadOnlyVector3D b) => GeometryExtensions.Subtract(a, b);
    public static Vector3D operator -(ReadOnlyVector3D value) => GeometryExtensions.Scale(value, -1);
    public static Vector3D operator *(ReadOnlyVector3D value, double scalar) => GeometryExtensions.Scale(value, scalar);
    public static Vector3D operator *(double scalar, ReadOnlyVector3D value) => value * scalar;
    public static Vector3D operator /(ReadOnlyVector3D value, double scalar) => GeometryExtensions.Divide(value, scalar);
}
