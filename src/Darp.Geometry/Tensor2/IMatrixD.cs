namespace Darp.Geometry.Tensor2;

/// <summary>A geometry value with writable matrix storage.</summary>
public interface IMatrixD : IReadOnlyMatrixD
{
    MatrixXD AsMatrix();
}
