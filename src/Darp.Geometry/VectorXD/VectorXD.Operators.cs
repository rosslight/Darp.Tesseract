namespace Darp.Geometry;

public readonly partial struct VectorXD
{
    public static VectorXD operator +(VectorXD a, VectorXD b) => GeometryExtensions.Add(a, b);
    public static VectorXD operator +(VectorXD a, ReadOnlyVectorXD b) => GeometryExtensions.Add(a, b);
    public static VectorXD operator -(VectorXD a, VectorXD b) => GeometryExtensions.Subtract(a, b);
    public static VectorXD operator -(VectorXD a, ReadOnlyVectorXD b) => GeometryExtensions.Subtract(a, b);
    public static VectorXD operator -(VectorXD value) => GeometryExtensions.Scale(value, -1);
    public static VectorXD operator *(VectorXD value, double scalar) => GeometryExtensions.Scale(value, scalar);
    public static VectorXD operator *(double scalar, VectorXD value) => value * scalar;
    public static VectorXD operator /(VectorXD value, double scalar) => GeometryExtensions.Divide(value, scalar);
    public static implicit operator ReadOnlyVectorXD(VectorXD value) => value.AsReadOnly();
    public static implicit operator MatrixXD(VectorXD value) => value.Data.AsMatrix();
    public static implicit operator ReadOnlyMatrixXD(VectorXD value) => value.Data.AsReadOnlyMatrix();
}
