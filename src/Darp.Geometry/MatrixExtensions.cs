using System.Numerics.Tensors;

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

    public static ReadOnlyMatrixXD Row<T>(this T matrix, int row)
        where T : IReadOnlyMatrixD => matrix.Block(row, 0, 1, matrix.Columns);

    public static ReadOnlyVectorXD Column<T>(this T matrix, int column)
        where T : IReadOnlyMatrixD
    {
        var block = matrix.Block(0, column, matrix.Rows, 1);
        return block.AsVector();
    }

    public static double Norm<T>(this T matrix)
        where T : IReadOnlyMatrixD
    {
        using var matrixLease = matrix.GetReadOnlyTensorSpan(out var matrixSpan);
        return TensorKernels.Norm(matrixSpan);
    }

    public static double SquaredNorm<T>(this T matrix)
        where T : IReadOnlyMatrixD
    {
        using var matrixLease = matrix.GetReadOnlyTensorSpan(out var matrixSpan);
        return TensorKernels.SquaredNorm(matrixSpan);
    }

    public static double InnerProduct<TLeft, TRight>(this TLeft left, TRight right)
        where TLeft : IReadOnlyMatrixD
        where TRight : IReadOnlyMatrixD
    {
        using var leftLease = left.GetReadOnlyTensorSpan(out var leftSpan);
        using var rightLease = right.GetReadOnlyTensorSpan(out var rightSpan);
        return TensorKernels.InnerProduct(leftSpan, rightSpan);
    }

    public static double Dot<TLeft, TRight>(this TLeft left, TRight right)
        where TLeft : IReadOnlyMatrixD
        where TRight : IReadOnlyMatrixD
    {
        using var leftLease = left.GetReadOnlyTensorSpan(out var leftSpan);
        using var rightLease = right.GetReadOnlyTensorSpan(out var rightSpan);
        return TensorKernels.Dot(leftSpan, rightSpan);
    }

    public static MatrixXD Normalized<TM>(in TM matrix)
        where TM : IReadOnlyMatrixD
    {
        using var matrixLease = matrix.GetReadOnlyTensorSpan(out var matrixSpan);
        return TensorKernels.Normalized(matrixSpan);
    }

    public static TM1 Add<TM1, TM2>(in TM1 left, in TM2 right)
        where TM1 : IReadOnlyMatrixD<TM1>
        where TM2 : IReadOnlyMatrixD<TM2>
    {
        using var leftLease = left.GetReadOnlyTensorSpan(out var leftSpan);
        using var rightLease = right.GetReadOnlyTensorSpan(out var rightSpan);
        MatrixData data = TensorKernels.Add(leftSpan, rightSpan);
        return TM1.Create(data);
    }

    public static TM1 Subtract<TM1, TM2>(in TM1 left, in TM2 right)
        where TM1 : IReadOnlyMatrixD<TM1>
        where TM2 : IReadOnlyMatrixD<TM2>
    {
        using var leftLease = left.GetReadOnlyTensorSpan(out var leftSpan);
        using var rightLease = right.GetReadOnlyTensorSpan(out var rightSpan);
        MatrixData data = TensorKernels.Subtract(leftSpan, rightSpan);
        return TM1.Create(data);
    }

    public static TM1 Multiply<TM1, TM2>(in TM1 left, in TM2 right)
        where TM1 : IReadOnlyMatrixD<TM1>
        where TM2 : IReadOnlyMatrixD<TM2>
    {
        using var leftLease = left.GetReadOnlyTensorSpan(out var leftSpan);
        using var rightLease = right.GetReadOnlyTensorSpan(out var rightSpan);
        MatrixData data =  TensorKernels.Multiply(leftSpan, rightSpan);
        return TM1.Create(data);
    }

    public static TM Scale<TM>(this TM matrix, double scalar)
        where TM : IReadOnlyMatrixD<TM>
    {
        using var matrixLease = matrix.GetReadOnlyTensorSpan(out var matrixSpan);
        MatrixData data =  TensorKernels.Scale(matrixSpan, scalar);
        return TM.Create(data);
    }

    public static TM Divide<TM>(this TM matrix, double scalar)
        where TM : IReadOnlyMatrixD<TM>
    {
        using var matrixLease = matrix.GetReadOnlyTensorSpan(out var matrixSpan);
        MatrixData data = TensorKernels.Divide(matrixSpan, scalar);
        return TM.Create(data);
    }

    public static MatrixXD Lerp(this IReadOnlyMatrixD left, IReadOnlyMatrixD right, double amount)
    {
        using var leftLease = left.GetReadOnlyTensorSpan(out var leftSpan);
        using var rightLease = right.GetReadOnlyTensorSpan(out var rightSpan);
        return TensorKernels.Lerp(leftSpan, rightSpan, amount);
    }

    public static Vector3D Cross(this ReadOnlyMatrixXD left, ReadOnlyMatrixXD right)
    {
        using TensorSpanLease leftLease = left.GetReadOnlyTensorSpan(out ReadOnlyTensorSpan<double> leftSpan);
        using TensorSpanLease rightLease = right.GetReadOnlyTensorSpan(out ReadOnlyTensorSpan<double> rightSpan);
        return TensorKernels.Cross(leftSpan, rightSpan);
    }

    public static double Determinant3x3(this in ReadOnlyMatrixXD matrix)
    {
        using TensorSpanLease matrixLease = matrix.GetReadOnlyTensorSpan(out ReadOnlyTensorSpan<double> matrixSpan);
        return TensorKernels.Determinant3x3(matrixSpan);
    }

    internal static string Format<TM>(in TM matrix)
        where TM : IReadOnlyMatrixD<TM>
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

public static class VectorExtensions
{
    public static VectorXD Multiply<TM1, TM2>(in TM1 matrix, in TM2 vector) =>
        VectorXD.FromMatrix(GeometryExtensions.Multiply(matrix, vector.AsReadOnlyMatrix()));
}
