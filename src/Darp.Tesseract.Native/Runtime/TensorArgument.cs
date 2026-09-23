using System.Runtime.InteropServices;
using Aardvark.Base;

namespace Darp.Tesseract.Native;

// A call owns one managed column-major copy. Eigen never receives a pointer into a public value.
internal sealed class TensorArgument : IDisposable
{
    private readonly GCHandle _pin;
    private readonly NativeOwner _owner;

    private TensorArgument(double[] coefficients, int rows, int columns)
    {
        _pin = GCHandle.Alloc(coefficients, GCHandleType.Pinned);
        try
        {
            _owner = new NativeOwner(DarpGeometryInterop.tensor(
                (ulong)_pin.AddrOfPinnedObject(), rows, columns, 1, Math.Max(1, rows)));
        }
        catch { _pin.Free(); throw; }
    }

    internal TensorArgument(double[] vector) : this((double[])vector.Clone(), vector.Length, 1) { }
    internal TensorArgument(V2d vector) : this([vector.X, vector.Y], 2, 1) { }
    internal TensorArgument(V3d vector) : this([vector.X, vector.Y, vector.Z], 3, 1) { }
    internal TensorArgument(V4d vector) : this([vector.X, vector.Y, vector.Z, vector.W], 4, 1) { }
    internal TensorArgument(QuaternionD quaternion) : this([quaternion.X, quaternion.Y, quaternion.Z, quaternion.W], 4, 1) { }
    internal TensorArgument(Euclidean3d transform) : this(Pack((M44d)transform), 4, 4) { }
    internal TensorArgument(double[,] matrix) : this(Pack(matrix), matrix.GetLength(0), matrix.GetLength(1)) { }

    private static double[] Pack(double[,] matrix)
    {
        int rows = matrix.GetLength(0), columns = matrix.GetLength(1);
        var result = new double[checked(rows * columns)];
        for (int column = 0; column < columns; column++)
        for (int row = 0; row < rows; row++)
            result[column * rows + row] = matrix[row, column];
        return result;
    }

    private static double[] Pack(M44d matrix)
    {
        var result = new double[16];
        for (int column = 0; column < 4; column++)
        for (int row = 0; row < 4; row++)
            result[column * 4 + row] = matrix[row, column];
        return result;
    }

    internal HandleRef Handle => _owner.Handle;
    internal bool HasOutput => DarpGeometryInterop.changed(Handle);
    internal double[] TakeVector() => TensorResult.Vector(_owner.Take());
    internal V2d TakeVector2() => TensorResult.Vector2(_owner.Take());
    internal V3d TakeVector3() => TensorResult.Vector3(_owner.Take());
    internal V4d TakeVector4() => TensorResult.Vector4(_owner.Take());
    internal double[,] TakeMatrix() => TensorResult.Matrix(_owner.Take());
    internal QuaternionD TakeQuaternion() => TensorResult.Quaternion(_owner.Take());
    internal Euclidean3d TakeIsometry() => TensorResult.Isometry(_owner.Take());

    internal void CopyBack(double[] destination)
    {
        var source = TakeVector();
        if (source.Length != destination.Length)
            throw new InvalidOperationException("A native Eigen::Ref output cannot resize its destination.");
        source.CopyTo(destination, 0);
    }
    internal void CopyBack(double[,] destination)
    {
        var source = TakeMatrix();
        if (source.GetLength(0) != destination.GetLength(0) || source.GetLength(1) != destination.GetLength(1))
            throw new InvalidOperationException("A native Eigen::Ref output cannot resize its destination.");
        Array.Copy(source, destination, source.Length);
    }
    public void Dispose()
    {
        try { _owner.Dispose(); }
        finally { _pin.Free(); }
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
