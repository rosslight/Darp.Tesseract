namespace Darp.Geometry;

/// <summary>Read-only access to independently retained geometry storage. Dispose each acquired view.</summary>
public interface IReadOnlyMatrix3D : IReadOnlyMatrixD
{
    IReadOnlyMatrixD Row(int row);
    IReadOnlyVector3D Column(int column);
    new IReadOnlyMatrix3D Transposed();
}
