namespace Darp.Geometry;

public readonly partial struct ReadOnlyMatrixXD
{
    public static ReadOnlyMatrixXD operator +(ReadOnlyMatrixXD a, ReadOnlyMatrixXD b) => Matrix.Add(a, b);

    public static ReadOnlyMatrixXD operator -(ReadOnlyMatrixXD a, ReadOnlyMatrixXD b) => Matrix.Subtract(a, b);

    public static ReadOnlyMatrixXD operator *(ReadOnlyMatrixXD a, ReadOnlyMatrixXD b) => Matrix.Multiply(a, b);

    public static ReadOnlyMatrixXD operator *(ReadOnlyMatrixXD a, double scalar) => Matrix.Scale(a, scalar);

    public static ReadOnlyMatrixXD operator *(double scalar, ReadOnlyMatrixXD a) => a * scalar;

    public static ReadOnlyMatrixXD operator /(ReadOnlyMatrixXD a, double scalar) => Matrix.Divide(a, scalar);

    public static ReadOnlyMatrixXD operator -(ReadOnlyMatrixXD a) => Matrix.Scale(a, -1);
}
