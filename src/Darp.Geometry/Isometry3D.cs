using System.Buffers;
using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>A mutable rigid transform. The caller preserves a proper rotation and last row [0,0,0,1].</summary>
public readonly struct Isometry3D : IMatrixD, IReadOnlyIsometry3D
{
    private readonly MatrixData _data;
    internal static readonly MatrixData ZeroData = MatrixData.ReadOnlyZero(4, 4);
    private MatrixData Data => _data.Storage is null ? ZeroData : _data;

    private Isometry3D(MatrixData data) => _data = data;

    internal MatrixStorage Storage => Data.Storage;
    internal MatrixLayout Layout => Data.Layout;
    public int Rows => Data.Rows;
    public int Columns => Data.Columns;
    public int RowStride => Data.RowStride;
    public int ColumnStride => Data.ColumnStride;

    public ReadOnlyMatrixXD AsReadOnlyMatrix() => Data.AsReadOnlyMatrix();

    TensorSpanLease IReadOnlyMatrixD.AcquireReadOnlyTensorSpan(out ReadOnlyTensorSpan<double> span) =>
        Data.AcquireReadOnlyTensorSpan(out span);

    MemoryHandle IReadOnlyMatrixD.Pin() => Data.Pin();

    public double this[int row, int column] => Data[row, column];

    public ReadOnlyMatrixXD Block(int row, int column, int rows, int columns) => Data.BlockReadOnly(row, column, rows, columns);

    public ReadOnlyMatrixXD Transposed() => Data.AsTransposedLayout();

    public ReadOnlyVectorXD AsVector() => Data.AsVectorLayout();

    public override string ToString() => MatrixExtensions.Format(this);

    internal Isometry3D(MatrixStorage storage, MatrixLayout layout)
        : this(new MatrixData(storage, layout)) { }

    private Isometry3D(Memory<double> memory, MatrixLayout layout)
        : this(new MatrixData(memory, layout)) { }

    public Isometry3D()
        : this(new MatrixData(4, 4))
    {
        for (int i = 0; i < 4; i++)
            Data.Set(i, i, 1);
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
    public static Isometry3D View(MatrixXD matrix) => new(matrix.Storage, matrix.Layout.Require(4, 4));

    /// <summary>Copies a matrix known by the caller to represent a rigid transform.</summary>
    public static Isometry3D FromMatrix(IReadOnlyMatrixD matrix)
    {
        using var matrixLease = matrix.GetReadOnlyTensorSpan(out var matrixSpan);
        MatrixShape.RequireSize(matrixSpan, 4, 4);
        return View(matrix.Clone());
    }

    public ReadOnlyIsometry3D AsReadOnly() => new ReadOnlyIsometry3D(Storage, Layout);

    public MatrixXD AsMatrix() => Data.AsMatrix();

    public MatrixXD Matrix => Data.AsMatrix();

    TensorSpanLease IMatrixD.AcquireTensorSpan(out TensorSpan<double> span) => Data.AcquireWritableTensorSpan(out span);

    public Vector3D Translation
    {
        get => new(Storage, Layout.Block(0, 3, 3, 1));
        set => SetTranslation(value);
    }
    ReadOnlyVector3D IReadOnlyIsometry3D.Translation
    {
        get => new ReadOnlyVector3D(Storage, Layout.Block(0, 3, 3, 1));
    }
    public Matrix3D RotationMatrix
    {
        get => new(Storage, Layout.Block(0, 0, 3, 3));
        set => SetRotationMatrix(value);
    }
    ReadOnlyMatrix3D IReadOnlyIsometry3D.RotationMatrix
    {
        get => new ReadOnlyMatrix3D(Storage, Layout.Block(0, 0, 3, 3));
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
        Data.Set(0, 3, x);
        Data.Set(1, 3, y);
        Data.Set(2, 3, z);
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
            Data.Set(r, c, copy[c * 3 + r]);
    }

    public static Isometry3D operator *(Isometry3D left, IReadOnlyIsometry3D right) => left.Multiply(right);

    public static Vector3D operator *(Isometry3D transform, IReadOnlyVector3D point) => transform.TransformPoint(point);
}
