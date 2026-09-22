namespace Darp.Geometry;

internal sealed class ReadOnlyVector3D : GeometryObject, IReadOnlyVector3D
{
    internal ReadOnlyVector3D(MatrixStorage storage, MatrixLayout layout, int offset = 0)
        : base(storage, layout, offset) { }

    public int Count => Rows;
    public double this[int index] => base[index, 0];

    public IReadOnlyVectorXD Slice(int start, int count) =>
        new ReadOnlyVectorXD(
            Storage,
            Layout.Block(start, 0, count, 1),
            count == 0 ? Offset : checked(Offset + start * RowStride)
        );

    public double X => base[0, 0];
    public double Y => base[1, 0];
    public double Z => base[2, 0];

    public IReadOnlyVectorXD AsVector() => new ReadOnlyVectorXD(Storage, Layout, Offset);

    public override string ToString() => $"[{string.Join(", ", GeometryExtensions.ToArray(this))}]";
}
