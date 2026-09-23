using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>A read-only matrix view over shared coefficient storage.</summary>
/// <remarks>
/// This view shares coefficients with its source. A writable alias can still change them. The default value is empty.
/// </remarks>
public readonly partial struct ReadOnlyMatrixXD : IReadOnlyMatrixD<ReadOnlyMatrixXD>
{
    private static readonly MatrixData s_zeroData = MatrixXD.s_zeroData;

    internal MatrixData Data => field.Storage is null ? s_zeroData : field;

    internal ReadOnlyMatrixXD(in MatrixData data) => Data = data;

    internal ReadOnlyMatrixXD(MatrixStorage storage, in MatrixLayout layout)
        : this(new MatrixData(storage, layout)) { }

    /// <inheritdoc/>
    public int Rows => Data.Rows;

    /// <inheritdoc/>
    public int Columns => Data.Columns;

    /// <inheritdoc/>
    public int RowStride => Data.RowStride;

    /// <inheritdoc/>
    public int ColumnStride => Data.ColumnStride;

    /// <inheritdoc/>
    public double this[int row, int column] => Data[row, column];

    /// <inheritdoc/>
    public double this[Index row, Index column] => Data[row, column];

    public TensorSpanLease GetReadOnlyTensorSpan(out ReadOnlyTensorSpan<double> span) =>
        Data.AcquireReadOnlyTensorSpan(out span);

    static ReadOnlyMatrixXD IReadOnlyMatrixD<ReadOnlyMatrixXD>.Create(in MatrixData data) => new(data);

    /// <summary>Returns a read-only view of the selected rows and columns.</summary>
    /// <remarks>Changes through a writable alias are visible through the view.</remarks>
    /// <param name="rows">The rows to include.</param>
    /// <param name="columns">The columns to include.</param>
    /// <returns>A view that shares coefficients with this matrix.</returns>
    public ReadOnlyMatrixXD this[Range rows, Range columns] => new(Data[rows, columns]);

    /// <summary>Returns a transposed read-only view of this matrix.</summary>
    /// <remarks>Changes through a writable alias are visible through the view.</remarks>
    /// <returns>A view with rows and columns exchanged that shares coefficients with this matrix.</returns>
    public ReadOnlyMatrixXD Transposed() => new(Data.AsTransposedLayout());

    /// <summary>Returns a read-only vector view of this matrix.</summary>
    /// <remarks>Changes through a writable alias are visible through the vector.</remarks>
    /// <returns>A vector that shares coefficients with this matrix.</returns>
    /// <exception cref="ArgumentException">This matrix does not have exactly one column.</exception>
    public ReadOnlyVectorXD AsVector() => new(Data.AsVectorLayout());

    /// <inheritdoc/>
    public override string ToString() => Matrix.Format(this);
}
