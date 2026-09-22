using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>A mutable rigid transform. The caller preserves a proper rotation and last row [0,0,0,1].</summary>
public sealed class Isometry3D : GeometryObject, IMatrixD, IReadOnlyIsometry3D
{
    internal Isometry3D(MatrixStorage storage, MatrixLayout layout, int offset = 0)
        : base(storage, layout, offset) { }

    private Isometry3D(Memory<double> memory, MatrixLayout layout)
        : base(memory, layout) { }

    public Isometry3D()
        : base(4, 4)
    {
        for (int i = 0; i < 4; i++)
            SetValue(i, i, 1);
    }

    public Isometry3D(IReadOnlyQuaternionD rotation, IReadOnlyVector3D translation)
        : this()
    {
        SetRotation(rotation);
        SetTranslation(translation);
    }

    public static Isometry3D Identity => new();

    public static Isometry3D CreateFromMemory(Memory<double> memory, int columnStride = 4) =>
        new(memory, MatrixLayout.Create(4, 4, 1, columnStride));

    /// <summary>Creates a shared view of a matrix known by the caller to represent a rigid transform.</summary>
    public static Isometry3D View(MatrixXD matrix) => new(matrix.Storage, matrix.Layout.Require(4, 4), matrix.Offset);

    /// <summary>Copies a matrix known by the caller to represent a rigid transform.</summary>
    public static Isometry3D FromMatrix(IReadOnlyMatrixD matrix)
    {
        using var matrixLease = matrix.GetReadOnlyTensorSpan(out var matrixSpan);
        MatrixShape.RequireSize(matrixSpan, 4, 4);
        return View(matrix.Clone());
    }

    public IReadOnlyIsometry3D AsReadOnly() => new ReadOnlyIsometry3D(Storage, Layout, Offset);

    public MatrixXD AsMatrix() => ViewMatrix();

    public MatrixXD Matrix => ViewMatrix();

    TensorSpanLease IMatrixD.AcquireTensorSpan(out TensorSpan<double> span) => AcquireWritableTensorSpan(out span);

    public Vector3D Translation
    {
        get => new(Storage, Layout.Block(0, 3, 3, 1), checked(Offset + 3 * ColumnStride));
        set => SetTranslation(value);
    }
    IReadOnlyVector3D IReadOnlyIsometry3D.Translation
    {
        get => new ReadOnlyVector3D(Storage, Layout.Block(0, 3, 3, 1), checked(Offset + 3 * ColumnStride));
    }
    public Matrix3D RotationMatrix
    {
        get => new(Storage, Layout.Block(0, 0, 3, 3), Offset);
        set => SetRotationMatrix(value);
    }
    IReadOnlyMatrix3D IReadOnlyIsometry3D.RotationMatrix
    {
        get => new ReadOnlyMatrix3D(Storage, Layout.Block(0, 0, 3, 3), Offset);
    }

    /// <summary>The getter computes an independent quaternion; assigning it updates the matrix.</summary>
    public QuaternionD Rotation
    {
        get
        {
            var matrix = RotationMatrix;
            return matrix.ToQuaternion();
        }
        set => SetRotation(value);
    }

    public void SetTranslation(IReadOnlyVector3D value)
    {
        double x = value.X,
            y = value.Y,
            z = value.Z;
        SetValue(0, 3, x);
        SetValue(1, 3, y);
        SetValue(2, 3, z);
    }

    public void SetRotation(IReadOnlyQuaternionD value)
    {
        var rotation = value.ToRotationMatrix();
        SetRotationMatrix(rotation);
    }

    public void SetRotationMatrix(IReadOnlyMatrix3D value)
    {
        // Snapshot first so overlapping transpose views are safe.
        Span<double> copy = stackalloc double[9];
        for (int c = 0; c < 3; c++)
        for (int r = 0; r < 3; r++)
            copy[c * 3 + r] = value[r, c];
        for (int c = 0; c < 3; c++)
        for (int r = 0; r < 3; r++)
            SetValue(r, c, copy[c * 3 + r]);
    }

    public static Isometry3D operator *(Isometry3D left, IReadOnlyIsometry3D right) => left.Multiply(right);

    public static Vector3D operator *(Isometry3D transform, IReadOnlyVector3D point) => transform.TransformPoint(point);
}
