using System.Buffers;
using Darp.Geometry;
using Shouldly;
using Xunit;

namespace Darp.Tesseract.Native.IntegrationTests;

public sealed class GeometryOwnershipTests
{
    [Fact]
    public void ViewsAndPinReleaseTransferredOwnerOnlyAfterLastRelease()
    {
        var owner = new CountingOwner();
        using var matrix = MatrixXD.CreateFromMemoryWithOwner(owner.Memory, owner, 2, 2);
        using var column = matrix.Column(0);
        using var readOnly = matrix.AsReadOnly();
        using var access = matrix.AsMatrix();
        var pin = matrix.Pin();
        var copiedPin = pin;
        matrix.Dispose();
        Should.Throw<ObjectDisposedException>(() => matrix[0, 0]);
        column[0] = 7;
        readOnly[0, 0].ShouldBe(7);
        access.AsTensorSpan()[1, 1] = 9;
        readOnly[1, 1].ShouldBe(9);
        column.Dispose();
        readOnly.Dispose();
        access.Dispose();
        owner.DisposeCount.ShouldBe(0);
        pin.Dispose();
        copiedPin.Dispose();
        owner.DisposeCount.ShouldBe(1);
        matrix.Dispose();
        owner.DisposeCount.ShouldBe(1);
    }

    [Fact]
    public void AssignmentAliasesButReadOnlyViewAndCloneHaveIndependentLifetimes()
    {
        using var source = new Vector3D(1, 2, 3);
        IReadOnlyVector3D alias = source;
        using var readOnly = source.AsReadOnly();
        (readOnly is IMatrixD).ShouldBeFalse();
        (readOnly is Vector3D).ShouldBeFalse();
        using var snapshot = readOnly.Clone();
        source.X = 8;
        readOnly.X.ShouldBe(8);
        snapshot.X.ShouldBe(1);
        alias.Dispose();
        Should.Throw<ObjectDisposedException>(() => source.X);
        readOnly.X.ShouldBe(8);
        using var access = readOnly.AsReadOnlyMatrix();
        readOnly.Dispose();
        access.AsReadOnlyTensorSpan()[0, 0].ShouldBe(8);
        access.Dispose();
        Should.Throw<ObjectDisposedException>(() => access.Rows);
    }

    [Fact]
    public void NestedReadOnlyViewsPreserveOffsetsAndStridesAfterParentsAreDisposed()
    {
        double[] values = [0, 1, 2, 3, 10, 11, 12, 13, 20, 21, 22, 23];
        using var matrix = MatrixXD.CreateFromMemory(values, 3, 4, columnStride: 1, rowStride: 4);
        using var readOnly = matrix.AsReadOnly();
        using var block = readOnly.Block(1, 1, 2, 3);
        using var transpose = block.Transposed();
        using var column = transpose.Column(1);
        matrix.Dispose();
        readOnly.Dispose();
        block.Dispose();
        transpose.Dispose();

        (column is IMatrixD).ShouldBeFalse();
        column.ToArray().ShouldBe(new double[] { 21, 22, 23 });
        values[10] = 222;
        column[1].ShouldBe(222);
        var span = column.AsReadOnlyTensorSpan();
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
        using var x = Vector3D.UnitX;
        using var z = Vector3D.UnitZ;
        using var cross = z.Cross(x);
        cross.Y.ShouldBe(1);
        using var rotation = QuaternionD.FromAxisAngle(z, Math.PI / 2);
        using var offset = new Vector3D(1, 2, 3);
        using var pose = new Isometry3D(rotation, offset);
        using var world = pose * x;
        world.X.ShouldBe(1, 1e-12);
        world.Y.ShouldBe(3, 1e-12);
        world.Z.ShouldBe(3, 1e-12);
        using var inverse = pose.Inverse();
        using var roundTrip = inverse * world;
        roundTrip.X.ShouldBe(1, 1e-12);
        roundTrip.Y.ShouldBe(0, 1e-12);
        roundTrip.Z.ShouldBe(0, 1e-12);
        using var matrix = rotation.ToRotationMatrix();
        matrix.Determinant().ShouldBe(1, 1e-12);
        using var transposed = matrix.Transposed();
        using var product = MatrixExtensions.Multiply(matrix, (IReadOnlyMatrixD)transposed);
        for (int row = 0; row < 3; row++)
            for (int column = 0; column < 3; column++)
                product[row, column].ShouldBe(row == column ? 1 : 0, 1e-12);
        pose.Dispose();
        world.Y.ShouldBe(3, 1e-12);
    }

    [Fact]
    public void EmptyAlgebraPreservesShapesAndProducesZeroInnerProducts()
    {
        using var empty = new VectorXD(0);
        empty.Norm().ShouldBe(0);
        empty.Dot(empty).ShouldBe(0);
        using var sum = empty + empty;
        sum.Count.ShouldBe(0);
        using var left = new MatrixXD(2, 0);
        using var right = new MatrixXD(0, 3);
        using var product = left * right;
        product.Rows.ShouldBe(2);
        product.Columns.ShouldBe(3);
        product.Norm().ShouldBe(0);
    }

    [Theory]
    [InlineData(1e200)]
    [InlineData(1e-200)]
    public void NormalizationIsStableAcrossContiguousAndStridedLayouts(double magnitude)
    {
        using var contiguous = new VectorXD(magnitude, magnitude);
        using var strided = VectorXD.CreateFromMemory(new double[] { magnitude, 99, magnitude }, 2, 2);
        (contiguous.Norm() / magnitude).ShouldBe(Math.Sqrt(2), 1e-12);
        (strided.Norm() / magnitude).ShouldBe(Math.Sqrt(2), 1e-12);
        using var unit = contiguous.Normalized();
        using var stridedUnit = strided.Normalized();
        for (int index = 0; index < 2; index++)
        {
            unit[index].ShouldBe(Math.Sqrt(0.5), 1e-12);
            stridedUnit[index].ShouldBe(Math.Sqrt(0.5), 1e-12);
        }
    }

    private sealed class CountingOwner : MemoryManager<double>
    {
        public int DisposeCount { get; private set; }
        private readonly double[] _values = [1, 2, 3, 4];
        public override Span<double> GetSpan()
        {
            ObjectDisposedException.ThrowIf(DisposeCount != 0, this);
            return _values;
        }
        public override MemoryHandle Pin(int elementIndex = 0) => _values.AsMemory(elementIndex).Pin();
        public override void Unpin() { }
        protected override void Dispose(bool disposing) => DisposeCount++;
    }
}
