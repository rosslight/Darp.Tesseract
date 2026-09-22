using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>A read-only rigid-transform view. The caller preserves rotation and homogeneous-row invariants.</summary>
public sealed class ReadOnlyIsometry3D : IReadOnlyMatrixD
{
    private readonly ReadOnlyMatrixXD _readOnlyMatrix;
    internal ReadOnlyIsometry3D(ReadOnlyMatrixXD readOnlyMatrix)
    {
        if (readOnlyMatrix.Rows != 4 || readOnlyMatrix.Columns != 4)
        {
            readOnlyMatrix.Dispose();
            throw new ArgumentException("Expected a 4 by 4 matrix.", nameof(readOnlyMatrix));
        }
        _readOnlyMatrix = readOnlyMatrix;
    }
    public static ReadOnlyIsometry3D Map(ReadOnlyMemory<double> memory, int columnStride = 4) =>
        new(ReadOnlyMatrixXD.Map(memory, 4, 4, columnStride));
    /// <summary>Retains an independent view of a matrix known by the caller to represent a rigid transform.</summary>
    public static ReadOnlyIsometry3D View(ReadOnlyMatrixXD readOnlyMatrix) => new(readOnlyMatrix.AsReadOnlyMatrix());
    public ReadOnlyMatrixXD ReadOnlyMatrix => _readOnlyMatrix.AsReadOnlyMatrix();
    public ReadOnlyVector3D Translation => new(new ReadOnlyVectorXD(_readOnlyMatrix.Block(0, 3, 3, 1)));
    public ReadOnlyMatrix3D RotationMatrix => new(_readOnlyMatrix.Block(0, 0, 3, 3));
    /// <summary>A computed, independent quaternion, not a view of the rotation matrix.</summary>
    public QuaternionD Rotation { get { using var rotation = RotationMatrix; return rotation.ToQuaternion(); } }
    public Isometry3D Clone() => new(_readOnlyMatrix.Clone());
    public Vector3D TransformPoint(IReadOnlyMatrixD point) => Transform(point, translate: true);
    public Vector3D TransformDirection(IReadOnlyMatrixD direction) => Transform(direction, translate: false);
    private Vector3D Transform(IReadOnlyMatrixD vector, bool translate)
    {
        using var view = vector.AsReadOnlyMatrix();
        MatrixShape.RequireSize(view.AsReadOnlyTensorSpan(), 3, 1);
        double x = view[0, 0], y = view[1, 0], z = view[2, 0];
        return new(
            _readOnlyMatrix[0, 0] * x + _readOnlyMatrix[0, 1] * y + _readOnlyMatrix[0, 2] * z + (translate ? _readOnlyMatrix[0, 3] : 0),
            _readOnlyMatrix[1, 0] * x + _readOnlyMatrix[1, 1] * y + _readOnlyMatrix[1, 2] * z + (translate ? _readOnlyMatrix[1, 3] : 0),
            _readOnlyMatrix[2, 0] * x + _readOnlyMatrix[2, 1] * y + _readOnlyMatrix[2, 2] * z + (translate ? _readOnlyMatrix[2, 3] : 0));
    }
    public Isometry3D Inverse()
    {
        using var originalRotation = RotationMatrix;
        using var rotation = originalRotation.Transposed();
        using var translation = Translation;
        using var rotated = rotation * translation;
        using var inverseTranslation = -rotated;
        var result = new Isometry3D();
        result.SetRotationMatrix(rotation);
        result.SetTranslation(inverseTranslation);
        return result;
    }
    /// <summary>Applies b first, then a. The result has independent storage.</summary>
    public static Isometry3D operator *(ReadOnlyIsometry3D a, ReadOnlyIsometry3D b) => new(MatrixOperations.Multiply(a, b));
    public static Isometry3D operator *(ReadOnlyIsometry3D a, Isometry3D b) => new(MatrixOperations.Multiply(a, b));
    public static Vector3D operator *(ReadOnlyIsometry3D transform, IReadOnlyMatrixD point) => transform.TransformPoint(point);
    public ReadOnlyMatrixXD AsReadOnlyMatrix() => _readOnlyMatrix.AsReadOnlyMatrix();
    public ReadOnlyTensorSpan<double> AsReadOnlyTensorSpan() => _readOnlyMatrix.AsReadOnlyTensorSpan();
    public ReadOnlyMatrixBorrow Borrow() => _readOnlyMatrix.BorrowReadOnly();
    public ReadOnlyMatrixBorrow BorrowReadOnly() => _readOnlyMatrix.BorrowReadOnly();
    public void Dispose() => _readOnlyMatrix.Dispose();
    public override string ToString() => _readOnlyMatrix.ToString();
}
