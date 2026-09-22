using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>A mutable X,Y,Z,W quaternion owning one reference to its coefficient storage.</summary>
public sealed class QuaternionD : GeometryObject, IMatrixD, IReadOnlyQuaternionD
{
    internal QuaternionD(MatrixStorage storage, MatrixLayout layout, int offset = 0) : base(storage, layout, offset) { }

    private QuaternionD(Memory<double> memory, MatrixLayout layout) : base(memory, layout) { }
    public QuaternionD(double x, double y, double z, double w) : base(4, 1) { X = x; Y = y; Z = z; W = w; }
    internal static QuaternionD FromOwnedMatrix(MatrixXD matrix) { using (matrix) return new(matrix.Storage, matrix.Layout.Require(4, 1), matrix.Offset); }
    public static QuaternionD Identity => new(0, 0, 0, 1);
    public static QuaternionD CreateFromMemory(Memory<double> memory, int stride = 1) =>
        new(memory, MatrixLayout.Create(4, 1, stride, checked(4 * stride)));
    public static QuaternionD FromMatrix(MatrixXD matrix) => new(matrix.Storage, matrix.Layout.Require(4, 1), matrix.Offset);
    public double X { get => base[0, 0]; set => SetValue(0, 0, value); }
    public double Y { get => base[1, 0]; set => SetValue(1, 0, value); }
    public double Z { get => base[2, 0]; set => SetValue(2, 0, value); }
    public double W { get => base[3, 0]; set => SetValue(3, 0, value); }
    public VectorXD Coefficients => new(Storage, Layout, Offset);
    IReadOnlyVectorXD IReadOnlyQuaternionD.Coefficients => new ReadOnlyVectorXD(Storage, Layout, Offset);
    public IReadOnlyQuaternionD AsReadOnly() => new ReadOnlyQuaternionD(Storage, Layout, Offset);
    public MatrixXD AsMatrix() => RetainMatrix();
    public TensorSpan<double> AsTensorSpan() => WritableSpan();
    public static QuaternionD FromAxisAngle(IReadOnlyVector3D axis, double angle) => QuaternionMath.FromAxisAngle(axis, angle);
    public static QuaternionD FromRotationMatrix(IReadOnlyMatrix3D matrix) => QuaternionMath.FromRotationMatrix(matrix);
    public static QuaternionD operator *(QuaternionD a, IReadOnlyQuaternionD b) => GeometryExtensions.Multiply(a, b);
    public static Vector3D operator *(QuaternionD rotation, IReadOnlyVector3D vector) => rotation.Rotate(vector);
    public static QuaternionD operator -(QuaternionD value) => new(-value.X, -value.Y, -value.Z, -value.W);
    public override string ToString() => $"(X={X}, Y={Y}, Z={Z}, W={W})";
}
