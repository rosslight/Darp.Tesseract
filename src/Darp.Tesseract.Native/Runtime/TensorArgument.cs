using System.Buffers;
using System.Runtime.InteropServices;
using Darp.Geometry;

namespace Darp.Tesseract.Native;

/// <summary>Call-scoped pinning and ownership transfer shared by generated typemaps.</summary>
internal sealed unsafe class TensorArgument : IDisposable
{
    private MemoryHandle _pin;
    private readonly NativeOwner _owner;
    internal TensorArgument(IReadOnlyMatrixD input)
    {
        var matrix = input;
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
    internal VectorXD TakeVectorXD()
    {
        var matrix = TakeMatrixXD();
        return matrix.AsVector();
    }
    internal Vector3D TakeVector3D()
    {
        var matrix = TakeMatrixXD();
        return Vector3D.FromMatrix(matrix);
    }
    internal Isometry3D TakeIsometry3D()
    {
        var matrix = TakeMatrixXD();
        return Isometry3D.View(matrix);
    }
    internal QuaternionD TakeQuaternionD() => TensorResult.MutableQuaternion(_owner.Take());
    internal void CopyBack(IMatrixD destination)
    {
        var source = TakeMatrixXD();
        var target = destination.AsMatrix();
        if (source.Rows != target.Rows || source.Columns != target.Columns)
            throw new InvalidOperationException("A native Eigen::Ref output cannot resize the destination.");
        for (int c = 0; c < source.Columns; c++)
            for (int r = 0; r < source.Rows; r++) target[r, c] = source[r, c];
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
    internal ContainerArgument(NativeOwner source)
    {
        using var lease = source.Borrow();
        _owner = new NativeOwner(DarpGeometryInterop.argument(lease.Handle));
    }
    internal HandleRef Handle => _owner.Handle;
    internal bool HasOutput => DarpGeometryInterop.changed(Handle);
    internal IntPtr Take() => _owner.Take();
    public void Dispose() => _owner.Dispose();
}
