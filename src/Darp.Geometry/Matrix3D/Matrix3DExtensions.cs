namespace Darp.Geometry;

/// <summary>Operations for fixed-size 3 by 3 matrices.</summary>
public static partial class GeometryExtensions
{
    public static Matrix3D Clone(this ReadOnlyMatrix3D matrix) => Matrix3D.FromArray(matrix.Data.Clone());

    public static Matrix3D Normalized(this in ReadOnlyMatrix3D value) =>
        Matrix.Normalized<ReadOnlyMatrix3D, Matrix3D>(value);

    public static double Determinant(this in ReadOnlyMatrix3D value) => Matrix.Determinant3x3(value);

    public static QuaternionD ToQuaternion(this in ReadOnlyMatrix3D value) => QuaternionD.FromRotationMatrix(value);

    public static Vector3D Multiply(this in ReadOnlyMatrix3D matrix, in ReadOnlyVector3D vector) =>
        new(Matrix.Multiply(matrix, vector).Data);

    public static Matrix3D Multiply(this in ReadOnlyMatrix3D left, in ReadOnlyMatrix3D right) =>
        new(Matrix.Multiply(left, right).Data);

    public static Matrix3D Add(this in ReadOnlyMatrix3D left, in ReadOnlyMatrix3D right) =>
        new(Matrix.Add(left, right).Data);

    public static Matrix3D Subtract(this in ReadOnlyMatrix3D left, in ReadOnlyMatrix3D right) =>
        new(Matrix.Subtract(left, right).Data);

    public static Matrix3D Scale(this in ReadOnlyMatrix3D value, double scalar) =>
        new(Matrix.Scale(value, scalar).Data);

    public static Matrix3D Divide(this in ReadOnlyMatrix3D value, double scalar) =>
        new(Matrix.Divide(value, scalar).Data);
}
