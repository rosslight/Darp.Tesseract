namespace Darp.Geometry;

internal sealed class ReadOnlyVectorXD : GeometryObject, IReadOnlyVectorXD
{
    internal ReadOnlyVectorXD(MatrixStorage storage, MatrixLayout layout, int offset = 0)
        : base(storage, layout, offset) { }

    public int Count => Rows;
    public double this[int index] => base[index, 0];

    public IReadOnlyVectorXD Slice(int start, int count) =>
        new ReadOnlyVectorXD(
            Storage,
            Layout.Block(start, 0, count, 1),
            count == 0 ? Offset : checked(Offset + start * RowStride)
        );

    public override string ToString() => $"[{string.Join(", ", this.ToArray())}]";
}
