using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>A mutable X,Y,Z,W quaternion owning a reference to its coefficient storage.</summary>
public sealed class QuaternionD : IMatrixD
{
    private readonly VectorXD _coefficients;
    internal QuaternionD(VectorXD coefficients)
    {
        if (coefficients.Count != 4)
        {
            coefficients.Dispose();
            throw new ArgumentException("Expected four coefficients.", nameof(coefficients));
        }
        _coefficients = coefficients;
    }
    public QuaternionD(double x, double y, double z, double w) : this(new VectorXD(x, y, z, w)) { }
    public static QuaternionD Identity => new(0, 0, 0, 1);
    public static QuaternionD Map(Memory<double> memory, int stride = 1) => new(VectorXD.Map(memory, 4, stride));
    public static QuaternionD FromMatrix(MatrixXD matrix) => new(matrix.AsVector());
    public double X { get => _coefficients[0]; set => _coefficients.SetValue(0, value); }
    public double Y { get => _coefficients[1]; set => _coefficients.SetValue(1, value); }
    public double Z { get => _coefficients[2]; set => _coefficients.SetValue(2, value); }
    public double W { get => _coefficients[3]; set => _coefficients.SetValue(3, value); }
    public VectorXD Coefficients => new(_coefficients.AsMatrix());
    public ReadOnlyQuaternionD AsReadOnly() => new(_coefficients.AsReadOnly());
    public QuaternionD Clone() => new(_coefficients.Clone());
    public double Norm() => _coefficients.Norm();
    public double SquaredNorm() => _coefficients.SquaredNorm();
    public double Dot(IReadOnlyMatrixD other) => _coefficients.Dot(other);
    public QuaternionD Normalized() => new(_coefficients.Normalized());
    public QuaternionD Conjugate() => new(-X, -Y, -Z, W);
    public QuaternionD Inverse() { using var view = AsReadOnly(); return view.Inverse(); }
    public static QuaternionD FromAxisAngle(IReadOnlyMatrixD axis, double angle) => ReadOnlyQuaternionD.FromAxisAngle(axis, angle);
    public (Vector3D Axis, double Angle) ToAxisAngle() { using var view = AsReadOnly(); return view.ToAxisAngle(); }
    public Vector3D Rotate(IReadOnlyMatrixD vector) { using var view = AsReadOnly(); return view.Rotate(vector); }
    public Matrix3D ToRotationMatrix() { using var view = AsReadOnly(); return view.ToRotationMatrix(); }
    public static QuaternionD FromRotationMatrix(IReadOnlyMatrixD matrix) => ReadOnlyQuaternionD.FromRotationMatrix(matrix);
    public static QuaternionD Slerp(IReadOnlyMatrixD a, IReadOnlyMatrixD b, double amount) => ReadOnlyQuaternionD.Slerp(a, b, amount);
    public static QuaternionD operator *(QuaternionD a, QuaternionD b)
    {
        using var left = a.AsReadOnly();
        using var right = b.AsReadOnly();
        return left * right;
    }
    public static QuaternionD operator *(QuaternionD a, ReadOnlyQuaternionD b)
    {
        using var left = a.AsReadOnly();
        return left * b;
    }
    public static Vector3D operator *(QuaternionD a, IReadOnlyMatrixD b) => a.Rotate(b);
    public static QuaternionD operator -(QuaternionD value) => new(-value._coefficients);
    public ReadOnlyMatrixXD AsReadOnlyMatrix() => _coefficients.AsReadOnlyMatrix();
    public MatrixXD AsMatrix() => _coefficients.AsMatrix();
    public ReadOnlyTensorSpan<double> AsReadOnlyTensorSpan() => _coefficients.AsReadOnlyTensorSpan();
    public TensorSpan<double> AsTensorSpan() => _coefficients.AsTensorSpan();
    public MatrixBorrow Borrow() => _coefficients.Borrow();
    public ReadOnlyMatrixBorrow BorrowReadOnly() => _coefficients.BorrowReadOnly();
    public void Dispose() => _coefficients.Dispose();
    public override string ToString() => $"(X={X}, Y={Y}, Z={Z}, W={W})";
}
