using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>A mutable 3 by 3 matrix specialization.</summary>
public sealed class Matrix3D : IMatrixD
{
    private readonly MatrixXD _matrix;
    internal Matrix3D(MatrixXD matrix)
    {
        if (matrix.Rows != 3 || matrix.Columns != 3)
        {
            matrix.Dispose();
            throw new ArgumentException("Expected a 3 by 3 matrix.", nameof(matrix));
        }
        _matrix = matrix;
    }
    public Matrix3D() : this(new MatrixXD(3, 3)) { }
    public static Matrix3D Identity => new(MatrixXD.Identity(3));
    public static Matrix3D FromArray(double[,] values) => new(MatrixXD.FromArray(values));
    public ReadOnlyMatrix3D AsReadOnly() => new(_matrix.AsReadOnly());
    public static Matrix3D Map(Memory<double> memory, int columnStride = 3, int rowStride = 1) =>
        new(MatrixXD.Map(memory, 3, 3, columnStride, rowStride));
    public static Matrix3D FromMatrix(MatrixXD matrix) => new(matrix.AsMatrix());
    public double this[int row, int column]
    {
        get => _matrix[row, column];
        set => _matrix.SetValue(row, column, value);
    }
    public MatrixXD Row(int row) => _matrix.Row(row);
    public Vector3D Column(int column) => new(_matrix.Column(column));
    public Matrix3D Normalized() => new(MatrixOperations.Normalized(this));
    public Matrix3D Clone() => new(_matrix.Clone());
    public double[,] ToArray() => _matrix.ToArray();
    public Matrix3D Transposed() => new(_matrix.Transposed());
    public double Determinant() => MatrixOperations.Determinant3x3(this);
    public QuaternionD ToQuaternion() => QuaternionD.FromRotationMatrix(this);
    public static Matrix3D operator +(Matrix3D a, Matrix3D b) => new(a._matrix + b._matrix);
    public static Matrix3D operator -(Matrix3D a, Matrix3D b) => new(a._matrix - b._matrix);
    public static Matrix3D operator -(Matrix3D value) => new(-value._matrix);
    public static Matrix3D operator *(Matrix3D a, Matrix3D b) => new(a._matrix * b._matrix);
    public static Vector3D operator *(Matrix3D a, Vector3D b) => new(new VectorXD(MatrixOperations.Multiply(a, b)));
    public static Vector3D operator *(Matrix3D a, ReadOnlyVector3D b) => new(new VectorXD(MatrixOperations.Multiply(a, b)));
    public static Matrix3D operator *(Matrix3D value, double scalar) => new(value._matrix * scalar);
    public static Matrix3D operator *(double scalar, Matrix3D value) => value * scalar;
    public static Matrix3D operator /(Matrix3D value, double scalar) => new(value._matrix / scalar);
    public static Matrix3D operator +(Matrix3D a, ReadOnlyMatrix3D b) => new(MatrixOperations.Add(a, b));
    public static Matrix3D operator +(ReadOnlyMatrix3D a, Matrix3D b) => new(MatrixOperations.Add(a, b));
    public static Matrix3D operator -(Matrix3D a, ReadOnlyMatrix3D b) => new(MatrixOperations.Subtract(a, b));
    public static Matrix3D operator -(ReadOnlyMatrix3D a, Matrix3D b) => new(MatrixOperations.Subtract(a, b));
    public static Matrix3D operator *(Matrix3D a, ReadOnlyMatrix3D b) => new(MatrixOperations.Multiply(a, b));
    public static Matrix3D operator *(ReadOnlyMatrix3D a, Matrix3D b) => new(MatrixOperations.Multiply(a, b));
    public int Rows => _matrix.Rows;
    public int Columns => _matrix.Columns;
    public ReadOnlyMatrixXD AsReadOnlyMatrix() => _matrix.AsReadOnly();
    public MatrixXD AsMatrix() => _matrix.AsMatrix();
    public ReadOnlyTensorSpan<double> AsReadOnlyTensorSpan() => _matrix.AsReadOnlyTensorSpan();
    public TensorSpan<double> AsTensorSpan() => _matrix.AsTensorSpan();
    public double Norm() => MatrixOperations.Norm(this);
    public double SquaredNorm() => MatrixOperations.SquaredNorm(this);
    public MatrixBorrow Borrow() => _matrix.Borrow();
    public ReadOnlyMatrixBorrow BorrowReadOnly() => _matrix.BorrowReadOnly();
    public void Dispose() => _matrix.Dispose();
    public override string ToString() => _matrix.ToString();
}
