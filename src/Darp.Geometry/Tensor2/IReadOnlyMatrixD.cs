namespace Darp.Geometry.Tensor2;

/// <summary>A geometry value exposing its coefficients as an owner-bearing read-only matrix view.</summary>
public interface IReadOnlyMatrixD
{
    /// <summary>Shares coefficients and retains their memory provider. Must not return a span-only borrow.</summary>
    ReadOnlyMatrixXD AsReadOnlyMatrix();
}
