using System.Numerics.Tensors;

namespace Darp.Geometry.Tensor2;

/// <summary>A mutable matrix descriptor. Views share storage; Clone and arithmetic allocate results.</summary>
/// <remarks>Default instances have no storage. Mapping does not establish native ownership.</remarks>
public readonly struct MatrixXD : IMatrixD
{
    private readonly Memory<double> _memory;
    private readonly MatrixLayout _layout;

    internal MatrixXD(Memory<double> memory, MatrixLayout layout)
    {
        _memory = memory[..layout.Extent];
        _layout = layout;
        _ = AsReadOnlyTensorSpan();
    }

    /// <summary> Initializes a new, empty matrix </summary>
    /// <param name="rows">The number of rows of this matrix</param>
    /// <param name="columns">The number of columns of this matrix</param>
    public MatrixXD(int rows, int columns)
    {
        _layout = MatrixLayout.Create(rows, columns, 1, Math.Max(1, rows));
        _memory = new double[_layout.Extent];
    }

    /// <summary>Maps column-major storage by default; explicit strides also support row-major and strided views.</summary>
    /// <param name="memory">The underlying memory representation</param>
    /// <param name="rows">The number of rows of this matrix</param>
    /// <param name="columns">The n</param>
    /// <param name="columnStride"></param>
    /// <param name="rowStride"></param>
    /// <returns></returns>
    public static MatrixXD Map(Memory<double> memory, int rows, int columns, int? columnStride = null, int rowStride = 1) =>
        new(memory, MatrixLayout.Create(rows, columns, rowStride, columnStride ?? Math.Max(1, checked(rows * rowStride))));

    /// <summary> The number of rows </summary>
    public int Rows => _layout.Rows;
    /// <summary> The number of columns </summary>
    public int Columns => _layout.Columns;

    public double this[int row, int column]
    {
        get => AsReadOnly()[row, column];
        set => SetValue(row, column, value);
    }
    internal void SetValue(int row, int column, double value)
    {
        try
        {
            _memory.Span[_layout.Offset(row, column)] = value;
        }
        finally
        {
            MemoryLifetime.KeepAlive(_memory);
        }
    }
    public ReadOnlyMatrixXD AsReadOnly() => new(_memory, _layout);
    public static implicit operator ReadOnlyMatrixXD(MatrixXD value) => value.AsReadOnly();
    public MatrixXD Block(int row, int column, int rows, int columns)
    {
        var layout = _layout.Block(row, column, rows, columns);
        return new(layout.Extent == 0 ? _memory[..0] : _memory[_layout.Offset(row, column)..], layout);
    }
    /// <summary>A 1 by N matrix view, preserving row orientation.</summary>
    public MatrixXD Row(int row) => Block(row, 0, 1, Columns);
    public VectorXD Column(int column) => new(Block(0, column, Rows, 1));
    /// <summary>A view with swapped axes; no coefficient copy.</summary>
    public MatrixXD Transposed() => new(_memory, _layout.Transposed());
    /// <summary>Specializes an N by 1 matrix without copying; rejects other shapes.</summary>
    public VectorXD AsVector() => new(this);
    public MatrixXD Clone() => AsReadOnly().Clone();
    public double[,] ToArray() => AsReadOnly().ToArray();

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
    public static MatrixXD operator +(MatrixXD a, MatrixXD b) => MatrixOperations.Add(a, b);
    public static MatrixXD operator -(MatrixXD a, MatrixXD b) => MatrixOperations.Subtract(a, b);
    public static MatrixXD operator *(MatrixXD a, MatrixXD b) => MatrixOperations.Multiply(a, b);
    public static VectorXD operator *(MatrixXD a, VectorXD b) => new(MatrixOperations.Multiply(a, b));
    public static VectorXD operator *(MatrixXD a, ReadOnlyVectorXD b) => new(MatrixOperations.Multiply(a, b));
    public static MatrixXD operator *(MatrixXD a, double scalar) => MatrixOperations.Scale(a, scalar);
    public static MatrixXD operator *(double scalar, MatrixXD a) => a * scalar;
    public static MatrixXD operator /(MatrixXD a, double scalar) => MatrixOperations.Divide(a, scalar);
    public static MatrixXD operator -(MatrixXD a) => MatrixOperations.Scale(a, -1);
    public TensorSpan<double> AsTensorSpan() => new(_memory.IsEmpty ? Array.Empty<double>().AsSpan() : _memory.Span, [Rows, Columns], [Rows <= 1 ? 0 : _layout.RowStride, Columns <= 1 ? 0 : _layout.ColumnStride]);
    public ReadOnlyMatrixXD AsReadOnlyMatrix() => AsReadOnly();
    public MatrixXD AsMatrix() => this;
    public ReadOnlyTensorSpan<double> AsReadOnlyTensorSpan() => new(_memory.IsEmpty ? Array.Empty<double>().AsSpan() : _memory.Span, [Rows, Columns], [Rows <= 1 ? 0 : _layout.RowStride, Columns <= 1 ? 0 : _layout.ColumnStride]);
    public double Norm() => MatrixOperations.Norm(this);
    public double SquaredNorm() => MatrixOperations.SquaredNorm(this);
    public MatrixXD Normalized() => MatrixOperations.Normalized(this);
    public override string ToString() => AsReadOnly().ToString();
}
