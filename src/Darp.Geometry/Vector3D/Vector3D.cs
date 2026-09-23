using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>A writable three-dimensional vector view over shared coefficient storage.</summary>
/// <remarks>Copying a vector copies its view, not its coefficients. The default value is a zero, read-only vector.</remarks>
public readonly partial struct Vector3D : IMatrixD<Vector3D>
{
    internal static readonly MatrixData s_zeroData = MatrixData.ReadOnlyZero(3, 1);
    private MatrixData Data => field.Storage is null ? s_zeroData : field;

    internal Vector3D(in MatrixData data) => Data = data.Require(3, 1);

    public Vector3D()
        : this(new MatrixData(3, 1)) { }

    internal Vector3D(MatrixStorage storage, in MatrixLayout layout)
        : this(new MatrixData(storage, layout)) { }

    private Vector3D(Memory<double> memory, in MatrixLayout layout)
        : this(new MatrixData(memory, layout)) { }

    internal MatrixStorage Storage => Data.Storage;
    internal MatrixLayout Layout => Data.Layout;
    public int Rows => Data.Rows;
    public int Columns => Data.Columns;
    public int RowStride => Data.RowStride;
    public int ColumnStride => Data.ColumnStride;

    TensorSpanLease IReadOnlyMatrixD<Vector3D>.GetReadOnlyTensorSpan(out ReadOnlyTensorSpan<double> span) =>
        Data.AcquireReadOnlyTensorSpan(out span);

    static Vector3D IReadOnlyMatrixD<Vector3D>.Create(in MatrixData data) => new(data);

    TensorSpanLease IMatrixD<Vector3D>.GetTensorSpan(out TensorSpan<double> span) =>
        Data.AcquireWritableTensorSpan(out span);

    public double this[int row, int column]
    {
        get => Data.Get(row, column);
        set => Data.Set(row, column, value);
    }

    public double this[Index row, Index column]
    {
        get => Data.Get(row, column);
        set => Data.Set(row, column, value);
    }

    public static Vector3D FromMatrix(in MatrixXD matrix) => new(matrix.Data.Require(3, 1));

    public Vector3D(double x, double y, double z)
        : this(new MatrixData(3, 1))
    {
        X = x;
        Y = y;
        Z = z;
    }

    public static Vector3D Zero => new(0, 0, 0);
    public static Vector3D UnitX => new(1, 0, 0);
    public static Vector3D UnitY => new(0, 1, 0);
    public static Vector3D UnitZ => new(0, 0, 1);

    public static Vector3D CreateFromMemory(Memory<double> memory, int stride = 1) =>
        new(memory, MatrixLayout.Create(3, 1, stride, checked(3 * stride)));

    public int Count => Rows;
    public double X
    {
        get => this[0, 0];
        set => this[0, 0] = value;
    }
    public double Y
    {
        get => this[1, 0];
        set => this[1, 0] = value;
    }
    public double Z
    {
        get => this[2, 0];
        set => this[2, 0] = value;
    }
    public double this[int index]
    {
        get => this[index, 0];
        set => this[index, 0] = value;
    }

    public VectorXD AsVector() => new(Storage, Layout);

    public ReadOnlyVector3D AsReadOnly() => new(Data);

    public MatrixXD AsMatrix() => new(Data);

    public VectorXD Slice(int start, int count) => new(Storage, Layout.Block(start, 0, count, 1));

    public MatrixXD Transposed() => new(Storage, Layout.Transposed());

    public override string ToString() => $"[{string.Join(", ", this.ToArray())}]";
}
