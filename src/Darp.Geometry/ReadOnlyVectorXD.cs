using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>A read-only N by 1 matrix specialization. All vector algebra uses the matrix kernels.</summary>
public sealed class ReadOnlyVectorXD : IReadOnlyMatrixD
{
    private readonly ReadOnlyMatrixXD _readOnlyMatrix;
    internal ReadOnlyVectorXD(ReadOnlyMatrixXD readOnlyMatrix)
    {
        if (readOnlyMatrix.Columns != 1)
        {
            readOnlyMatrix.Dispose();
            throw new ArgumentException("Expected an N by 1 matrix.", nameof(readOnlyMatrix));
        }
        _readOnlyMatrix = readOnlyMatrix;
    }
    
    public static ReadOnlyVectorXD Map(ReadOnlyMemory<double> memory, int count, int stride = 1) =>
        new(ReadOnlyMatrixXD.Map(memory, count, 1, rowStride: stride));
    public int Count => _readOnlyMatrix.Rows;
    public double this[int index] => _readOnlyMatrix[index, 0];
    public ReadOnlyMatrixXD AsMatrix() => _readOnlyMatrix.AsReadOnlyMatrix();
    public ReadOnlyMatrixXD Transposed() => _readOnlyMatrix.Transposed();
    public ReadOnlyVectorXD Slice(int start, int count) => new(_readOnlyMatrix.Block(start, 0, count, 1));
    public VectorXD Clone() => new(_readOnlyMatrix.Clone());
    public double[] ToArray()
    {
        var result = new double[Count];
        for (int i = 0; i < Count; i++) result[i] = this[i];
        return result;
    }
    public VectorXD Normalized() => new(MatrixOperations.Normalized(this));
    public static VectorXD Lerp(IReadOnlyMatrixD a, IReadOnlyMatrixD b, double amount) =>
        new(MatrixOperations.Lerp(a, b, amount));
    public static VectorXD operator +(ReadOnlyVectorXD a, ReadOnlyVectorXD b) => new(a._readOnlyMatrix + b._readOnlyMatrix);
    public static VectorXD operator -(ReadOnlyVectorXD a, ReadOnlyVectorXD b) => new(a._readOnlyMatrix - b._readOnlyMatrix);
    public static VectorXD operator -(ReadOnlyVectorXD value) => new(-value._readOnlyMatrix);
    public static VectorXD operator *(ReadOnlyVectorXD value, double scalar) => new(value._readOnlyMatrix * scalar);
    public static VectorXD operator *(double scalar, ReadOnlyVectorXD value) => value * scalar;
    public static VectorXD operator /(ReadOnlyVectorXD value, double scalar) => new(value._readOnlyMatrix / scalar);
    public int Rows => _readOnlyMatrix.Rows;
    public int Columns => _readOnlyMatrix.Columns;
    public ReadOnlyMatrixXD AsReadOnlyMatrix() => _readOnlyMatrix.AsReadOnlyMatrix();
    public ReadOnlyTensorSpan<double> AsReadOnlyTensorSpan() => _readOnlyMatrix.AsReadOnlyTensorSpan();
    public double Norm() => MatrixOperations.Norm(this);
    public double SquaredNorm() => MatrixOperations.SquaredNorm(this);
    public double Dot<TOther>(TOther other) where TOther : IReadOnlyMatrixD => MatrixOperations.Dot(this, other);
    public ReadOnlyMatrixBorrow Borrow() => _readOnlyMatrix.Borrow();
    public ReadOnlyMatrixBorrow BorrowReadOnly() => _readOnlyMatrix.BorrowReadOnly();
    public void Dispose() => _readOnlyMatrix.Dispose();
    public override string ToString() => $"[{string.Join(", ", ToArray())}]";
}
