using System.Buffers;
using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>A read-only view of shared coefficients. Other aliases may change them.</summary>
public readonly struct ReadOnlyVector3D : IReadOnlyMatrixD<ReadOnlyVector3D>
{
    private static readonly MatrixData s_zeroData = Vector3D.s_zeroData;
    private MatrixData Data => field.Storage is null ? s_zeroData : field;

    internal ReadOnlyVector3D(in MatrixData data) => Data = data.Require(3, 1);

    internal ReadOnlyVector3D(MatrixStorage storage, in MatrixLayout layout)
        : this(new MatrixData(storage, layout)) { }

    public int Rows => Data.Rows;
    public int Columns => Data.Columns;
    public int RowStride => Data.RowStride;
    public int ColumnStride => Data.ColumnStride;

    public double X => Data[0, 0];
    public double Y => Data[1, 0];
    public double Z => Data[2, 0];
    public int Count => Rows;

    TensorSpanLease IReadOnlyMatrixD<ReadOnlyVector3D>.GetReadOnlyTensorSpan(out ReadOnlyTensorSpan<double> span) =>
        Data.AcquireReadOnlyTensorSpan(out span);

    static ReadOnlyVector3D IReadOnlyMatrixD<ReadOnlyVector3D>.Create(in MatrixData data) => new(data);

    public double this[int index] => Data[index, 0];
    double IReadOnlyMatrixD<ReadOnlyVector3D>.this[int row, int column] => Data[row, column];
    double IReadOnlyMatrixD<ReadOnlyVector3D>.this[Index row, Index column] => Data[row, column];

    public ReadOnlyMatrixXD Transposed() => new(Data.AsTransposedLayout());

    public override string ToString() => $"[X = {X}, Y = {Y}, Z = {Z}]";
}
