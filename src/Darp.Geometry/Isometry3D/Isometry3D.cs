using System.Numerics.Tensors;

namespace Darp.Geometry;

/// <summary>A writable rigid-transform view over shared matrix storage.</summary>
/// <remarks>Copying an isometry copies its view, not its coefficients. The default value is a zero, read-only matrix and is not an identity transform.</remarks>
public readonly partial struct Isometry3D : IMatrixD<Isometry3D>
{
    internal static readonly MatrixData s_zeroData = MatrixData.ReadOnlyZero(4, 4);
    private MatrixData Data => field.Storage is null ? s_zeroData : field;

    internal Isometry3D(in MatrixData data) => Data = data.Require(4, 4);

    internal MatrixStorage Storage => Data.Storage;
    internal MatrixLayout Layout => Data.Layout;
    public int Rows => Data.Rows;
    public int Columns => Data.Columns;
    public int RowStride => Data.RowStride;
    public int ColumnStride => Data.ColumnStride;

    TensorSpanLease IReadOnlyMatrixD<Isometry3D>.GetReadOnlyTensorSpan(out ReadOnlyTensorSpan<double> span) =>
        Data.AcquireReadOnlyTensorSpan(out span);

    static Isometry3D IReadOnlyMatrixD<Isometry3D>.Create(in MatrixData data) => new(data);

    TensorSpanLease IMatrixD<Isometry3D>.GetTensorSpan(out TensorSpan<double> span) =>
        Data.AcquireWritableTensorSpan(out span);

    public double this[int row, int column]
    {
        get => Data.Get(row, column);
        set => Data.Set(row, column, value);
    }

    public double this[Index row, Index column]
    {
        get => Data.Get(row, column);
        set => Data.Set(row, column, value);
    }

    public override string ToString() => Matrix.Format(this);

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

    public Isometry3D(ReadOnlyQuaternionD rotation, ReadOnlyVector3D translation)
        : this()
    {
        SetRotation(rotation);
        SetTranslation(translation);
    }

    public static Isometry3D Identity => new();

    public static Isometry3D CreateFromMemory(Memory<double> memory, int columnStride = 4) =>
        new(memory, MatrixLayout.Create(4, 4, 1, columnStride));

    /// <summary>Creates a shared view of a matrix known by the caller to represent a rigid transform.</summary>
    public static Isometry3D View(in MatrixXD matrix) => new(matrix.Data.Require(4, 4));

    /// <summary>Copies a matrix known by the caller to represent a rigid transform.</summary>
    public static Isometry3D FromMatrix<TM>(in TM matrix)
        where TM : IReadOnlyMatrixD<TM>
    {
        using var matrixLease = matrix.GetReadOnlyTensorSpan(out var matrixSpan);
        MatrixShape.RequireSize(matrixSpan, 4, 4);
        var copy = new MatrixXD(4, 4);
        for (int r = 0; r < 4; r++)
        for (int c = 0; c < 4; c++)
            copy[r, c] = matrix[r, c];
        return View(copy);
    }

    public ReadOnlyIsometry3D AsReadOnly() => new(Data);

    public MatrixXD AsMatrix() => new(Data);

    public Vector3D Translation
    {
        get => new(Storage, Layout.Block(0, 3, 3, 1));
        set => SetTranslation(value);
    }
    public Matrix3D RotationMatrix
    {
        get => new(Storage, Layout.Block(0, 0, 3, 3));
        set => SetRotationMatrix(value);
    }

    /// <summary>The getter computes an independent quaternion; assigning it updates the matrix.</summary>
    public QuaternionD Rotation
    {
        get
        {
            var matrix = RotationMatrix;
            return matrix.AsReadOnly().ToQuaternion();
        }
        set => SetRotation(value);
    }

    public void SetTranslation(ReadOnlyVector3D value)
    {
        this[0, 3] = value.X;
        this[1, 3] = value.Y;
        this[2, 3] = value.Z;
    }

    public void SetRotation(ReadOnlyQuaternionD value)
    {
        var rotation = value.ToRotationMatrix();
        SetRotationMatrix(rotation);
    }

    public void SetRotationMatrix(ReadOnlyMatrix3D value)
    {
        // Snapshot first so overlapping transpose views are safe.
        Span<double> copy = stackalloc double[9];
        for (int c = 0; c < 3; c++)
        for (int r = 0; r < 3; r++)
            copy[c * 3 + r] = value[r, c];
        for (int c = 0; c < 3; c++)
        for (int r = 0; r < 3; r++)
            this[r, c] = copy[c * 3 + r];
    }
}
