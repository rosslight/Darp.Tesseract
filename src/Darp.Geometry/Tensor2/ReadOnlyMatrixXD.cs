using System.Numerics.Tensors;
using System.Buffers;

namespace Darp.Geometry.Tensor2;

/// <summary>A read-only view. Other writable aliases can still change its coefficients.</summary>
/// <remarks>No conversion to mutable storage is provided. Clone creates an independent writable matrix.</remarks>
public readonly struct ReadOnlyMatrixXD : IReadOnlyMatrixD
{
    private readonly ReadOnlyMemory<double> _memory;
    private readonly MatrixLayout _layout;

    internal ReadOnlyMatrixXD(ReadOnlyMemory<double> memory, MatrixLayout layout)
    {
        _memory = memory[..layout.Extent];
        _layout = layout;
        _ = AsReadOnlyTensorSpan();
    }

    public static ReadOnlyMatrixXD Map(ReadOnlyMemory<double> memory, int rows, int columns, int? columnStride = null, int rowStride = 1) =>
        new(memory, MatrixLayout.Create(rows, columns, rowStride, columnStride ?? Math.Max(1, checked(rows * rowStride))));

    /// <summary>Pins the backing memory and retains its provider until the handle is disposed.</summary>
    public MemoryHandle Pin() => _memory.Pin();
    public int Rows => _layout.Rows;
    public int Columns => _layout.Columns;
    public int RowStride => _layout.RowStride;
    public int ColumnStride => _layout.ColumnStride;
    public double this[int row, int column]
    {
        get
        {
            try
            {
                return _memory.Span[_layout.Offset(row, column)];
            }
            finally
            {
                KeepAlive();
            }
        }
    }
    internal void KeepAlive() => MemoryLifetime.KeepAlive(_memory);
    public ReadOnlyMatrixXD Block(int row, int column, int rows, int columns)
    {
        var layout = _layout.Block(row, column, rows, columns);
        return new(layout.Extent == 0 ? _memory[..0] : _memory[_layout.Offset(row, column)..], layout);
    }
    public ReadOnlyMatrixXD Row(int row) => Block(row, 0, 1, Columns);
    public ReadOnlyVectorXD Column(int column) => new(Block(0, column, Rows, 1));
    public ReadOnlyMatrixXD Transposed() => new(_memory, _layout.Transposed());
    public ReadOnlyVectorXD AsVector() => new(this);

    public MatrixXD Clone()
    {
        var result = new MatrixXD(Rows, Columns);
        for (int c = 0; c < Columns; c++)
            for (int r = 0; r < Rows; r++) result[r, c] = this[r, c];
        return result;
    }
    public double[,] ToArray()
    {
        var result = new double[Rows, Columns];
        for (int c = 0; c < Columns; c++)
            for (int r = 0; r < Rows; r++) result[r, c] = this[r, c];
        return result;
    }
    public static MatrixXD operator +(ReadOnlyMatrixXD a, ReadOnlyMatrixXD b) => MatrixOperations.Add(a, b);
    public static MatrixXD operator -(ReadOnlyMatrixXD a, ReadOnlyMatrixXD b) => MatrixOperations.Subtract(a, b);
    public static MatrixXD operator *(ReadOnlyMatrixXD a, ReadOnlyMatrixXD b) => MatrixOperations.Multiply(a, b);
    public static VectorXD operator *(ReadOnlyMatrixXD a, ReadOnlyVectorXD b) => new(MatrixOperations.Multiply(a, b));
    public static MatrixXD operator *(ReadOnlyMatrixXD a, double scalar) => MatrixOperations.Scale(a, scalar);
    public static MatrixXD operator /(ReadOnlyMatrixXD a, double scalar) => MatrixOperations.Divide(a, scalar);
    public static MatrixXD operator *(double scalar, ReadOnlyMatrixXD a) => MatrixOperations.Scale(a, scalar);
    public static MatrixXD operator -(ReadOnlyMatrixXD a) => MatrixOperations.Scale(a, -1);
    public ReadOnlyMatrixXD AsReadOnlyMatrix() => this;
    public ReadOnlyTensorSpan<double> AsReadOnlyTensorSpan() => new(_memory.IsEmpty ? Array.Empty<double>().AsSpan() : _memory.Span, [Rows, Columns], [Rows <= 1 ? 0 : RowStride, Columns <= 1 ? 0 : ColumnStride]);
    public double Norm() => MatrixOperations.Norm(this);
    public double SquaredNorm() => MatrixOperations.SquaredNorm(this);
    public MatrixXD Normalized() => MatrixOperations.Normalized(this);
    public override string ToString()
    {
        var rows = new string[Rows];
        for (int r = 0; r < Rows; r++)
        {
            var values = new string[Columns];
            for (int c = 0; c < Columns; c++) values[c] = this[r, c].ToString();
            rows[r] = $"[{string.Join(", ", values)}]";
        }
        return string.Join(Environment.NewLine, rows);
    }
}
