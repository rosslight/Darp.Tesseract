using System.Buffers;
using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>Mutable geometry sharing its coefficient storage with derived views.</summary>
public readonly struct VectorXD : IMatrixD, IReadOnlyVectorXD
{
    private readonly MatrixData _data;
    internal static readonly MatrixData ZeroData = MatrixData.ReadOnlyZero(0, 1);
    private MatrixData Data => _data.Storage is null ? ZeroData : _data;

    internal VectorXD(MatrixData data) => _data = data;

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

    public ReadOnlyVectorXD AsVector() => Data.AsVectorLayout();

    public VectorXD()
        : this(new MatrixData(0, 1)) { }

    internal VectorXD(MatrixStorage storage, MatrixLayout layout)
        : this(new MatrixData(storage, layout)) { }

    private VectorXD(Memory<double> memory, MatrixLayout layout)
        : this(new MatrixData(memory, layout)) { }

    public static VectorXD FromMatrix(MatrixXD matrix) =>
        new(matrix.Storage, matrix.Layout.Require(null, 1));

    public ReadOnlyVectorXD AsReadOnly() => new ReadOnlyVectorXD(Storage, Layout);

    public MatrixXD AsMatrix() => Data.AsMatrix();

    TensorSpanLease IMatrixD.AcquireTensorSpan(out TensorSpan<double> span) => Data.AcquireWritableTensorSpan(out span);

    public VectorXD(int count)
        : this(new MatrixData(count, 1)) { }

    public VectorXD(params ReadOnlySpan<double> values)
        : this(values.Length)
    {
        for (int i = 0; i < values.Length; i++)
            this[i] = values[i];
    }

    public static VectorXD CreateFromMemory(Memory<double> memory, int count, int stride = 1) =>
        new(memory, MatrixLayout.Create(count, 1, stride, Math.Max(1, checked(count * stride))));

    public int Count => Rows;
    public double this[int index]
    {
        get => Data[index, 0];
        set => Data.Set(index, 0, value);
    }

    public VectorXD Slice(int start, int count)
    {
        var layout = Layout.Block(start, 0, count, 1);
        return new(Storage, layout);
    }

    ReadOnlyVectorXD IReadOnlyVectorXD.Slice(int start, int count) =>
        new ReadOnlyVectorXD(
            Storage,
            Layout.Block(start, 0, count, 1)
        );

    public MatrixXD Transposed() => new(Storage, Layout.Transposed());

    public static VectorXD operator +(VectorXD a, VectorXD b) => GeometryExtensions.Add(a, b);

    public static VectorXD operator +(VectorXD a, ReadOnlyVectorXD b) => GeometryExtensions.Add(a, b);

    public static VectorXD operator -(VectorXD a, VectorXD b) => GeometryExtensions.Subtract(a, b);

    public static VectorXD operator -(VectorXD a, ReadOnlyVectorXD b) => GeometryExtensions.Subtract(a, b);

    public static VectorXD operator -(VectorXD value) => GeometryExtensions.Scale(value, -1);

    public static VectorXD operator *(VectorXD value, double scalar) => GeometryExtensions.Scale(value, scalar);

    public static VectorXD operator *(double scalar, VectorXD value) => value * scalar;

    public static VectorXD operator /(VectorXD value, double scalar) => GeometryExtensions.Divide(value, scalar);

    public static implicit operator ReadOnlyVectorXD(VectorXD value) => value.AsReadOnly();

    public static implicit operator MatrixXD(VectorXD value) => value.Data.AsMatrix();

    public static implicit operator ReadOnlyMatrixXD(VectorXD value) => value.Data.AsReadOnlyMatrix();

    public override string ToString() => $"[{string.Join(", ", this.ToArray())}]";
}
