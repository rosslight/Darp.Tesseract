using System.Buffers;
using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>Disposable mutable matrix. Views retain shared storage; Clone and arithmetic create independent results.</summary>
public sealed class MatrixXD : IMatrixD
{
    private readonly MatrixView _view;
    internal MatrixXD(MatrixView view) => _view = view;

    public MatrixXD(int rows, int columns)
    {
        var layout = MatrixLayout.Create(rows, columns, 1, Math.Max(1, rows));
        var memory = new double[layout.Extent].AsMemory();
        _view = MatrixView.Create(memory, memory, layout);
    }

    /// <summary>Maps caller-owned memory without copying or taking responsibility for its disposal.</summary>
    public static MatrixXD Map(Memory<double> memory, int rows, int columns, int? columnStride = null, int rowStride = 1) =>
        new(MatrixView.Create(memory, memory, MatrixLayout.Create(rows, columns, rowStride, columnStride ?? Math.Max(1, checked(rows * rowStride)))));

    /// <summary>Transfers a storage owner's disposal to the shared matrix storage. The last view or pin releases it.</summary>
    /// <remarks>Do not dispose the transferred owner separately. Failed construction also disposes the owner.</remarks>
    public static MatrixXD Own(Memory<double> memory, IDisposable owner, int rows, int columns, int? columnStride = null, int rowStride = 1)
    {
        ArgumentNullException.ThrowIfNull(owner);
        MatrixLayout layout;
        try { layout = MatrixLayout.Create(rows, columns, rowStride, columnStride ?? Math.Max(1, checked(rows * rowStride))); }
        catch { owner.Dispose(); throw; }
        return new(MatrixView.Create(memory, memory, layout, owner));
    }

    public int Rows { get { _view.ThrowIfDisposed(); return _view.Layout.Rows; } }
    public int Columns { get { _view.ThrowIfDisposed(); return _view.Layout.Columns; } }
    public double this[int row, int column]
    {
        get => _view.Get(row, column);
        set => _view.Set(row, column, value);
    }
    internal void SetValue(int row, int column, double value) => _view.Set(row, column, value);
    public ReadOnlyMatrixXD AsReadOnly() => new(_view.Retain());
    public MatrixXD Block(int row, int column, int rows, int columns) => new(_view.Block(row, column, rows, columns));
    public MatrixXD Row(int row) => Block(row, 0, 1, Columns);
    public VectorXD Column(int column) => new(Block(0, column, Rows, 1));
    public MatrixXD Transposed() => new(_view.Transposed());
    public VectorXD AsVector() => new(AsMatrix());
    public MatrixXD Clone() { using var view = AsReadOnly(); return view.Clone(); }
    public double[,] ToArray() { using var view = AsReadOnly(); return view.ToArray(); }

    public static MatrixXD FromArray(double[,] values)
    {
        var result = new MatrixXD(values.GetLength(0), values.GetLength(1));
        for (int c = 0; c < result.Columns; c++)
            for (int r = 0; r < result.Rows; r++) result[r, c] = values[r, c];
        return result;
    }
    public static MatrixXD Identity(int size)
    {
        var result = new MatrixXD(size, size);
        for (int i = 0; i < size; i++) result[i, i] = 1;
        return result;
    }
    public static MatrixXD operator +(MatrixXD a, IReadOnlyMatrixD b) => MatrixOperations.Add(a, b);
    public static MatrixXD operator -(MatrixXD a, IReadOnlyMatrixD b) => MatrixOperations.Subtract(a, b);
    public static MatrixXD operator *(MatrixXD a, IReadOnlyMatrixD b) => MatrixOperations.Multiply(a, b);
    public static VectorXD operator *(MatrixXD a, VectorXD b) => new(MatrixOperations.Multiply(a, b));
    public static VectorXD operator *(MatrixXD a, ReadOnlyVectorXD b) => new(MatrixOperations.Multiply(a, b));
    public static MatrixXD operator *(MatrixXD a, double scalar) => MatrixOperations.Scale(a, scalar);
    public static MatrixXD operator *(double scalar, MatrixXD a) => a * scalar;
    public static MatrixXD operator /(MatrixXD a, double scalar) => MatrixOperations.Divide(a, scalar);
    public static MatrixXD operator -(MatrixXD a) => MatrixOperations.Scale(a, -1);
    /// <summary>Raw access; keep this object alive and undisposed until the span's last use, or use Borrow().</summary>
    public TensorSpan<double> AsTensorSpan() => _view.Span();
    public ReadOnlyTensorSpan<double> AsReadOnlyTensorSpan() => _view.ReadOnlySpan();
    public ReadOnlyMatrixXD AsReadOnlyMatrix() => AsReadOnly();
    public MatrixXD AsMatrix() => new(_view.Retain());
    public MatrixBorrow Borrow() => new(_view.Retain());
    public ReadOnlyMatrixBorrow BorrowReadOnly() => new(_view.Retain());
    public MemoryHandle Pin() => _view.Pin();
    public void Dispose() => _view.Dispose();
    public double Norm() => MatrixOperations.Norm(this);
    public double SquaredNorm() => MatrixOperations.SquaredNorm(this);
    public MatrixXD Normalized() => MatrixOperations.Normalized(this);
    public override string ToString() { using var view = AsReadOnly(); return view.ToString(); }
}
