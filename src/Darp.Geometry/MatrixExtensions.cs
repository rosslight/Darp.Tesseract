namespace Darp.Geometry;

/// <summary>Operations on read-only matrix access. Results own independent storage.</summary>
/// <remarks>Callers must not dispose inputs concurrently with an operation.</remarks>
public static class MatrixExtensions
{
    public static MatrixXD Clone(this IReadOnlyMatrixD matrix)
    {
        var values = matrix.AsReadOnlyTensorSpan();
        var result = new MatrixXD(matrix.Rows, matrix.Columns);
        var target = result.AsTensorSpan();
        for (int c = 0; c < matrix.Columns; c++)
        for (int r = 0; r < matrix.Rows; r++)
            target[r, c] = values[r, c];
        GC.KeepAlive(matrix);
        return result;
    }

    public static double[,] ToArray(this IReadOnlyMatrixD matrix)
    {
        var values = matrix.AsReadOnlyTensorSpan();
        var result = new double[matrix.Rows, matrix.Columns];
        for (int c = 0; c < matrix.Columns; c++)
        for (int r = 0; r < matrix.Rows; r++)
            result[r, c] = values[r, c];
        GC.KeepAlive(matrix);
        return result;
    }

    public static IReadOnlyMatrixD Row(this IReadOnlyMatrixD matrix, int row) =>
        matrix.Block(row, 0, 1, matrix.Columns);

    public static IReadOnlyVectorXD Column(this IReadOnlyMatrixD matrix, int column)
    {
        using var block = matrix.Block(0, column, matrix.Rows, 1);
        return block.AsVector();
    }

    public static double Norm(this IReadOnlyMatrixD matrix)
    {
        var result = TensorKernels.Norm(matrix.AsReadOnlyTensorSpan());
        GC.KeepAlive(matrix);
        return result;
    }

    public static double SquaredNorm(this IReadOnlyMatrixD matrix)
    {
        var result = TensorKernels.SquaredNorm(matrix.AsReadOnlyTensorSpan());
        GC.KeepAlive(matrix);
        return result;
    }

    public static double InnerProduct(this IReadOnlyMatrixD left, IReadOnlyMatrixD right)
    {
        var result = TensorKernels.InnerProduct(left.AsReadOnlyTensorSpan(), right.AsReadOnlyTensorSpan());
        GC.KeepAlive(left);
        GC.KeepAlive(right);
        return result;
    }

    public static double Dot(this IReadOnlyMatrixD left, IReadOnlyMatrixD right)
    {
        var result = TensorKernels.Dot(left.AsReadOnlyTensorSpan(), right.AsReadOnlyTensorSpan());
        GC.KeepAlive(left);
        GC.KeepAlive(right);
        return result;
    }

    public static MatrixXD Normalized(this IReadOnlyMatrixD matrix)
    {
        var result = TensorKernels.Normalized(matrix.AsReadOnlyTensorSpan());
        GC.KeepAlive(matrix);
        return result;
    }

    public static MatrixXD Add(this IReadOnlyMatrixD left, IReadOnlyMatrixD right)
    {
        var result = TensorKernels.Add(left.AsReadOnlyTensorSpan(), right.AsReadOnlyTensorSpan());
        GC.KeepAlive(left);
        GC.KeepAlive(right);
        return result;
    }

    public static MatrixXD Subtract(this IReadOnlyMatrixD left, IReadOnlyMatrixD right)
    {
        var result = TensorKernels.Subtract(left.AsReadOnlyTensorSpan(), right.AsReadOnlyTensorSpan());
        GC.KeepAlive(left);
        GC.KeepAlive(right);
        return result;
    }

    public static MatrixXD Multiply(this IReadOnlyMatrixD left, IReadOnlyMatrixD right)
    {
        var result = TensorKernels.Multiply(left.AsReadOnlyTensorSpan(), right.AsReadOnlyTensorSpan());
        GC.KeepAlive(left);
        GC.KeepAlive(right);
        return result;
    }

    public static MatrixXD Scale(this IReadOnlyMatrixD matrix, double scalar)
    {
        var result = TensorKernels.Scale(matrix.AsReadOnlyTensorSpan(), scalar);
        GC.KeepAlive(matrix);
        return result;
    }

    public static MatrixXD Divide(this IReadOnlyMatrixD matrix, double scalar)
    {
        var result = TensorKernels.Divide(matrix.AsReadOnlyTensorSpan(), scalar);
        GC.KeepAlive(matrix);
        return result;
    }

    public static MatrixXD Lerp(this IReadOnlyMatrixD left, IReadOnlyMatrixD right, double amount)
    {
        var result = TensorKernels.Lerp(left.AsReadOnlyTensorSpan(), right.AsReadOnlyTensorSpan(), amount);
        GC.KeepAlive(left);
        GC.KeepAlive(right);
        return result;
    }

    public static Vector3D Cross(this IReadOnlyMatrixD left, IReadOnlyMatrixD right)
    {
        var result = TensorKernels.Cross(left.AsReadOnlyTensorSpan(), right.AsReadOnlyTensorSpan());
        GC.KeepAlive(left);
        GC.KeepAlive(right);
        return result;
    }

    public static double Determinant3x3(this IReadOnlyMatrixD matrix)
    {
        var result = TensorKernels.Determinant3x3(matrix.AsReadOnlyTensorSpan());
        GC.KeepAlive(matrix);
        return result;
    }

    public static VectorXD Multiply(this IReadOnlyMatrixD matrix, IReadOnlyVectorXD vector) =>
        VectorXD.FromOwnedMatrix(matrix.Multiply((IReadOnlyMatrixD)vector));

    internal static string Format(IReadOnlyMatrixD matrix)
    {
        var values = matrix.AsReadOnlyTensorSpan();
        var rows = new string[matrix.Rows];
        for (int r = 0; r < matrix.Rows; r++)
        {
            var row = new string[matrix.Columns];
            for (int c = 0; c < matrix.Columns; c++)
                row[c] = values[r, c].ToString();
            rows[r] = $"[{string.Join(", ", row)}]";
        }
        GC.KeepAlive(matrix);
        return string.Join(Environment.NewLine, rows);
    }
}
