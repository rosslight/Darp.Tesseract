namespace Darp.Geometry;

public readonly partial struct Vector3D
{
    public static Vector3D operator +(Vector3D a, ReadOnlyVector3D b) => a.Add(b);
    public static Vector3D operator -(Vector3D a, ReadOnlyVector3D b) => a.Subtract(b);
    public static Vector3D operator -(Vector3D value) => GeometryExtensions.Scale((ReadOnlyVector3D)value, -1);
    public static Vector3D operator *(Vector3D value, double scalar) =>
        GeometryExtensions.Scale((ReadOnlyVector3D)value, scalar);
    public static Vector3D operator *(double scalar, Vector3D value) => value * scalar;
    public static Vector3D operator /(Vector3D value, double scalar) =>
        GeometryExtensions.Divide((ReadOnlyVector3D)value, scalar);
    public static implicit operator ReadOnlyVector3D(Vector3D value) => new(value.Storage, value.Layout);
    public static implicit operator VectorXD(Vector3D value) => new(value.Storage, value.Layout);
    public static implicit operator ReadOnlyVectorXD(Vector3D value) => new(value.Storage, value.Layout);
    public static implicit operator MatrixXD(Vector3D value) => value.Data.AsMatrix();
    public static implicit operator ReadOnlyMatrixXD(Vector3D value) => value.Data.AsReadOnlyMatrix();
}
