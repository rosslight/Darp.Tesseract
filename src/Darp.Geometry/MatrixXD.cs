using System.Buffers;
using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>Mutable matrix with shared storage views. Clone and arithmetic create independent results.</summary>
public sealed class MatrixXD : GeometryObject, IMatrixD
{
    internal MatrixXD(MatrixStorage storage, MatrixLayout layout, int offset = 0)
        : base(storage, layout, offset) { }

    private MatrixXD(Memory<double> memory, MatrixLayout layout, MemoryManager<double>? owner = null)
        : base(memory, layout, owner) { }

    public MatrixXD(int rows, int columns)
        : base(rows, columns) { }

    /// <summary>Maps caller-owned memory without copying or taking responsibility for its disposal.</summary>
    public static MatrixXD CreateFromMemory(
        Memory<double> memory,
        int rows,
        int columns,
        int? columnStride = null,
        int rowStride = 1
    )
    {
        var layout = MatrixLayout.Create(
            rows,
            columns,
            rowStride,
            columnStride ?? Math.Max(1, checked(rows * rowStride))
        );
        return new MatrixXD(memory, layout);
    }

    /// <summary>Transfers ownership to shared storage. The owner is released when the storage is collected.</summary>
    /// <remarks>Do not dispose the transferred owner separately. Failed construction also disposes the owner.</remarks>
    public static MatrixXD CreateFromMemoryWithOwner(
        Memory<double> memory,
        MemoryManager<double> owner,
        int rows,
        int columns,
        int? columnStride = null,
        int rowStride = 1
    )
    {
        ArgumentNullException.ThrowIfNull(owner);
        MatrixLayout layout;
        try
        {
            layout = MatrixLayout.Create(
                rows,
                columns,
                rowStride,
                columnStride ?? Math.Max(1, checked(rows * rowStride))
            );
        }
        catch
        {
            (owner as IDisposable).Dispose();
            throw;
        }

        return new MatrixXD(memory, layout, owner);
    }

    public new double this[int row, int column]
    {
        get => base[row, column];
        set => SetValue(row, column, value);
    }

    public IReadOnlyMatrixD AsReadOnly() => AsReadOnlyMatrix();

    public MatrixXD Block(int row, int column, int rows, int columns)
    {
        var layout = Layout.Block(row, column, rows, columns);
        return new MatrixXD(
            Storage,
            layout,
            layout.Extent == 0 ? Offset : checked(Offset + Layout.Offset(row, column))
        );
    }

    public MatrixXD Row(int row) => Block(row, 0, 1, Columns);

    public VectorXD Column(int column) =>
        new(Storage, Layout.Block(0, column, Rows, 1), Rows == 0 ? Offset : checked(Offset + column * ColumnStride));

    public MatrixXD Transposed() => new(Storage, Layout.Transposed(), Offset);

    public VectorXD AsVector() => new(Storage, Layout.Require(null, 1), Offset);

    public static MatrixXD FromArray(double[,] values)
    {
        var result = new MatrixXD(values.GetLength(0), values.GetLength(1));
        for (int c = 0; c < result.Columns; c++)
        for (int r = 0; r < result.Rows; r++)
            result[r, c] = values[r, c];
        return result;
    }

    public static MatrixXD Identity(int size)
    {
        var result = new MatrixXD(size, size);
        for (int i = 0; i < size; i++)
            result[i, i] = 1;
        return result;
    }

    public static MatrixXD operator +(MatrixXD a, IReadOnlyMatrixD b) => a.Add(b);

    public static MatrixXD operator -(MatrixXD a, IReadOnlyMatrixD b) => a.Subtract(b);

    public static MatrixXD operator *(MatrixXD a, IReadOnlyMatrixD b) => a.Multiply(b);

    public static VectorXD operator *(MatrixXD a, VectorXD b) => a.Multiply(b);

    public static VectorXD operator *(MatrixXD a, IReadOnlyVectorXD b) => a.Multiply(b);

    public static MatrixXD operator *(MatrixXD a, double scalar) => a.Scale(scalar);

    public static MatrixXD operator *(double scalar, MatrixXD a) => a * scalar;

    public static MatrixXD operator /(MatrixXD a, double scalar) => a.Divide(scalar);

    public static MatrixXD operator -(MatrixXD a) => a.Scale(-1);

    /// <summary>Keep this object alive and undisposed until the span's last use.</summary>
    TensorSpanLease IMatrixD.AcquireTensorSpan(out TensorSpan<double> span) => AcquireWritableTensorSpan(out span);

    public MatrixXD AsMatrix() => ViewMatrix();

    public override string ToString() => MatrixExtensions.Format(this);
}
