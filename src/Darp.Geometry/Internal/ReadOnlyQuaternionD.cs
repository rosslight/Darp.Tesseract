namespace Darp.Geometry;

internal sealed class ReadOnlyQuaternionD : GeometryObject, IReadOnlyQuaternionD
{
    internal ReadOnlyQuaternionD(MatrixStorage storage, MatrixLayout layout, int offset = 0)
        : base(storage, layout, offset) { }

    public double X => base[0, 0];
    public double Y => base[1, 0];
    public double Z => base[2, 0];
    public double W => base[3, 0];
    public IReadOnlyVectorXD Coefficients => new ReadOnlyVectorXD(Storage, Layout, Offset);

    public override string ToString() => $"(X={X}, Y={Y}, Z={Z}, W={W})";
}
