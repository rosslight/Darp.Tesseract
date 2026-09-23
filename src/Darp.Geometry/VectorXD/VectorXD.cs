using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>A writable vector view over shared coefficient storage.</summary>
/// <remarks>Copying a vector copies its view, not its coefficients. The default value is an empty, read-only vector.</remarks>
public readonly partial struct VectorXD : IMatrixD<VectorXD>
{
    internal static readonly MatrixData s_zeroData = MatrixData.ReadOnlyZero(0, 1);
    internal MatrixData Data => field.Storage is null ? s_zeroData : field;

    internal VectorXD(in MatrixData data) => Data = data.Require(null, 1);

    internal MatrixStorage Storage => Data.Storage;
    internal MatrixLayout Layout => Data.Layout;
    public int Rows => Data.Rows;
    public int Columns => Data.Columns;
    public int RowStride => Data.RowStride;
    public int ColumnStride => Data.ColumnStride;

    TensorSpanLease IReadOnlyMatrixD<VectorXD>.GetReadOnlyTensorSpan(out ReadOnlyTensorSpan<double> span) =>
        Data.AcquireReadOnlyTensorSpan(out span);

    static VectorXD IReadOnlyMatrixD<VectorXD>.Create(in MatrixData data) => new(data);

    TensorSpanLease IMatrixD<VectorXD>.GetTensorSpan(out TensorSpan<double> span) =>
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

    public VectorXD()
        : this(new MatrixData(0, 1)) { }

    internal VectorXD(MatrixStorage storage, MatrixLayout layout)
        : this(new MatrixData(storage, layout)) { }

    private VectorXD(Memory<double> memory, MatrixLayout layout)
        : this(new MatrixData(memory, layout)) { }

    public static VectorXD FromMatrix(in MatrixXD matrix) => new(matrix.Data.Require(null, 1));

    public ReadOnlyVectorXD AsReadOnly() => new(Data);

    public MatrixXD AsMatrix() => new(Data);

    /// <summary>Creates a zero-filled vector with the specified number of elements.</summary>
    public VectorXD(int count)
        : this(new MatrixData(count, 1)) { }

    public VectorXD(params ReadOnlySpan<double> values)
        : this(values.Length)
    {
        for (int i = 0; i < values.Length; i++)
            this[i] = values[i];
    }

    /// <summary>Maps caller-owned memory without copying it. The caller keeps the memory valid while this vector is in use.</summary>
    public static VectorXD CreateFromMemory(Memory<double> memory, int count, int stride = 1) =>
        new(memory, MatrixLayout.Create(count, 1, stride, Math.Max(1, checked(count * stride))));

    public int Count => Rows;
    public double this[int index]
    {
        get => Data[index, 0];
        set => Data[index, 0] = value;
    }

    public VectorXD Slice(int start, int count)
    {
        var layout = Layout.Block(start, 0, count, 1);
        return new(Storage, layout);
    }

    public MatrixXD Transposed() => new(Storage, Layout.Transposed());

    public override string ToString() => $"[{string.Join(", ", this.ToArray())}]";
}
