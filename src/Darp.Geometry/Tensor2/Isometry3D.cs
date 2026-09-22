using System.Numerics.Tensors;

namespace Darp.Geometry.Tensor2;

/// <summary>A mutable rigid-transform descriptor. Views must retain a proper rotation and last row [0,0,0,1].</summary>
public readonly struct Isometry3D : IMatrixD
{
    private readonly MatrixXD _matrix;
    internal Isometry3D(MatrixXD matrix)
    {
        if (matrix.Rows != 4 || matrix.Columns != 4) throw new ArgumentException("Expected a 4 by 4 matrix.", nameof(matrix));
        _matrix = matrix;
    }
    public Isometry3D() : this(MatrixXD.Identity(4)) { }
    public Isometry3D(ReadOnlyQuaternionD rotation, ReadOnlyVector3D translation) : this()
    {
        SetRotation(rotation);
        SetTranslation(translation);
    }
    public static Isometry3D Identity => new();
    public static Isometry3D Map(Memory<double> memory, int columnStride = 4) => new(MatrixXD.Map(memory, 4, 4, columnStride));
    /// <summary>Shares a matrix known by the caller to represent a rigid transform.</summary>
    public static Isometry3D View(MatrixXD matrix) => new(matrix);
    /// <summary>Copies a matrix known by the caller to represent a rigid transform.</summary>
    public static Isometry3D FromMatrix(ReadOnlyMatrixXD readOnlyMatrix) => new(readOnlyMatrix.Clone());
    public ReadOnlyIsometry3D AsReadOnly() => new(_matrix.AsReadOnly());
    public static implicit operator ReadOnlyIsometry3D(Isometry3D value) => value.AsReadOnly();
    public MatrixXD Matrix => _matrix;
    public Vector3D Translation
    {
        get => new(_matrix.Column(3).Slice(0, 3));
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
        get => AsReadOnly().Rotation;
        set => SetRotation(value);
    }
    public void SetTranslation(ReadOnlyVector3D value)
    {
        double x = value.X, y = value.Y, z = value.Z;
        _matrix.SetValue(0, 3, x);
        _matrix.SetValue(1, 3, y);
        _matrix.SetValue(2, 3, z);
    }
    public void SetRotation(ReadOnlyQuaternionD value) => SetRotationMatrix(value.ToRotationMatrix());
    public void SetRotationMatrix(ReadOnlyMatrix3D value)
    {
        // Snapshot on the stack so overlapping views, including transpose views, are safe.
        Span<double> copy = stackalloc double[9];
        for (int c = 0; c < 3; c++)
            for (int r = 0; r < 3; r++) copy[c * 3 + r] = value[r, c];
        for (int c = 0; c < 3; c++)
            for (int r = 0; r < 3; r++) _matrix.SetValue(r, c, copy[c * 3 + r]);
    }
    public Isometry3D Clone() => AsReadOnly().Clone();
    public Vector3D TransformPoint(ReadOnlyVector3D point) => AsReadOnly().TransformPoint(point);
    public Vector3D TransformDirection(ReadOnlyVector3D direction) => AsReadOnly().TransformDirection(direction);
    public Isometry3D Inverse() => AsReadOnly().Inverse();
    public static Isometry3D operator *(Isometry3D a, Isometry3D b) => a.AsReadOnly() * b.AsReadOnly();
    public static Vector3D operator *(Isometry3D a, ReadOnlyVector3D b) => a.TransformPoint(b);
    public ReadOnlyMatrixXD AsReadOnlyMatrix() => _matrix.AsReadOnly();
    public MatrixXD AsMatrix() => _matrix;
    public ReadOnlyTensorSpan<double> AsReadOnlyTensorSpan() => _matrix.AsReadOnlyTensorSpan();
    public TensorSpan<double> AsTensorSpan() => _matrix.AsTensorSpan();
    public override string ToString() => _matrix.ToString();
}
