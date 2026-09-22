using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>A read-only 3 by 1 matrix specialization with named components.</summary>
public sealed class ReadOnlyVector3D : IReadOnlyMatrixD
{
    private readonly ReadOnlyVectorXD _vector;
    internal ReadOnlyVector3D(ReadOnlyVectorXD vector)
    {
        if (vector.Count != 3)
        {
            vector.Dispose();
            throw new ArgumentException("Expected three coefficients.", nameof(vector));
        }
        _vector = vector;
    }
    
    public static ReadOnlyVector3D Map(ReadOnlyMemory<double> memory, int stride = 1) => new(ReadOnlyVectorXD.Map(memory, 3, stride));
    public static ReadOnlyVector3D FromMatrix(ReadOnlyMatrixXD readOnlyMatrix) => new(readOnlyMatrix.AsVector());
    public double X => _vector[0];
    public double Y => _vector[1];
    public double Z => _vector[2];
    public double this[int index] => _vector[index];
    public ReadOnlyVectorXD AsVector() => new(_vector.AsReadOnlyMatrix());
    public ReadOnlyMatrixXD AsMatrix() => _vector.AsMatrix();
    public ReadOnlyMatrixXD Transposed() => _vector.Transposed();
    public Vector3D Clone() => new(_vector.Clone());
    public double[] ToArray() => _vector.ToArray();
    public Vector3D Normalized() => new(new VectorXD(MatrixOperations.Normalized(this)));
    public static Vector3D Lerp(IReadOnlyMatrixD a, IReadOnlyMatrixD b, double amount) => new(new VectorXD(MatrixOperations.Lerp(a, b, amount)));
    public static Vector3D operator +(ReadOnlyVector3D a, ReadOnlyVector3D b) => new(a._vector + b._vector);
    public static Vector3D operator -(ReadOnlyVector3D a, ReadOnlyVector3D b) => new(a._vector - b._vector);
    public static Vector3D operator -(ReadOnlyVector3D value) => new(-value._vector);
    public static Vector3D operator *(ReadOnlyVector3D value, double scalar) => new(value._vector * scalar);
    public static Vector3D operator *(double scalar, ReadOnlyVector3D value) => value * scalar;
    public static Vector3D operator /(ReadOnlyVector3D value, double scalar) => new(value._vector / scalar);
    public int Rows => _vector.Rows;
    public int Columns => _vector.Columns;
    public int Count => _vector.Count;
    public ReadOnlyMatrixXD AsReadOnlyMatrix() => _vector.AsReadOnlyMatrix();
    public ReadOnlyTensorSpan<double> AsReadOnlyTensorSpan() => _vector.AsReadOnlyTensorSpan();
    public double Norm() => MatrixOperations.Norm(this);
    public double SquaredNorm() => MatrixOperations.SquaredNorm(this);
    public double Dot<TOther>(TOther other) where TOther : IReadOnlyMatrixD => MatrixOperations.Dot(this, other);
    public Vector3D Cross<TOther>(TOther other) where TOther : IReadOnlyMatrixD => MatrixOperations.Cross(this, other);
    public ReadOnlyMatrixBorrow Borrow() => _vector.Borrow();
    public ReadOnlyMatrixBorrow BorrowReadOnly() => _vector.BorrowReadOnly();
    public void Dispose() => _vector.Dispose();
    public override string ToString() => _vector.ToString();
}
