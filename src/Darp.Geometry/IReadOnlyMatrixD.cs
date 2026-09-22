using System.Numerics.Tensors;

namespace Darp.Geometry;

public interface IReadOnlyMatrixD<out TM> where TM : IReadOnlyMatrixD<TM>
{
    /// <summary> The rows of this matrix </summary>
    int Rows { get; }

    /// <summary> The columns of this matrix </summary>
    int Columns { get; }

    /// <summary> The row stride of this matrix </summary>
    int RowStride { get; }

    /// <summary> The column stride of this matrix </summary>
    int ColumnStride { get; }

    /// <summary> Gets the element at the (row, column) coordinate </summary>
    /// <param name="row">The row to get</param>
    /// <param name="column">The column to get</param>
    double this[int row, int column] { get; }

    /// <summary> Gets the element at the (row, column) coordinate </summary>
    /// <param name="row">The row to get</param>
    /// <param name="column">The column to get</param>
    double this[Index row, Index column] { get; }

    internal TensorSpanLease GetReadOnlyTensorSpan(out ReadOnlyTensorSpan<double> span);

    internal static abstract TM Create(in MatrixData data);
}
