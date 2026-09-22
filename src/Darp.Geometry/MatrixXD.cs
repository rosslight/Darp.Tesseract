using System.Buffers;
using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>A writable matrix view over shared coefficient storage.</summary>
/// <remarks>
/// Copying a matrix copies its view, not its coefficients. Cloning creates independent storage.
/// The default value is an empty, read-only matrix. Construct a matrix to get writable storage.
/// </remarks>
public readonly struct MatrixXD : IMatrixD<MatrixXD>
{
    internal static readonly MatrixData s_zeroData = MatrixData.ReadOnlyZero(0, 0);
    internal MatrixData Data => field.Storage is null ? s_zeroData : field;

    /// <inheritdoc/>
    public int Rows => Data.Rows;

    /// <inheritdoc/>
    public int Columns => Data.Columns;

    /// <inheritdoc/>
    public int RowStride => Data.RowStride;

    /// <inheritdoc/>
    public int ColumnStride => Data.ColumnStride;

    private MatrixXD(in MatrixData data) => Data = data;

    internal MatrixXD(MatrixStorage storage, in MatrixLayout layout)
        : this(new MatrixData(storage, layout)) { }

    private MatrixXD(Memory<double> memory, in MatrixLayout layout, MemoryManager<double>? owner = null)
        : this(new MatrixData(memory, layout, owner)) { }

    /// <summary>Creates a zero-filled matrix with the specified shape.</summary>
    /// <param name="rows">The number of rows.</param>
    /// <param name="columns">The number of columns.</param>
    public MatrixXD(int rows, int columns)
        : this(new MatrixData(rows, columns)) { }

    /// <summary>Creates an empty, writable matrix.</summary>
    public MatrixXD()
        : this(new MatrixData(0, 0)) { }

    static MatrixXD IReadOnlyMatrixD<MatrixXD>.Create(in MatrixData data) => new(data);

    TensorSpanLease IReadOnlyMatrixD<MatrixXD>.GetReadOnlyTensorSpan(out ReadOnlyTensorSpan<double> span) =>
        Data.AcquireReadOnlyTensorSpan(out span);

    TensorSpanLease IMatrixD<MatrixXD>.GetTensorSpan(out TensorSpan<double> span) => Data.AcquireWritableTensorSpan(out span);

    /// <summary>Maps caller-owned memory without copying it.</summary>
    /// <remarks>
    /// Keep the memory valid while this matrix or any of its views is in use. Strides are measured in elements.
    /// </remarks>
    /// <param name="memory">The memory containing the matrix coefficients.</param>
    /// <param name="rows">The number of rows.</param>
    /// <param name="columns">The number of columns.</param>
    /// <param name="columnStride">The distance between columns, in elements. Defaults to the packed column-major stride.</param>
    /// <param name="rowStride">The distance between rows, in elements. Defaults to 1.</param>
    /// <returns>A writable matrix mapped over <paramref name="memory"/>.</returns>
    /// <exception cref="ArgumentException">The strides or memory length cannot represent the requested shape.</exception>
    public static MatrixXD CreateFromMemory(
        Memory<double> memory,
        int rows,
        int columns,
        int? columnStride = null,
        int rowStride = 1
    )
    {
        var layout = MatrixLayout.Create(
            rows,
            columns,
            rowStride,
            columnStride ?? Math.Max(1, checked(rows * rowStride))
        );
        return new MatrixXD(memory, layout);
    }

    /// <summary>Maps memory and transfers its owner's lifetime to the matrix storage.</summary>
    /// <remarks>
    /// The owner is disposed after this matrix and its views become unreachable. Do not dispose it separately.
    /// Failed construction also disposes the owner.
    /// </remarks>
    /// <param name="memory">Memory backed by <paramref name="owner"/>.</param>
    /// <param name="owner">The memory owner whose lifetime is transferred.</param>
    /// <param name="rows">The number of rows.</param>
    /// <param name="columns">The number of columns.</param>
    /// <param name="columnStride">The distance between columns, in elements. Defaults to the packed column-major stride.</param>
    /// <param name="rowStride">The distance between rows, in elements. Defaults to 1.</param>
    /// <returns>A writable matrix mapped over <paramref name="memory"/>.</returns>
    /// <exception cref="ArgumentException">The strides or memory length cannot represent the requested shape.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="owner"/> is null.</exception>
    public static MatrixXD CreateFromMemoryWithOwner(
        Memory<double> memory,
        MemoryManager<double> owner,
        int rows,
        int columns,
        int? columnStride = null,
        int rowStride = 1
    )
    {
        ArgumentNullException.ThrowIfNull(owner);
        MatrixLayout layout;
        try
        {
            layout = MatrixLayout.Create(
                rows,
                columns,
                rowStride,
                columnStride ?? Math.Max(1, checked(rows * rowStride))
            );
        }
        catch
        {
            (owner as IDisposable).Dispose();
            throw;
        }

        return new MatrixXD(memory, layout, owner);
    }

    /// <summary>Copies a two-dimensional array into a matrix.</summary>
    /// <param name="values">The values to copy.</param>
    /// <returns>A new matrix with the same row and column dimensions.</returns>
    public static MatrixXD FromArray(double[,] values)
    {
        var result = new MatrixXD(values.GetLength(0), values.GetLength(1));
        for (int c = 0; c < result.Columns; c++)
        for (int r = 0; r < result.Rows; r++)
            result[r, c] = values[r, c];
        return result;
    }

    /// <summary>Creates an identity matrix of the specified size.</summary>
    /// <param name="size">The number of rows and columns.</param>
    /// <returns>A new square matrix with ones on the diagonal and zeros elsewhere.</returns>
    public static MatrixXD Identity(int size)
    {
        var result = new MatrixXD(size, size);
        for (int i = 0; i < size; i++)
            result[i, i] = 1;
        return result;
    }

    /// <inheritdoc/>
    public double this[int row, int column]
    {
        get => Data[row, column];
        set => Data[row, column] = value;
    }

    /// <inheritdoc/>
    public double this[Index row, Index column]
    {
        get => Data[row, column];
        set => Data[row, column] = value;
    }

    /// <summary>Returns a writable view of the selected rows and columns.</summary>
    /// <remarks>Changes through the view are visible through this matrix.</remarks>
    /// <param name="rows">The rows to include.</param>
    /// <param name="columns">The columns to include.</param>
    /// <returns>A view that shares coefficients with this matrix.</returns>
    public MatrixXD this[Range rows, Range columns] => new(Data[rows, columns]);

    /// <summary>Returns a writable view of one row.</summary>
    /// <remarks>Changes through the view are visible through this matrix.</remarks>
    /// <param name="row">The row index.</param>
    /// <returns>A one-row matrix that shares coefficients with this matrix.</returns>
    public MatrixXD SliceRow(int row) => this[row..(row + 1), ..];

    /// <summary>Returns a writable view of one column.</summary>
    /// <remarks>Changes through the view are visible through this matrix.</remarks>
    /// <param name="column">The column index.</param>
    /// <returns>A vector that shares coefficients with this matrix.</returns>
    public VectorXD SliceColumn(int column) => new(Data[.., column..(column + 1)].Require(null, 1));

    /// <summary>Returns a writable transposed view of this matrix.</summary>
    /// <remarks>Changes through the view are visible through this matrix.</remarks>
    /// <returns>A view with rows and columns exchanged that shares coefficients with this matrix.</returns>
    public MatrixXD Transposed() => new(Data.Storage, Data.Layout.Transposed());

    /// <summary>Returns a writable vector view of this matrix.</summary>
    /// <remarks>Changes through the vector are visible through this matrix.</remarks>
    /// <returns>A vector that shares coefficients with this matrix.</returns>
    /// <exception cref="ArgumentException">This matrix does not have exactly one column.</exception>
    public VectorXD AsVector() => new(Data.Require(null, 1));

    /// <summary>Returns a read-only view of this matrix.</summary>
    /// <returns>A view that shares coefficients with this matrix.</returns>
    public ReadOnlyMatrixXD AsReadOnly() => new(Data);

    public static MatrixXD operator +(MatrixXD a, ReadOnlyMatrixXD b) => MatrixExtensions.Add(a, b);

    public static MatrixXD operator -(MatrixXD a, ReadOnlyMatrixXD b) => MatrixExtensions.Subtract(a, b);

    public static MatrixXD operator *(MatrixXD a, ReadOnlyMatrixXD b) => MatrixExtensions.Multiply(a, b);

    public static MatrixXD operator *(MatrixXD a, double scalar) => a.Scale(scalar);

    public static MatrixXD operator *(double scalar, MatrixXD a) => a * scalar;

    public static MatrixXD operator /(MatrixXD a, double scalar) => a.Divide(scalar);

    public static MatrixXD operator -(MatrixXD a) => a.Scale(-1);

    /// <summary>Creates a read-only view that shares coefficients with <paramref name="value"/>.</summary>
    /// <param name="value">The matrix to view.</param>
    public static implicit operator ReadOnlyMatrixXD(MatrixXD value) => new(value.Data);

    /// <inheritdoc/>
    public override string ToString() => MatrixExtensions.Format(this);
}
