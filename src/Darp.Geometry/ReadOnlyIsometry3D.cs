using System.Buffers;
using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>A read-only view of shared coefficients. Other aliases may change them.</summary>
public readonly struct ReadOnlyIsometry3D : IReadOnlyIsometry3D
{
    private readonly MatrixData _data;
    private static readonly MatrixData ZeroData = Isometry3D.ZeroData;
    private MatrixData Data => _data.Storage is null ? ZeroData : _data;

    private ReadOnlyIsometry3D(MatrixData data) => _data = data;

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

    public override string ToString() => MatrixExtensions.Format(this);

    internal ReadOnlyIsometry3D(MatrixStorage storage, MatrixLayout layout)
        : this(new MatrixData(storage, layout)) { }

    public ReadOnlyVector3D Translation =>
        new ReadOnlyVector3D(Storage, Layout.Block(0, 3, 3, 1));
    public ReadOnlyMatrix3D RotationMatrix => new(Storage, Layout.Block(0, 0, 3, 3));

    public QuaternionD Rotation
    {
        get
        {
            var matrix = RotationMatrix;
            return matrix.ToQuaternion();
        }
    }
}
