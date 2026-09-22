using System.Numerics.Tensors;

namespace Darp.Geometry.Tensor2;

/// <summary>A mutable 3 by 1 matrix specialization with named components.</summary>
public readonly struct Vector3D : IMatrixD
{
    private readonly VectorXD _vector;
    internal Vector3D(VectorXD vector)
    {
        if (vector.Count != 3) throw new ArgumentException("Expected three coefficients.", nameof(vector));
        _vector = vector;
    }
    public Vector3D(double x, double y, double z) : this(new VectorXD(x, y, z)) { }
    public static Vector3D Zero => new(0, 0, 0);
    public static Vector3D UnitX => new(1, 0, 0);
    public static Vector3D UnitY => new(0, 1, 0);
    public static Vector3D UnitZ => new(0, 0, 1);
    public ReadOnlyVector3D AsReadOnly() => new(_vector.AsReadOnly());
    public static implicit operator ReadOnlyVector3D(Vector3D value) => value.AsReadOnly();
    public static Vector3D Map(Memory<double> memory, int stride = 1) => new(VectorXD.Map(memory, 3, stride));
    public static Vector3D FromMatrix(MatrixXD matrix) => new(matrix.AsVector());
    public double X { get => _vector[0]; set => _vector.SetValue(0, value); }
    public double Y { get => _vector[1]; set => _vector.SetValue(1, value); }
    public double Z { get => _vector[2]; set => _vector.SetValue(2, value); }
    public double this[int index] { get => _vector[index]; set => _vector.SetValue(index, value); }
    public VectorXD AsVector() => _vector;
    public MatrixXD AsMatrix() => _vector.AsMatrix();
    public MatrixXD Transposed() => _vector.Transposed();
    public Vector3D Clone() => new(_vector.Clone());
    public double[] ToArray() => _vector.ToArray();
    public Vector3D Normalized() => new(new VectorXD(MatrixOperations.Normalized(this)));
    public static Vector3D Lerp(ReadOnlyVector3D a, ReadOnlyVector3D b, double amount) => new(new VectorXD(MatrixOperations.Lerp(a, b, amount)));
    public static Vector3D operator +(Vector3D a, Vector3D b) => new(a._vector + b._vector);
    public static Vector3D operator -(Vector3D a, Vector3D b) => new(a._vector - b._vector);
    public static Vector3D operator -(Vector3D value) => new(-value._vector);
    public static Vector3D operator *(Vector3D value, double scalar) => new(value._vector * scalar);
    public static Vector3D operator *(double scalar, Vector3D value) => value * scalar;
    public static Vector3D operator /(Vector3D value, double scalar) => new(value._vector / scalar);
    public int Rows => _vector.AsMatrix().Rows;
    public int Columns => _vector.AsMatrix().Columns;
    public int Count => _vector.Count;
    public ReadOnlyMatrixXD AsReadOnlyMatrix() => _vector.AsReadOnlyMatrix();
    public ReadOnlyTensorSpan<double> AsReadOnlyTensorSpan() => _vector.AsReadOnlyTensorSpan();
    public TensorSpan<double> AsTensorSpan() => _vector.AsTensorSpan();
    public double Norm() => MatrixOperations.Norm(this);
    public double SquaredNorm() => MatrixOperations.SquaredNorm(this);
    public double Dot<TOther>(TOther other) where TOther : IReadOnlyMatrixD => MatrixOperations.Dot(this, other);
    public Vector3D Cross<TOther>(TOther other) where TOther : IReadOnlyMatrixD => MatrixOperations.Cross(this, other);
    public override string ToString() => _vector.ToString();
}
