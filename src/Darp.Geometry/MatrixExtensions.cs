namespace Darp.Geometry;

/// <summary>Operations on read-only matrix access. Results own independent storage.</summary>
public static class MatrixExtensions
{
    public static MatrixXD Clone(this IReadOnlyMatrixD matrix)
    {
        using var valuesLease = matrix.GetReadOnlyTensorSpan(out var values);
        var result = new MatrixXD(matrix.Rows, matrix.Columns);
        using var targetLease = result.GetTensorSpan(out var target);
        for (int c = 0; c < matrix.Columns; c++)
        for (int r = 0; r < matrix.Rows; r++)
            target[r, c] = values[r, c];
        return result;
    }

    public static double[,] ToArray(this IReadOnlyMatrixD matrix)
    {
        using var valuesLease = matrix.GetReadOnlyTensorSpan(out var values);
        var result = new double[matrix.Rows, matrix.Columns];
        for (int c = 0; c < matrix.Columns; c++)
        for (int r = 0; r < matrix.Rows; r++)
            result[r, c] = values[r, c];
        return result;
    }

    public static IReadOnlyMatrixD Row(this IReadOnlyMatrixD matrix, int row) =>
        matrix.Block(row, 0, 1, matrix.Columns);

    public static IReadOnlyVectorXD Column(this IReadOnlyMatrixD matrix, int column)
    {
        var block = matrix.Block(0, column, matrix.Rows, 1);
        return block.AsVector();
    }

    public static double Norm(this IReadOnlyMatrixD matrix)
    {
        using var matrixLease = matrix.GetReadOnlyTensorSpan(out var matrixSpan);
        return TensorKernels.Norm(matrixSpan);
    }

    public static double SquaredNorm(this IReadOnlyMatrixD matrix)
    {
        using var matrixLease = matrix.GetReadOnlyTensorSpan(out var matrixSpan);
        return TensorKernels.SquaredNorm(matrixSpan);
    }

    public static double InnerProduct(this IReadOnlyMatrixD left, IReadOnlyMatrixD right)
    {
        using var leftLease = left.GetReadOnlyTensorSpan(out var leftSpan);
        using var rightLease = right.GetReadOnlyTensorSpan(out var rightSpan);
        return TensorKernels.InnerProduct(leftSpan, rightSpan);
    }

    public static double Dot(this IReadOnlyMatrixD left, IReadOnlyMatrixD right)
    {
        using var leftLease = left.GetReadOnlyTensorSpan(out var leftSpan);
        using var rightLease = right.GetReadOnlyTensorSpan(out var rightSpan);
        return TensorKernels.Dot(leftSpan, rightSpan);
    }

    public static MatrixXD Normalized(this IReadOnlyMatrixD matrix)
    {
        using var matrixLease = matrix.GetReadOnlyTensorSpan(out var matrixSpan);
        return TensorKernels.Normalized(matrixSpan);
    }

    public static MatrixXD Add(this IReadOnlyMatrixD left, IReadOnlyMatrixD right)
    {
        using var leftLease = left.GetReadOnlyTensorSpan(out var leftSpan);
        using var rightLease = right.GetReadOnlyTensorSpan(out var rightSpan);
        return TensorKernels.Add(leftSpan, rightSpan);
    }

    public static MatrixXD Subtract(this IReadOnlyMatrixD left, IReadOnlyMatrixD right)
    {
        using var leftLease = left.GetReadOnlyTensorSpan(out var leftSpan);
        using var rightLease = right.GetReadOnlyTensorSpan(out var rightSpan);
        return TensorKernels.Subtract(leftSpan, rightSpan);
    }

    public static MatrixXD Multiply(this IReadOnlyMatrixD left, IReadOnlyMatrixD right)
    {
        using var leftLease = left.GetReadOnlyTensorSpan(out var leftSpan);
        using var rightLease = right.GetReadOnlyTensorSpan(out var rightSpan);
        return TensorKernels.Multiply(leftSpan, rightSpan);
    }

    public static MatrixXD Scale(this IReadOnlyMatrixD matrix, double scalar)
    {
        using var matrixLease = matrix.GetReadOnlyTensorSpan(out var matrixSpan);
        return TensorKernels.Scale(matrixSpan, scalar);
    }

    public static MatrixXD Divide(this IReadOnlyMatrixD matrix, double scalar)
    {
        using var matrixLease = matrix.GetReadOnlyTensorSpan(out var matrixSpan);
        return TensorKernels.Divide(matrixSpan, scalar);
    }

    public static MatrixXD Lerp(this IReadOnlyMatrixD left, IReadOnlyMatrixD right, double amount)
    {
        using var leftLease = left.GetReadOnlyTensorSpan(out var leftSpan);
        using var rightLease = right.GetReadOnlyTensorSpan(out var rightSpan);
        return TensorKernels.Lerp(leftSpan, rightSpan, amount);
    }

    public static Vector3D Cross(this IReadOnlyMatrixD left, IReadOnlyMatrixD right)
    {
        using var leftLease = left.GetReadOnlyTensorSpan(out var leftSpan);
        using var rightLease = right.GetReadOnlyTensorSpan(out var rightSpan);
        return TensorKernels.Cross(leftSpan, rightSpan);
    }

    public static double Determinant3x3(this IReadOnlyMatrixD matrix)
    {
        using var matrixLease = matrix.GetReadOnlyTensorSpan(out var matrixSpan);
        return TensorKernels.Determinant3x3(matrixSpan);
    }

    public static VectorXD Multiply(this IReadOnlyMatrixD matrix, IReadOnlyVectorXD vector) =>
        VectorXD.FromMatrix(matrix.Multiply((IReadOnlyMatrixD)vector));

    internal static string Format(IReadOnlyMatrixD matrix)
    {
        using var valuesLease = matrix.GetReadOnlyTensorSpan(out var values);
        var rows = new string[matrix.Rows];
        for (int r = 0; r < matrix.Rows; r++)
        {
            var row = new string[matrix.Columns];
            for (int c = 0; c < matrix.Columns; c++)
                row[c] = values[r, c].ToString();
            rows[r] = $"[{string.Join(", ", row)}]";
        }
        return string.Join(Environment.NewLine, rows);
    }
}
