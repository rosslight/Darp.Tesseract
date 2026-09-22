using System.Buffers;
using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>A geometry object sharing its coefficient storage with derived views.</summary>
public abstract class GeometryObject : IReadOnlyMatrixD
{
    internal readonly MatrixStorage Storage;
    internal readonly MatrixLayout Layout;
    internal readonly int Offset;

    private protected GeometryObject(int rows, int columns)
        : this(
            new double[MatrixLayout.Create(rows, columns, 1, Math.Max(1, rows)).Extent],
            MatrixLayout.Create(rows, columns, 1, Math.Max(1, rows))
        ) { }

    private protected GeometryObject(Memory<double> memory, MatrixLayout layout, MemoryManager<double>? owner = null)
    {
        try
        {
            var selected = memory[..layout.Extent];
            _ = new ReadOnlyTensorSpan<double>(
                selected.IsEmpty ? Array.Empty<double>().AsSpan() : selected.Span,
                [layout.Rows, layout.Columns],
                [layout.Rows <= 1 ? 0 : layout.RowStride, layout.Columns <= 1 ? 0 : layout.ColumnStride]
            );
            Storage = new MatrixStorage(memory, owner);
            Layout = layout;
        }
        catch
        {
            ((IDisposable?)owner)?.Dispose();
            throw;
        }
    }

    private protected GeometryObject(MatrixStorage storage, MatrixLayout layout, int offset = 0)
    {
        Storage = storage;
        Layout = layout;
        Offset = offset;
    }

    public int Rows => Layout.Rows;
    public int Columns => Layout.Columns;
    public int RowStride => Layout.RowStride;
    public int ColumnStride => Layout.ColumnStride;
    public double this[int row, int column] => Get(row, column);

    public IReadOnlyMatrixD AsReadOnlyMatrix() => new ReadOnlyMatrix(Storage, Layout, Offset);

    protected MatrixXD ViewMatrix() => new(Storage, Layout, Offset);

    IReadOnlyMatrixD IReadOnlyMatrixD.Transposed() => new ReadOnlyMatrix(Storage, Layout.Transposed(), Offset);

    IReadOnlyVectorXD IReadOnlyMatrixD.AsVector() => new ReadOnlyVectorXD(Storage, Layout.Require(null, 1), Offset);

    IReadOnlyMatrixD IReadOnlyMatrixD.Block(int row, int column, int rows, int columns)
    {
        var layout = Layout.Block(row, column, rows, columns);
        return new ReadOnlyMatrix(
            Storage,
            layout,
            layout.Extent == 0 ? Offset : checked(Offset + Layout.Offset(row, column))
        );
    }

    internal double Get(int row, int column)
    {
        using var lease = MatrixMarshal.GetReadOnlyTensorSpan(this, out var values);
        return values[row, column];
    }

    protected void SetValue(int row, int column, double value)
    {
        using var lease = AcquireWritableTensorSpan(out var values);
        values[row, column] = value;
    }

    TensorSpanLease IReadOnlyMatrixD.AcquireReadOnlyTensorSpan(out ReadOnlyTensorSpan<double> span)
    {
        var lease = new TensorSpanLease(Storage);
        var memory = Storage.Memory.Slice(Offset, Layout.Extent);
        span = new ReadOnlyTensorSpan<double>(
            memory.IsEmpty ? [] : memory.Span,
            [Layout.Rows, Layout.Columns],
            [Layout.Rows <= 1 ? 0 : Layout.RowStride, Layout.Columns <= 1 ? 0 : Layout.ColumnStride]);
        return lease;
    }

    private protected TensorSpanLease AcquireWritableTensorSpan(out TensorSpan<double> span)
    {
        var lease = new TensorSpanLease(Storage);
        var memory = Storage.Memory.Slice(Offset, Layout.Extent);
        span = new TensorSpan<double>(
            memory.IsEmpty ? [] : memory.Span,
            [Layout.Rows, Layout.Columns],
            [Layout.Rows <= 1 ? 0 : Layout.RowStride, Layout.Columns <= 1 ? 0 : Layout.ColumnStride]);
        return lease;
    }

    unsafe MemoryHandle IReadOnlyMatrixD.Pin()
    {
        var pin = Storage.Memory.Slice(Offset, Layout.Extent).Pin();
        return new MemoryHandle(pin.Pointer, default, new StoragePin(Storage, pin));
    }

    public override string ToString() => MatrixExtensions.Format(this);

    private sealed class StoragePin(MatrixStorage storage, MemoryHandle pin) : IPinnable
    {
        private MatrixStorage? _storage = storage;
        private MemoryHandle _pin = pin;

        public MemoryHandle Pin(int elementIndex) => throw new NotSupportedException();

        public void Unpin()
        {
            var storage = Interlocked.Exchange(ref _storage, null);
            if (storage is null) return;
            try { _pin.Dispose(); }
            finally { GC.KeepAlive(storage); }
        }
    }
}
