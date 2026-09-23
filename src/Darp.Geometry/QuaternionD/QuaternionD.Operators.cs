namespace Darp.Geometry;

public readonly partial struct QuaternionD
{
    public static QuaternionD operator *(QuaternionD a, ReadOnlyQuaternionD b) => GeometryExtensions.Multiply(a, b);
    public static Vector3D operator *(QuaternionD rotation, ReadOnlyVector3D vector) =>
        GeometryExtensions.Rotate(rotation, vector);
    public static QuaternionD operator -(QuaternionD value) => new(-value.X, -value.Y, -value.Z, -value.W);
    public static implicit operator ReadOnlyQuaternionD(QuaternionD value) => value.AsReadOnly();
    public static implicit operator VectorXD(QuaternionD value) => value.AsVector();
    public static implicit operator ReadOnlyVectorXD(QuaternionD value) => value.AsReadOnly().AsVector();
    public static implicit operator MatrixXD(QuaternionD value) => new(value.Data);
    public static implicit operator ReadOnlyMatrixXD(QuaternionD value) => new(value.Data);
}
