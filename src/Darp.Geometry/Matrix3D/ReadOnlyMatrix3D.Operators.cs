namespace Darp.Geometry;

public readonly partial struct ReadOnlyMatrix3D
{
    public static ReadOnlyMatrix3D operator +(ReadOnlyMatrix3D a, ReadOnlyMatrix3D b) => Matrix.Add(a, b);

    public static ReadOnlyMatrix3D operator -(ReadOnlyMatrix3D a, ReadOnlyMatrix3D b) => Matrix.Subtract(a, b);

    public static ReadOnlyMatrix3D operator *(ReadOnlyMatrix3D a, ReadOnlyMatrix3D b) => Matrix.Multiply(a, b);

    public static ReadOnlyMatrix3D operator *(ReadOnlyMatrix3D a, double scalar) => Matrix.Scale(a, scalar);

    public static ReadOnlyMatrix3D operator *(double scalar, ReadOnlyMatrix3D a) => a * scalar;

    public static ReadOnlyMatrix3D operator /(ReadOnlyMatrix3D a, double scalar) => Matrix.Divide(a, scalar);

    public static ReadOnlyMatrix3D operator -(ReadOnlyMatrix3D a) => Matrix.Scale(a, -1);

    public static implicit operator ReadOnlyMatrixXD(ReadOnlyMatrix3D value) => new(value.Data);
}
