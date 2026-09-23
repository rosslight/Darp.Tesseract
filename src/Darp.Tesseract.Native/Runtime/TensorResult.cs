using Aardvark.Base;

namespace Darp.Tesseract.Native;

// Native Value owns the temporary Eigen result. Every public result is copied before it is released.
internal static unsafe class TensorResult
{
    private readonly ref struct NativeTensor
    {
        private readonly NativeOwner _owner;
        private readonly double* _data;
        private readonly int _rowStride;
        private readonly int _columnStride;
        internal int Rows { get; }
        internal int Columns { get; }

        internal NativeTensor(IntPtr pointer)
        {
            _owner = new(pointer);
            try
            {
                Rows = DarpGeometryInterop.rows(_owner.Handle);
                Columns = DarpGeometryInterop.columns(_owner.Handle);
                _rowStride = DarpGeometryInterop.rowStride(_owner.Handle);
                _columnStride = DarpGeometryInterop.columnStride(_owner.Handle);
                _data = (double*)DarpGeometryInterop.data(_owner.Handle);
            }
            catch { _owner.Dispose(); throw; }
        }

        internal double Get(int row, int column) => _data[row * _rowStride + column * _columnStride];
        internal void Require(int rows, int columns)
        {
            if (Rows != rows || Columns != columns)
                throw new ArgumentException($"Expected {rows} by {columns} native geometry, received {Rows} by {Columns}.");
        }
        internal void Dispose() => _owner.Dispose();
    }

    internal static double[] Vector(IntPtr pointer)
    {
        using var source = new NativeTensor(pointer);
        if (source.Columns != 1) throw new ArgumentException("Expected a native column vector.");
        var result = new double[source.Rows];
        for (int row = 0; row < result.Length; row++) result[row] = source.Get(row, 0);
        return result;
    }

    internal static V2d Vector2(IntPtr pointer)
    {
        using var source = new NativeTensor(pointer);
        source.Require(2, 1);
        return new(source.Get(0, 0), source.Get(1, 0));
    }

    internal static V3d Vector3(IntPtr pointer)
    {
        using var source = new NativeTensor(pointer);
        source.Require(3, 1);
        return new(source.Get(0, 0), source.Get(1, 0), source.Get(2, 0));
    }

    internal static V4d Vector4(IntPtr pointer)
    {
        using var source = new NativeTensor(pointer);
        source.Require(4, 1);
        return new(source.Get(0, 0), source.Get(1, 0), source.Get(2, 0), source.Get(3, 0));
    }

    internal static double[,] Matrix(IntPtr pointer)
    {
        using var source = new NativeTensor(pointer);
        var result = new double[source.Rows, source.Columns];
        for (int column = 0; column < source.Columns; column++)
        for (int row = 0; row < source.Rows; row++)
            result[row, column] = source.Get(row, column);
        return result;
    }

    internal static QuaternionD Quaternion(IntPtr pointer)
    {
        using var source = new NativeTensor(pointer);
        source.Require(4, 1);
        // Eigen stores quaternion coefficients as X, Y, Z, W.
        return new(source.Get(3, 0), source.Get(0, 0), source.Get(1, 0), source.Get(2, 0));
    }

    internal static Euclidean3d Isometry(IntPtr pointer)
    {
        using var source = new NativeTensor(pointer);
        source.Require(4, 4);
        var result = new M44d();
        for (int row = 0; row < 4; row++)
        for (int column = 0; column < 4; column++)
            result[row, column] = source.Get(row, column);
        return Euclidean3d.FromM44d(result);
    }
}
