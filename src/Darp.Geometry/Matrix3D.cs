using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>Mutable geometry owning one reference to its coefficient storage.</summary>
public sealed class Matrix3D : GeometryObject, IMatrixD, IReadOnlyMatrix3D
{
    internal Matrix3D(MatrixStorage storage, MatrixLayout layout, int offset = 0) : base(storage, layout, offset) { }

    private Matrix3D(Memory<double> memory, MatrixLayout layout) : base(memory, layout) { }
    internal static Matrix3D FromOwnedMatrix(MatrixXD matrix)
    {
        using (matrix) return new(matrix.Storage, matrix.Layout.Require(3, 3), matrix.Offset);
    }
    public static Matrix3D FromMatrix(MatrixXD matrix) => new(matrix.Storage, matrix.Layout.Require(3, 3), matrix.Offset);
    public IReadOnlyMatrix3D AsReadOnly() => new ReadOnlyMatrix3D(Storage, Layout, Offset);
    public MatrixXD AsMatrix() => RetainMatrix();
    public TensorSpan<double> AsTensorSpan() => WritableSpan();
    public Matrix3D() : base(3, 3) { }
    public static Matrix3D Identity
    {
        get { var result = new Matrix3D(); for (int i = 0; i < 3; i++) result[i, i] = 1; return result; }
    }
    public static Matrix3D FromArray(double[,] values) => FromOwnedMatrix(MatrixXD.FromArray(values));
    public static Matrix3D CreateFromMemory(Memory<double> memory, int columnStride = 3, int rowStride = 1) =>
        new(memory, MatrixLayout.Create(3, 3, rowStride, columnStride));
    public new double this[int row, int column] { get => base[row, column]; set => SetValue(row, column, value); }
    public MatrixXD Row(int row) => new(Storage, Layout.Block(row, 0, 1, Columns), checked(Offset + row * RowStride));
    IReadOnlyMatrixD IReadOnlyMatrix3D.Row(int row) => new ReadOnlyMatrix(Storage, Layout.Block(row, 0, 1, Columns), checked(Offset + row * RowStride));
    public Vector3D Column(int column) => new(Storage, Layout.Block(0, column, 3, 1), checked(Offset + column * ColumnStride));
    IReadOnlyVector3D IReadOnlyMatrix3D.Column(int column) => new ReadOnlyVector3D(Storage, Layout.Block(0, column, 3, 1), checked(Offset + column * ColumnStride));
    public Matrix3D Transposed() => new(Storage, Layout.Transposed(), Offset);
    IReadOnlyMatrix3D IReadOnlyMatrix3D.Transposed() => new ReadOnlyMatrix3D(Storage, Layout.Transposed(), Offset);
    public static Matrix3D operator *(Matrix3D a, IReadOnlyMatrix3D b) => a.Multiply(b);
    public static Vector3D operator *(Matrix3D a, IReadOnlyVector3D b) => a.Multiply(b);
    public static Matrix3D operator +(Matrix3D a, IReadOnlyMatrix3D b) => a.Add(b);
    public static Matrix3D operator -(Matrix3D a, IReadOnlyMatrix3D b) => a.Subtract(b);
    public static Matrix3D operator -(Matrix3D value) => value.Scale(-1);
    public static Matrix3D operator *(Matrix3D value, double scalar) => value.Scale(scalar);
    public static Matrix3D operator *(double scalar, Matrix3D value) => value * scalar;
    public static Matrix3D operator /(Matrix3D value, double scalar) => value.Divide(scalar);
}
