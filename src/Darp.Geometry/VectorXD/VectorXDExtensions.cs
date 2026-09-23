namespace Darp.Geometry;

public static partial class GeometryExtensions
{
    public static double[] ToArray(this VectorXD vector) => vector.AsReadOnly().ToArray();

    public static double[] ToArray(this ReadOnlyVectorXD vector)
    {
        var result = new double[vector.Count];
        for (int i = 0; i < result.Length; i++)
            result[i] = vector[i];
        return result;
    }

    public static VectorXD Clone(this ReadOnlyVectorXD vector) => new(vector.ToArray());

    public static VectorXD Normalized(this in ReadOnlyVectorXD value) =>
        Matrix.Normalized<ReadOnlyVectorXD, VectorXD>(value);

    public static double Dot(this in ReadOnlyVectorXD left, in ReadOnlyVectorXD right) => Matrix.Dot(left, right);

    public static VectorXD Add(this in ReadOnlyVectorXD left, in ReadOnlyVectorXD right)
    {
        using var leftLease = MatrixMarshal.GetReadOnlyTensorSpan(left, out var leftSpan);
        using var rightLease = MatrixMarshal.GetReadOnlyTensorSpan(right, out var rightSpan);
        return new VectorXD(TensorKernels.Add(leftSpan, rightSpan));
    }

    public static VectorXD Subtract(this in ReadOnlyVectorXD left, in ReadOnlyVectorXD right)
    {
        using var leftLease = MatrixMarshal.GetReadOnlyTensorSpan(left, out var leftSpan);
        using var rightLease = MatrixMarshal.GetReadOnlyTensorSpan(right, out var rightSpan);
        return new VectorXD(TensorKernels.Subtract(leftSpan, rightSpan));
    }

    public static VectorXD Scale(this in ReadOnlyVectorXD value, double scalar)
    {
        using var valueLease = MatrixMarshal.GetReadOnlyTensorSpan(value, out var valueSpan);
        return new VectorXD(TensorKernels.Scale(valueSpan, scalar));
    }

    public static VectorXD Divide(this in ReadOnlyVectorXD value, double scalar)
    {
        using var valueLease = MatrixMarshal.GetReadOnlyTensorSpan(value, out var valueSpan);
        return new VectorXD(TensorKernels.Divide(valueSpan, scalar));
    }

    public static VectorXD Lerp(this in ReadOnlyVectorXD left, in ReadOnlyVectorXD right, double amount)
    {
        using var leftLease = MatrixMarshal.GetReadOnlyTensorSpan(left, out var leftSpan);
        using var rightLease = MatrixMarshal.GetReadOnlyTensorSpan(right, out var rightSpan);
        return new VectorXD(TensorKernels.Lerp(leftSpan, rightSpan, amount));
    }
}
