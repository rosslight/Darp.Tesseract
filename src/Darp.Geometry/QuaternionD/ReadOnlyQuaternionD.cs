using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>A read-only view of shared coefficients. Other aliases may change them.</summary>
public readonly partial struct ReadOnlyQuaternionD : IReadOnlyMatrixD<ReadOnlyQuaternionD>
{
    private static readonly MatrixData s_zeroData = QuaternionD.s_zeroData;
    private MatrixData Data => field.Storage is null ? s_zeroData : field;

    internal ReadOnlyQuaternionD(in MatrixData data) => Data = data;

    internal ReadOnlyQuaternionD(MatrixStorage storage, in MatrixLayout layout)
        : this(new MatrixData(storage, layout)) { }

    public int Rows => Data.Rows;
    public int Columns => Data.Columns;
    public int RowStride => Data.RowStride;
    public int ColumnStride => Data.ColumnStride;

    public double X => Data[0, 0];
    public double Y => Data[1, 0];
    public double Z => Data[2, 0];
    public double W => Data[3, 0];
    public ReadOnlyVectorXD Coefficients => new(Data);

    public double this[Index row, Index column] => throw new NotImplementedException();

    TensorSpanLease IReadOnlyMatrixD<ReadOnlyQuaternionD>.GetReadOnlyTensorSpan(out ReadOnlyTensorSpan<double> span) =>
        Data.AcquireReadOnlyTensorSpan(out span);

    static ReadOnlyQuaternionD IReadOnlyMatrixD<ReadOnlyQuaternionD>.Create(in MatrixData data) => new(data);

    public double this[int row, int column] => Data[row, column];

    public ReadOnlyMatrixXD Transposed() => new(Data.AsTransposedLayout());

    public ReadOnlyVectorXD AsVector() => new(Data.AsVectorLayout());

    public override string ToString() => $"(X={X}, Y={Y}, Z={Z}, W={W})";
}
