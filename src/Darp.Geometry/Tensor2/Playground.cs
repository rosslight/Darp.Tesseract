using System.Numerics.Tensors;

namespace Darp.Geometry.Tensor2;

/// <summary>Editable examples of matrix specializations and mutable/read-only views.</summary>
public static class Playground
{
    public static void Run(TextWriter? output = null)
    {
        output ??= Console.Out;

        double[] buffer = [1, 2, 3];
        var vector = Vector3D.Map(buffer);
        ReadOnlyVector3D readOnly = vector; // Descriptor conversion; no coefficient copy.
        ReadOnlyMemory<double> readOnlyMemory = buffer;
        var mappedReadOnly = ReadOnlyVector3D.Map(readOnlyMemory);
        var snapshot = readOnly.Clone();
        TensorSpan<double> writableTensor = vector.AsTensorSpan();
        ReadOnlyTensorSpan<double> readableTensor = readOnly.AsReadOnlyTensorSpan();
        writableTensor[0, 0] = 1; // Explicit array-backed span, shape [3,1].
        output.WriteLine($"Read-only tensor coefficient: {readableTensor[0, 0]}");
        IReadOnlyMatrixD matrixInterface = readOnly;
        output.WriteLine($"Interface norm: {MatrixOperations.Norm(matrixInterface)}");
        var externalMatrix = ReadOnlyMatrixXD.Map(new double[] { 3, 4, 0 }, 3, 1);
        output.WriteLine($"External memory norm: {MatrixOperations.Norm(externalMatrix)}");
        output.WriteLine($"Mixed descriptor dot: {MatrixOperations.Dot(vector, externalMatrix)}");
        var singleton = new VectorXD(2.0);
        output.WriteLine($"Singleton vector norm: {MatrixOperations.Norm(singleton)}");
        // MatrixOperations.Dot(vector, MatrixXD.Identity(3)); // Throws: expected column vectors.
        // MatrixOperations.Norm(readableTensor); // Does not compile: a span carries no owner.
        // TensorSpan<double> invalid = readOnly.AsTensorSpan(); // No writable access.
        vector.Z = 7;
        output.WriteLine($"Mutable: {vector}; read-only view: {readOnly}; mapped read-only: {mappedReadOnly}");
        output.WriteLine($"Independent snapshot: {snapshot}");
        // readOnly.Z = 9; // Does not compile: read-only views have no setters.

        MatrixXD column = vector.AsMatrix(); // 3 by 1, sharing the same buffer.
        MatrixXD row = vector.Transposed();  // 1 by 3, also sharing the buffer.
        row[0, 0] = 4;
        output.WriteLine($"Edited through transpose: {vector}");
        output.WriteLine($"Inner product (1 by 1 matrix): {row * column}");
        output.WriteLine($"Outer product (3 by 3 matrix):\n{column * row}");
        output.WriteLine($"Same inner product as scalar: {vector.Dot(readOnly)}");
        output.WriteLine($"Shared norm implementation: {vector.Norm()}; read-only: {readOnly.Norm()}");
        Vector3D unit = vector.Normalized(); // Typed forwarding method preserves Vector3D.
        output.WriteLine($"Normalized vector: {unit}; squared norm: {unit.SquaredNorm()}");
        output.WriteLine($"Mixed vector specializations: {vector.Dot(readOnly.AsVector())}");
        output.WriteLine($"Interface-based multiply with different descriptor types:\n{MatrixOperations.Multiply(column, readOnly.Transposed())}");
        var stridedVector = VectorXD.Map(new double[] { 1, 99, 2, 99, 3 }, 3, stride: 2);
        output.WriteLine($"Strided norm: {stridedVector.Norm()}; contiguous: {stridedVector.AsReadOnlyTensorSpan().Strides[0] == 1}");

        var matrix = MatrixXD.FromArray(new double[,] { { 1, 2, 3 }, { 4, 5, 6 } });
        ReadOnlyMatrixXD readOnlyReadOnlyMatrix = matrix;
        var readOnlyBlock = readOnlyReadOnlyMatrix.Block(0, 1, 2, 2);
        matrix[0, 1] = 20;
        output.WriteLine($"Read-only block sees the edit:\n{readOnlyBlock}");
        output.WriteLine($"Read-only matrix times writable vector: {readOnlyReadOnlyMatrix * vector.AsVector()}");
        output.WriteLine($"Writable matrix times read-only vector: {matrix * readOnly.AsVector()}");
        output.WriteLine($"Mixed sum:\n{matrix + readOnlyReadOnlyMatrix}");
        output.WriteLine($"Reverse mixed sum:\n{readOnlyReadOnlyMatrix + matrix}");
        output.WriteLine($"Shared second row as column vector: {matrix.Row(1).Transposed().AsVector()}");

        double[] rowMajor = [1, 2, 3, 4, 5, 6];
        var mappedMatrix = MatrixXD.Map(rowMajor, 2, 3, columnStride: 1, rowStride: 3);
        output.WriteLine($"Row-major mapped matrix:\n{mappedMatrix}");
        output.WriteLine($"Transpose view:\n{mappedMatrix.Transposed()}");

        var rotation = QuaternionD.FromAxisAngle(Vector3D.UnitZ, Math.PI / 2);
        ReadOnlyQuaternionD readOnlyRotation = rotation;
        output.WriteLine($"Quaternion composition: {rotation * readOnlyRotation}");
        output.WriteLine($"Reverse quaternion composition: {readOnlyRotation * rotation}");
        var transform = new Isometry3D(rotation, new Vector3D(1, 2, 3));
        ReadOnlyIsometry3D pose = transform;
        var point = Vector3D.UnitX;
        output.WriteLine($"World point: {pose * point}"); // Approximately [1,3,3].
        output.WriteLine($"Back in tool: {pose.Inverse() * (pose * point)}");
        output.WriteLine($"Mixed transform composition:\n{transform * pose}");
        output.WriteLine($"Reverse mixed transform composition:\n{pose * transform}");
        output.WriteLine($"Mixed vector sum: {point + readOnly}");
        output.WriteLine($"Reverse mixed vector sum: {readOnly + point}");

        var rotationMatrix = rotation.ToRotationMatrix();
        ReadOnlyMatrix3D readOnlyRotationMatrix = rotationMatrix;
        output.WriteLine($"Mixed rotation matrices:\n{rotationMatrix * readOnlyRotationMatrix}");
        output.WriteLine($"Reverse mixed rotation matrices:\n{readOnlyRotationMatrix * rotationMatrix}");
        output.WriteLine($"Read-only rotation times vector: {readOnlyRotationMatrix * point}");
        output.WriteLine($"Rotation matrix round trip: {readOnlyRotationMatrix.ToQuaternion()}");
        output.WriteLine($"Half rotation: {QuaternionD.Slerp(QuaternionD.Identity, readOnlyRotation, 0.5)}");

        var translation = transform.Translation;
        translation.Z = 9;
        output.WriteLine($"Read-only translation observes update: {pose.Translation}");
        var saved = pose.Clone();
        translation.Z = 10;
        output.WriteLine($"Independent transform translation: {saved.Translation}");
        // pose.Matrix[0, 3] = 100; // Does not compile; subviews remain read-only.
    }
}
