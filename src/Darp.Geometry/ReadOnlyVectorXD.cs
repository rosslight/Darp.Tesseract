using System.Buffers;
using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>A read-only view of shared coefficients. Other aliases may change them.</summary>
public readonly struct ReadOnlyVectorXD
{
    private static readonly MatrixData s_zeroData = VectorXD.ZeroData;

    private MatrixData Data => field.Storage is null ? s_zeroData : field;

    internal ReadOnlyVectorXD(MatrixData data) => Data = data;

    internal MatrixStorage Storage => Data.Storage;
    internal MatrixLayout Layout => Data.Layout;
    public int Rows => Data.Rows;
    public int Columns => Data.Columns;
    public int RowStride => Data.RowStride;
    public int ColumnStride => Data.ColumnStride;

    public ReadOnlyMatrixXD AsReadOnlyMatrix() => Data.AsReadOnlyMatrix();

    TensorSpanLease IReadOnlyMatrixD.AcquireReadOnlyTensorSpan(out ReadOnlyTensorSpan<double> span) =>
        Data.AcquireReadOnlyTensorSpan(out span);

    MemoryHandle IReadOnlyMatrixD.Pin() => Data.Pin();

    public double this[int row, int column] => Data[row, column];

    public ReadOnlyMatrixXD Block(int row, int column, int rows, int columns) => Data.BlockReadOnly(row, column, rows, columns);

    public ReadOnlyMatrixXD Transposed() => Data.AsTransposedLayout();

    public ReadOnlyVectorXD AsVector() => Data.AsVectorLayout();

    internal ReadOnlyVectorXD(MatrixStorage storage, MatrixLayout layout)
        : this(new MatrixData(storage, layout)) { }

    public int Count => Rows;
    public double this[int index] => Data[index, 0];

    public ReadOnlyVectorXD Slice(int start, int count) =>
        new ReadOnlyVectorXD(
            Storage,
            Layout.Block(start, 0, count, 1)
        );

    public override string ToString() => $"[{string.Join(", ", this.ToArray())}]";
}
