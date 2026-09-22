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
both concrete structs and interface values:

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

Geometry types are non-disposable `readonly struct`s. Each contains a reference
to shared coefficient storage and its view layout. Copying a struct aliases that
storage; it does not copy the coefficients. Views keep their shared storage alive;
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
values. `AsReadOnly()` returns a concrete `ReadOnlyVector3D`, `ReadOnlyMatrixXD`,
or corresponding readonly struct. These views expose no writable access.
Assigning a writable struct to an interface boxes it; the boxed copy still shares
its storage. `MatrixMarshal` and scalar matrix operations use generic constraints
to avoid boxing concrete structs. Shape-specific arithmetic extensions still
accept interfaces and can box their arguments.

## Default values

`default(Vector3D)` is a 3-element zero vector backed by shared read-only storage.
The same applies to other fixed-size types. `default(VectorXD)` is an empty 0 by 1
vector; `default(MatrixXD)` is an empty 0 by 0 matrix. Reading, readonly views and
readonly tensor access work. Setters and writable tensor access throw
`InvalidOperationException`, including through derived views.

```csharp
Vector3D zero = default;
Console.WriteLine(zero.X); // 0
// zero.X = 1;            // Throws: shared zero storage is read-only.

var writable = new Vector3D(); // Allocates writable zero coefficients.
writable.X = 1;
var copy = zero.Clone();       // Also creates independent writable storage.
```

`readonly` describes the struct's storage reference and layout, not its
coefficients. Constructors allocate writable storage. `new Isometry3D()` creates
an identity transform; `QuaternionD.Identity` creates an identity quaternion.
Their `default` values contain all zeros and do **not** represent valid rotations
or rigid transforms.

For a view returned by a property, store it in a local before assigning its
coefficients, for example `var translation = pose.Translation; translation.X = 1;`.
The local shares storage with `pose`.

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

Use `MatrixMarshal.Pin(matrix)` when native code needs a stable read-only pointer.
Do not write through that pointer, including when pinning shared default storage.
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
