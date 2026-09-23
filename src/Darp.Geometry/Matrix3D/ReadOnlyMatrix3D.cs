using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>A read-only view of shared coefficients. Other aliases may change them.</summary>
public readonly partial struct ReadOnlyMatrix3D : IReadOnlyMatrixD<ReadOnlyMatrix3D>
{
    private static readonly MatrixData s_zeroData = Matrix3D.s_zeroData;
    internal MatrixData Data => field.Storage is null ? s_zeroData : field;

    internal ReadOnlyMatrix3D(in MatrixData data) => Data = data;

    internal ReadOnlyMatrix3D(MatrixStorage storage, in MatrixLayout layout)
        : this(new MatrixData(storage, layout)) { }

    /// <inheritdoc/>
    public int Rows => Data.Rows;

    /// <inheritdoc/>
    public int Columns => Data.Columns;

    /// <inheritdoc/>
    public int RowStride => Data.RowStride;

    /// <inheritdoc/>
    public int ColumnStride => Data.ColumnStride;

    static ReadOnlyMatrix3D IReadOnlyMatrixD<ReadOnlyMatrix3D>.Create(in MatrixData data) => new(data);

    TensorSpanLease IReadOnlyMatrixD<ReadOnlyMatrix3D>.GetReadOnlyTensorSpan(out ReadOnlyTensorSpan<double> span) =>
        Data.AcquireReadOnlyTensorSpan(out span);

    /// <inheritdoc/>
    public double this[int row, int column] => Data.Get(row, column);

    /// <inheritdoc/>
    public double this[Index row, Index column] => Data.Get(row, column);

    /// <summary>Returns a read-only view of the selected rows and columns.</summary>
    /// <remarks>Changes through a writable alias are visible through the view.</remarks>
    /// <param name="rows">The rows to include.</param>
    /// <param name="columns">The columns to include.</param>
    /// <returns>A view that shares coefficients with this matrix.</returns>
    public ReadOnlyMatrixXD this[Range rows, Range columns] => new(Data.Get(rows, columns));

    public ReadOnlyMatrix3D Transposed() => new(Data.Storage, Data.Layout.Transposed());

    public ReadOnlyVector3D AsVector() => new(Data.AsVectorLayout());

    public override string ToString() => Matrix.Format(this);
}
