using System.Buffers;
using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>Mutable geometry sharing its coefficient storage with derived views.</summary>
public readonly partial struct Matrix3D : IMatrixD<Matrix3D>
{
    internal static readonly MatrixData s_zeroData = MatrixData.ReadOnlyZero(3, 3);
    internal MatrixData Data => field.Storage is null ? s_zeroData : field;

    public int Rows => Data.Rows;
    public int Columns => Data.Columns;
    public int RowStride => Data.RowStride;
    public int ColumnStride => Data.ColumnStride;

    internal Matrix3D(in MatrixData data) => Data = data;

    internal Matrix3D(MatrixStorage storage, in MatrixLayout layout)
        : this(new MatrixData(storage, layout)) { }

    private Matrix3D(Memory<double> memory, in MatrixLayout layout)
        : this(new MatrixData(memory, layout)) { }

    public Matrix3D()
        : this(new MatrixData(3, 3)) { }

    public static Matrix3D Identity
    {
        get
        {
            var result = new Matrix3D();
            for (int i = 0; i < 3; i++)
                result[i, i] = 1;
            return result;
        }
    }

    public static Matrix3D FromArray(double[,] values) => FromMatrix(MatrixXD.FromArray(values));

    public static Matrix3D FromMatrix(in MatrixXD matrix) => new(matrix.Data.Require(3, 3));

    public static Matrix3D CreateFromMemory(Memory<double> memory, int columnStride = 3, int rowStride = 1) =>
        new(memory, MatrixLayout.Create(3, 3, rowStride, columnStride));

    static Matrix3D IReadOnlyMatrixD<Matrix3D>.Create(in MatrixData data) => new(data);

    TensorSpanLease IMatrixD<Matrix3D>.GetTensorSpan(out TensorSpan<double> span) =>
        Data.AcquireWritableTensorSpan(out span);

    TensorSpanLease IReadOnlyMatrixD<Matrix3D>.GetReadOnlyTensorSpan(out ReadOnlyTensorSpan<double> span) =>
        Data.AcquireReadOnlyTensorSpan(out span);

    /// <inheritdoc/>
    public double this[int row, int column]
    {
        get => Data[row, column];
        set => Data[row, column] = value;
    }

    /// <inheritdoc/>
    public double this[Index row, Index column]
    {
        get => Data[row, column];
        set => Data[row, column] = value;
    }

    /// <summary>Returns a writable view of the selected rows and columns.</summary>
    /// <remarks>Changes through the view are visible through this matrix.</remarks>
    /// <param name="rows">The rows to include.</param>
    /// <param name="columns">The columns to include.</param>
    /// <returns>A view that shares coefficients with this matrix.</returns>
    public MatrixXD this[Range rows, Range columns] => new(Data[rows, columns]);

    public ReadOnlyVector3D AsVector() => new(Data.AsVectorLayout());

    public override string ToString() => Matrix.Format(this);

    public Matrix3D Transposed() => new(Data.Storage, Data.Layout.Transposed());

    public ReadOnlyMatrix3D AsReadOnly() => new(Data);
}
