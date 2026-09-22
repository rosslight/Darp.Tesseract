using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>Shared numerical algorithms over rank-two tensor views. Results own their coefficients.</summary>
internal static class TensorKernels
{
    /// <summary>Frobenius norm (Euclidean norm for column vectors).</summary>
    public static double Norm(ReadOnlyTensorSpan<double> matrix)
    {
        MatrixShape.RequireMatrix(matrix);
        if (matrix.Lengths[0] == 0 || matrix.Lengths[1] == 0)
            return 0;
        double scale = 0;
        for (int c = 0; c < matrix.Lengths[1]; c++)
        for (int r = 0; r < matrix.Lengths[0]; r++)
            scale = Math.Max(scale, Math.Abs(matrix[r, c]));
        if (scale == 0 || !double.IsFinite(scale))
            return scale;
        double sum = 0;
        for (int c = 0; c < matrix.Lengths[1]; c++)
        for (int r = 0; r < matrix.Lengths[0]; r++)
        {
            double x = matrix[r, c] / scale;
            sum += x * x;
        }
        return scale * Math.Sqrt(sum);
    }

    public static double SquaredNorm(ReadOnlyTensorSpan<double> matrix)
    {
        MatrixShape.RequireMatrix(matrix);
        return InnerProduct(matrix, matrix);
    }

    /// <summary>Frobenius inner product of matrices with identical shapes.</summary>
    public static double InnerProduct(ReadOnlyTensorSpan<double> left, ReadOnlyTensorSpan<double> right)
    {
        MatrixShape.RequireSameShape(left, right);
        double sum = 0;
        for (int c = 0; c < left.Lengths[1]; c++)
        {
            if (TryGetContiguousColumn(left, c, out var a) && TryGetContiguousColumn(right, c, out var b))
                sum += TensorPrimitives.Dot(a, b);
            else
                for (int r = 0; r < left.Lengths[0]; r++)
                    sum += left[r, c] * right[r, c];
        }
        return sum;
    }

    public static double Dot(ReadOnlyTensorSpan<double> left, ReadOnlyTensorSpan<double> right)
    {
        MatrixShape.RequireVectors(left, right);
        return InnerProduct(left, right);
    }

    public static MatrixXD Normalized(ReadOnlyTensorSpan<double> matrix)
    {
        MatrixShape.RequireMatrix(matrix);
        double norm = Norm(matrix);
        if (!(norm > 0) || !double.IsFinite(norm))
            throw new InvalidOperationException("Finite nonzero coefficients are required.");
        return Divide(matrix, norm);
    }

    public static MatrixData Add(ReadOnlyTensorSpan<double> left, ReadOnlyTensorSpan<double> right)
    {
        MatrixShape.RequireSameShape(left, right);
        var result = new MatrixData((int)left.Lengths[0], (int)left.Lengths[1]);
        using var destinationLease = result.AcquireWritableTensorSpan(out var destination);
        for (int c = 0; c < left.Lengths[1]; c++)
        {
            if (TryGetContiguousColumn(left, c, out var a) && TryGetContiguousColumn(right, c, out var b))
                TensorPrimitives.Add(a, b, destination.GetSpan([0, c], result.Rows));
            else
                for (int r = 0; r < left.Lengths[0]; r++)
                    destination[r, c] = left[r, c] + right[r, c];
        }
        return result;
    }

    public static MatrixData Subtract(ReadOnlyTensorSpan<double> left, ReadOnlyTensorSpan<double> right)
    {
        MatrixShape.RequireSameShape(left, right);
        var result = new MatrixData((int)left.Lengths[0], (int)left.Lengths[1]);
        using var destinationLease = result.AcquireWritableTensorSpan(out var destination);
        for (int c = 0; c < left.Lengths[1]; c++)
        {
            if (TryGetContiguousColumn(left, c, out var a) && TryGetContiguousColumn(right, c, out var b))
                TensorPrimitives.Subtract(a, b, destination.GetSpan([0, c], result.Rows));
            else
                for (int r = 0; r < left.Lengths[0]; r++)
                    destination[r, c] = left[r, c] - right[r, c];
        }
        return result;
    }

    /// <summary>Algebraic matrix multiplication; left.Lengths[1] must equal right.Lengths[0].</summary>
    public static MatrixData Multiply(ReadOnlyTensorSpan<double> left, ReadOnlyTensorSpan<double> right)
    {
        MatrixShape.RequireProduct(left, right);
        var result = new MatrixData((int)left.Lengths[0], (int)right.Lengths[1]);
        using var destinationLease = result.AcquireWritableTensorSpan(out var destination);
        for (int c = 0; c < right.Lengths[1]; c++)
        for (int k = 0; k < left.Lengths[1]; k++)
        for (int r = 0; r < left.Lengths[0]; r++)
            destination[r, c] += left[r, k] * right[k, c];
        return result;
    }

    public static MatrixData Scale(ReadOnlyTensorSpan<double> matrix, double scalar)
    {
        MatrixShape.RequireMatrix(matrix);
        var result = new MatrixData((int)matrix.Lengths[0], (int)matrix.Lengths[1]);
        using var destinationLease = result.AcquireWritableTensorSpan(out var destination);
        for (int c = 0; c < matrix.Lengths[1]; c++)
        {
            if (TryGetContiguousColumn(matrix, c, out var column))
                TensorPrimitives.Multiply(column, scalar, destination.GetSpan([0, c], result.Rows));
            else
                for (int r = 0; r < matrix.Lengths[0]; r++)
                    destination[r, c] = matrix[r, c] * scalar;
        }
        return result;
    }

    public static MatrixData Divide(ReadOnlyTensorSpan<double> matrix, double scalar)
    {
        MatrixShape.RequireMatrix(matrix);
        var result = new MatrixData((int)matrix.Lengths[0], (int)matrix.Lengths[1]);
        using var destinationLease = result.AcquireWritableTensorSpan(out var destination);
        for (int c = 0; c < matrix.Lengths[1]; c++)
        for (int r = 0; r < matrix.Lengths[0]; r++)
            destination[r, c] = matrix[r, c] / scalar;
        return result;
    }

    public static MatrixXD Lerp(ReadOnlyTensorSpan<double> left, ReadOnlyTensorSpan<double> right, double amount)
    {
        MatrixShape.RequireSameShape(left, right);
        var result = new MatrixXD((int)left.Lengths[0], (int)left.Lengths[1]);
        using var destinationLease = result.GetTensorSpan(out var destination);
        for (int c = 0; c < left.Lengths[1]; c++)
        for (int r = 0; r < left.Lengths[0]; r++)
            destination[r, c] = (1 - amount) * left[r, c] + amount * right[r, c];
        return result;
    }

    public static Vector3D Cross(ReadOnlyTensorSpan<double> left, ReadOnlyTensorSpan<double> right)
    {
        MatrixShape.RequireVector3Pair(left, right);
        return new(
            left[1, 0] * right[2, 0] - left[2, 0] * right[1, 0],
            left[2, 0] * right[0, 0] - left[0, 0] * right[2, 0],
            left[0, 0] * right[1, 0] - left[1, 0] * right[0, 0]
        );
    }

    /// <summary>Determinant specialized to 3 by 3 matrices.</summary>
    public static double Determinant3x3(ReadOnlyTensorSpan<double> matrix)
    {
        MatrixShape.RequireSize(matrix, 3, 3);
        return matrix[0, 0] * (matrix[1, 1] * matrix[2, 2] - matrix[1, 2] * matrix[2, 1])
            - matrix[0, 1] * (matrix[1, 0] * matrix[2, 2] - matrix[1, 2] * matrix[2, 0])
            + matrix[0, 2] * (matrix[1, 0] * matrix[2, 1] - matrix[1, 1] * matrix[2, 0]);
    }

    private static bool TryGetContiguousColumn(
        ReadOnlyTensorSpan<double> matrix,
        int column,
        out ReadOnlySpan<double> values
    )
    {
        if (matrix.Lengths[0] > 0 && (matrix.Lengths[0] == 1 || matrix.Strides[0] == 1))
            return matrix.TryGetSpan([0, column], (int)matrix.Lengths[0], out values);
        values = default;
        return false;
    }
}
