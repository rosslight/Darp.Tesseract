using System.Buffers;
using System.Numerics.Tensors;

namespace Darp.Geometry;

// Shared addressing and access mechanics; public geometry values contain this directly.
internal readonly struct MatrixData
{
    internal readonly MatrixStorage Storage; // 8 bytes
    internal readonly MatrixLayout Layout; // 20 bytes

    internal static MatrixData ReadOnlyZero(int rows, int columns)
    {
        var layout = MatrixLayout.Create(rows, columns, 1, Math.Max(1, rows));
        return new(new MatrixStorage(new double[layout.Extent], null, isReadOnly: true), layout);
    }

    internal MatrixData(int rows, int columns)
        : this(
            new double[MatrixLayout.Create(rows, columns, 1, Math.Max(1, rows)).Extent],
            MatrixLayout.Create(rows, columns, 1, Math.Max(1, rows))
        ) { }

    internal MatrixData(Memory<double> memory, MatrixLayout layout, MemoryManager<double>? owner = null)
    {
        try
        {
            var selected = memory.Slice(layout.Offset, layout.Extent);
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

    internal MatrixData(MatrixStorage storage, MatrixLayout layout)
    {
        Storage = storage;
        Layout = layout;
    }

    public int Rows => Layout.Rows;
    public int Columns => Layout.Columns;
    public int RowStride => Layout.RowStride;
    public int ColumnStride => Layout.ColumnStride;

    internal MatrixData AsTransposedLayout() => new(Storage, Layout.Transposed());

    internal MatrixData AsVectorLayout() => new(Storage, Layout.Require(null, 1));

    internal MatrixData Slice(int row, int column, int rows, int columns)
    {
        MatrixLayout layout = Layout.Block(row, column, rows, columns);
        return new MatrixData(Storage, layout);
    }

    public double Get(int row, int column)
    {
        try
        {
            return Storage.Memory.Span[Layout.GetOffset(row, column)];
        }
        finally
        {
            GC.KeepAlive(Storage);
        }
    }

    public double Get(Index row, Index column) => Get(row.GetOffset(Rows), column.GetOffset(Columns));

    public MatrixData Get(Range rows, Range columns)
    {
        (int row, int rowCount) = rows.GetOffsetAndLength(Rows);
        (int column, int columnCount) = columns.GetOffsetAndLength(Columns);

        return Slice(row, column, rowCount, columnCount);
    }

    public void Set(int row, int column, double value)
    {
        Storage.RequireWritable();
        try
        {
            Storage.Memory.Span[Layout.GetOffset(row, column)] = value;
        }
        finally
        {
            GC.KeepAlive(Storage);
        }
    }

    public void Set(Index row, Index column, double value) => Set(row.GetOffset(Rows), column.GetOffset(Columns), value);

    internal TensorSpanLease AcquireReadOnlyTensorSpan(out ReadOnlyTensorSpan<double> span)
    {
        var lease = new TensorSpanLease(Storage);
        var memory = Storage.Memory.Slice(Layout.Offset, Layout.Extent);
        span = new ReadOnlyTensorSpan<double>(
            memory.IsEmpty ? Array.Empty<double>().AsSpan() : memory.Span,
            [Layout.Rows, Layout.Columns],
            [Layout.Rows <= 1 ? 0 : Layout.RowStride, Layout.Columns <= 1 ? 0 : Layout.ColumnStride]
        );
        return lease;
    }

    internal TensorSpanLease AcquireWritableTensorSpan(out TensorSpan<double> span)
    {
        Storage.RequireWritable();
        var lease = new TensorSpanLease(Storage);
        var memory = Storage.Memory.Slice(Layout.Offset, Layout.Extent);
        span = new TensorSpan<double>(
            memory.IsEmpty ? Array.Empty<double>().AsSpan() : memory.Span,
            [Layout.Rows, Layout.Columns],
            [Layout.Rows <= 1 ? 0 : Layout.RowStride, Layout.Columns <= 1 ? 0 : Layout.ColumnStride]
        );
        return lease;
    }

    internal unsafe MemoryHandle Pin()
    {
        var pin = Storage.Memory.Slice(Layout.Offset, Layout.Extent).Pin();
        return new MemoryHandle(pin.Pointer, default, new StoragePin(Storage, pin));
    }

    private sealed class StoragePin(MatrixStorage storage, MemoryHandle pin) : IPinnable
    {
        private MatrixStorage? _storage = storage;
        private MemoryHandle _pin = pin;

        public MemoryHandle Pin(int elementIndex) => throw new NotSupportedException();

        public void Unpin()
        {
            var storage = Interlocked.Exchange(ref _storage, null);
            if (storage is null)
                return;
            try
            {
                _pin.Dispose();
            }
            finally
            {
                GC.KeepAlive(storage);
            }
        }
    }

    public MatrixData Require(int? rows, int columns)
    {
        _ = Layout.Require(rows, columns);
        return this;
    }
}
