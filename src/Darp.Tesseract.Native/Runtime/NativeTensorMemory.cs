using System.Buffers;
using System.Runtime.InteropServices;
using Darp.Geometry.Tensor2;

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
    internal MatrixXD Matrix => MatrixXD.Map(CreateMemory(_extent), Rows, Columns, ColumnStride, RowStride);
    public override Span<double> GetSpan() => new(_data, _extent);
    public override MemoryHandle Pin(int elementIndex = 0)
    {
        if ((uint)elementIndex > (uint)_extent) throw new ArgumentOutOfRangeException(nameof(elementIndex));
        return new MemoryHandle(_data + elementIndex, GCHandle.Alloc(this));
    }
    public override void Unpin() { }
    // Descriptors, slices and pins share this manager. No public disposal path
    // releases their storage; SafeHandle finalization releases the native owner.
    protected override void Dispose(bool disposing) => GC.KeepAlive(_owner);
}

internal static class TensorResult
{
    internal static MatrixXD MutableMatrix(IntPtr pointer) => new NativeTensorMemory(pointer).Matrix;
    internal static ReadOnlyMatrixXD Matrix(IntPtr pointer) => MutableMatrix(pointer).AsReadOnly();
    internal static ReadOnlyVectorXD Vector(IntPtr pointer) => Matrix(pointer).AsVector();
    internal static ReadOnlyVector3D Vector3(IntPtr pointer) => ReadOnlyVector3D.FromMatrix(Matrix(pointer));
    internal static ReadOnlyIsometry3D Isometry(IntPtr pointer) => ReadOnlyIsometry3D.View(Matrix(pointer));
    internal static ReadOnlyQuaternionD Quaternion(IntPtr pointer) => MutableQuaternion(pointer).AsReadOnly();
    internal static QuaternionD MutableQuaternion(IntPtr pointer)
    {
        var memory = new NativeTensorMemory(pointer);
        return QuaternionD.Map(memory.Memory, memory.RowStride);
    }
}
