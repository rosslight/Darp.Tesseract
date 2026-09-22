using System.Buffers;
using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>A read-only view of shared coefficients. Other aliases may change them.</summary>
public readonly struct ReadOnlyVector3D : IReadOnlyVector3D
{
    private static readonly MatrixData s_zeroData = Vector3D.ZeroData;
    private MatrixData Data => field.Storage is null ? s_zeroData : field;

    internal ReadOnlyVector3D(MatrixData data) => Data = data.Require(3, 1);

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

    internal ReadOnlyVector3D(MatrixStorage storage, MatrixLayout layout)
        : this(new MatrixData(storage, layout)) { }

    public int Count => Rows;
    public double this[int index] => Data[index, 0];

    public ReadOnlyVectorXD Slice(int start, int count) =>
        new ReadOnlyVectorXD(
            Storage,
            Layout.Block(start, 0, count, 1)
        );

    public double X => Data[0, 0];
    public double Y => Data[1, 0];
    public double Z => Data[2, 0];

    public ReadOnlyVectorXD AsVector() => new ReadOnlyVectorXD(Storage, Layout);

    public override string ToString() => $"[{string.Join(", ", this.ToArray())}]";
}
