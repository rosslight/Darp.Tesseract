using System.Buffers;
using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>Mutable geometry sharing its coefficient storage with derived views.</summary>
public readonly struct Matrix3D : IMatrixD, IReadOnlyMatrix3D
{
    private readonly MatrixData _data;
    internal static readonly MatrixData ZeroData = MatrixData.ReadOnlyZero(3, 3);
    private MatrixData Data => _data.Storage is null ? ZeroData : _data;

    private Matrix3D(MatrixData data) => _data = data;

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

    public ReadOnlyMatrixXD Block(int row, int column, int rows, int columns) => Data.BlockReadOnly(row, column, rows, columns);

    ReadOnlyMatrixXD IReadOnlyMatrixD.Transposed() => Data.AsTransposedLayout();

    public ReadOnlyVectorXD AsVector() => Data.AsVectorLayout();

    public override string ToString() => MatrixExtensions.Format(this);

    internal Matrix3D(MatrixStorage storage, MatrixLayout layout)
        : this(new MatrixData(storage, layout)) { }

    private Matrix3D(Memory<double> memory, MatrixLayout layout)
        : this(new MatrixData(memory, layout)) { }

    public static Matrix3D FromMatrix(MatrixXD matrix) =>
        new(matrix.Storage, matrix.Layout.Require(3, 3));

    public ReadOnlyMatrix3D AsReadOnly() => new ReadOnlyMatrix3D(Storage, Layout);

    public MatrixXD AsMatrix() => Data.AsMatrix();

    TensorSpanLease IMatrixD.AcquireTensorSpan(out TensorSpan<double> span) => Data.AcquireWritableTensorSpan(out span);

    public Matrix3D()
        : this(new MatrixData(3, 3)) { }

    public static Matrix3D Identity
    {
        get
        {
            var result = new Matrix3D();
            for (int i = 0; i < 3; i++)
                result[i, i] = 1;
            return result;
        }
    }

    public static Matrix3D FromArray(double[,] values) => FromMatrix(MatrixXD.FromArray(values));

    public static Matrix3D CreateFromMemory(Memory<double> memory, int columnStride = 3, int rowStride = 1) =>
        new(memory, MatrixLayout.Create(3, 3, rowStride, columnStride));

    public double this[int row, int column]
    {
        get => Data[row, column];
        set => Data.Set(row, column, value);
    }

    public MatrixXD Row(int row) => new(Storage, Layout.Block(row, 0, 1, Columns));

    ReadOnlyMatrixXD IReadOnlyMatrix3D.Row(int row) =>
        new ReadOnlyMatrixXD(Storage, Layout.Block(row, 0, 1, Columns));

    public Vector3D Column(int column) =>
        new(Storage, Layout.Block(0, column, 3, 1));

    ReadOnlyVector3D IReadOnlyMatrix3D.Column(int column) =>
        new ReadOnlyVector3D(Storage, Layout.Block(0, column, 3, 1));

    public Matrix3D Transposed() => new(Storage, Layout.Transposed());

    ReadOnlyMatrix3D IReadOnlyMatrix3D.Transposed() => new ReadOnlyMatrix3D(Storage, Layout.Transposed());

    public static Matrix3D operator *(Matrix3D a, IReadOnlyMatrix3D b) => a.Multiply(b);

    public static Vector3D operator *(Matrix3D a, IReadOnlyVector3D b) => a.Multiply(b);

    public static Matrix3D operator +(Matrix3D a, IReadOnlyMatrix3D b) => a.Add(b);

    public static Matrix3D operator -(Matrix3D a, IReadOnlyMatrix3D b) => a.Subtract(b);

    public static Matrix3D operator -(Matrix3D value) => value.Scale(-1);

    public static Matrix3D operator *(Matrix3D value, double scalar) => value.Scale(scalar);

    public static Matrix3D operator *(double scalar, Matrix3D value) => value * scalar;

    public static Matrix3D operator /(Matrix3D value, double scalar) => value.Divide(scalar);
}
