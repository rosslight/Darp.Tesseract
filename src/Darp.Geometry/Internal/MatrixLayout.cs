namespace Darp.Geometry;

// Shared addressing for mutable/read-only matrices, blocks, transposes, and column vectors.
internal readonly record struct MatrixLayout(int Rows, int Columns, int RowStride, int ColumnStride, int Offset = 0)
{
    internal MatrixLayout Require(int? rows, int columns)
    {
        if ((rows.HasValue && Rows != rows.Value) || Columns != columns)
            throw new ArgumentException($"Expected a {rows?.ToString() ?? "N"} by {columns} matrix.");
        return this;
    }

    public int Extent =>
        Rows == 0 || Columns == 0 ? 0 : checked((Rows - 1) * RowStride + (Columns - 1) * ColumnStride + 1);

    public static MatrixLayout Create(int rows, int columns, int rowStride, int columnStride)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(rows);
        ArgumentOutOfRangeException.ThrowIfNegative(columns);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(rowStride);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(columnStride);
        return new(rows, columns, rowStride, columnStride);
    }

    public int GetOffset(int row, int column)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(row);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(row, Rows);
        ArgumentOutOfRangeException.ThrowIfNegative(column);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(column, Columns);
        return checked(Offset + row * RowStride + column * ColumnStride);
    }

    public MatrixLayout Block(int row, int column, int rows, int columns)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(row);
        ArgumentOutOfRangeException.ThrowIfNegative(column);
        ArgumentOutOfRangeException.ThrowIfNegative(rows);
        ArgumentOutOfRangeException.ThrowIfNegative(columns);
        if (row > Rows - rows || column > Columns - columns)
            throw new ArgumentException("Block must fit within the matrix.");
        return new MatrixLayout(
            rows,
            columns,
            RowStride,
            ColumnStride,
            rows == 0 || columns == 0 ? Offset : GetOffset(row, column)
        );
    }

    public MatrixLayout Transposed() => new(Columns, Rows, ColumnStride, RowStride, Offset);
}
