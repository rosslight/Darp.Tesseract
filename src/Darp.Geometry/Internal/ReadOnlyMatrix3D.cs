namespace Darp.Geometry;

internal sealed class ReadOnlyMatrix3D : GeometryObject, IReadOnlyMatrix3D
{
    internal ReadOnlyMatrix3D(MatrixStorage storage, MatrixLayout layout, int offset = 0)
        : base(storage, layout, offset) { }

    public IReadOnlyMatrixD Row(int row) =>
        new ReadOnlyMatrix(Storage, Layout.Block(row, 0, 1, Columns), checked(Offset + row * RowStride));

    public IReadOnlyVector3D Column(int column) =>
        new ReadOnlyVector3D(Storage, Layout.Block(0, column, 3, 1), checked(Offset + column * ColumnStride));

    public IReadOnlyMatrix3D Transposed() => new ReadOnlyMatrix3D(Storage, Layout.Transposed(), Offset);
}
