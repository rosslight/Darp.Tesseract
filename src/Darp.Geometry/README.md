# Darp.Geometry

Double-precision matrices, column vectors, quaternions and rigid transforms backed
by `System.Numerics.Tensors`. This is the sole geometry implementation.

Run the editable examples with:

```powershell
dotnet run --project examples/Darp.Geometry.Playground
```

## Ownership and views

All mutable and read-only geometry types are disposable classes. Each object owns
one reference to shared storage. `AsReadOnly()`, matrix conversions, blocks, rows,
columns and transposes create independently retained views. Dispose every returned
view or arithmetic result when finished. Assignment aliases the same object;
`Clone()` allocates independent writable storage. There are no implicit conversions.

```csharp
using var matrix = MatrixXD.Identity(3);
using var column = matrix.Column(0);
using var readable = column.AsReadOnly();
using var snapshot = readable.Clone();
matrix.Dispose();
column[0] = 7;
Console.WriteLine(readable[0]); // 7: live read-only view.
Console.WriteLine(snapshot[0]); // 1: independent copy.
```

Disposing an object invalidates that object and every ordinary C# alias to it.
Retained views remain valid. The last view, borrow or pin releases the underlying
owner once. Finalization provides a fallback; deterministic disposal is preferred.
Read-only access prevents writes through that view, not through other aliases.
Concurrent coefficient mutation needs caller synchronization.

`Map(memory, ...)` borrows external storage; callers keep its external owner valid.
`MatrixXD.Own(memory, owner, ...)` and `ReadOnlyMatrixXD.Own(...)` transfer disposal
of an external owner to the shared storage, including on failed construction.
Do not separately dispose an owner after transferring it.

## Tensor access

Ordinary scalar access and math retain storage internally. `Borrow()` and
`BorrowReadOnly()` provide explicit retained access for custom algorithms:

```csharp
using var matrix = MatrixXD.Identity(3);
using var access = matrix.Borrow();
var tensor = access.AsTensorSpan();
matrix.Dispose();
tensor[0, 0] = 2; // Access remains alive and undisposed.
```

A tensor span does not own a reference. Never use an extracted span after its
borrow is disposed, or concurrently with disposal of that borrow. Direct
`AsTensorSpan()` / `AsReadOnlyTensorSpan()` access requires keeping the source
object alive and undisposed throughout the span's use. A `Pin()` handle independently
retains storage until disposed. These are runtime contracts, not compiler-enforced
borrow checking.

## Shapes and operations

`MatrixXD` is the storage foundation. `VectorXD` specializes an N by 1 matrix;
`Vector3D` specializes three coefficients. `Matrix3D` has shape 3 by 3.
Quaternion coefficients use X/Y/Z/W in a 4 by 1 matrix, while `Isometry3D` exposes
a homogeneous 4 by 4 matrix. Each type has a matching `ReadOnly...` class.

`Row(i)` preserves its 1 by N orientation, `Column(i)` returns a vector, and
`AsVector()` requires one column. `Transposed()` shares storage. Matrix multiplication
is algebraic. `MatrixOperations` centralizes kernels behind `IReadOnlyMatrixD` and
`IMatrixD`; tensor spans are an explicit access API, not the numerical input contract.

Owned matrices use column-major storage. Mapping supports positive strides,
including row-major, padded, strided vectors, blocks and transposes. Tensor spans
use rank two and logical [row, column] indexing. Empty dimensions are valid;
empty norms/dot products are zero, and normalization of zero norm is rejected.

Angles are radians, transforms act on column vectors, and `a * b` applies b first.
Rotation operations normalize quaternion inputs locally; Hamilton multiplication
preserves algebraic coefficients. Isometry setters copy inputs, including overlapping
views. Callers editing raw matrix coefficients must preserve orthonormal rotation
and homogeneous last row [0,0,0,1]. `Rotation` computes an independent quaternion;
`Translation` and `RotationMatrix` return retained views.

General solvers and decompositions are outside this implementation.
The [native bindings](../Darp.Tesseract.Native/README.md) use the same disposable
geometry types for native results and call-scoped input pinning.
