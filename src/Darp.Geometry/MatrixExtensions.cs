namespace Darp.Geometry;

/// <summary>Operations on read-only matrix access. Results own independent storage.</summary>
public static class MatrixExtensions
{
    public static MatrixXD Clone(this IReadOnlyMatrixD matrix)
    {
        using var source = matrix.AsReadOnlyMatrix();
        var values = source.AsReadOnlyTensorSpan();
        var result = new MatrixXD(source.Rows, source.Columns);
        var target = result.AsTensorSpan();
        for (int c = 0; c < source.Columns; c++)
            for (int r = 0; r < source.Rows; r++) target[r, c] = values[r, c];
        return result;
    }

    public static double[,] ToArray(this IReadOnlyMatrixD matrix)
    {
        using var source = matrix.AsReadOnlyMatrix();
        var values = source.AsReadOnlyTensorSpan();
        var result = new double[source.Rows, source.Columns];
        for (int c = 0; c < source.Columns; c++)
            for (int r = 0; r < source.Rows; r++) result[r, c] = values[r, c];
        return result;
    }

    public static IReadOnlyMatrixD Row(this IReadOnlyMatrixD matrix, int row) => matrix.Block(row, 0, 1, matrix.Columns);
    public static IReadOnlyVectorXD Column(this IReadOnlyMatrixD matrix, int column)
    {
        using var block = matrix.Block(0, column, matrix.Rows, 1);
        return block.AsVector();
    }

    public static double Norm(this IReadOnlyMatrixD matrix) => MatrixOperations.Norm(matrix);
    public static double SquaredNorm(this IReadOnlyMatrixD matrix) => MatrixOperations.SquaredNorm(matrix);
    public static MatrixXD Normalized(this IReadOnlyMatrixD matrix) => MatrixOperations.Normalized(matrix);
    public static MatrixXD Add(this IReadOnlyMatrixD matrix, IReadOnlyMatrixD other) => MatrixOperations.Add(matrix, other);
    public static MatrixXD Subtract(this IReadOnlyMatrixD matrix, IReadOnlyMatrixD other) => MatrixOperations.Subtract(matrix, other);
    public static MatrixXD Multiply(this IReadOnlyMatrixD matrix, IReadOnlyMatrixD other) => MatrixOperations.Multiply(matrix, other);
    public static VectorXD Multiply(this IReadOnlyMatrixD matrix, IReadOnlyVectorXD vector) =>
        VectorXD.FromOwnedMatrix(MatrixOperations.Multiply(matrix, vector));
    public static MatrixXD Scale(this IReadOnlyMatrixD matrix, double scalar) => MatrixOperations.Scale(matrix, scalar);
    public static MatrixXD Divide(this IReadOnlyMatrixD matrix, double scalar) => MatrixOperations.Divide(matrix, scalar);

    internal static string Format(IReadOnlyMatrixD matrix)
    {
        using var source = matrix.AsReadOnlyMatrix();
        var values = source.AsReadOnlyTensorSpan();
        var rows = new string[source.Rows];
        for (int r = 0; r < source.Rows; r++)
        {
            var row = new string[source.Columns];
            for (int c = 0; c < source.Columns; c++) row[c] = values[r, c].ToString();
            rows[r] = $"[{string.Join(", ", row)}]";
        }
        return string.Join(Environment.NewLine, rows);
    }
}
