using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>A writable quaternion view over shared X, Y, Z, W coefficient storage.</summary>
/// <remarks>Copying a quaternion copies its view, not its coefficients. The default value is a zero, read-only quaternion.</remarks>
public readonly partial struct QuaternionD : IMatrixD<QuaternionD>
{
    internal static readonly MatrixData s_zeroData = MatrixData.ReadOnlyZero(4, 1);
    private MatrixData Data => field.Storage is null ? s_zeroData : field;

    private QuaternionD(in MatrixData data) => Data = data.Require(4, 1);

    public int Rows => Data.Rows;
    public int Columns => Data.Columns;
    public int RowStride => Data.RowStride;
    public int ColumnStride => Data.ColumnStride;

    TensorSpanLease IReadOnlyMatrixD<QuaternionD>.GetReadOnlyTensorSpan(out ReadOnlyTensorSpan<double> span) =>
        Data.AcquireReadOnlyTensorSpan(out span);

    static QuaternionD IReadOnlyMatrixD<QuaternionD>.Create(in MatrixData data) => new(data);

    TensorSpanLease IMatrixD<QuaternionD>.GetTensorSpan(out TensorSpan<double> span) =>
        Data.AcquireWritableTensorSpan(out span);

    public double this[int row, int column]
    {
        get => Data[row, column];
        set => Data[row, column] = value;
    }

    public double this[Index row, Index column]
    {
        get => Data[row, column];
        set => Data[row, column] = value;
    }

    public MatrixXD Transposed() => new(Data.AsTransposedLayout());

    public VectorXD AsVector() => new(Data.AsVectorLayout());

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

    public static QuaternionD FromMatrix(in MatrixXD matrix) => new(matrix.Data.Require(4, 1));

    public static QuaternionD FromAxisAngle(ReadOnlyVector3D axis, double angle) =>
        QuaternionMath.FromAxisAngle(axis, angle);

    public static QuaternionD FromRotationMatrix(in ReadOnlyMatrix3D matrix) =>
        QuaternionMath.FromRotationMatrix(matrix);

    public double X
    {
        get => Data[0, 0];
        set => Data[0, 0] = value;
    }
    public double Y
    {
        get => Data[1, 0];
        set => Data[1, 0] = value;
    }
    public double Z
    {
        get => Data[2, 0];
        set => Data[2, 0] = value;
    }
    public double W
    {
        get => Data[3, 0];
        set => Data[3, 0] = value;
    }
    public int Count => 4;
    public double this[int index]
    {
        get => Data[index, 0];
        set => Data[index, 0] = value;
    }
    public VectorXD Coefficients => new(Data);

    public MatrixXD AsMatrix() => new(Data);

    public ReadOnlyQuaternionD AsReadOnly() => new(Data);

    public override string ToString() => $"(X={X}, Y={Y}, Z={Z}, W={W})";
}
