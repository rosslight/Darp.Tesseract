using System.Numerics.Tensors;

namespace Darp.Geometry.Tensor2;

/// <summary>A read-only rigid-transform view. The caller preserves rotation and homogeneous-row invariants.</summary>
public readonly struct ReadOnlyIsometry3D : IReadOnlyMatrixD
{
    private readonly ReadOnlyMatrixXD _readOnlyMatrix;
    internal ReadOnlyIsometry3D(ReadOnlyMatrixXD readOnlyMatrix)
    {
        if (readOnlyMatrix.Rows != 4 || readOnlyMatrix.Columns != 4) throw new ArgumentException("Expected a 4 by 4 matrix.", nameof(readOnlyMatrix));
        _readOnlyMatrix = readOnlyMatrix;
    }
    public static ReadOnlyIsometry3D Map(ReadOnlyMemory<double> memory, int columnStride = 4) =>
        new(ReadOnlyMatrixXD.Map(memory, 4, 4, columnStride));
    /// <summary>Shares a matrix known by the caller to represent a rigid transform.</summary>
    public static ReadOnlyIsometry3D View(ReadOnlyMatrixXD readOnlyMatrix) => new(readOnlyMatrix);
    public ReadOnlyMatrixXD ReadOnlyMatrix => _readOnlyMatrix;
    public ReadOnlyVector3D Translation => new(_readOnlyMatrix.Column(3).Slice(0, 3));
    public ReadOnlyMatrix3D RotationMatrix => new(_readOnlyMatrix.Block(0, 0, 3, 3));
    /// <summary>A computed, independent quaternion, not a view of the rotation matrix.</summary>
    public QuaternionD Rotation => RotationMatrix.ToQuaternion();
    public Isometry3D Clone() => new(_readOnlyMatrix.Clone());
    public Vector3D TransformPoint(ReadOnlyVector3D point) => RotationMatrix * point + Translation;
    public Vector3D TransformDirection(ReadOnlyVector3D direction) => RotationMatrix * direction;
    public Isometry3D Inverse()
    {
        var rotation = RotationMatrix.Transposed();
        var result = new Isometry3D();
        result.SetRotationMatrix(rotation);
        result.Translation = -(rotation * Translation);
        return result;
    }
    /// <summary>Applies b first, then a. The result has independent storage.</summary>
    public static Isometry3D operator *(ReadOnlyIsometry3D a, ReadOnlyIsometry3D b) => new(a._readOnlyMatrix * b._readOnlyMatrix);
    public static Vector3D operator *(ReadOnlyIsometry3D transform, ReadOnlyVector3D point) => transform.TransformPoint(point);
    public ReadOnlyMatrixXD AsReadOnlyMatrix() => _readOnlyMatrix;
    public ReadOnlyTensorSpan<double> AsReadOnlyTensorSpan() => _readOnlyMatrix.AsReadOnlyTensorSpan();
    public override string ToString() => _readOnlyMatrix.ToString();
}
