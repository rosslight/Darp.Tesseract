namespace Darp.Geometry;

/// <summary>Read-only access to shared geometry storage; other aliases may mutate it.</summary>
public interface IReadOnlyMatrix3D : IReadOnlyMatrixD
{
    IReadOnlyMatrixD Row(int row);
    IReadOnlyVector3D Column(int column);
    new IReadOnlyMatrix3D Transposed();
}
