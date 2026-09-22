namespace Darp.Geometry;

/// <summary>Shared operations retaining each input allocation until its tensor kernel finishes.</summary>
public static class MatrixOperations
{

    public static double Norm(IReadOnlyMatrixD matrix)
    {
        using var view = matrix.AsReadOnlyMatrix();
        return TensorKernels.Norm(view.AsReadOnlyTensorSpan());
    }

    public static double SquaredNorm(IReadOnlyMatrixD matrix)
    {
        using var view = matrix.AsReadOnlyMatrix();
        return TensorKernels.SquaredNorm(view.AsReadOnlyTensorSpan());
    }

    public static double InnerProduct(IReadOnlyMatrixD left, IReadOnlyMatrixD right)
    {
        using var a = left.AsReadOnlyMatrix();
        using var b = right.AsReadOnlyMatrix();
        return TensorKernels.InnerProduct(a.AsReadOnlyTensorSpan(), b.AsReadOnlyTensorSpan());
    }

    public static double Dot(IReadOnlyMatrixD left, IReadOnlyMatrixD right)
    {
        using var a = left.AsReadOnlyMatrix();
        using var b = right.AsReadOnlyMatrix();
        return TensorKernels.Dot(a.AsReadOnlyTensorSpan(), b.AsReadOnlyTensorSpan());
    }

    public static MatrixXD Normalized(IReadOnlyMatrixD matrix)
    {
        using var view = matrix.AsReadOnlyMatrix();
        return TensorKernels.Normalized(view.AsReadOnlyTensorSpan());
    }

    public static MatrixXD Add(IReadOnlyMatrixD left, IReadOnlyMatrixD right)
    {
        using var a = left.AsReadOnlyMatrix();
        using var b = right.AsReadOnlyMatrix();
        return TensorKernels.Add(a.AsReadOnlyTensorSpan(), b.AsReadOnlyTensorSpan());
    }

    public static MatrixXD Subtract(IReadOnlyMatrixD left, IReadOnlyMatrixD right)
    {
        using var a = left.AsReadOnlyMatrix();
        using var b = right.AsReadOnlyMatrix();
        return TensorKernels.Subtract(a.AsReadOnlyTensorSpan(), b.AsReadOnlyTensorSpan());
    }

    public static MatrixXD Multiply(IReadOnlyMatrixD left, IReadOnlyMatrixD right)
    {
        using var a = left.AsReadOnlyMatrix();
        using var b = right.AsReadOnlyMatrix();
        return TensorKernels.Multiply(a.AsReadOnlyTensorSpan(), b.AsReadOnlyTensorSpan());
    }

    public static MatrixXD Scale(IReadOnlyMatrixD matrix, double scalar)
    {
        using var view = matrix.AsReadOnlyMatrix();
        return TensorKernels.Scale(view.AsReadOnlyTensorSpan(), scalar);
    }

    public static MatrixXD Divide(IReadOnlyMatrixD matrix, double scalar)
    {
        using var view = matrix.AsReadOnlyMatrix();
        return TensorKernels.Divide(view.AsReadOnlyTensorSpan(), scalar);
    }

    public static MatrixXD Lerp(IReadOnlyMatrixD left, IReadOnlyMatrixD right, double amount)
    {
        using var a = left.AsReadOnlyMatrix();
        using var b = right.AsReadOnlyMatrix();
        return TensorKernels.Lerp(a.AsReadOnlyTensorSpan(), b.AsReadOnlyTensorSpan(), amount);
    }

    public static Vector3D Cross(IReadOnlyMatrixD left, IReadOnlyMatrixD right)
    {
        using var a = left.AsReadOnlyMatrix();
        using var b = right.AsReadOnlyMatrix();
        return TensorKernels.Cross(a.AsReadOnlyTensorSpan(), b.AsReadOnlyTensorSpan());
    }

    public static double Determinant3x3(IReadOnlyMatrixD matrix)
    {
        using var view = matrix.AsReadOnlyMatrix();
        return TensorKernels.Determinant3x3(view.AsReadOnlyTensorSpan());
    }
}

