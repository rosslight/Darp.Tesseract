using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>Mutable geometry owning one reference to its coefficient storage.</summary>
public sealed class VectorXD : GeometryObject, IMatrixD, IReadOnlyVectorXD
{
    internal VectorXD(MatrixStorage storage, MatrixLayout layout, int offset = 0)
        : base(storage, layout, offset) { }

    private VectorXD(Memory<double> memory, MatrixLayout layout)
        : base(memory, layout) { }

    internal static VectorXD FromOwnedMatrix(MatrixXD matrix)
    {
        using (matrix)
            return new(matrix.Storage, matrix.Layout.Require(null, 1), matrix.Offset);
    }

    public static VectorXD FromMatrix(MatrixXD matrix) =>
        new(matrix.Storage, matrix.Layout.Require(null, 1), matrix.Offset);

    public IReadOnlyVectorXD AsReadOnly() => new ReadOnlyVectorXD(Storage, Layout, Offset);

    public MatrixXD AsMatrix() => RetainMatrix();

    public TensorSpan<double> AsTensorSpan() => WritableSpan();

    public VectorXD(int count)
        : base(count, 1) { }

    public VectorXD(params ReadOnlySpan<double> values)
        : this(values.Length)
    {
        for (int i = 0; i < values.Length; i++)
            this[i] = values[i];
    }

    public static VectorXD CreateFromMemory(Memory<double> memory, int count, int stride = 1) =>
        new(memory, MatrixLayout.Create(count, 1, stride, Math.Max(1, checked(count * stride))));

    public int Count => Rows;
    public double this[int index]
    {
        get => base[index, 0];
        set => SetValue(index, 0, value);
    }

    public VectorXD Slice(int start, int count)
    {
        var layout = Layout.Block(start, 0, count, 1);
        return new(Storage, layout, layout.Extent == 0 ? Offset : checked(Offset + start * RowStride));
    }

    IReadOnlyVectorXD IReadOnlyVectorXD.Slice(int start, int count) =>
        new ReadOnlyVectorXD(
            Storage,
            Layout.Block(start, 0, count, 1),
            count == 0 ? Offset : checked(Offset + start * RowStride)
        );

    public MatrixXD Transposed() => new(Storage, Layout.Transposed(), Offset);

    public static VectorXD operator +(VectorXD a, IReadOnlyVectorXD b) => a.Add(b);

    public static VectorXD operator -(VectorXD a, IReadOnlyVectorXD b) => a.Subtract(b);

    public static VectorXD operator -(VectorXD value) => value.Scale(-1);

    public static VectorXD operator *(VectorXD value, double scalar) => value.Scale(scalar);

    public static VectorXD operator *(double scalar, VectorXD value) => value * scalar;

    public static VectorXD operator /(VectorXD value, double scalar) => value.Divide(scalar);

    public override string ToString() => $"[{string.Join(", ", this.ToArray())}]";
}
