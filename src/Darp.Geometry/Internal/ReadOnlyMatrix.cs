namespace Darp.Geometry;

internal sealed class ReadOnlyMatrix(MatrixStorage storage, MatrixLayout layout, int offset = 0)
    : GeometryObject(storage, layout, offset);
