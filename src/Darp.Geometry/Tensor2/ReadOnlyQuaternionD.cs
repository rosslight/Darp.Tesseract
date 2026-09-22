using System.Numerics.Tensors;

namespace Darp.Geometry.Tensor2;

/// <summary>A read-only X,Y,Z,W quaternion view. Rotation operations normalize locally.</summary>
public readonly struct ReadOnlyQuaternionD : IReadOnlyMatrixD
{
    private readonly ReadOnlyVectorXD _coefficients;
    internal ReadOnlyQuaternionD(ReadOnlyVectorXD coefficients)
    {
        if (coefficients.Count != 4) throw new ArgumentException("Expected four coefficients.", nameof(coefficients));
        _coefficients = coefficients;
    }
    public static ReadOnlyQuaternionD Map(ReadOnlyMemory<double> memory, int stride = 1) => new(ReadOnlyVectorXD.Map(memory, 4, stride));
    public double X => _coefficients[0];
    public double Y => _coefficients[1];
    public double Z => _coefficients[2];
    public double W => _coefficients[3];
    public ReadOnlyVectorXD Coefficients => _coefficients;
    public QuaternionD Clone() => new(_coefficients.Clone());
    public double Norm() => _coefficients.Norm();
    public double SquaredNorm() => _coefficients.SquaredNorm();
    public double Dot(ReadOnlyQuaternionD other) => _coefficients.Dot(other._coefficients);
    public QuaternionD Normalized() => new(_coefficients.Normalized());
    public QuaternionD Conjugate() => new(-X, -Y, -Z, W);
    public QuaternionD Inverse()
    {
        double norm = Norm();
        if (!(norm > 0) || !double.IsFinite(norm))
            throw new InvalidOperationException("A finite nonzero quaternion is required.");
        // Divide twice to avoid squaring a large norm.
        return new QuaternionD(-X / norm / norm, -Y / norm / norm, -Z / norm / norm, W / norm / norm);
    }

    /// <summary>Constructs a unit quaternion. The angle is in radians; the axis must be finite and nonzero.</summary>
    public static QuaternionD FromAxisAngle(ReadOnlyVector3D axis, double angle)
    {
        if (!double.IsFinite(angle)) throw new ArgumentOutOfRangeException(nameof(angle));
        var unit = axis.Normalized();
        double sine = Math.Sin(angle / 2);
        return new(unit.X * sine, unit.Y * sine, unit.Z * sine, Math.Cos(angle / 2));
    }

    /// <summary>Returns an angle in [0, pi]. Identity uses UnitX as its arbitrary axis.</summary>
    public (Vector3D Axis, double Angle) ToAxisAngle()
    {
        var q = Normalized();
        if (q.W < 0) q = -q;
        double sine = Math.Sqrt(q.X * q.X + q.Y * q.Y + q.Z * q.Z);
        if (sine < 1e-15) return (Vector3D.UnitX, 0);
        return (new Vector3D(q.X / sine, q.Y / sine, q.Z / sine), 2 * Math.Atan2(sine, q.W));
    }

    public Vector3D Rotate(ReadOnlyVector3D vector)
    {
        var q = Normalized();
        var imaginary = new Vector3D(q.X, q.Y, q.Z);
        var twiceCross = 2 * imaginary.Cross(vector);
        return vector + q.W * twiceCross + imaginary.Cross(twiceCross);
    }

    public Matrix3D ToRotationMatrix()
    {
        var q = Normalized();
        double x = q.X, y = q.Y, z = q.Z, w = q.W;
        return Matrix3D.FromArray(new double[,]
        {
            { 1 - 2 * (y * y + z * z), 2 * (x * y - z * w), 2 * (x * z + y * w) },
            { 2 * (x * y + z * w), 1 - 2 * (x * x + z * z), 2 * (y * z - x * w) },
            { 2 * (x * z - y * w), 2 * (y * z + x * w), 1 - 2 * (x * x + y * y) }
        });
    }

    /// <summary>Converts a proper orthonormal rotation matrix. The caller guarantees the rotation invariant.</summary>
    public static QuaternionD FromRotationMatrix(ReadOnlyMatrix3D matrix)
    {
        double trace = matrix[0, 0] + matrix[1, 1] + matrix[2, 2];
        QuaternionD result;
        if (trace > 0)
        {
            double s = 2 * Math.Sqrt(trace + 1);
            result = new((matrix[2, 1] - matrix[1, 2]) / s,
                (matrix[0, 2] - matrix[2, 0]) / s, (matrix[1, 0] - matrix[0, 1]) / s, s / 4);
        }
        else if (matrix[0, 0] > matrix[1, 1] && matrix[0, 0] > matrix[2, 2])
        {
            double s = 2 * Math.Sqrt(1 + matrix[0, 0] - matrix[1, 1] - matrix[2, 2]);
            result = new(s / 4, (matrix[0, 1] + matrix[1, 0]) / s,
                (matrix[0, 2] + matrix[2, 0]) / s, (matrix[2, 1] - matrix[1, 2]) / s);
        }
        else if (matrix[1, 1] > matrix[2, 2])
        {
            double s = 2 * Math.Sqrt(1 + matrix[1, 1] - matrix[0, 0] - matrix[2, 2]);
            result = new((matrix[0, 1] + matrix[1, 0]) / s, s / 4,
                (matrix[1, 2] + matrix[2, 1]) / s, (matrix[0, 2] - matrix[2, 0]) / s);
        }
        else
        {
            double s = 2 * Math.Sqrt(1 + matrix[2, 2] - matrix[0, 0] - matrix[1, 1]);
            result = new((matrix[0, 2] + matrix[2, 0]) / s,
                (matrix[1, 2] + matrix[2, 1]) / s, s / 4, (matrix[1, 0] - matrix[0, 1]) / s);
        }
        return result.Normalized();
    }

    /// <summary>Shortest-path spherical interpolation; amount may extrapolate beyond [0,1].</summary>
    public static QuaternionD Slerp(ReadOnlyQuaternionD a, ReadOnlyQuaternionD b, double amount)
    {
        a = a.Normalized();
        b = b.Normalized();
        double dot = a.Dot(b);
        if (dot < 0)
        {
            b = -b;
            dot = -dot;
        }
        dot = Math.Clamp(dot, -1, 1);
        if (dot > 0.9995)
            return new QuaternionD(VectorXD.Lerp(a._coefficients, b._coefficients, amount)).Normalized();
        double angle = Math.Acos(dot);
        double denominator = Math.Sin(angle);
        return new QuaternionD(
            a._coefficients * (Math.Sin((1 - amount) * angle) / denominator)
            + b._coefficients * (Math.Sin(amount * angle) / denominator)).Normalized();
    }

    /// <summary>Hamilton product: for rotations, apply b first, then a.</summary>
    public static QuaternionD operator *(ReadOnlyQuaternionD a, ReadOnlyQuaternionD b) => new(
        a.W * b.X + a.X * b.W + a.Y * b.Z - a.Z * b.Y,
        a.W * b.Y - a.X * b.Z + a.Y * b.W + a.Z * b.X,
        a.W * b.Z + a.X * b.Y - a.Y * b.X + a.Z * b.W,
        a.W * b.W - a.X * b.X - a.Y * b.Y - a.Z * b.Z);
    public static Vector3D operator *(ReadOnlyQuaternionD rotation, ReadOnlyVector3D vector) => rotation.Rotate(vector);
    public static QuaternionD operator -(ReadOnlyQuaternionD value) => new(-value._coefficients);
    public ReadOnlyMatrixXD AsReadOnlyMatrix() => _coefficients.AsReadOnlyMatrix();
    public ReadOnlyTensorSpan<double> AsReadOnlyTensorSpan() => _coefficients.AsReadOnlyTensorSpan();
    public override string ToString() => $"(X={X}, Y={Y}, Z={Z}, W={W})";
}
