namespace Darp.Geometry;

public readonly partial struct MatrixXD
{
    public static MatrixXD operator +(MatrixXD a, ReadOnlyMatrixXD b) => Matrix.Add(a, b);

    public static MatrixXD operator -(MatrixXD a, ReadOnlyMatrixXD b) => Matrix.Subtract(a, b);

    public static MatrixXD operator *(MatrixXD a, ReadOnlyMatrixXD b) => Matrix.Multiply(a, b);

    public static MatrixXD operator *(MatrixXD a, double scalar) => Matrix.Scale(a, scalar);

    public static MatrixXD operator *(double scalar, MatrixXD a) => a * scalar;

    public static MatrixXD operator /(MatrixXD a, double scalar) => Matrix.Divide(a, scalar);

    public static MatrixXD operator -(MatrixXD a) => Matrix.Scale(a, -1);

    /// <summary>Creates a read-only view that shares coefficients with <paramref name="value"/>.</summary>
    /// <param name="value">The matrix to view.</param>
    public static implicit operator ReadOnlyMatrixXD(MatrixXD value) => new(value.Data);
}
