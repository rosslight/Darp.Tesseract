using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>A read-only rigid-transform view over shared matrix storage.</summary>
/// <remarks>This view shares coefficients with its source. A writable alias can still change them. The default value is a zero matrix, not an identity transform.</remarks>
public readonly partial struct ReadOnlyIsometry3D : IReadOnlyMatrixD<ReadOnlyIsometry3D>
{
    private static readonly MatrixData s_zeroData = Isometry3D.s_zeroData;
    private MatrixData Data => field.Storage is null ? s_zeroData : field;

    internal ReadOnlyIsometry3D(in MatrixData data) => Data = data.Require(4, 4);

    internal MatrixStorage Storage => Data.Storage;
    internal MatrixLayout Layout => Data.Layout;
    public int Rows => Data.Rows;
    public int Columns => Data.Columns;
    public int RowStride => Data.RowStride;
    public int ColumnStride => Data.ColumnStride;

    TensorSpanLease IReadOnlyMatrixD<ReadOnlyIsometry3D>.GetReadOnlyTensorSpan(out ReadOnlyTensorSpan<double> span) =>
        Data.AcquireReadOnlyTensorSpan(out span);

    static ReadOnlyIsometry3D IReadOnlyMatrixD<ReadOnlyIsometry3D>.Create(in MatrixData data) => new(data);

    public double this[int row, int column] => Data[row, column];
    public double this[Index row, Index column] => Data[row, column];

    public override string ToString() => Matrix.Format(this);

    public ReadOnlyMatrixXD AsMatrix() => new(Data);

    public ReadOnlyMatrixXD Transposed() => new(Data.AsTransposedLayout());

    public ReadOnlyVector3D Translation => new(Data.Slice(0, 3, 3, 1));
    public ReadOnlyMatrix3D RotationMatrix => new(Data.Slice(0, 0, 3, 3));

    public QuaternionD Rotation
    {
        get
        {
            var matrix = RotationMatrix;
            return matrix.ToQuaternion();
        }
    }
}
