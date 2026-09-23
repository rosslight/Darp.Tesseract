namespace Darp.Geometry;

public readonly partial struct ReadOnlyVectorXD
{
    public static VectorXD operator +(ReadOnlyVectorXD a, ReadOnlyVectorXD b) => GeometryExtensions.Add(a, b);
    public static VectorXD operator -(ReadOnlyVectorXD a, ReadOnlyVectorXD b) => GeometryExtensions.Subtract(a, b);
    public static VectorXD operator -(ReadOnlyVectorXD value) => GeometryExtensions.Scale(value, -1);
    public static VectorXD operator *(ReadOnlyVectorXD value, double scalar) => GeometryExtensions.Scale(value, scalar);
    public static VectorXD operator *(double scalar, ReadOnlyVectorXD value) => value * scalar;
    public static VectorXD operator /(ReadOnlyVectorXD value, double scalar) => GeometryExtensions.Divide(value, scalar);
}
