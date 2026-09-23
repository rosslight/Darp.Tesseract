using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>A read-only quaternion view over shared X, Y, Z, W coefficient storage.</summary>
/// <remarks>This view shares coefficients with its source. A writable alias can still change them. The default value is zero.</remarks>
public readonly partial struct ReadOnlyQuaternionD : IReadOnlyMatrixD<ReadOnlyQuaternionD>
{
    private static readonly MatrixData s_zeroData = QuaternionD.s_zeroData;
    private MatrixData Data => field.Storage is null ? s_zeroData : field;

    internal ReadOnlyQuaternionD(in MatrixData data) => Data = data.Require(4, 1);

    internal ReadOnlyQuaternionD(MatrixStorage storage, in MatrixLayout layout)
        : this(new MatrixData(storage, layout)) { }

    public int Rows => Data.Rows;
    public int Columns => Data.Columns;
    public int RowStride => Data.RowStride;
    public int ColumnStride => Data.ColumnStride;

    public double X => Data.Get(0, 0);
    public double Y => Data.Get(1, 0);
    public double Z => Data.Get(2, 0);
    public double W => Data.Get(3, 0);
    public int Count => 4;
    public ReadOnlyVectorXD Coefficients => new(Data);

    public ReadOnlyMatrixXD AsMatrix() => new(Data);

    public double this[int index] => Data.Get(index, 0);

    public double this[Index row, Index column] => Data.Get(row, column);

    TensorSpanLease IReadOnlyMatrixD<ReadOnlyQuaternionD>.GetReadOnlyTensorSpan(out ReadOnlyTensorSpan<double> span) =>
        Data.AcquireReadOnlyTensorSpan(out span);

    static ReadOnlyQuaternionD IReadOnlyMatrixD<ReadOnlyQuaternionD>.Create(in MatrixData data) => new(data);

    public double this[int row, int column] => Data.Get(row, column);

    public ReadOnlyMatrixXD Transposed() => new(Data.AsTransposedLayout());

    public ReadOnlyVectorXD AsVector() => new(Data.AsVectorLayout());

    public override string ToString() => $"(X={X}, Y={Y}, Z={Z}, W={W})";
}
