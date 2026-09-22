using System.Buffers;
using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>Retains writable storage independently of the object from which it was borrowed.</summary>
/// <remarks>Extracted spans must not be used after this borrow is disposed.</remarks>
public sealed class MatrixBorrow : IDisposable
{
    private readonly MatrixView _view;
    internal MatrixBorrow(MatrixView view) => _view = view;
    public int Rows { get { _view.ThrowIfDisposed(); return _view.Layout.Rows; } }
    public int Columns { get { _view.ThrowIfDisposed(); return _view.Layout.Columns; } }
    public int RowStride { get { _view.ThrowIfDisposed(); return _view.Layout.RowStride; } }
    public int ColumnStride { get { _view.ThrowIfDisposed(); return _view.Layout.ColumnStride; } }
    public TensorSpan<double> AsTensorSpan() => _view.Span();
    public ReadOnlyTensorSpan<double> AsReadOnlyTensorSpan() => _view.ReadOnlySpan();
    public MemoryHandle Pin() => _view.Pin();
    public void Dispose() => _view.Dispose();
}

/// <summary>Retains read-only access independently of the source object's disposal.</summary>
/// <remarks>Extracted spans must not be used after this borrow is disposed. Writable aliases may still modify storage.</remarks>
public sealed class ReadOnlyMatrixBorrow : IDisposable
{
    private readonly MatrixView _view;
    internal ReadOnlyMatrixBorrow(MatrixView view) => _view = view;
    public int Rows { get { _view.ThrowIfDisposed(); return _view.Layout.Rows; } }
    public int Columns { get { _view.ThrowIfDisposed(); return _view.Layout.Columns; } }
    public int RowStride { get { _view.ThrowIfDisposed(); return _view.Layout.RowStride; } }
    public int ColumnStride { get { _view.ThrowIfDisposed(); return _view.Layout.ColumnStride; } }
    public ReadOnlyTensorSpan<double> AsReadOnlyTensorSpan() => _view.ReadOnlySpan();
    public MemoryHandle Pin() => _view.Pin();
    public void Dispose() => _view.Dispose();
}
