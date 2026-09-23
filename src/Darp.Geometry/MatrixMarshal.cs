using System.Buffers;
using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>Scoped access to matrix coefficients and stable pointers for native interop.</summary>
public static class MatrixMarshal
{
    /// <summary>Acquires read access. Keep the returned lease alive and undisposed through the span's last use.</summary>
    public static TensorSpanLease GetReadOnlyTensorSpan<TM>(in TM matrix, out ReadOnlyTensorSpan<double> span)
        where TM : IReadOnlyMatrixD<TM> =>
        matrix.GetReadOnlyTensorSpan(out span);

    /// <summary>Acquires write access. Keep the returned lease alive and undisposed through the span's last use.</summary>
    public static TensorSpanLease GetTensorSpan<TM>(in TM matrix, out TensorSpan<double> span)
        where TM : IMatrixD<TM> =>
        matrix.GetTensorSpan(out span);

    /// <summary>Pins coefficients for read-only native pointer access until the returned handle is disposed.</summary>
    public static MemoryHandle Pin(this in ReadOnlyMatrixXD matrix) => matrix.Data.Pin();
}
