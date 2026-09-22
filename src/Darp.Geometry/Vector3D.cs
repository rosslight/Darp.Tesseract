using System.Buffers;
using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>Mutable geometry sharing its coefficient storage with derived views.</summary>
public readonly struct Vector3D : IMatrixD, IReadOnlyVector3D
{
    private readonly MatrixData _data;
    internal static readonly MatrixData ZeroData = MatrixData.ReadOnlyZero(3, 1);
    private MatrixData Data => _data.Storage is null ? ZeroData : _data;

    private Vector3D(MatrixData data) => _data = data;

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

    ReadOnlyMatrixXD IReadOnlyMatrixD.Transposed() => Data.AsTransposedLayout();

    public Vector3D()
        : this(new MatrixData(3, 1)) { }

    internal Vector3D(MatrixStorage storage, MatrixLayout layout)
        : this(new MatrixData(storage, layout)) { }

    private Vector3D(Memory<double> memory, MatrixLayout layout)
        : this(new MatrixData(memory, layout)) { }

    public static Vector3D FromMatrix(MatrixXD matrix) =>
        new(matrix.Storage, matrix.Layout.Require(3, 1));

    TensorSpanLease IMatrixD.AcquireTensorSpan(out TensorSpan<double> span) => Data.AcquireWritableTensorSpan(out span);

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
    public double this[int index]
    {
        get => Data[index, 0];
        set => Data.Set(index, 0, value);
    }

    public VectorXD AsVector() => new(Storage, Layout);

    ReadOnlyVectorXD IReadOnlyMatrixD.AsVector() => new ReadOnlyVectorXD(Storage, Layout);

    public VectorXD Slice(int start, int count) =>
        new(Storage, Layout.Block(start, 0, count, 1));

    ReadOnlyVectorXD IReadOnlyVectorXD.Slice(int start, int count) =>
        new ReadOnlyVectorXD(
            Storage,
            Layout.Block(start, 0, count, 1)
        );

    public MatrixXD Transposed() => new(Storage, Layout.Transposed());

    public static Vector3D operator +(Vector3D a, ReadOnlyVector3D b) => a.Add(b);

    public static Vector3D operator -(Vector3D a, ReadOnlyVector3D b) => a.Subtract(b);

    public static Vector3D operator -(Vector3D value) => GeometryExtensions.Scale((ReadOnlyVector3D)value, -1);

    public static Vector3D operator *(Vector3D value, double scalar) =>
        GeometryExtensions.Scale((ReadOnlyVector3D)value, scalar);

    public static Vector3D operator *(double scalar, Vector3D value) => value * scalar;

    public static Vector3D operator /(Vector3D value, double scalar) =>
        GeometryExtensions.Divide((ReadOnlyVector3D)value, scalar);

    public static implicit operator ReadOnlyVector3D(Vector3D value) => new(value.Storage, value.Layout);

    public static implicit operator VectorXD(Vector3D value) => new(value.Storage, value.Layout);

    public static implicit operator ReadOnlyVectorXD(Vector3D value) => new(value.Storage, value.Layout);

    public static implicit operator MatrixXD(Vector3D value) => value.Data.AsMatrix();

    public static implicit operator ReadOnlyMatrixXD(Vector3D value) => value.Data.AsReadOnlyMatrix();

    public override string ToString() => $"[{string.Join(", ", this.ToArray())}]";
}
