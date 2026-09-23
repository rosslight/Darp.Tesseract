using System.Buffers;
using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>A mutable X,Y,Z,W quaternion sharing its coefficient storage.</summary>
public readonly partial struct QuaternionD : IMatrixD<QuaternionD>
{
    internal static readonly MatrixData s_zeroData = MatrixData.ReadOnlyZero(4, 1);
    private MatrixData Data => field.Storage is null ? s_zeroData : field;

    private QuaternionD(MatrixData data) => Data = data;

    public int Rows => Data.Rows;
    public int Columns => Data.Columns;
    public int RowStride => Data.RowStride;
    public int ColumnStride => Data.ColumnStride;

    TensorSpanLease IReadOnlyMatrixD<QuaternionD>.GetReadOnlyTensorSpan(out ReadOnlyTensorSpan<double> span) =>
        Data.AcquireReadOnlyTensorSpan(out span);

    static QuaternionD IReadOnlyMatrixD<QuaternionD>.Create(in MatrixData data) => throw new NotImplementedException();

    TensorSpanLease IMatrixD<QuaternionD>.GetTensorSpan(out TensorSpan<double> span) =>
        Data.AcquireWritableTensorSpan(out span);

    public double this[int row, int column] => Data[row, column];

    public ReadOnlyMatrixXD Transposed() => new(Data.AsTransposedLayout());

    public ReadOnlyVectorXD AsVector() => new(Data.AsVectorLayout());

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

    public static QuaternionD FromMatrix(MatrixXD matrix) => new(matrix.Data.Require(4, 1));

    public static QuaternionD FromAxisAngle(ReadOnlyVector3D axis, double angle) =>
        QuaternionMath.FromAxisAngle(axis, angle);

    public static QuaternionD FromRotationMatrix(in ReadOnlyMatrix3D matrix) =>
        QuaternionMath.FromRotationMatrix(matrix);

    double IMatrixD<QuaternionD>.this[int row, int column]
    {
        get => Data[row, column];
        set => Data[row, column] = value;
    }
    double IReadOnlyMatrixD<QuaternionD>.this[Index row, Index column] => Data[row, column];
    double IMatrixD<QuaternionD>.this[Index row, Index column]
    {
        get => Data[row, column];
        set => Data[row, column] = value;
    }

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
    public VectorXD Coefficients => new(Data);

    public ReadOnlyQuaternionD AsReadOnly() => new(Data);

    public override string ToString() => $"(X={X}, Y={Y}, Z={Z}, W={W})";
}
