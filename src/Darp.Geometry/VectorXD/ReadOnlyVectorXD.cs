using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>A read-only vector view over shared coefficient storage.</summary>
/// <remarks>This view shares coefficients with its source. A writable alias can still change them. The default value is empty.</remarks>
public readonly partial struct ReadOnlyVectorXD : IReadOnlyMatrixD<ReadOnlyVectorXD>
{
    private static readonly MatrixData s_zeroData = VectorXD.s_zeroData;

    private MatrixData Data => field.Storage is null ? s_zeroData : field;

    internal ReadOnlyVectorXD(in MatrixData data) => Data = data.Require(null, 1);

    internal MatrixStorage Storage => Data.Storage;
    internal MatrixLayout Layout => Data.Layout;
    public int Rows => Data.Rows;
    public int Columns => Data.Columns;
    public int RowStride => Data.RowStride;
    public int ColumnStride => Data.ColumnStride;

    TensorSpanLease IReadOnlyMatrixD<ReadOnlyVectorXD>.GetReadOnlyTensorSpan(out ReadOnlyTensorSpan<double> span) =>
        Data.AcquireReadOnlyTensorSpan(out span);

    static ReadOnlyVectorXD IReadOnlyMatrixD<ReadOnlyVectorXD>.Create(in MatrixData data) => new(data);

    public double this[int row, int column] => Data[row, column];
    public double this[Index row, Index column] => Data[row, column];

    public ReadOnlyMatrixXD Transposed() => new(Data.AsTransposedLayout());

    public ReadOnlyMatrixXD AsMatrix() => new(Data);

    internal ReadOnlyVectorXD(MatrixStorage storage, MatrixLayout layout)
        : this(new MatrixData(storage, layout)) { }

    public int Count => Rows;
    public double this[int index] => Data[index, 0];

    public ReadOnlyVectorXD Slice(int start, int count) => new(Storage, Layout.Block(start, 0, count, 1));

    public override string ToString() => $"[{string.Join(", ", this.ToArray())}]";
}
