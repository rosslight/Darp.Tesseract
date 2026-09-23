namespace Darp.Geometry;

public static partial class GeometryExtensions
{
    public static double[] ToArray(this Vector3D vector) => [vector.X, vector.Y, vector.Z];

    public static Vector3D Clone(this ReadOnlyVector3D vector) => new(vector.X, vector.Y, vector.Z);

    public static Vector3D Normalized(this in ReadOnlyVector3D value) =>
        Matrix.Normalized<ReadOnlyVector3D, Vector3D>(value);

    public static Vector3D Cross(this in ReadOnlyVector3D value, in ReadOnlyVector3D other) =>
        Matrix.Cross(value, other);

    public static double Dot(this in ReadOnlyVector3D left, in ReadOnlyVector3D right) => Matrix.Dot(left, right);

    public static Vector3D Add(this in ReadOnlyVector3D left, in ReadOnlyVector3D right)
    {
        using var leftLease = MatrixMarshal.GetReadOnlyTensorSpan(left, out var leftSpan);
        using var rightLease = MatrixMarshal.GetReadOnlyTensorSpan(right, out var rightSpan);
        return new Vector3D(TensorKernels.Add(leftSpan, rightSpan));
    }

    public static Vector3D Subtract(this in ReadOnlyVector3D left, in ReadOnlyVector3D right)
    {
        using var leftLease = MatrixMarshal.GetReadOnlyTensorSpan(left, out var leftSpan);
        using var rightLease = MatrixMarshal.GetReadOnlyTensorSpan(right, out var rightSpan);
        return new Vector3D(TensorKernels.Subtract(leftSpan, rightSpan));
    }

    public static Vector3D Scale(this in ReadOnlyVector3D value, double scalar)
    {
        using var valueLease = MatrixMarshal.GetReadOnlyTensorSpan(value, out var valueSpan);
        return new Vector3D(TensorKernels.Scale(valueSpan, scalar));
    }

    public static Vector3D Divide(this in ReadOnlyVector3D value, double scalar)
    {
        using var valueLease = MatrixMarshal.GetReadOnlyTensorSpan(value, out var valueSpan);
        return new Vector3D(TensorKernels.Divide(valueSpan, scalar));
    }

    public static Vector3D Lerp(this in ReadOnlyVector3D left, in ReadOnlyVector3D right, double amount)
    {
        using var leftLease = MatrixMarshal.GetReadOnlyTensorSpan(left, out var leftSpan);
        using var rightLease = MatrixMarshal.GetReadOnlyTensorSpan(right, out var rightSpan);
        return new Vector3D(TensorKernels.Lerp(leftSpan, rightSpan, amount));
    }
}
