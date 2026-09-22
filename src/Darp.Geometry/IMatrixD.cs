using System.Numerics.Tensors;

namespace Darp.Geometry;

public interface IMatrixD : IReadOnlyMatrixD
{
    MatrixXD AsMatrix();
    internal TensorSpanLease AcquireTensorSpan(out TensorSpan<double> span);
}
