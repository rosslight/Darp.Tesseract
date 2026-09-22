using System.Buffers;
using System.Numerics.Tensors;
namespace Darp.Geometry;
/// <summary>A disposable geometry view retaining shared coefficient storage.</summary>
public abstract class GeometryObject : IReadOnlyMatrixD
{
    private MatrixStorage? _storage;
    internal readonly MatrixLayout Layout;
    internal readonly int Offset;

    private protected GeometryObject(int rows, int columns)
        : this(new double[MatrixLayout.Create(rows, columns, 1, Math.Max(1, rows)).Extent],
            MatrixLayout.Create(rows, columns, 1, Math.Max(1, rows))) { }

    private protected GeometryObject(Memory<double> memory, MatrixLayout layout, MemoryManager<double>? owner = null)
    {
        try
        {
            var selected = memory[..layout.Extent];
            _ = new ReadOnlyTensorSpan<double>(selected.IsEmpty ? Array.Empty<double>().AsSpan() : selected.Span,
                [layout.Rows, layout.Columns],
                [layout.Rows <= 1 ? 0 : layout.RowStride, layout.Columns <= 1 ? 0 : layout.ColumnStride]);
            _storage = new MatrixStorage(memory, owner);
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
        storage.Retain();
        _storage = storage;
        Layout = layout;
        Offset = offset;
    }

    public int Rows { get { ThrowIfDisposed(); return Layout.Rows; } }
    public int Columns { get { ThrowIfDisposed(); return Layout.Columns; } }
    public int RowStride { get { ThrowIfDisposed(); return Layout.RowStride; } }
    public int ColumnStride { get { ThrowIfDisposed(); return Layout.ColumnStride; } }
    public double this[int row, int column] => Get(row, column);
    public IReadOnlyMatrixD AsReadOnlyMatrix() => new ReadOnlyMatrix(Storage, Layout, Offset);
    protected MatrixXD RetainMatrix() => new(Storage, Layout, Offset);
    IReadOnlyMatrixD IReadOnlyMatrixD.Transposed() => new ReadOnlyMatrix(Storage, Layout.Transposed(), Offset);
    IReadOnlyVectorXD IReadOnlyMatrixD.AsVector() => new ReadOnlyVectorXD(Storage, Layout.Require(null, 1), Offset);
    IReadOnlyMatrixD IReadOnlyMatrixD.Block(int row, int column, int rows, int columns)
    {
        var layout = Layout.Block(row, column, rows, columns);
        return new ReadOnlyMatrix(Storage, layout, layout.Extent == 0 ? Offset : checked(Offset + Layout.Offset(row, column)));
    }
    internal MatrixStorage Storage => Volatile.Read(ref _storage) ?? throw new ObjectDisposedException("Geometry view");
    internal void ThrowIfDisposed() => _ = Storage;
    internal MatrixStorage Acquire()
    {
        var storage = Storage;
        storage.Retain();
        return storage;
    }
    internal double Get(int row, int column)
    {
        var storage = Acquire();
        try { return storage.Memory.Span[checked(Offset + Layout.Offset(row, column))]; }
        finally { storage.Release(); }
    }
    protected void SetValue(int row, int column, double value)
    {
        var storage = Acquire();
        try { storage.Memory.Span[checked(Offset + Layout.Offset(row, column))] = value; }
        finally { storage.Release(); }
    }
    public ReadOnlyTensorSpan<double> AsReadOnlyTensorSpan()
    {
        var memory = Storage.Memory.Slice(Offset, Layout.Extent);
        return new(memory.IsEmpty ? Array.Empty<double>().AsSpan() : memory.Span,
            [Layout.Rows, Layout.Columns],
            [Layout.Rows <= 1 ? 0 : Layout.RowStride, Layout.Columns <= 1 ? 0 : Layout.ColumnStride]);
    }
    protected TensorSpan<double> WritableSpan()
    {
        var memory = Storage.Memory.Slice(Offset, Layout.Extent);
        return new(memory.IsEmpty ? Array.Empty<double>().AsSpan() : memory.Span,
            [Layout.Rows, Layout.Columns],
            [Layout.Rows <= 1 ? 0 : Layout.RowStride, Layout.Columns <= 1 ? 0 : Layout.ColumnStride]);
    }
    public unsafe MemoryHandle Pin()
    {
        var storage = Acquire();
        try
        {
            var pin = storage.Memory.Slice(Offset, Layout.Extent).Pin();
            var lease = new StoragePin(storage, pin);
            return new MemoryHandle(pin.Pointer, default, lease);
        }
        catch { storage.Release(); throw; }
    }
    public override string ToString() => MatrixExtensions.Format(this);

    public void Dispose()
    {
        Interlocked.Exchange(ref _storage, null)?.Release();
        GC.SuppressFinalize(this);
    }
    ~GeometryObject()
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


