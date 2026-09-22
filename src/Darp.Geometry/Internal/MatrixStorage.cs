using System.Buffers;
using System.Numerics.Tensors;
using System.Threading;

namespace Darp.Geometry;

// Counts explicitly retained views, not C# variable aliases. The final release
// returns transferred storage to its owner; mapped memory remains caller-owned.
internal sealed class MatrixStorage(ReadOnlyMemory<double> memory, Memory<double> writable, IDisposable? owner)
{
    private int _references = 1;
    internal ReadOnlyMemory<double> Memory = memory;
    internal Memory<double> Writable = writable;
    private IDisposable? _owner = owner;

    internal void Retain()
    {
        int count = Volatile.Read(ref _references);
        while (count != 0)
        {
            int observed = Interlocked.CompareExchange(ref _references, checked(count + 1), count);
            if (observed == count) return;
            count = observed;
        }
        throw new ObjectDisposedException(nameof(MatrixStorage));
    }

    internal void Release()
    {
        if (Interlocked.Decrement(ref _references) != 0) return;
        var owner = _owner;
        _owner = null;
        Memory = default;
        Writable = default;
        owner?.Dispose();
    }
}

// A finalizable lease gives forgotten disposable objects a single cleanup path.
// Each public geometry object or borrow owns a distinct lease.
internal sealed class MatrixView : IDisposable
{
    private MatrixStorage? _storage;
    internal readonly MatrixLayout Layout;
    private readonly int _offset;

    internal MatrixView(MatrixStorage storage, MatrixLayout layout, int offset = 0)
    {
        _storage = storage;
        Layout = layout;
        _offset = offset;
    }

    internal static MatrixView Create(ReadOnlyMemory<double> memory, Memory<double> writable, MatrixLayout layout, IDisposable? owner = null)
    {
        try
        {
            var extent = layout.Extent;
            var selected = memory[..extent];
            _ = new ReadOnlyTensorSpan<double>(selected.IsEmpty ? Array.Empty<double>().AsSpan() : selected.Span,
                [layout.Rows, layout.Columns],
                [layout.Rows <= 1 ? 0 : layout.RowStride, layout.Columns <= 1 ? 0 : layout.ColumnStride]);
        }
        catch
        {
            owner?.Dispose();
            throw;
        }
        return new MatrixView(new MatrixStorage(memory, writable, owner), layout);
    }

    private MatrixStorage Storage => Volatile.Read(ref _storage) ?? throw new ObjectDisposedException("Geometry view");
    internal void ThrowIfDisposed() => _ = Storage;
    internal MatrixStorage Acquire()
    {
        var storage = Storage;
        storage.Retain();
        return storage;
    }
    internal MatrixView Retain() => new(Acquire(), Layout, _offset);
    internal MatrixView Transposed() => new(Acquire(), Layout.Transposed(), _offset);
    internal MatrixView Block(int row, int column, int rows, int columns)
    {
        ThrowIfDisposed();
        var layout = Layout.Block(row, column, rows, columns);
        var offset = layout.Extent == 0 ? _offset : checked(_offset + Layout.Offset(row, column));
        return new MatrixView(Acquire(), layout, offset);
    }

    internal double Get(int row, int column)
    {
        var storage = Acquire();
        try { return storage.Memory.Span[checked(_offset + Layout.Offset(row, column))]; }
        finally { storage.Release(); }
    }
    internal void Set(int row, int column, double value)
    {
        var storage = Acquire();
        try { storage.Writable.Span[checked(_offset + Layout.Offset(row, column))] = value; }
        finally { storage.Release(); }
    }
    internal ReadOnlyTensorSpan<double> ReadOnlySpan()
    {
        var memory = Storage.Memory.Slice(_offset, Layout.Extent);
        return new(memory.IsEmpty ? Array.Empty<double>().AsSpan() : memory.Span,
            [Layout.Rows, Layout.Columns],
            [Layout.Rows <= 1 ? 0 : Layout.RowStride, Layout.Columns <= 1 ? 0 : Layout.ColumnStride]);
    }
    internal TensorSpan<double> Span()
    {
        var memory = Storage.Writable.Slice(_offset, Layout.Extent);
        return new(memory.IsEmpty ? Array.Empty<double>().AsSpan() : memory.Span,
            [Layout.Rows, Layout.Columns],
            [Layout.Rows <= 1 ? 0 : Layout.RowStride, Layout.Columns <= 1 ? 0 : Layout.ColumnStride]);
    }
    internal unsafe MemoryHandle Pin()
    {
        var storage = Acquire();
        try
        {
            var pin = storage.Memory.Slice(_offset, Layout.Extent).Pin();
            var lease = new StoragePin(storage, pin);
            return new MemoryHandle(pin.Pointer, default, lease);
        }
        catch { storage.Release(); throw; }
    }
    public void Dispose()
    {
        Interlocked.Exchange(ref _storage, null)?.Release();
        GC.SuppressFinalize(this);
    }
    ~MatrixView()
    {
        // A transferred owner's Dispose may throw; finalizers cannot propagate it.
        try { Interlocked.Exchange(ref _storage, null)?.Release(); }
        catch { }
    }

    private sealed class StoragePin(MatrixStorage storage, MemoryHandle pin) : IPinnable
    {
        private MatrixStorage? _storage = storage;
        private MemoryHandle _pin = pin;
        public MemoryHandle Pin(int elementIndex) => throw new NotSupportedException();
        public void Unpin()
        {
            Release();
            GC.SuppressFinalize(this);
        }
        ~StoragePin()
        {
            try { Release(); }
            catch { }
        }
        private void Release()
        {
            var retained = Interlocked.Exchange(ref _storage, null);
            if (retained is null) return;
            try { _pin.Dispose(); }
            finally { retained.Release(); }
        }
    }
}
