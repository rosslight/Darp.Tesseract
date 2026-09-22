using System.Numerics.Tensors;
namespace Darp.Geometry;

public interface IMatrixD : IReadOnlyMatrixD
{
    MatrixXD AsMatrix();
    TensorSpan<double> AsTensorSpan();
}
