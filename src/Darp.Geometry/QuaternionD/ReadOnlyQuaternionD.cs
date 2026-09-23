using System.Buffers;
using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>A read-only view of shared coefficients. Other aliases may change them.</summary>
public readonly struct ReadOnlyQuaternionD : IReadOnlyQuaternionD
{
    private readonly MatrixData _data;
    private static readonly MatrixData ZeroData = QuaternionD.ZeroData;
    private MatrixData Data => _data.Storage is null ? ZeroData : _data;

    private ReadOnlyQuaternionD(MatrixData data) => _data = data;

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

    internal ReadOnlyQuaternionD(MatrixStorage storage, MatrixLayout layout)
        : this(new MatrixData(storage, layout)) { }

    public double X => Data[0, 0];
    public double Y => Data[1, 0];
    public double Z => Data[2, 0];
    public double W => Data[3, 0];
    public ReadOnlyVectorXD Coefficients => new(Storage, Layout);

    public override string ToString() => $"(X={X}, Y={Y}, Z={Z}, W={W})";
}
