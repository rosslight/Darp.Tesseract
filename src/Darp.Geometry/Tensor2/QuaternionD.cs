using System.Numerics.Tensors;

namespace Darp.Geometry.Tensor2;

/// <summary>A mutable X,Y,Z,W quaternion descriptor. Construction preserves coefficients.</summary>
public readonly struct QuaternionD : IMatrixD
{
    private readonly VectorXD _coefficients;
    internal QuaternionD(VectorXD coefficients)
    {
        if (coefficients.Count != 4) throw new ArgumentException("Expected four coefficients.", nameof(coefficients));
        _coefficients = coefficients;
    }
    public QuaternionD(double x, double y, double z, double w) : this(new VectorXD(x, y, z, w)) { }
    public static QuaternionD Identity => new(0, 0, 0, 1);
    public static QuaternionD Map(Memory<double> memory, int stride = 1) => new(VectorXD.Map(memory, 4, stride));
    public double X { get => _coefficients[0]; set => _coefficients.SetValue(0, value); }
    public double Y { get => _coefficients[1]; set => _coefficients.SetValue(1, value); }
    public double Z { get => _coefficients[2]; set => _coefficients.SetValue(2, value); }
    public double W { get => _coefficients[3]; set => _coefficients.SetValue(3, value); }
    public VectorXD Coefficients => _coefficients;
    public ReadOnlyQuaternionD AsReadOnly() => new(_coefficients.AsReadOnly());
    public static implicit operator ReadOnlyQuaternionD(QuaternionD value) => value.AsReadOnly();
    public QuaternionD Clone() => AsReadOnly().Clone();
    public double Norm() => AsReadOnly().Norm();
    public double SquaredNorm() => AsReadOnly().SquaredNorm();
    public double Dot(ReadOnlyQuaternionD other) => AsReadOnly().Dot(other);
    public QuaternionD Normalized() => AsReadOnly().Normalized();
    public QuaternionD Conjugate() => AsReadOnly().Conjugate();
    public QuaternionD Inverse() => AsReadOnly().Inverse();
    public static QuaternionD FromAxisAngle(ReadOnlyVector3D axis, double angle) => ReadOnlyQuaternionD.FromAxisAngle(axis, angle);
    public (Vector3D Axis, double Angle) ToAxisAngle() => AsReadOnly().ToAxisAngle();
    public Vector3D Rotate(ReadOnlyVector3D vector) => AsReadOnly().Rotate(vector);
    public Matrix3D ToRotationMatrix() => AsReadOnly().ToRotationMatrix();
    public static QuaternionD FromRotationMatrix(ReadOnlyMatrix3D matrix) => ReadOnlyQuaternionD.FromRotationMatrix(matrix);
    public static QuaternionD Slerp(ReadOnlyQuaternionD a, ReadOnlyQuaternionD b, double amount) => ReadOnlyQuaternionD.Slerp(a, b, amount);
    public static QuaternionD operator *(QuaternionD a, QuaternionD b) => a.AsReadOnly() * b.AsReadOnly();
    public static Vector3D operator *(QuaternionD a, ReadOnlyVector3D b) => a.Rotate(b);
    public static QuaternionD operator -(QuaternionD value) => -value.AsReadOnly();
    public ReadOnlyMatrixXD AsReadOnlyMatrix() => _coefficients.AsReadOnlyMatrix();
    public MatrixXD AsMatrix() => _coefficients.AsMatrix();
    public ReadOnlyTensorSpan<double> AsReadOnlyTensorSpan() => _coefficients.AsReadOnlyTensorSpan();
    public TensorSpan<double> AsTensorSpan() => _coefficients.AsTensorSpan();
    public override string ToString() => AsReadOnly().ToString();
}
