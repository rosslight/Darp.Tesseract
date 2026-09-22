using System.Buffers;
using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>Scoped access to matrix coefficients and stable pointers for native interop.</summary>
public static class MatrixMarshal
{
    /// <summary>Acquires read access. Keep the returned lease alive and undisposed through the span's last use.</summary>
    public static TensorSpanLease GetReadOnlyTensorSpan(this IReadOnlyMatrixD matrix, out ReadOnlyTensorSpan<double> span)
        => matrix.AcquireReadOnlyTensorSpan(out span);

    /// <summary>Acquires write access. Keep the returned lease alive and undisposed through the span's last use.</summary>
    public static TensorSpanLease GetTensorSpan(this IMatrixD matrix, out TensorSpan<double> span)
        => matrix.AcquireTensorSpan(out span);

    /// <summary>Pins coefficients for native pointer access until the returned handle is disposed.</summary>
    public static MemoryHandle Pin(this IReadOnlyMatrixD matrix) => matrix.Pin();
}
