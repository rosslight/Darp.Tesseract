using System.Buffers;
using Darp.Geometry;
using Shouldly;
using Xunit;

namespace Darp.Tesseract.Native.IntegrationTests;

public sealed class GeometryOwnershipTests
{
    [Fact]
    public void TensorAccessDoesNotPinOrDisposeSharedStorage()
    {
        var owner = new CountingOwner();
        var matrix = MatrixXD.CreateFromMemoryWithOwner(owner.Memory, owner, 2, 2);
        var column = matrix.Column(0);
        var readOnly = matrix.AsReadOnly();
        using (matrix.GetTensorSpan(out var values))
        {
            values[1, 1] = 9;
            owner.PinCount.ShouldBe(0);
        }
        column[0] = 7;
        readOnly[0, 0].ShouldBe(7);
        readOnly[1, 1].ShouldBe(9);
        owner.DisposeCount.ShouldBe(0);
        using var pin = matrix.Pin();
        owner.PinCount.ShouldBe(1);
    }

    [Fact]
    public void AssignmentAndReadOnlyViewsShareCoefficientsButClonesDoNot()
    {
        var source = new Vector3D(1, 2, 3);
        IReadOnlyVector3D alias = source;
        var readOnly = source.AsReadOnly();
        (readOnly is IMatrixD).ShouldBeFalse();
        var snapshot = readOnly.Clone();
        source.X = 8;
        alias.X.ShouldBe(8);
        readOnly.X.ShouldBe(8);
        snapshot.X.ShouldBe(1);
        using var lease = readOnly.GetReadOnlyTensorSpan(out var values);
        values[0, 0].ShouldBe(8);
    }

    [Fact]
    public void NestedReadOnlyViewsPreserveOffsetsAndStridesWithSharedStorage()
    {
        double[] values = [0, 1, 2, 3, 10, 11, 12, 13, 20, 21, 22, 23];
        var matrix = MatrixXD.CreateFromMemory(values, 3, 4, columnStride: 1, rowStride: 4);
        var readOnly = matrix.AsReadOnly();
        var block = readOnly.Block(1, 1, 2, 3);
        var transpose = block.Transposed();
        var column = transpose.Column(1);

        (column is IMatrixD).ShouldBeFalse();
        column.ToArray().ShouldBe(new double[] { 21, 22, 23 });
        values[10] = 222;
        column[1].ShouldBe(222);
        using var lease = column.GetReadOnlyTensorSpan(out var span);
        span[0, 0].ShouldBe(21);
        span[1, 0].ShouldBe(222);
        span[2, 0].ShouldBe(23);
    }

    [Fact]
    public void FailedOwnershipTransferReleasesOwner()
    {
        var owner = new CountingOwner();
        Should.Throw<ArgumentException>(() => MatrixXD.CreateFromMemoryWithOwner(owner.Memory[..1], owner, 2, 2));
        owner.DisposeCount.ShouldBe(1);
        var invalidShapeOwner = new CountingOwner();
        Should.Throw<ArgumentOutOfRangeException>(() => MatrixXD.CreateFromMemoryWithOwner(invalidShapeOwner.Memory, invalidShapeOwner, -1, 2));
        invalidShapeOwner.DisposeCount.ShouldBe(1);
    }

    [Fact]
    public void GeometryOperationsPreserveOrientationAndIndependentResults()
    {
        var x = Vector3D.UnitX;
        var z = Vector3D.UnitZ;
        var cross = z.Cross(x);
        cross.Y.ShouldBe(1);
        var rotation = QuaternionD.FromAxisAngle(z, Math.PI / 2);
        var offset = new Vector3D(1, 2, 3);
        var pose = new Isometry3D(rotation, offset);
        var world = pose * x;
        world.X.ShouldBe(1, 1e-12);
        world.Y.ShouldBe(3, 1e-12);
        world.Z.ShouldBe(3, 1e-12);
        var inverse = pose.Inverse();
        var roundTrip = inverse * world;
        roundTrip.X.ShouldBe(1, 1e-12);
        roundTrip.Y.ShouldBe(0, 1e-12);
        roundTrip.Z.ShouldBe(0, 1e-12);
        var matrix = rotation.ToRotationMatrix();
        matrix.Determinant().ShouldBe(1, 1e-12);
        var transposed = matrix.Transposed();
        var product = MatrixExtensions.Multiply(matrix, (IReadOnlyMatrixD)transposed);
        for (int row = 0; row < 3; row++)
            for (int column = 0; column < 3; column++)
                product[row, column].ShouldBe(row == column ? 1 : 0, 1e-12);
        world.Y.ShouldBe(3, 1e-12);
    }

    [Fact]
    public void EmptyAlgebraPreservesShapesAndProducesZeroInnerProducts()
    {
        var empty = new VectorXD(0);
        empty.Norm().ShouldBe(0);
        empty.Dot(empty).ShouldBe(0);
        var sum = empty + empty;
        sum.Count.ShouldBe(0);
        var left = new MatrixXD(2, 0);
        var right = new MatrixXD(0, 3);
        var product = left * right;
        product.Rows.ShouldBe(2);
        product.Columns.ShouldBe(3);
        product.Norm().ShouldBe(0);
    }

    [Theory]
    [InlineData(1e200)]
    [InlineData(1e-200)]
    public void NormalizationIsStableAcrossContiguousAndStridedLayouts(double magnitude)
    {
        var contiguous = new VectorXD(magnitude, magnitude);
        var strided = VectorXD.CreateFromMemory(new double[] { magnitude, 99, magnitude }, 2, 2);
        (contiguous.Norm() / magnitude).ShouldBe(Math.Sqrt(2), 1e-12);
        (strided.Norm() / magnitude).ShouldBe(Math.Sqrt(2), 1e-12);
        var unit = contiguous.Normalized();
        var stridedUnit = strided.Normalized();
        for (int index = 0; index < 2; index++)
        {
            unit[index].ShouldBe(Math.Sqrt(0.5), 1e-12);
            stridedUnit[index].ShouldBe(Math.Sqrt(0.5), 1e-12);
        }
    }

    private sealed class CountingOwner : MemoryManager<double>
    {
        public int DisposeCount { get; private set; }
        public int PinCount { get; private set; }
        private readonly double[] _values = [1, 2, 3, 4];
        public override Span<double> GetSpan()
        {
            ObjectDisposedException.ThrowIf(DisposeCount != 0, this);
            return _values;
        }
        public override MemoryHandle Pin(int elementIndex = 0)
        {
            PinCount++;
            return _values.AsMemory(elementIndex).Pin();
        }
        public override void Unpin() { }
        protected override void Dispose(bool disposing) => DisposeCount++;
    }
}
