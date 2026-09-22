using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>A read-only 3 by 3 matrix specialization.</summary>
public sealed class ReadOnlyMatrix3D : IReadOnlyMatrixD
{
    private readonly ReadOnlyMatrixXD _readOnlyMatrix;
    internal ReadOnlyMatrix3D(ReadOnlyMatrixXD readOnlyMatrix)
    {
        if (readOnlyMatrix.Rows != 3 || readOnlyMatrix.Columns != 3)
        {
            readOnlyMatrix.Dispose();
            throw new ArgumentException("Expected a 3 by 3 matrix.", nameof(readOnlyMatrix));
        }
        _readOnlyMatrix = readOnlyMatrix;
    }
    
    public static ReadOnlyMatrix3D Map(ReadOnlyMemory<double> memory, int columnStride = 3, int rowStride = 1) =>
        new(ReadOnlyMatrixXD.Map(memory, 3, 3, columnStride, rowStride));
    public static ReadOnlyMatrix3D FromMatrix(ReadOnlyMatrixXD readOnlyMatrix) => new(readOnlyMatrix.AsReadOnlyMatrix());
    public double this[int row, int column] => _readOnlyMatrix[row, column];
    public ReadOnlyMatrixXD Row(int row) => _readOnlyMatrix.Row(row);
    public ReadOnlyVector3D Column(int column) => new(_readOnlyMatrix.Column(column));
    public ReadOnlyMatrixXD AsMatrix() => _readOnlyMatrix.AsReadOnlyMatrix();
    public Matrix3D Normalized() => new(MatrixOperations.Normalized(this));
    public Matrix3D Clone() => new(_readOnlyMatrix.Clone());
    public double[,] ToArray() => _readOnlyMatrix.ToArray();
    public ReadOnlyMatrix3D Transposed() => new(_readOnlyMatrix.Transposed());
    public double Determinant() => MatrixOperations.Determinant3x3(this);
    public QuaternionD ToQuaternion() => QuaternionD.FromRotationMatrix(this);
    public static Matrix3D operator +(ReadOnlyMatrix3D a, ReadOnlyMatrix3D b) => new(a._readOnlyMatrix + b._readOnlyMatrix);
    public static Matrix3D operator -(ReadOnlyMatrix3D a, ReadOnlyMatrix3D b) => new(a._readOnlyMatrix - b._readOnlyMatrix);
    public static Matrix3D operator -(ReadOnlyMatrix3D value) => new(-value._readOnlyMatrix);
    public static Matrix3D operator *(ReadOnlyMatrix3D a, ReadOnlyMatrix3D b) => new(a._readOnlyMatrix * b._readOnlyMatrix);
    public static Vector3D operator *(ReadOnlyMatrix3D a, Vector3D b) => new(new VectorXD(MatrixOperations.Multiply(a, b)));
    public static Vector3D operator *(ReadOnlyMatrix3D a, ReadOnlyVector3D b) => new(new VectorXD(MatrixOperations.Multiply(a, b)));
    public static Matrix3D operator *(ReadOnlyMatrix3D value, double scalar) => new(value._readOnlyMatrix * scalar);
    public static Matrix3D operator *(double scalar, ReadOnlyMatrix3D value) => value * scalar;
    public static Matrix3D operator /(ReadOnlyMatrix3D value, double scalar) => new(value._readOnlyMatrix / scalar);
    public int Rows => _readOnlyMatrix.Rows;
    public int Columns => _readOnlyMatrix.Columns;
    public ReadOnlyMatrixXD AsReadOnlyMatrix() => _readOnlyMatrix.AsReadOnlyMatrix();
    public ReadOnlyTensorSpan<double> AsReadOnlyTensorSpan() => _readOnlyMatrix.AsReadOnlyTensorSpan();
    public double Norm() => MatrixOperations.Norm(this);
    public double SquaredNorm() => MatrixOperations.SquaredNorm(this);
    public ReadOnlyMatrixBorrow Borrow() => _readOnlyMatrix.Borrow();
    public ReadOnlyMatrixBorrow BorrowReadOnly() => _readOnlyMatrix.BorrowReadOnly();
    public void Dispose() => _readOnlyMatrix.Dispose();
    public override string ToString() => _readOnlyMatrix.ToString();
}
