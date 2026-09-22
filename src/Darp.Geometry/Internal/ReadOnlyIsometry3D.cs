namespace Darp.Geometry;

internal sealed class ReadOnlyIsometry3D : GeometryObject, IReadOnlyIsometry3D
{
    internal ReadOnlyIsometry3D(MatrixStorage storage, MatrixLayout layout, int offset = 0)
        : base(storage, layout, offset) { }

    public IReadOnlyVector3D Translation =>
        new ReadOnlyVector3D(Storage, Layout.Block(0, 3, 3, 1), checked(Offset + 3 * ColumnStride));
    public IReadOnlyMatrix3D RotationMatrix => new ReadOnlyMatrix3D(Storage, Layout.Block(0, 0, 3, 3), Offset);

    public QuaternionD Rotation
    {
        get
        {
            var matrix = RotationMatrix;
            return matrix.ToQuaternion();
        }
    }
}
