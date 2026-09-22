using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>A mutable N by 1 matrix specialization. All vector algebra uses the matrix kernels.</summary>
public sealed class VectorXD : IMatrixD
{
    private readonly MatrixXD _matrix;
    internal VectorXD(MatrixXD matrix)
    {
        if (matrix.Columns != 1)
        {
            matrix.Dispose();
            throw new ArgumentException("Expected an N by 1 matrix.", nameof(matrix));
        }
        _matrix = matrix;
    }
    public VectorXD(int count) : this(new MatrixXD(count, 1)) { }
    public VectorXD(params ReadOnlySpan<double> values) : this(values.Length)
    {
        for (int i = 0; i < values.Length; i++) this[i] = values[i];
    }
    public ReadOnlyVectorXD AsReadOnly() => new(_matrix.AsReadOnly());
    public static VectorXD Map(Memory<double> memory, int count, int stride = 1) =>
        new(MatrixXD.Map(memory, count, 1, rowStride: stride));
    public int Count => _matrix.Rows;
    public double this[int index]
    {
        get => _matrix[index, 0];
        set => SetValue(index, value);
    }
    internal void SetValue(int index, double value) => _matrix.SetValue(index, 0, value);
    public MatrixXD AsMatrix() => _matrix.AsMatrix();
    public MatrixXD Transposed() => _matrix.Transposed();
    public VectorXD Slice(int start, int count) => new(_matrix.Block(start, 0, count, 1));
    public VectorXD Clone() => new(_matrix.Clone());
    public double[] ToArray()
    {
        var result = new double[Count];
        for (int i = 0; i < Count; i++) result[i] = this[i];
        return result;
    }
    public VectorXD Normalized() => new(MatrixOperations.Normalized(this));
    public static VectorXD Lerp(IReadOnlyMatrixD a, IReadOnlyMatrixD b, double amount) =>
        new(MatrixOperations.Lerp(a, b, amount));
    public static VectorXD operator +(VectorXD a, VectorXD b) => new(a._matrix + b._matrix);
    public static VectorXD operator -(VectorXD a, VectorXD b) => new(a._matrix - b._matrix);
    public static VectorXD operator -(VectorXD value) => new(-value._matrix);
    public static VectorXD operator *(VectorXD value, double scalar) => new(value._matrix * scalar);
    public static VectorXD operator *(double scalar, VectorXD value) => value * scalar;
    public static VectorXD operator /(VectorXD value, double scalar) => new(value._matrix / scalar);
    public static VectorXD operator +(VectorXD a, ReadOnlyVectorXD b) => new(MatrixOperations.Add(a, b));
    public static VectorXD operator +(ReadOnlyVectorXD a, VectorXD b) => new(MatrixOperations.Add(a, b));
    public static VectorXD operator -(VectorXD a, ReadOnlyVectorXD b) => new(MatrixOperations.Subtract(a, b));
    public static VectorXD operator -(ReadOnlyVectorXD a, VectorXD b) => new(MatrixOperations.Subtract(a, b));
    public int Rows => _matrix.Rows;
    public int Columns => _matrix.Columns;
    public ReadOnlyMatrixXD AsReadOnlyMatrix() => _matrix.AsReadOnly();
    public ReadOnlyTensorSpan<double> AsReadOnlyTensorSpan() => _matrix.AsReadOnlyTensorSpan();
    public TensorSpan<double> AsTensorSpan() => _matrix.AsTensorSpan();
    public double Norm() => MatrixOperations.Norm(this);
    public double SquaredNorm() => MatrixOperations.SquaredNorm(this);
    public double Dot<TOther>(TOther other) where TOther : IReadOnlyMatrixD => MatrixOperations.Dot(this, other);
    public MatrixBorrow Borrow() => _matrix.Borrow();
    public ReadOnlyMatrixBorrow BorrowReadOnly() => _matrix.BorrowReadOnly();
    public void Dispose() => _matrix.Dispose();
    public override string ToString() => $"[{string.Join(", ", ToArray())}]";
}
