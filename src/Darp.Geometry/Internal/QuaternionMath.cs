namespace Darp.Geometry;

internal static class QuaternionMath
{
    public static QuaternionD FromAxisAngle(IReadOnlyVector3D axis, double angle)
    {
        if (!double.IsFinite(angle))
            throw new ArgumentOutOfRangeException(nameof(angle));
        using var view = axis.AsReadOnlyMatrix();
        MatrixShape.RequireSize(view.AsReadOnlyTensorSpan(), 3, 1);
        using var unit = MatrixOperations.Normalized(view);
        double sine = Math.Sin(angle / 2);
        return new(unit[0, 0] * sine, unit[1, 0] * sine, unit[2, 0] * sine, Math.Cos(angle / 2));
    }

    public static QuaternionD FromRotationMatrix(IReadOnlyMatrix3D value)
    {
        using var matrix = value.AsReadOnlyMatrix();
        MatrixShape.RequireSize(matrix.AsReadOnlyTensorSpan(), 3, 3);
        double trace = matrix[0, 0] + matrix[1, 1] + matrix[2, 2];
        QuaternionD result;
        if (trace > 0)
        {
            double s = 2 * Math.Sqrt(trace + 1);
            result = new(
                (matrix[2, 1] - matrix[1, 2]) / s,
                (matrix[0, 2] - matrix[2, 0]) / s,
                (matrix[1, 0] - matrix[0, 1]) / s,
                s / 4
            );
        }
        else if (matrix[0, 0] > matrix[1, 1] && matrix[0, 0] > matrix[2, 2])
        {
            double s = 2 * Math.Sqrt(1 + matrix[0, 0] - matrix[1, 1] - matrix[2, 2]);
            result = new(
                s / 4,
                (matrix[0, 1] + matrix[1, 0]) / s,
                (matrix[0, 2] + matrix[2, 0]) / s,
                (matrix[2, 1] - matrix[1, 2]) / s
            );
        }
        else if (matrix[1, 1] > matrix[2, 2])
        {
            double s = 2 * Math.Sqrt(1 + matrix[1, 1] - matrix[0, 0] - matrix[2, 2]);
            result = new(
                (matrix[0, 1] + matrix[1, 0]) / s,
                s / 4,
                (matrix[1, 2] + matrix[2, 1]) / s,
                (matrix[0, 2] - matrix[2, 0]) / s
            );
        }
        else
        {
            double s = 2 * Math.Sqrt(1 + matrix[2, 2] - matrix[0, 0] - matrix[1, 1]);
            result = new(
                (matrix[0, 2] + matrix[2, 0]) / s,
                (matrix[1, 2] + matrix[2, 1]) / s,
                s / 4,
                (matrix[1, 0] - matrix[0, 1]) / s
            );
        }
        using (result)
            return result.Normalized();
    }

    public static QuaternionD Slerp(IReadOnlyQuaternionD a, IReadOnlyQuaternionD b, double amount)
    {
        if (!double.IsFinite(amount))
            throw new ArgumentOutOfRangeException(nameof(amount));
        using var unitA = a.Normalized();
        using var unitB = b.Normalized();
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
            unitA.W * weightA + unitB.W * weightB
        );
        return result.Normalized();
    }
}
