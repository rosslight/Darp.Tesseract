namespace Darp.Geometry;

/// <summary>Geometry operations returning independent results.</summary>
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

    public static QuaternionD Clone(this IReadOnlyQuaternionD value) => new(value.X, value.Y, value.Z, value.W);

    public static Isometry3D Clone(this IReadOnlyIsometry3D value) => Isometry3D.FromMatrix(value);

    public static VectorXD Normalized(this in ReadOnlyVectorXD value) =>
        VectorXD.FromMatrix(Matrix.Normalized(value));

    public static Vector3D Normalized(this in ReadOnlyVector3D value) =>
        Vector3D.FromMatrix(Matrix.Normalized(value));

    public static QuaternionD Normalized(this in ReadOnlyQuaternionD value) =>
        QuaternionD.FromMatrix(Matrix.Normalized(value));

    public static Vector3D Cross(this in ReadOnlyVector3D value, in ReadOnlyVector3D other) =>
        Matrix.Cross(value, other);

    public static double Dot(this in ReadOnlyVectorXD left, in ReadOnlyVectorXD right) => Matrix.Dot(left, right);

    public static double Dot(this in ReadOnlyQuaternionD left, in ReadOnlyQuaternionD right) =>
        Matrix.Dot(left, right);

    public static VectorXD Add(this in ReadOnlyVectorXD left, in ReadOnlyVectorXD right) =>
        VectorXD.FromMatrix(Matrix.Add(left, right));

    public static VectorXD Subtract(this in ReadOnlyVectorXD left, in ReadOnlyVectorXD right) =>
        VectorXD.FromMatrix(Matrix.Subtract(left, right));

    public static VectorXD Scale(this in ReadOnlyVectorXD value, double scalar) =>
        VectorXD.FromMatrix(Matrix.Scale(value, scalar));

    public static VectorXD Divide(this in ReadOnlyVectorXD value, double scalar) =>
        VectorXD.FromMatrix(Matrix.Divide(value, scalar));

    public static VectorXD Lerp(this IReadOnlyVectorXD left, IReadOnlyVectorXD right, double amount) =>
        VectorXD.FromMatrix(Matrix.Lerp(left, right, amount));

    public static Vector3D Add(this IReadOnlyVector3D left, IReadOnlyVector3D right) =>
        Vector3D.FromMatrix(Matrix.Add(left, right));

    public static Vector3D Subtract(this IReadOnlyVector3D left, IReadOnlyVector3D right) =>
        Vector3D.FromMatrix(Matrix.Subtract(left, right));

    public static Vector3D Scale(this in ReadOnlyVector3D value, double scalar) =>
        Vector3D.FromMatrix(Matrix.Scale(value, scalar));

    public static Vector3D Divide(this in ReadOnlyVector3D value, double scalar) =>
        Vector3D.FromMatrix(Matrix.Divide(value, scalar));

    public static Vector3D Lerp(this IReadOnlyVector3D left, IReadOnlyVector3D right, double amount) =>
        Vector3D.FromMatrix(Matrix.Lerp(left, right, amount));

}
