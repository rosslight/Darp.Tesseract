namespace Darp.Geometry;

public static partial class GeometryExtensions
{
    public static Vector3D TransformPoint(this IReadOnlyIsometry3D transform, IReadOnlyVector3D point) =>
        Transform(transform, point, true);

    public static Vector3D TransformDirection(this IReadOnlyIsometry3D transform, IReadOnlyVector3D direction) =>
        Transform(transform, direction, false);

    private static Vector3D Transform(IReadOnlyIsometry3D transform, IReadOnlyVector3D vector, bool translate)
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

    public static Isometry3D Inverse(this IReadOnlyIsometry3D transform)
    {
        using var originalRotation = transform.RotationMatrix;
        using var rotation = originalRotation.Transposed();
        using var translation = transform.Translation;
        using var rotated = rotation.Multiply(translation);
        using var inverseTranslation = -rotated;
        var result = new Isometry3D();
        result.SetRotationMatrix(rotation);
        result.SetTranslation(inverseTranslation);
        return result;
    }

    /// <summary>Applies right first, then left. The result has independent storage.</summary>
    public static Isometry3D Multiply(this IReadOnlyIsometry3D left, IReadOnlyIsometry3D right) =>
        Isometry3D.FromOwnedMatrix(MatrixOperations.Multiply(left, right));
}
