namespace Darp.Geometry;

public static partial class GeometryExtensions
{
    public static Isometry3D Clone(this in ReadOnlyIsometry3D value) => Isometry3D.FromMatrix(value);

    public static Vector3D TransformPoint(this in ReadOnlyIsometry3D transform, in ReadOnlyVector3D point) =>
        Transform(transform, point, true);

    public static Vector3D TransformDirection(this in ReadOnlyIsometry3D transform, in ReadOnlyVector3D direction) =>
        Transform(transform, direction, false);

    private static Vector3D Transform(in ReadOnlyIsometry3D transform, in ReadOnlyVector3D vector, bool translate)
    {
        double x = vector.X,
            y = vector.Y,
            z = vector.Z;
        return new(
            transform[0, 0] * x + transform[0, 1] * y + transform[0, 2] * z + (translate ? transform[0, 3] : 0),
            transform[1, 0] * x + transform[1, 1] * y + transform[1, 2] * z + (translate ? transform[1, 3] : 0),
            transform[2, 0] * x + transform[2, 1] * y + transform[2, 2] * z + (translate ? transform[2, 3] : 0)
        );
    }

    public static Isometry3D Inverse(this in ReadOnlyIsometry3D transform)
    {
        var rotation = transform.RotationMatrix;
        var translation = transform.Translation;
        var inverseTranslation = new Vector3D(
            -(rotation[0, 0] * translation.X + rotation[1, 0] * translation.Y + rotation[2, 0] * translation.Z),
            -(rotation[0, 1] * translation.X + rotation[1, 1] * translation.Y + rotation[2, 1] * translation.Z),
            -(rotation[0, 2] * translation.X + rotation[1, 2] * translation.Y + rotation[2, 2] * translation.Z)
        );
        var result = new Isometry3D();
        for (int r = 0; r < 3; r++)
        for (int c = 0; c < 3; c++)
            result[r, c] = rotation[c, r];
        result.SetTranslation(inverseTranslation);
        return result;
    }

    /// <summary>Applies right first, then left. The result has independent storage.</summary>
    public static Isometry3D Multiply(this in ReadOnlyIsometry3D left, in ReadOnlyIsometry3D right)
    {
        using var leftLease = MatrixMarshal.GetReadOnlyTensorSpan(in left, out var leftSpan);
        using var rightLease = MatrixMarshal.GetReadOnlyTensorSpan(in right, out var rightSpan);
        return new Isometry3D(TensorKernels.Multiply(leftSpan, rightSpan));
    }
}
