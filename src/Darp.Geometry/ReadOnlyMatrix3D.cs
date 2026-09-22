using System.Buffers;
using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>A read-only view of shared coefficients. Other aliases may change them.</summary>
public readonly struct ReadOnlyMatrix3D : IReadOnlyMatrixD<ReadOnlyMatrix3D>
{
    private static readonly MatrixData s_zeroData = Matrix3D.ZeroData;
    private MatrixData Data => field.Storage is null ? s_zeroData : field;

    private ReadOnlyMatrix3D(in MatrixData data) => Data = data;

    internal ReadOnlyMatrix3D(MatrixStorage storage, in MatrixLayout layout)
        : this(new MatrixData(storage, layout)) { }

    /// <inheritdoc/>
    public int Rows => Data.Rows;
    /// <inheritdoc/>
    public int Columns => Data.Columns;
    /// <inheritdoc/>
    public int RowStride => Data.RowStride;
    /// <inheritdoc/>
    public int ColumnStride => Data.ColumnStride;


    static ReadOnlyMatrix3D IReadOnlyMatrixD<ReadOnlyMatrix3D>.Create(in MatrixData data) => new(data);

    TensorSpanLease IReadOnlyMatrixD<ReadOnlyMatrix3D>.GetReadOnlyTensorSpan(out ReadOnlyTensorSpan<double> span) =>
        Data.AcquireReadOnlyTensorSpan(out span);

    /// <inheritdoc/>
    public double this[int row, int column] => Data[row, column];
    /// <inheritdoc/>
    public double this[Index row, Index column] => Data[row, column];

    public ReadOnlyMatrixXD this[Range rows, Range columns] => new(Data[rows, columns]);

    public ReadOnlyMatrix3D Transposed() => new(Data.Storage, Data.Layout.Transposed());

    public ReadOnlyVector3D AsVector() => new(Data.AsVectorLayout());

    public static ReadOnlyMatrix3D operator +(ReadOnlyMatrix3D a, ReadOnlyMatrix3D b) => MatrixExtensions.Add(a, b);

    public static ReadOnlyMatrix3D operator -(ReadOnlyMatrix3D a, ReadOnlyMatrix3D b) => MatrixExtensions.Subtract(a, b);

    public static ReadOnlyMatrix3D operator *(ReadOnlyMatrix3D a, ReadOnlyMatrix3D b) => MatrixExtensions.Multiply(a, b);

    public static ReadOnlyMatrix3D operator *(ReadOnlyMatrix3D a, double scalar) => a.Scale(scalar);

    public static ReadOnlyMatrix3D operator *(double scalar, ReadOnlyMatrix3D a) => a * scalar;

    public static ReadOnlyMatrix3D operator /(ReadOnlyMatrix3D a, double scalar) => a.Divide(scalar);

    public static ReadOnlyMatrix3D operator -(ReadOnlyMatrix3D a) => a.Scale(-1);

    public static implicit operator ReadOnlyMatrixXD(ReadOnlyMatrix3D value) => new(value.Data);

    public override string ToString() => MatrixExtensions.Format(this);
}
