using System.Buffers;
using System.Runtime.InteropServices;
using Darp.Geometry.Tensor2;

namespace Darp.Tesseract.Native;

/// <summary>Call-scoped pinning and ownership transfer shared by generated typemaps.</summary>
internal sealed unsafe class TensorArgument : IDisposable
{
    private MemoryHandle _pin;
    private readonly NativeOwner _owner;
    internal TensorArgument(ReadOnlyMatrixXD matrix)
    {
        _pin = matrix.Pin();
        try
        {
            _owner = new NativeOwner(DarpGeometryInterop.tensor((ulong)_pin.Pointer,
                matrix.Rows, matrix.Columns, matrix.RowStride, matrix.ColumnStride));
        }
        catch { _pin.Dispose(); throw; }
    }
    internal HandleRef Handle => _owner.Handle;
    internal bool HasOutput => DarpGeometryInterop.changed(Handle);
    internal MatrixXD TakeMatrixXD() => TensorResult.MutableMatrix(_owner.Take());
    internal VectorXD TakeVectorXD() => TakeMatrixXD().AsVector();
    internal Vector3D TakeVector3D() => Vector3D.FromMatrix(TakeMatrixXD());
    internal Isometry3D TakeIsometry3D() => Isometry3D.View(TakeMatrixXD());
    internal QuaternionD TakeQuaternionD() => TensorResult.MutableQuaternion(_owner.Take());
    internal void CopyBack(MatrixXD destination)
    {
        var source = TakeMatrixXD();
        if (source.Rows != destination.Rows || source.Columns != destination.Columns)
            throw new InvalidOperationException("A native Eigen::Ref output cannot resize the destination.");
        for (int c = 0; c < source.Columns; c++)
            for (int r = 0; r < source.Rows; r++) destination[r, c] = source[r, c];
    }
    public void Dispose()
    {
        try { _owner.Dispose(); }
        finally { _pin.Dispose(); }
    }
}

internal sealed class ContainerArgument : IDisposable
{
    private readonly NativeOwner _owner;
    internal ContainerArgument(HandleRef source) => _owner = new NativeOwner(DarpGeometryInterop.argument(source));
    internal HandleRef Handle => _owner.Handle;
    internal bool HasOutput => DarpGeometryInterop.changed(Handle);
    internal IntPtr Take() => _owner.Take();
    public void Dispose() => _owner.Dispose();
}
