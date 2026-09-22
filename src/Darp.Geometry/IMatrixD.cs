namespace Darp.Geometry;

/// <summary>A geometry object with writable storage.</summary>
public interface IMatrixD : IReadOnlyMatrixD
{
    /// <summary>Creates a new independently disposable view of the same coefficients.</summary>
    MatrixXD AsMatrix();
    MatrixBorrow Borrow();
}
