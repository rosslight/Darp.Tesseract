using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>A mutable rigid transform. The caller preserves a proper rotation and last row [0,0,0,1].</summary>
public sealed class Isometry3D : IMatrixD
{
    private readonly MatrixXD _matrix;
    internal Isometry3D(MatrixXD matrix)
    {
        if (matrix.Rows != 4 || matrix.Columns != 4)
        {
            matrix.Dispose();
            throw new ArgumentException("Expected a 4 by 4 matrix.", nameof(matrix));
        }
        _matrix = matrix;
    }
    public Isometry3D() : this(MatrixXD.Identity(4)) { }
    public Isometry3D(IReadOnlyMatrixD rotation, IReadOnlyMatrixD translation) : this()
    {
        try
        {
            SetRotation(rotation);
            SetTranslation(translation);
        }
        catch
        {
            Dispose();
            throw;
        }
    }
    public static Isometry3D Identity => new();
    public static Isometry3D Map(Memory<double> memory, int columnStride = 4) => new(MatrixXD.Map(memory, 4, 4, columnStride));
    /// <summary>Retains an independent view of a matrix known by the caller to represent a rigid transform.</summary>
    public static Isometry3D View(MatrixXD matrix) => new(matrix.AsMatrix());
    /// <summary>Copies a matrix known by the caller to represent a rigid transform.</summary>
    public static Isometry3D FromMatrix(IReadOnlyMatrixD matrix)
    {
        using var view = matrix.AsReadOnlyMatrix();
        MatrixShape.RequireSize(view.AsReadOnlyTensorSpan(), 4, 4);
        return new(view.Clone());
    }
    public ReadOnlyIsometry3D AsReadOnly() => new(_matrix.AsReadOnly());
    public MatrixXD Matrix => _matrix.AsMatrix();
    public Vector3D Translation
    {
        get => new(new VectorXD(_matrix.Block(0, 3, 3, 1)));
        set => SetTranslation(value);
    }
    public Matrix3D RotationMatrix
    {
        get => new(_matrix.Block(0, 0, 3, 3));
        set => SetRotationMatrix(value);
    }
    /// <summary>The getter computes an independent quaternion; assigning it updates the matrix.</summary>
    public QuaternionD Rotation
    {
        get { using var view = AsReadOnly(); return view.Rotation; }
        set => SetRotation(value);
    }
    public void SetTranslation(IReadOnlyMatrixD value)
    {
        using var view = value.AsReadOnlyMatrix();
        MatrixShape.RequireSize(view.AsReadOnlyTensorSpan(), 3, 1);
        double x = view[0, 0], y = view[1, 0], z = view[2, 0];
        _matrix.SetValue(0, 3, x);
        _matrix.SetValue(1, 3, y);
        _matrix.SetValue(2, 3, z);
    }
    public void SetRotation(IReadOnlyMatrixD value)
    {
        using var quaternion = new ReadOnlyQuaternionD(new ReadOnlyVectorXD(value.AsReadOnlyMatrix()));
        using var rotation = quaternion.ToRotationMatrix();
        SetRotationMatrix(rotation);
    }
    public void SetRotationMatrix(IReadOnlyMatrixD value)
    {
        using var view = value.AsReadOnlyMatrix();
        MatrixShape.RequireSize(view.AsReadOnlyTensorSpan(), 3, 3);
        // Snapshot on the stack so overlapping views, including transpose views, are safe.
        Span<double> copy = stackalloc double[9];
        for (int c = 0; c < 3; c++)
            for (int r = 0; r < 3; r++) copy[c * 3 + r] = view[r, c];
        for (int c = 0; c < 3; c++)
            for (int r = 0; r < 3; r++) _matrix.SetValue(r, c, copy[c * 3 + r]);
    }
    public Isometry3D Clone() => new(_matrix.Clone());
    public Vector3D TransformPoint(IReadOnlyMatrixD point) { using var view = AsReadOnly(); return view.TransformPoint(point); }
    public Vector3D TransformDirection(IReadOnlyMatrixD direction) { using var view = AsReadOnly(); return view.TransformDirection(direction); }
    public Isometry3D Inverse() { using var view = AsReadOnly(); return view.Inverse(); }
    public static Isometry3D operator *(Isometry3D a, Isometry3D b) => new(MatrixOperations.Multiply(a, b));
    public static Isometry3D operator *(Isometry3D a, ReadOnlyIsometry3D b) => new(MatrixOperations.Multiply(a, b));
    public static Vector3D operator *(Isometry3D a, IReadOnlyMatrixD b) => a.TransformPoint(b);
    public ReadOnlyMatrixXD AsReadOnlyMatrix() => _matrix.AsReadOnly();
    public MatrixXD AsMatrix() => _matrix.AsMatrix();
    public ReadOnlyTensorSpan<double> AsReadOnlyTensorSpan() => _matrix.AsReadOnlyTensorSpan();
    public TensorSpan<double> AsTensorSpan() => _matrix.AsTensorSpan();
    public MatrixBorrow Borrow() => _matrix.Borrow();
    public ReadOnlyMatrixBorrow BorrowReadOnly() => _matrix.BorrowReadOnly();
    public void Dispose() => _matrix.Dispose();
    public override string ToString() => _matrix.ToString();
}
