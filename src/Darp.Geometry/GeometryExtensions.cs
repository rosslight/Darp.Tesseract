namespace Darp.Geometry;

/// <summary>Geometry operations returning independent, disposable results.</summary>
public static partial class GeometryExtensions
{
    public static double[] ToArray(this IReadOnlyVectorXD vector)
    {
        var result = new double[vector.Count];
        for (int i = 0; i < result.Length; i++)
            result[i] = vector[i];
        return result;
    }

    public static VectorXD Clone(this IReadOnlyVectorXD vector) => new(vector.ToArray());

    public static Vector3D Clone(this IReadOnlyVector3D vector) => new(vector.X, vector.Y, vector.Z);

    public static Matrix3D Clone(this IReadOnlyMatrix3D matrix) => Matrix3D.FromArray(matrix.ToArray());

    public static QuaternionD Clone(this IReadOnlyQuaternionD value) => new(value.X, value.Y, value.Z, value.W);

    public static Isometry3D Clone(this IReadOnlyIsometry3D value) => Isometry3D.FromMatrix(value);

    public static VectorXD Normalized(this IReadOnlyVectorXD value) =>
        VectorXD.FromOwnedMatrix(MatrixOperations.Normalized(value));

    public static Vector3D Normalized(this IReadOnlyVector3D value) =>
        Vector3D.FromOwnedMatrix(MatrixOperations.Normalized(value));

    public static Matrix3D Normalized(this IReadOnlyMatrix3D value) =>
        Matrix3D.FromOwnedMatrix(MatrixOperations.Normalized(value));

    public static QuaternionD Normalized(this IReadOnlyQuaternionD value) =>
        QuaternionD.FromOwnedMatrix(MatrixOperations.Normalized(value));

    public static Vector3D Cross(this IReadOnlyVector3D value, IReadOnlyVector3D other) =>
        MatrixOperations.Cross(value, other);

    public static double Determinant(this IReadOnlyMatrix3D value) => MatrixOperations.Determinant3x3(value);

    public static QuaternionD ToQuaternion(this IReadOnlyMatrix3D value) => QuaternionD.FromRotationMatrix(value);

    public static Vector3D Multiply(this IReadOnlyMatrix3D matrix, IReadOnlyVector3D vector) =>
        Vector3D.FromOwnedMatrix(MatrixOperations.Multiply(matrix, vector));

    public static Matrix3D Multiply(this IReadOnlyMatrix3D left, IReadOnlyMatrix3D right) =>
        Matrix3D.FromOwnedMatrix(MatrixOperations.Multiply(left, right));

    public static double Dot(this IReadOnlyVectorXD left, IReadOnlyVectorXD right) => MatrixOperations.Dot(left, right);

    public static double Dot(this IReadOnlyQuaternionD left, IReadOnlyQuaternionD right) =>
        MatrixOperations.Dot(left, right);

    public static VectorXD Add(this IReadOnlyVectorXD left, IReadOnlyVectorXD right) =>
        VectorXD.FromOwnedMatrix(MatrixOperations.Add(left, right));

    public static VectorXD Subtract(this IReadOnlyVectorXD left, IReadOnlyVectorXD right) =>
        VectorXD.FromOwnedMatrix(MatrixOperations.Subtract(left, right));

    public static VectorXD Scale(this IReadOnlyVectorXD value, double scalar) =>
        VectorXD.FromOwnedMatrix(MatrixOperations.Scale(value, scalar));

    public static VectorXD Divide(this IReadOnlyVectorXD value, double scalar) =>
        VectorXD.FromOwnedMatrix(MatrixOperations.Divide(value, scalar));

    public static VectorXD Lerp(this IReadOnlyVectorXD left, IReadOnlyVectorXD right, double amount) =>
        VectorXD.FromOwnedMatrix(MatrixOperations.Lerp(left, right, amount));

    public static Vector3D Add(this IReadOnlyVector3D left, IReadOnlyVector3D right) =>
        Vector3D.FromOwnedMatrix(MatrixOperations.Add(left, right));

    public static Vector3D Subtract(this IReadOnlyVector3D left, IReadOnlyVector3D right) =>
        Vector3D.FromOwnedMatrix(MatrixOperations.Subtract(left, right));

    public static Vector3D Scale(this IReadOnlyVector3D value, double scalar) =>
        Vector3D.FromOwnedMatrix(MatrixOperations.Scale(value, scalar));

    public static Vector3D Divide(this IReadOnlyVector3D value, double scalar) =>
        Vector3D.FromOwnedMatrix(MatrixOperations.Divide(value, scalar));

    public static Vector3D Lerp(this IReadOnlyVector3D left, IReadOnlyVector3D right, double amount) =>
        Vector3D.FromOwnedMatrix(MatrixOperations.Lerp(left, right, amount));

    public static Matrix3D Add(this IReadOnlyMatrix3D left, IReadOnlyMatrix3D right) =>
        Matrix3D.FromOwnedMatrix(MatrixOperations.Add(left, right));

    public static Matrix3D Subtract(this IReadOnlyMatrix3D left, IReadOnlyMatrix3D right) =>
        Matrix3D.FromOwnedMatrix(MatrixOperations.Subtract(left, right));

    public static Matrix3D Scale(this IReadOnlyMatrix3D value, double scalar) =>
        Matrix3D.FromOwnedMatrix(MatrixOperations.Scale(value, scalar));

    public static Matrix3D Divide(this IReadOnlyMatrix3D value, double scalar) =>
        Matrix3D.FromOwnedMatrix(MatrixOperations.Divide(value, scalar));
}
