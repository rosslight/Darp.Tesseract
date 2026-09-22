using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>A read-only X,Y,Z,W quaternion view. Rotation operations normalize locally.</summary>
public sealed class ReadOnlyQuaternionD : IReadOnlyMatrixD
{
    private readonly ReadOnlyVectorXD _coefficients;
    internal ReadOnlyQuaternionD(ReadOnlyVectorXD coefficients)
    {
        if (coefficients.Count != 4)
        {
            coefficients.Dispose();
            throw new ArgumentException("Expected four coefficients.", nameof(coefficients));
        }
        _coefficients = coefficients;
    }
    public static ReadOnlyQuaternionD Map(ReadOnlyMemory<double> memory, int stride = 1) => new(ReadOnlyVectorXD.Map(memory, 4, stride));
    public static ReadOnlyQuaternionD FromMatrix(ReadOnlyMatrixXD matrix) => new(matrix.AsVector());
    public double X => _coefficients[0];
    public double Y => _coefficients[1];
    public double Z => _coefficients[2];
    public double W => _coefficients[3];
    public ReadOnlyVectorXD Coefficients => new(_coefficients.AsReadOnlyMatrix());
    public QuaternionD Clone() => new(_coefficients.Clone());
    public double Norm() => _coefficients.Norm();
    public double SquaredNorm() => _coefficients.SquaredNorm();
    public double Dot(IReadOnlyMatrixD other) => _coefficients.Dot(other);
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
    public static QuaternionD FromAxisAngle(IReadOnlyMatrixD axis, double angle)
    {
        if (!double.IsFinite(angle)) throw new ArgumentOutOfRangeException(nameof(angle));
        using var view = axis.AsReadOnlyMatrix();
        MatrixShape.RequireSize(view.AsReadOnlyTensorSpan(), 3, 1);
        using var unit = MatrixOperations.Normalized(view);
        double sine = Math.Sin(angle / 2);
        return new(unit[0, 0] * sine, unit[1, 0] * sine, unit[2, 0] * sine, Math.Cos(angle / 2));
    }

    /// <summary>Returns an angle in [0, pi]. Identity uses UnitX as its arbitrary axis.</summary>
    public (Vector3D Axis, double Angle) ToAxisAngle()
    {
        using var q = Normalized();
        double sign = q.W < 0 ? -1 : 1;
        double sine = Math.Sqrt(q.X * q.X + q.Y * q.Y + q.Z * q.Z);
        if (sine < 1e-15) return (Vector3D.UnitX, 0);
        return (new Vector3D(sign * q.X / sine, sign * q.Y / sine, sign * q.Z / sine), 2 * Math.Atan2(sine, sign * q.W));
    }

    public Vector3D Rotate(IReadOnlyMatrixD vector)
    {
        using var v = vector.AsReadOnlyMatrix();
        MatrixShape.RequireSize(v.AsReadOnlyTensorSpan(), 3, 1);
        using var q = Normalized();
        double x = v[0, 0], y = v[1, 0], z = v[2, 0];
        double tx = 2 * (q.Y * z - q.Z * y);
        double ty = 2 * (q.Z * x - q.X * z);
        double tz = 2 * (q.X * y - q.Y * x);
        return new(x + q.W * tx + q.Y * tz - q.Z * ty,
            y + q.W * ty + q.Z * tx - q.X * tz,
            z + q.W * tz + q.X * ty - q.Y * tx);
    }

    public Matrix3D ToRotationMatrix()
    {
        using var q = Normalized();
        double x = q.X, y = q.Y, z = q.Z, w = q.W;
        return Matrix3D.FromArray(new double[,]
        {
            { 1 - 2 * (y * y + z * z), 2 * (x * y - z * w), 2 * (x * z + y * w) },
            { 2 * (x * y + z * w), 1 - 2 * (x * x + z * z), 2 * (y * z - x * w) },
            { 2 * (x * z - y * w), 2 * (y * z + x * w), 1 - 2 * (x * x + y * y) }
        });
    }

    /// <summary>Converts a proper orthonormal rotation matrix. The caller guarantees the rotation invariant.</summary>
    public static QuaternionD FromRotationMatrix(IReadOnlyMatrixD value)
    {
        using var matrix = value.AsReadOnlyMatrix();
        MatrixShape.RequireSize(matrix.AsReadOnlyTensorSpan(), 3, 3);
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
        using (result) return result.Normalized();
    }

    /// <summary>Shortest-path spherical interpolation; amount may extrapolate beyond [0,1].</summary>
    public static QuaternionD Slerp(IReadOnlyMatrixD a, IReadOnlyMatrixD b, double amount)
    {
        if (!double.IsFinite(amount)) throw new ArgumentOutOfRangeException(nameof(amount));
        using var left = new ReadOnlyQuaternionD(new ReadOnlyVectorXD(a.AsReadOnlyMatrix()));
        using var right = new ReadOnlyQuaternionD(new ReadOnlyVectorXD(b.AsReadOnlyMatrix()));
        using var unitA = left.Normalized();
        using var unitB = right.Normalized();
        double dot = unitA.Dot(unitB);
        double sign = dot < 0 ? -1 : 1;
        dot = Math.Clamp(Math.Abs(dot), 0, 1);
        double weightA = 1 - amount;
        double weightB = amount;
        if (dot <= 0.9995)
        {
            double angle = Math.Acos(dot);
            double denominator = Math.Sin(angle);
            weightA = Math.Sin((1 - amount) * angle) / denominator;
            weightB = Math.Sin(amount * angle) / denominator;
        }
        weightB *= sign;
        using var result = new QuaternionD(
            unitA.X * weightA + unitB.X * weightB,
            unitA.Y * weightA + unitB.Y * weightB,
            unitA.Z * weightA + unitB.Z * weightB,
            unitA.W * weightA + unitB.W * weightB);
        return result.Normalized();
    }

    /// <summary>Hamilton product: for rotations, apply b first, then a.</summary>
    public static QuaternionD operator *(ReadOnlyQuaternionD a, ReadOnlyQuaternionD b) => new(
        a.W * b.X + a.X * b.W + a.Y * b.Z - a.Z * b.Y,
        a.W * b.Y - a.X * b.Z + a.Y * b.W + a.Z * b.X,
        a.W * b.Z + a.X * b.Y - a.Y * b.X + a.Z * b.W,
        a.W * b.W - a.X * b.X - a.Y * b.Y - a.Z * b.Z);
    public static QuaternionD operator *(ReadOnlyQuaternionD a, QuaternionD b)
    {
        using var right = b.AsReadOnly();
        return a * right;
    }
    public static Vector3D operator *(ReadOnlyQuaternionD rotation, IReadOnlyMatrixD vector) => rotation.Rotate(vector);
    public static QuaternionD operator -(ReadOnlyQuaternionD value) => new(-value._coefficients);
    public ReadOnlyMatrixXD AsReadOnlyMatrix() => _coefficients.AsReadOnlyMatrix();
    public ReadOnlyTensorSpan<double> AsReadOnlyTensorSpan() => _coefficients.AsReadOnlyTensorSpan();
    public ReadOnlyMatrixBorrow Borrow() => _coefficients.BorrowReadOnly();
    public ReadOnlyMatrixBorrow BorrowReadOnly() => _coefficients.BorrowReadOnly();
    public void Dispose() => _coefficients.Dispose();
    public override string ToString() => $"(X={X}, Y={Y}, Z={Z}, W={W})";
}
