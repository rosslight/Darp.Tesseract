using System.Buffers;
using Darp.Geometry;

namespace Darp.Tesseract.Native;

internal sealed unsafe class NativeTensorMemory : MemoryManager<double>
{
    private readonly NativeOwner _owner;
    private readonly double* _data;
    private readonly int _extent;

    public int Rows { get; }
    public int Columns { get; }
    public int RowStride { get; }
    public int ColumnStride { get; }

    internal NativeTensorMemory(IntPtr pointer)
    {
        _owner = new NativeOwner(pointer);
        try
        {
            Rows = DarpGeometryInterop.rows(_owner.Handle);
            Columns = DarpGeometryInterop.columns(_owner.Handle);
            RowStride = DarpGeometryInterop.rowStride(_owner.Handle);
            ColumnStride = DarpGeometryInterop.columnStride(_owner.Handle);
            _data = (double*)DarpGeometryInterop.data(_owner.Handle);
            _extent = Rows == 0 || Columns == 0
                ? 0
                : checked((Rows - 1) * RowStride + (Columns - 1) * ColumnStride + 1);
        }
        catch
        {
            _owner.Dispose(); throw;
        }
    }

    internal MatrixXD TakeMatrix()
    {
        var memory = CreateMemory(_extent);
        return MatrixXD.CreateFromMemoryWithOwner(memory, this, Rows, Columns, ColumnStride, RowStride);
    }

    public override Span<double> GetSpan()
    {
        ObjectDisposedException.ThrowIf(_owner.IsClosed, this);
        return new(_data, _extent);
    }

    public override MemoryHandle Pin(int elementIndex = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(elementIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(elementIndex, _extent);
        return new MemoryHandle(_data + elementIndex, pinnable: _owner.Borrow());
    }

    public override void Unpin() { }

    protected override void Dispose(bool disposing) => _owner.Dispose();
}

internal static class TensorResult
{
    internal static MatrixXD MutableMatrix(IntPtr pointer)
    {
        var memory = new NativeTensorMemory(pointer);
        return memory.TakeMatrix();
    }
    internal static IReadOnlyMatrixD Matrix(IntPtr pointer)
    {
        using var matrix = MutableMatrix(pointer);
        return matrix.AsReadOnly();
    }
    internal static IReadOnlyVectorXD Vector(IntPtr pointer)
    {
        using var matrix = MutableMatrix(pointer);
        using var vector = matrix.AsVector();
        return vector.AsReadOnly();
    }
    internal static IReadOnlyVector3D Vector3(IntPtr pointer)
    {
        using var matrix = MutableMatrix(pointer);
        using var vector = Vector3D.FromMatrix(matrix);
        return vector.AsReadOnly();
    }
    internal static IReadOnlyIsometry3D Isometry(IntPtr pointer)
    {
        using var matrix = MutableMatrix(pointer);
        using var transform = Isometry3D.View(matrix);
        return transform.AsReadOnly();
    }
    internal static IReadOnlyQuaternionD Quaternion(IntPtr pointer)
    {
        using var quaternion = MutableQuaternion(pointer);
        return quaternion.AsReadOnly();
    }
    internal static QuaternionD MutableQuaternion(IntPtr pointer)
    {
        using var matrix = MutableMatrix(pointer);
        return QuaternionD.FromMatrix(matrix);
    }
}
