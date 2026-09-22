using System.Numerics.Tensors;
using System.Buffers;

namespace Darp.Geometry;

/// <summary>A disposable read-only view. Writable aliases may still change the shared coefficients.</summary>
public sealed class ReadOnlyMatrixXD : IReadOnlyMatrixD
{
    private readonly MatrixView _view;
    internal ReadOnlyMatrixXD(MatrixView view) => _view = view;

    public static ReadOnlyMatrixXD Map(ReadOnlyMemory<double> memory, int rows, int columns, int? columnStride = null, int rowStride = 1) =>
        new(MatrixView.Create(memory, default, MatrixLayout.Create(rows, columns, rowStride, columnStride ?? Math.Max(1, checked(rows * rowStride)))));

    /// <summary>Transfers storage ownership; the last retained view or pin disposes the owner.</summary>
    public static ReadOnlyMatrixXD Own(ReadOnlyMemory<double> memory, IDisposable owner, int rows, int columns, int? columnStride = null, int rowStride = 1)
    {
        ArgumentNullException.ThrowIfNull(owner);
        MatrixLayout layout;
        try { layout = MatrixLayout.Create(rows, columns, rowStride, columnStride ?? Math.Max(1, checked(rows * rowStride))); }
        catch { owner.Dispose(); throw; }
        return new(MatrixView.Create(memory, default, layout, owner));
    }

    public MemoryHandle Pin() => _view.Pin();
    public int Rows { get { _view.ThrowIfDisposed(); return _view.Layout.Rows; } }
    public int Columns { get { _view.ThrowIfDisposed(); return _view.Layout.Columns; } }
    public int RowStride { get { _view.ThrowIfDisposed(); return _view.Layout.RowStride; } }
    public int ColumnStride { get { _view.ThrowIfDisposed(); return _view.Layout.ColumnStride; } }
    public double this[int row, int column] => _view.Get(row, column);
    public ReadOnlyMatrixXD Block(int row, int column, int rows, int columns) => new(_view.Block(row, column, rows, columns));
    public ReadOnlyMatrixXD Row(int row) => Block(row, 0, 1, Columns);
    public ReadOnlyVectorXD Column(int column) => new(Block(0, column, Rows, 1));
    public ReadOnlyMatrixXD Transposed() => new(_view.Transposed());
    public ReadOnlyVectorXD AsVector() => new(AsReadOnlyMatrix());

    public MatrixXD Clone()
    {
        using var source = Borrow();
        var values = source.AsReadOnlyTensorSpan();
        var result = new MatrixXD(source.Rows, source.Columns);
        using var destination = result.Borrow();
        var target = destination.AsTensorSpan();
        for (int c = 0; c < source.Columns; c++)
            for (int r = 0; r < source.Rows; r++) target[r, c] = values[r, c];
        return result;
    }
    public double[,] ToArray()
    {
        using var source = Borrow();
        var values = source.AsReadOnlyTensorSpan();
        var result = new double[source.Rows, source.Columns];
        for (int c = 0; c < source.Columns; c++)
            for (int r = 0; r < source.Rows; r++) result[r, c] = values[r, c];
        return result;
    }
    public static MatrixXD operator +(ReadOnlyMatrixXD a, IReadOnlyMatrixD b) => MatrixOperations.Add(a, b);
    public static MatrixXD operator -(ReadOnlyMatrixXD a, IReadOnlyMatrixD b) => MatrixOperations.Subtract(a, b);
    public static MatrixXD operator *(ReadOnlyMatrixXD a, IReadOnlyMatrixD b) => MatrixOperations.Multiply(a, b);
    public static VectorXD operator *(ReadOnlyMatrixXD a, ReadOnlyVectorXD b) => new(MatrixOperations.Multiply(a, b));
    public static MatrixXD operator *(ReadOnlyMatrixXD a, double scalar) => MatrixOperations.Scale(a, scalar);
    public static MatrixXD operator /(ReadOnlyMatrixXD a, double scalar) => MatrixOperations.Divide(a, scalar);
    public static MatrixXD operator *(double scalar, ReadOnlyMatrixXD a) => MatrixOperations.Scale(a, scalar);
    public static MatrixXD operator -(ReadOnlyMatrixXD a) => MatrixOperations.Scale(a, -1);
    public ReadOnlyMatrixXD AsReadOnlyMatrix() => new(_view.Retain());
    public ReadOnlyMatrixBorrow Borrow() => new(_view.Retain());
    public ReadOnlyMatrixBorrow BorrowReadOnly() => Borrow();
    public void Dispose() => _view.Dispose();
    /// <summary>Raw access; keep this object alive and undisposed until the span's last use, or use Borrow().</summary>
    public ReadOnlyTensorSpan<double> AsReadOnlyTensorSpan() => _view.ReadOnlySpan();
    public double Norm() => MatrixOperations.Norm(this);
    public double SquaredNorm() => MatrixOperations.SquaredNorm(this);
    public MatrixXD Normalized() => MatrixOperations.Normalized(this);
    public override string ToString()
    {
        using var source = Borrow();
        var matrix = source.AsReadOnlyTensorSpan();
        var rows = new string[source.Rows];
        for (int r = 0; r < source.Rows; r++)
        {
            var values = new string[source.Columns];
            for (int c = 0; c < source.Columns; c++) values[c] = matrix[r, c].ToString();
            rows[r] = $"[{string.Join(", ", values)}]";
        }
        return string.Join(Environment.NewLine, rows);
    }
}
