namespace Darp.Geometry;

/// <summary>A disposable geometry object with independently retained read-only access.</summary>
public interface IReadOnlyMatrixD : IDisposable
{
    /// <summary>Creates a new independently disposable view of the same coefficients.</summary>
    ReadOnlyMatrixXD AsReadOnlyMatrix();
    /// <summary>Retains storage for the duration of a tensor-span operation.</summary>
    ReadOnlyMatrixBorrow BorrowReadOnly();
}
