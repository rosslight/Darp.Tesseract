using System.Buffers;
using Darp.Geometry;

namespace Darp.Tesseract.Native;

internal sealed unsafe class NativeTensorMemory : MemoryManager<double>
{
    private readonly NativeOwner _owner;
    private readonly double* _data;
    private readonly int _extent;
    internal int Rows { get; }
    internal int Columns { get; }
    internal int RowStride { get; }
    internal int ColumnStride { get; }

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
            _extent = Rows == 0 || Columns == 0 ? 0 : checked((Rows - 1) * RowStride + (Columns - 1) * ColumnStride + 1);
        }
        catch { _owner.Dispose(); throw; }
    }
    internal MatrixXD TakeMatrix() => MatrixXD.Own(CreateMemory(_extent), this, Rows, Columns, ColumnStride, RowStride);
    public override Span<double> GetSpan()
    {
        ObjectDisposedException.ThrowIf(_owner.IsClosed, this);
        return new(_data, _extent);
    }
    public override MemoryHandle Pin(int elementIndex = 0)
    {
        if ((uint)elementIndex > (uint)_extent) throw new ArgumentOutOfRangeException(nameof(elementIndex));
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
        try { return memory.TakeMatrix(); }
        catch { ((IDisposable)memory).Dispose(); throw; }
    }
    internal static ReadOnlyMatrixXD Matrix(IntPtr pointer)
    {
        using var matrix = MutableMatrix(pointer);
        return matrix.AsReadOnly();
    }
    internal static ReadOnlyVectorXD Vector(IntPtr pointer)
    {
        using var matrix = Matrix(pointer);
        return matrix.AsVector();
    }
    internal static ReadOnlyVector3D Vector3(IntPtr pointer)
    {
        using var matrix = Matrix(pointer);
        return ReadOnlyVector3D.FromMatrix(matrix);
    }
    internal static ReadOnlyIsometry3D Isometry(IntPtr pointer)
    {
        using var matrix = Matrix(pointer);
        return ReadOnlyIsometry3D.View(matrix);
    }
    internal static ReadOnlyQuaternionD Quaternion(IntPtr pointer)
    {
        using var matrix = Matrix(pointer);
        return ReadOnlyQuaternionD.FromMatrix(matrix);
    }
    internal static QuaternionD MutableQuaternion(IntPtr pointer)
    {
        using var matrix = MutableMatrix(pointer);
        return QuaternionD.FromMatrix(matrix);
    }
}
