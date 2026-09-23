namespace Darp.Geometry;

public static partial class GeometryExtensions
{
    public static QuaternionD Clone(this ReadOnlyQuaternionD value) => new(value.X, value.Y, value.Z, value.W);

    public static QuaternionD Normalized(this in ReadOnlyQuaternionD value) =>
        Matrix.Normalized<ReadOnlyQuaternionD, QuaternionD>(value);

    public static double Dot(this in ReadOnlyQuaternionD left, in ReadOnlyQuaternionD right) => Matrix.Dot(left, right);

    public static QuaternionD Slerp(this in ReadOnlyQuaternionD left, in ReadOnlyQuaternionD right, double amount) =>
        QuaternionMath.Slerp(left, right, amount);

    public static QuaternionD Conjugate(this in ReadOnlyQuaternionD value) => new(-value.X, -value.Y, -value.Z, value.W);

    public static QuaternionD Inverse(this in ReadOnlyQuaternionD value)
    {
        double norm = Matrix.Norm(value);
        if (!(norm > 0) || !double.IsFinite(norm))
            throw new InvalidOperationException("A finite nonzero quaternion is required.");
        return new(-value.X / norm / norm, -value.Y / norm / norm, -value.Z / norm / norm, value.W / norm / norm);
    }

    /// <summary>Returns an angle in [0, pi]. Identity uses UnitX as its arbitrary axis.</summary>
    public static (Vector3D Axis, double Angle) ToAxisAngle(this in ReadOnlyQuaternionD value)
    {
        var q = value.Normalized();
        double sign = q.W < 0 ? -1 : 1;
        double sine = Math.Sqrt(q.X * q.X + q.Y * q.Y + q.Z * q.Z);
        if (sine < 1e-15)
            return (Vector3D.UnitX, 0);
        return (new(sign * q.X / sine, sign * q.Y / sine, sign * q.Z / sine), 2 * Math.Atan2(sine, sign * q.W));
    }

    public static Vector3D Rotate(in ReadOnlyQuaternionD value, in ReadOnlyVector3D vector)
    {
        var q = value.Normalized();
        double x = vector.X,
            y = vector.Y,
            z = vector.Z;
        double tx = 2 * (q.Y * z - q.Z * y);
        double ty = 2 * (q.Z * x - q.X * z);
        double tz = 2 * (q.X * y - q.Y * x);
        return new(
            x + q.W * tx + q.Y * tz - q.Z * ty,
            y + q.W * ty + q.Z * tx - q.X * tz,
            z + q.W * tz + q.X * ty - q.Y * tx
        );
    }

    public static Matrix3D ToRotationMatrix(this in ReadOnlyQuaternionD value)
    {
        var q = value.Normalized();
        double x = q.X,
            y = q.Y,
            z = q.Z,
            w = q.W;
        var result = new Matrix3D();
        result[0, 0] = 1 - 2 * (y * y + z * z);
        result[0, 1] = 2 * (x * y - z * w);
        result[0, 2] = 2 * (x * z + y * w);
        result[1, 0] = 2 * (x * y + z * w);
        result[1, 1] = 1 - 2 * (x * x + z * z);
        result[1, 2] = 2 * (y * z - x * w);
        result[2, 0] = 2 * (x * z - y * w);
        result[2, 1] = 2 * (y * z + x * w);
        result[2, 2] = 1 - 2 * (x * x + y * y);
        return result;
    }

    /// <summary>Hamilton product: for rotations, apply right first, then left.</summary>
    public static QuaternionD Multiply(this in ReadOnlyQuaternionD left, in ReadOnlyQuaternionD right) =>
        new(
            left.W * right.X + left.X * right.W + left.Y * right.Z - left.Z * right.Y,
            left.W * right.Y - left.X * right.Z + left.Y * right.W + left.Z * right.X,
            left.W * right.Z + left.X * right.Y - left.Y * right.X + left.Z * right.W,
            left.W * right.W - left.X * right.X - left.Y * right.Y - left.Z * right.Z
        );
}
