namespace Darp.Geometry;

/// <summary>Editable examples of retained views, tensor access and geometry operations.</summary>
public static class Playground
{
    public static void Run(TextWriter? output = null)
    {
        output ??= Console.Out;
        using var vector = new Vector3D(1, 2, 3);
        using var readable = vector.AsReadOnly();
        using var snapshot = readable.Clone();
        using var column = vector.AsMatrix();
        using var row = vector.Transposed();
        using var outer = column * row;
        vector.Z = 7;
        output.WriteLine($"Live read-only view: {readable}; independent clone: {snapshot}");
        output.WriteLine($"Outer product:\n{outer}");

        using var access = vector.Borrow();
        vector.Dispose();
        var coefficients = access.AsTensorSpan();
        coefficients[0, 0] = 4;
        output.WriteLine($"Retained view after parent disposal: {readable}");

        using var axis = Vector3D.UnitZ;
        using var rotation = QuaternionD.FromAxisAngle(axis, Math.PI / 2);
        using var translation = new Vector3D(1, 2, 3);
        using var transform = new Isometry3D(rotation, translation);
        using var pose = transform.AsReadOnly();
        using var point = Vector3D.UnitX;
        using var world = pose * point;
        using var inverse = pose.Inverse();
        using var roundTrip = inverse * world;
        output.WriteLine($"World point: {world}; back in tool: {roundTrip}");

        double[] rowMajor = [1, 2, 3, 4, 5, 6];
        using var matrix = MatrixXD.Map(rowMajor, 2, 3, columnStride: 1, rowStride: 3);
        using var transpose = matrix.Transposed();
        output.WriteLine($"Mapped row-major matrix:\n{matrix}\nTranspose view:\n{transpose}");
    }
}
