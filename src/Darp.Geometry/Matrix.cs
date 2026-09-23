using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>Operations on read-only matrix access. Results own independent storage.</summary>
public static class Matrix
{
    public static TM Clone<TM>(in ReadOnlyMatrixXD matrix)
        where TM : IReadOnlyMatrixD<TM>
    {
        using var valuesLease = matrix.GetReadOnlyTensorSpan(out var values);
        var result = new MatrixData(matrix.Rows, matrix.Columns);
        using var targetLease = result.AcquireWritableTensorSpan(out var target);
        for (int c = 0; c < matrix.Columns; c++)
        for (int r = 0; r < matrix.Rows; r++)
            target[r, c] = values[r, c];
        return TM.Create(result);
    }

    public static double[,] ToArray<TM>(in TM matrix)
        where TM : IReadOnlyMatrixD<TM>
    {
        using var valuesLease = matrix.GetReadOnlyTensorSpan(out var values);
        var result = new double[matrix.Rows, matrix.Columns];
        for (int c = 0; c < matrix.Columns; c++)
        for (int r = 0; r < matrix.Rows; r++)
            result[r, c] = values[r, c];
        return result;
    }

    public static double Norm<TM>(in TM matrix)
        where TM : IReadOnlyMatrixD<TM>
    {
        using var matrixLease = matrix.GetReadOnlyTensorSpan(out var matrixSpan);
        return TensorKernels.Norm(matrixSpan);
    }

    public static double SquaredNorm<TM>(in TM matrix)
        where TM : IReadOnlyMatrixD<TM>
    {
        using var matrixLease = matrix.GetReadOnlyTensorSpan(out var matrixSpan);
        return TensorKernels.SquaredNorm(matrixSpan);
    }

    public static double InnerProduct<TM1, TM2>(in TM1 left, TM2 right)
        where TM1 : IReadOnlyMatrixD<TM1>
        where TM2 : IReadOnlyMatrixD<TM2>
    {
        using var leftLease = left.GetReadOnlyTensorSpan(out var leftSpan);
        using var rightLease = right.GetReadOnlyTensorSpan(out var rightSpan);
        return TensorKernels.InnerProduct(leftSpan, rightSpan);
    }

    public static double Dot<TM1, TM2>(in TM1 left, TM2 right)
        where TM1 : IReadOnlyMatrixD<TM1>
        where TM2 : IReadOnlyMatrixD<TM2>
    {
        using var leftLease = left.GetReadOnlyTensorSpan(out var leftSpan);
        using var rightLease = right.GetReadOnlyTensorSpan(out var rightSpan);
        return TensorKernels.Dot(leftSpan, rightSpan);
    }

    public static TMR Normalized<TM, TMR>(in TM matrix)
        where TM : IReadOnlyMatrixD<TM>
        where TMR : IReadOnlyMatrixD<TMR>
    {
        using var matrixLease = matrix.GetReadOnlyTensorSpan(out var matrixSpan);
        var data = TensorKernels.Normalized(matrixSpan);
        return TMR.Create(data);
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
        MatrixData data = TensorKernels.Multiply(leftSpan, rightSpan);
        return TM1.Create(data);
    }

    public static TM Scale<TM>(in TM matrix, double scalar)
        where TM : IReadOnlyMatrixD<TM>
    {
        using var matrixLease = matrix.GetReadOnlyTensorSpan(out var matrixSpan);
        MatrixData data = TensorKernels.Scale(matrixSpan, scalar);
        return TM.Create(data);
    }

    public static TM Divide<TM>(in TM matrix, double scalar)
        where TM : IReadOnlyMatrixD<TM>
    {
        using var matrixLease = matrix.GetReadOnlyTensorSpan(out var matrixSpan);
        MatrixData data = TensorKernels.Divide(matrixSpan, scalar);
        return TM.Create(data);
    }

    public static TM1 Lerp<TM1, TM2>(in TM1 left, in TM2 right, double amount)
        where TM1 : IReadOnlyMatrixD<TM1>
        where TM2 : IReadOnlyMatrixD<TM2>
    {
        using var leftLease = left.GetReadOnlyTensorSpan(out var leftSpan);
        using var rightLease = right.GetReadOnlyTensorSpan(out var rightSpan);
        MatrixData data = TensorKernels.Lerp(leftSpan, rightSpan, amount);
        return TM1.Create(data);
    }

    public static Vector3D Cross<TM1, TM2>(in TM1 left, in TM2 right)
        where TM1 : IReadOnlyMatrixD<TM1>
        where TM2 : IReadOnlyMatrixD<TM2>
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
