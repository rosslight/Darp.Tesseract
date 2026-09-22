# Darp.Geometry

Double-precision vectors, matrices, quaternions and rigid transforms for .NET 10.
The library uses `System.Numerics.Tensors` and works without the Tesseract native
runtime.

## Types and conventions

| Type | Shape | Use |
| --- | --- | --- |
| `VectorXD` | N by 1 | Joint values and arbitrary column vectors |
| `Vector3D` | 3 by 1 | Positions, directions and axes |
| `MatrixXD` | Rows by columns | General matrices and Jacobians |
| `Matrix3D` | 3 by 3 | Small matrices, including rotations |
| `QuaternionD` | 4 by 1, X/Y/Z/W | Rotations |
| `Isometry3D` | 4 by 4 | Rigid transforms |

Angles are radians. Transforms act on column vectors, and `a * b` applies `b`
first. Matrices allocated by the library use column-major storage.

```csharp
using Darp.Geometry;

var axis = Vector3D.UnitZ;
var rotation = QuaternionD.FromAxisAngle(axis, Math.PI / 2);
var translation = new Vector3D(1, 2, 3);
var transform = new Isometry3D(rotation, translation);
var point = Vector3D.UnitX;
var world = transform * point;
// world is approximately [1, 3, 3].
```

Named operations are extension methods on read-only interfaces. They work on
both concrete objects and interface values:

```csharp
var vector = new Vector3D(3, 0, 4);
var readable = vector.AsReadOnly();
var unit = readable.Normalized(); // Returns a new Vector3D.
double length = vector.Norm();          // 5
```

Import `Darp.Geometry` to use these extensions. Concrete types also provide
operators. Use named operations such as `Add`, `Multiply` and `Scale` when the
left operand is an interface.

## Views and copies

Geometry objects are not disposable. Views keep their shared storage alive;
`Clone()` and arithmetic create independent coefficients.

```csharp
var matrix = MatrixXD.Identity(3);
var column = matrix.Column(0);
var readable = column.AsReadOnly();
var snapshot = readable.Clone();
column[0] = 7;
Console.WriteLine(readable[0]); // 7
Console.WriteLine(snapshot[0]); // 1
```

Managed arrays follow normal GC lifetime. Native-backed results share an owner
that is released when the storage becomes unreachable; release is not deterministic.

Read-only means no writes through that view. Other aliases can still change its
values. `AsReadOnly()` returns an internal implementation without writable access.
Assigning a mutable object to `IReadOnlyVector3D` only changes the variable's type;
it neither creates a view nor removes the object's writable API.

## Mapping existing memory

`CreateFromMemory` maps memory without copying it:

```csharp
double[] values = [1, 2, 3, 4, 5, 6];
var matrix = MatrixXD.CreateFromMemory(
    values, rows: 2, columns: 3, columnStride: 1, rowStride: 3);

matrix[1, 2] = 9; // Also changes values[5].
```

Strides count doubles, not bytes. Positive strides support row-major, padded and
strided layouts. `Row(i)` has shape 1 by N, `Column(i)` is a column vector, and
`AsVector()` requires one column.

For externally owned memory, keep its owner valid while any view uses it.
`MatrixXD.CreateFromMemoryWithOwner` instead transfers a
`MemoryManager<double>` to the shared storage. The shared storage disposes it when collected.
Failed construction also disposes the transferred manager.

## Tensor spans

Use `MatrixMarshal` extensions for rank-two spans with `[row, column]` indexing:

```csharp
var matrix = MatrixXD.Identity(3);
using (matrix.GetTensorSpan(out var coefficients))
{
    coefficients[0, 1] = 2;
}

using (matrix.GetReadOnlyTensorSpan(out var coefficients))
{
    Console.WriteLine(coefficients[0, 1]);
}
```

The returned `TensorSpanLease` keeps shared storage alive without pinning managed
arrays. Dispose it after the span's last use. This is an access contract, not a
compiler-enforced relationship: do not use a span after disposing its lease.
Disposing the lease does not invalidate the matrix or its views.

Internal math follows the same scoped-access pattern. A lease neither prevents
concurrent mutation nor protects caller-owned memory from external disposal.

Use `MatrixMarshal.Pin(matrix)` only when native code needs a stable pointer.
Dispose that `MemoryHandle` after the pointer's last use. Both span leases and
pins keep transferred storage owners alive for their scope.

## Numerical behavior

Empty matrices and vectors are allowed. Empty norms and dot products are zero;
normalizing a zero norm throws.

Quaternion rotation operations normalize their inputs locally. Quaternion
multiplication preserves the algebraic coefficients.

For an isometry, `Translation` and `RotationMatrix` return shared views.
`Rotation` computes a new quaternion. Setters copy their inputs. If you edit an
isometry through matrix or span access, you are responsible for keeping its
rotation orthonormal and its last row equal to [0, 0, 0, 1].

General linear solvers and decompositions are not implemented.
See the [geometry tests](../../tests/Darp.Tesseract.Native.IntegrationTests/GeometryOwnershipTests.cs)
for executable examples of views, scoped access, layouts and transform operations.
