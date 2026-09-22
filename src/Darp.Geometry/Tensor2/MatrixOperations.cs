namespace Darp.Geometry.Tensor2;

/// <summary>Shared operations retaining each input memory manager until its tensor kernel finishes.</summary>
public static class MatrixOperations
{

    public static double Norm<TMatrix>(TMatrix matrix) where TMatrix : IReadOnlyMatrixD
    {
        var view = matrix.AsReadOnlyMatrix();
        try
        {
            return TensorKernels.Norm(view.AsReadOnlyTensorSpan());
        }
        finally
        {
            view.KeepAlive();
        }
    }

    public static double SquaredNorm<TMatrix>(TMatrix matrix) where TMatrix : IReadOnlyMatrixD
    {
        var view = matrix.AsReadOnlyMatrix();
        try
        {
            return TensorKernels.SquaredNorm(view.AsReadOnlyTensorSpan());
        }
        finally
        {
            view.KeepAlive();
        }
    }

    public static double InnerProduct<TLeft, TRight>(TLeft left, TRight right)
        where TLeft : IReadOnlyMatrixD
        where TRight : IReadOnlyMatrixD
    {
        var a = left.AsReadOnlyMatrix();
        var b = right.AsReadOnlyMatrix();
        try
        {
            return TensorKernels.InnerProduct(a.AsReadOnlyTensorSpan(), b.AsReadOnlyTensorSpan());
        }
        finally
        {
            a.KeepAlive();
            b.KeepAlive();
        }
    }

    public static double Dot<TLeft, TRight>(TLeft left, TRight right)
        where TLeft : IReadOnlyMatrixD
        where TRight : IReadOnlyMatrixD
    {
        var a = left.AsReadOnlyMatrix();
        var b = right.AsReadOnlyMatrix();
        try
        {
            return TensorKernels.Dot(a.AsReadOnlyTensorSpan(), b.AsReadOnlyTensorSpan());
        }
        finally
        {
            a.KeepAlive();
            b.KeepAlive();
        }
    }

    public static MatrixXD Normalized<TMatrix>(TMatrix matrix) where TMatrix : IReadOnlyMatrixD
    {
        var view = matrix.AsReadOnlyMatrix();
        try
        {
            return TensorKernels.Normalized(view.AsReadOnlyTensorSpan());
        }
        finally
        {
            view.KeepAlive();
        }
    }

    public static MatrixXD Add<TLeft, TRight>(TLeft left, TRight right)
        where TLeft : IReadOnlyMatrixD
        where TRight : IReadOnlyMatrixD
    {
        var a = left.AsReadOnlyMatrix();
        var b = right.AsReadOnlyMatrix();
        try
        {
            return TensorKernels.Add(a.AsReadOnlyTensorSpan(), b.AsReadOnlyTensorSpan());
        }
        finally
        {
            a.KeepAlive();
            b.KeepAlive();
        }
    }

    public static MatrixXD Subtract<TLeft, TRight>(TLeft left, TRight right)
        where TLeft : IReadOnlyMatrixD
        where TRight : IReadOnlyMatrixD
    {
        var a = left.AsReadOnlyMatrix();
        var b = right.AsReadOnlyMatrix();
        try
        {
            return TensorKernels.Subtract(a.AsReadOnlyTensorSpan(), b.AsReadOnlyTensorSpan());
        }
        finally
        {
            a.KeepAlive();
            b.KeepAlive();
        }
    }

    public static MatrixXD Multiply<TLeft, TRight>(TLeft left, TRight right)
        where TLeft : IReadOnlyMatrixD
        where TRight : IReadOnlyMatrixD
    {
        var a = left.AsReadOnlyMatrix();
        var b = right.AsReadOnlyMatrix();
        try
        {
            return TensorKernels.Multiply(a.AsReadOnlyTensorSpan(), b.AsReadOnlyTensorSpan());
        }
        finally
        {
            a.KeepAlive();
            b.KeepAlive();
        }
    }

    public static MatrixXD Scale<TMatrix>(TMatrix matrix, double scalar) where TMatrix : IReadOnlyMatrixD
    {
        var view = matrix.AsReadOnlyMatrix();
        try
        {
            return TensorKernels.Scale(view.AsReadOnlyTensorSpan(), scalar);
        }
        finally
        {
            view.KeepAlive();
        }
    }

    public static MatrixXD Divide<TMatrix>(TMatrix matrix, double scalar) where TMatrix : IReadOnlyMatrixD
    {
        var view = matrix.AsReadOnlyMatrix();
        try
        {
            return TensorKernels.Divide(view.AsReadOnlyTensorSpan(), scalar);
        }
        finally
        {
            view.KeepAlive();
        }
    }

    public static MatrixXD Lerp<TLeft, TRight>(TLeft left, TRight right, double amount)
        where TLeft : IReadOnlyMatrixD
        where TRight : IReadOnlyMatrixD
    {
        var a = left.AsReadOnlyMatrix();
        var b = right.AsReadOnlyMatrix();
        try
        {
            return TensorKernels.Lerp(a.AsReadOnlyTensorSpan(), b.AsReadOnlyTensorSpan(), amount);
        }
        finally
        {
            a.KeepAlive();
            b.KeepAlive();
        }
    }

    public static Vector3D Cross<TLeft, TRight>(TLeft left, TRight right)
        where TLeft : IReadOnlyMatrixD
        where TRight : IReadOnlyMatrixD
    {
        var a = left.AsReadOnlyMatrix();
        var b = right.AsReadOnlyMatrix();
        try
        {
            return TensorKernels.Cross(a.AsReadOnlyTensorSpan(), b.AsReadOnlyTensorSpan());
        }
        finally
        {
            a.KeepAlive();
            b.KeepAlive();
        }
    }

    public static double Determinant3x3<TMatrix>(TMatrix matrix) where TMatrix : IReadOnlyMatrixD
    {
        var view = matrix.AsReadOnlyMatrix();
        try
        {
            return TensorKernels.Determinant3x3(view.AsReadOnlyTensorSpan());
        }
        finally
        {
            view.KeepAlive();
        }
    }
}
