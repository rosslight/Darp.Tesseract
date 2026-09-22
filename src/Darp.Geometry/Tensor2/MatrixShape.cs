using System.Numerics.Tensors;

namespace Darp.Geometry.Tensor2;

// Checks precede dimension indexing so rank errors are reported at the operation boundary.
internal static class MatrixShape
{
    public static void RequireMatrix(ReadOnlyTensorSpan<double> matrix)
    {
        if (matrix.Rank != 2 || matrix.Lengths[0] <= 0 || matrix.Lengths[1] <= 0 ||
            matrix.Lengths[0] > int.MaxValue || matrix.Lengths[1] > int.MaxValue)
            throw new ArgumentException("Expected a nonempty rank-two matrix with Int32 dimensions.", nameof(matrix));
    }

    public static void RequireSameShape(ReadOnlyTensorSpan<double> left, ReadOnlyTensorSpan<double> right)
    {
        RequireMatrix(left);
        RequireMatrix(right);
        if (left.Lengths[0] != right.Lengths[0] || left.Lengths[1] != right.Lengths[1])
            throw new ArgumentException("Matrix shapes must match.", nameof(right));
    }

    public static void RequireProduct(ReadOnlyTensorSpan<double> left, ReadOnlyTensorSpan<double> right)
    {
        RequireMatrix(left);
        RequireMatrix(right);
        if (left.Lengths[1] != right.Lengths[0])
            throw new ArgumentException("Inner dimensions must match.", nameof(right));
    }

    public static void RequireVectors(ReadOnlyTensorSpan<double> left, ReadOnlyTensorSpan<double> right)
    {
        RequireSameShape(left, right);
        if (left.Lengths[1] != 1)
            throw new ArgumentException("Expected N by 1 column vectors.");
    }

    public static void RequireSize(ReadOnlyTensorSpan<double> matrix, int rows, int columns)
    {
        RequireMatrix(matrix);
        if (matrix.Lengths[0] != rows || matrix.Lengths[1] != columns)
            throw new ArgumentException($"Expected a {rows} by {columns} matrix.", nameof(matrix));
    }

    public static void RequireVector3Pair(ReadOnlyTensorSpan<double> left, ReadOnlyTensorSpan<double> right)
    {
        RequireSize(left, 3, 1);
        RequireSize(right, 3, 1);
    }
}
