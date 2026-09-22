using System.Buffers;
using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>Read-only access to coefficients; other aliases may still mutate shared storage.</summary>
public interface IReadOnlyMatrixD : IDisposable
{
    int Rows { get; }
    int Columns { get; }
    int RowStride { get; }
    int ColumnStride { get; }
    double this[int row, int column] { get; }
    /// <summary>Keep this object alive and undisposed until the span's last use.</summary>
    ReadOnlyTensorSpan<double> AsReadOnlyTensorSpan();
    MemoryHandle Pin();
    IReadOnlyMatrixD AsReadOnlyMatrix();
    IReadOnlyMatrixD Block(int row, int column, int rows, int columns);
    IReadOnlyMatrixD Transposed();
    IReadOnlyVectorXD AsVector();
}
