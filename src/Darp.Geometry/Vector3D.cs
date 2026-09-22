using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>Mutable geometry sharing its coefficient storage with derived views.</summary>
public sealed class Vector3D : GeometryObject, IMatrixD, IReadOnlyVector3D
{
    internal Vector3D(MatrixStorage storage, MatrixLayout layout, int offset = 0)
        : base(storage, layout, offset) { }

    private Vector3D(Memory<double> memory, MatrixLayout layout)
        : base(memory, layout) { }

    public static Vector3D FromMatrix(MatrixXD matrix) =>
        new(matrix.Storage, matrix.Layout.Require(3, 1), matrix.Offset);

    public IReadOnlyVector3D AsReadOnly() => new ReadOnlyVector3D(Storage, Layout, Offset);

    public MatrixXD AsMatrix() => ViewMatrix();

    TensorSpanLease IMatrixD.AcquireTensorSpan(out TensorSpan<double> span) => AcquireWritableTensorSpan(out span);

    public Vector3D(double x, double y, double z)
        : base(3, 1)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public static Vector3D Zero => new(0, 0, 0);
    public static Vector3D UnitX => new(1, 0, 0);
    public static Vector3D UnitY => new(0, 1, 0);
    public static Vector3D UnitZ => new(0, 0, 1);

    public static Vector3D CreateFromMemory(Memory<double> memory, int stride = 1) =>
        new(memory, MatrixLayout.Create(3, 1, stride, checked(3 * stride)));

    public int Count => Rows;
    public double X
    {
        get => base[0, 0];
        set => SetValue(0, 0, value);
    }
    public double Y
    {
        get => base[1, 0];
        set => SetValue(1, 0, value);
    }
    public double Z
    {
        get => base[2, 0];
        set => SetValue(2, 0, value);
    }
    public double this[int index]
    {
        get => base[index, 0];
        set => SetValue(index, 0, value);
    }

    public VectorXD AsVector() => new(Storage, Layout, Offset);

    IReadOnlyVectorXD IReadOnlyMatrixD.AsVector() => new ReadOnlyVectorXD(Storage, Layout, Offset);

    public VectorXD Slice(int start, int count) =>
        new(Storage, Layout.Block(start, 0, count, 1), count == 0 ? Offset : checked(Offset + start * RowStride));

    IReadOnlyVectorXD IReadOnlyVectorXD.Slice(int start, int count) =>
        new ReadOnlyVectorXD(
            Storage,
            Layout.Block(start, 0, count, 1),
            count == 0 ? Offset : checked(Offset + start * RowStride)
        );

    public MatrixXD Transposed() => new(Storage, Layout.Transposed(), Offset);

    public static Vector3D operator +(Vector3D a, IReadOnlyVector3D b) => a.Add(b);

    public static Vector3D operator -(Vector3D a, IReadOnlyVector3D b) => a.Subtract(b);

    public static Vector3D operator -(Vector3D value) => value.Scale(-1);

    public static Vector3D operator *(Vector3D value, double scalar) => value.Scale(scalar);

    public static Vector3D operator *(double scalar, Vector3D value) => value * scalar;

    public static Vector3D operator /(Vector3D value, double scalar) => value.Divide(scalar);

    public override string ToString() => $"[{string.Join(", ", this.ToArray())}]";
}
