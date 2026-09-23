using System.Buffers;
using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>A mutable X,Y,Z,W quaternion sharing its coefficient storage.</summary>
public readonly partial struct QuaternionD : IMatrixD, IReadOnlyQuaternionD
{
    private readonly MatrixData _data;
    internal static readonly MatrixData ZeroData = MatrixData.ReadOnlyZero(4, 1);
    private MatrixData Data => _data.Storage is null ? ZeroData : _data;

    private QuaternionD(MatrixData data) => _data = data;

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

    public QuaternionD()
        : this(new MatrixData(4, 1)) { }

    internal QuaternionD(MatrixStorage storage, MatrixLayout layout)
        : this(new MatrixData(storage, layout)) { }

    private QuaternionD(Memory<double> memory, MatrixLayout layout)
        : this(new MatrixData(memory, layout)) { }

    public QuaternionD(double x, double y, double z, double w)
        : this(new MatrixData(4, 1))
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    public static QuaternionD Identity => new(0, 0, 0, 1);

    public static QuaternionD CreateFromMemory(Memory<double> memory, int stride = 1) =>
        new(memory, MatrixLayout.Create(4, 1, stride, checked(4 * stride)));

    public static QuaternionD FromMatrix(MatrixXD matrix) =>
        new(matrix.Storage, matrix.Layout.Require(4, 1));

    public static QuaternionD FromAxisAngle(ReadOnlyVector3D axis, double angle) =>
        QuaternionMath.FromAxisAngle(axis, angle);

    public static QuaternionD FromRotationMatrix(in ReadOnlyMatrix3D matrix) =>
        QuaternionMath.FromRotationMatrix(matrix);

    public double X
    {
        get => Data[0, 0];
        set => Data.Set(0, 0, value);
    }
    public double Y
    {
        get => Data[1, 0];
        set => Data.Set(1, 0, value);
    }
    public double Z
    {
        get => Data[2, 0];
        set => Data.Set(2, 0, value);
    }
    public double W
    {
        get => Data[3, 0];
        set => Data.Set(3, 0, value);
    }
    public VectorXD Coefficients => new(Storage, Layout);
    ReadOnlyVectorXD IReadOnlyQuaternionD.Coefficients => new ReadOnlyVectorXD(Storage, Layout);

    public ReadOnlyQuaternionD AsReadOnly() => new ReadOnlyQuaternionD(Storage, Layout);

    public MatrixXD AsMatrix() => Data.AsMatrix();

    TensorSpanLease IMatrixD.AcquireTensorSpan(out TensorSpan<double> span) => Data.AcquireWritableTensorSpan(out span);

    public override string ToString() => $"(X={X}, Y={Y}, Z={Z}, W={W})";
}
