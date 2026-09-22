using System.Numerics.Tensors;

namespace Darp.Geometry;

public interface IMatrixD<out TM>:IReadOnlyMatrixD<TM> where TM : IReadOnlyMatrixD<TM> 
{
    /// <summary> Gets/sets the element at the (row, column) coordinate </summary>
    /// <param name="row">The row to get/set</param>
    /// <param name="column">The column to get/set</param>
    new double this[int row, int column] { get; set; }

    /// <summary> Gets/sets the element at the (row, column) coordinate </summary>
    /// <param name="row">The row to get/set</param>
    /// <param name="column">The column to get/set</param>
    new double this[Index row, Index column] { get; set; }

    internal TensorSpanLease GetTensorSpan(out TensorSpan<double> span);
}
