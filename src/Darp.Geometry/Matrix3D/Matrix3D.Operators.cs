namespace Darp.Geometry;

public readonly partial struct Matrix3D
{
    public static Matrix3D operator +(Matrix3D a, ReadOnlyMatrix3D b) => Matrix.Add(a, b);

    public static Matrix3D operator -(Matrix3D a, ReadOnlyMatrix3D b) => Matrix.Subtract(a, b);

    public static Matrix3D operator *(Matrix3D a, ReadOnlyMatrix3D b) => Matrix.Multiply(a, b);

    public static Matrix3D operator -(Matrix3D value) => Matrix.Scale(value, -1);

    public static Matrix3D operator *(Matrix3D value, double scalar) => Matrix.Scale(value, scalar);

    public static Matrix3D operator *(double scalar, Matrix3D value) => value * scalar;

    public static Matrix3D operator /(Matrix3D value, double scalar) => Matrix.Divide(value, scalar);

    /// <summary>Creates a read-only view that shares coefficients with <paramref name="value"/>.</summary>
    /// <param name="value">The matrix to view.</param>
    public static implicit operator ReadOnlyMatrix3D(Matrix3D value) => new(value.Data);
}
