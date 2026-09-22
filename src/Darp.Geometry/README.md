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

using var axis = Vector3D.UnitZ;
using var rotation = QuaternionD.FromAxisAngle(axis, Math.PI / 2);
using var translation = new Vector3D(1, 2, 3);
using var transform = new Isometry3D(rotation, translation);
using var point = Vector3D.UnitX;
using var world = transform * point;
// world is approximately [1, 3, 3].
```

Named operations are extension methods on read-only interfaces. They work on
both concrete objects and interface values:

```csharp
using var vector = new Vector3D(3, 0, 4);
using var readable = vector.AsReadOnly();
using var unit = readable.Normalized(); // Returns a new Vector3D.
double length = vector.Norm();          // 5
```

Import `Darp.Geometry` to use these extensions. Concrete types also provide
operators. Use named operations such as `Add`, `Multiply` and `Scale` when the
left operand is an interface.

## Views, copies and disposal

Every geometry object is disposable. A view shares coefficients but has its own
reference to their storage. Dispose returned views and arithmetic results when
finished.

| Operation | Shares coefficients? | Independent lifetime? |
| --- | --- | --- |
| Assignment, including assignment to a read-only interface | Same object | No |
| `AsReadOnly()`, `AsMatrix()`, `AsReadOnlyMatrix()` | Yes | Yes |
| `Block()`, `Slice()`, `Row()`, `Column()`, `Transposed()` | Yes | Yes |
| `Clone()` and arithmetic results | No | Yes |

```csharp
using var matrix = MatrixXD.Identity(3);
using var column = matrix.Column(0);
using var readable = column.AsReadOnly();
using var snapshot = readable.Clone();

matrix.Dispose();
column[0] = 7;
Console.WriteLine(readable[0]); // 7
Console.WriteLine(snapshot[0]); // 1
```

Disposing `matrix` invalidates that object, but the retained column and read-only
view remain usable. The last view or pin releases the storage owner.

Read-only means no writes through that view. Other aliases can still change its
values. `AsReadOnly()` returns an internal implementation without writable access.
Assigning a mutable object to `IReadOnlyVector3D` only changes the variable's type;
it neither creates a view nor removes the object's writable API.

## Mapping existing memory

`CreateFromMemory` maps memory without copying it:

```csharp
double[] values = [1, 2, 3, 4, 5, 6];
using var matrix = MatrixXD.CreateFromMemory(
    values, rows: 2, columns: 3, columnStride: 1, rowStride: 3);

matrix[1, 2] = 9; // Also changes values[5].
```

Strides count doubles, not bytes. Positive strides support row-major, padded and
strided layouts. `Row(i)` has shape 1 by N, `Column(i)` is a column vector, and
`AsVector()` requires one column.

For externally owned memory, keep its owner valid while any view uses it.
`MatrixXD.CreateFromMemoryWithOwner` instead transfers a
`MemoryManager<double>` to the shared storage. The last view or pin disposes it.
Failed construction also disposes the transferred manager.

## Tensor spans

`AsTensorSpan()` and `AsReadOnlyTensorSpan()` expose rank-two spans with
`[row, column]` indexing. They do not retain storage.

```csharp
using var matrix = MatrixXD.Identity(3);
var coefficients = matrix.AsTensorSpan();
coefficients[0, 1] = 2;
GC.KeepAlive(matrix); // After the span's last use.
```

Keep the source object alive and undisposed until the span's last use.
If access must outlive the original object, first create a retained view with
`AsMatrix()` or `AsReadOnlyMatrix()`, then obtain the span from that view.
`Pin()` also retains storage until its handle is disposed.

Synchronous math reads spans directly and keeps its inputs alive for the call.
It does not allocate retained input views. Callers must not dispose inputs during
a call or mutate shared coefficients concurrently without synchronization.

## Numerical behavior

Empty matrices and vectors are allowed. Empty norms and dot products are zero;
normalizing a zero norm throws.

Quaternion rotation operations normalize their inputs locally. Quaternion
multiplication preserves the algebraic coefficients.

For an isometry, `Translation` and `RotationMatrix` return retained views.
`Rotation` computes a new quaternion. Setters copy their inputs. If you edit an
isometry through matrix or span access, you are responsible for keeping its
rotation orthonormal and its last row equal to [0, 0, 0, 1].

General linear solvers and decompositions are not implemented.
See the [geometry tests](../../tests/Darp.Tesseract.Native.IntegrationTests/GeometryOwnershipTests.cs)
for executable examples of views, disposal, layouts and transform operations.
