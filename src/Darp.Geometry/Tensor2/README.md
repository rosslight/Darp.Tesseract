# Tensor2: matrices, vectors, and read-only views

This variant uses `System.Numerics.Tensors` and retains double precision.
It is independent of the original `Tensor` and `NumFlat` implementations.

From the repository root:

```powershell
dotnet run --project examples/Darp.Geometry.Playground -- tensor2
```

Use `all` to print all three variants. `Playground.cs` contains editable examples;
there are no tests. A generated native binding surface demonstrates mapping native results.

## Vectors specialize matrices

`MatrixXD` is the storage and arithmetic foundation. `VectorXD` wraps an N by 1
matrix, and `Vector3D` narrows that to three coefficients with X/Y/Z access.
`Matrix3D` similarly specializes a 3 by 3 matrix. These are composition-based
specializations, not inheritance or compile-time generic matrix dimensions.

```csharp
using Darp.Geometry.Tensor2;

var vector = new Vector3D(1, 2, 3);
MatrixXD column = vector.AsMatrix(); // 3 by 1 view.
MatrixXD row = vector.Transposed();  // 1 by 3 view.

MatrixXD inner = row * column; // 1 by 1 result; inner[0,0] == vector.Dot(vector).
MatrixXD outer = column * row; // 3 by 3 result.
column[0, 0] = 4;              // vector.X is now 4.
```

`Row(i)` returns a 1 by N matrix view, preserving row orientation.
`Column(i)` returns an N-component column vector. `AsVector()` specializes an N by 1
matrix without copying and rejects other shapes. To treat a row as a column
vector, use `matrix.Row(i).Transposed().AsVector()`.

**In Tensor2, `Transposed()` is a shared view.** Use `Transposed().Clone()` for an
independent transpose. This deliberately differs from the original Tensor variant.

Vector arithmetic, dot products, norms, and normalization delegate to matrix
kernels. Quaternion and rigid-transform operations sit above those specializations.
Matrix multiplication is algebraic; it is not element-wise multiplication.

## Owner-aware interfaces and shared operations

Every descriptor implements `IReadOnlyMatrixD`, whose `AsReadOnlyMatrix()` method
returns a shared `ReadOnlyMatrixXD` view retaining its memory provider. Mutable
descriptors also implement `IMatrixD`, exposing a writable `AsMatrix()` view.
Vectors remain N by 1 matrices; quaternions expose X/Y/Z/W as 4 by 1, and
isometries expose their homogeneous 4 by 4 matrix.

`MatrixOperations` uses constrained generic parameters. Concrete struct inputs
are not boxed. Each operation obtains the owner-bearing matrix views, calls its
internal `TensorKernels` implementation, and retains each memory manager through
completion, including exception paths. Lifetime retention does not box the
matrix or its `Memory<double>`: it obtains the manager via
`MemoryMarshal.TryGetMemoryManager` and passes that reference to `GC.KeepAlive`.
Storing a struct in an interface-typed variable still boxes it normally.

```csharp
var vector = new Vector3D(1, 2, 3);
ReadOnlyVectorXD other = vector.AsVector();

double length = MatrixOperations.Norm(vector);
double dot = vector.Dot(other);
MatrixXD product = MatrixOperations.Multiply(vector, other.Transposed());
Vector3D unit = vector.Normalized();

// External buffers enter the same owner-aware API through a mapped descriptor.
var mapped = ReadOnlyMatrixXD.Map(new double[] { 3, 4, 0 }, 3, 1);
double mixedDot = MatrixOperations.Dot(vector, mapped);
```

Norms, inner/dot/cross products, addition, subtraction, multiplication, scaling,
division, normalization, interpolation, and the 3 by 3 determinant are implemented
once in `TensorKernels`. Kernels validate shape before calculating. Contiguous
columns use `TensorPrimitives`; other layouts use logical indexing. Public
convenience methods and operators preserve specialized result types.

Tensor spans are available explicitly through `AsTensorSpan()` and
`AsReadOnlyTensorSpan()` for advanced interoperability. There are no implicit
span conversions, and public numerical methods do not accept raw spans.
Read-only descriptors do not expose writable tensor spans.

## Mutable and read-only descriptors

Each geometry type has a read-only counterpart:

| Mutable | Read-only |
| --- | --- |
| `MatrixXD` | `ReadOnlyMatrixXD` |
| `Matrix3D` | `ReadOnlyMatrix3D` |
| `VectorXD` | `ReadOnlyVectorXD` |
| `Vector3D` | `ReadOnlyVector3D` |
| `QuaternionD` | `ReadOnlyQuaternionD` |
| `Isometry3D` | `ReadOnlyIsometry3D` |

All are readonly descriptor structs, with different access to their backing storage.
Mutable descriptors store `Memory<double>` through the matrix layer and expose
coefficient get/set properties. Read-only descriptors store `ReadOnlyMemory<double>`
and return scalar values. Read-only blocks, rows, columns, transpose views,
quaternion coefficients, and transform subviews all remain read-only.

```csharp
double[] buffer = [1, 2, 3];
var writable = Vector3D.Map(buffer);
ReadOnlyVector3D readable = writable; // Or writable.AsReadOnly().
var snapshot = readable.Clone();     // New writable storage.

writable.Z = 7;
Console.WriteLine(readable.Z); // 7: read-only is a live alias, not a snapshot.
Console.WriteLine(snapshot.Z); // 3.
// readable.Z = 8;             // Does not compile.

ReadOnlyMemory<double> input = buffer;
var mappedReadOnly = ReadOnlyVector3D.Map(input); // No writable-memory cast.
var sum = writable + mappedReadOnly;             // New writable result.
```

Conversion from mutable to read-only is implicit and shares coefficients. There
is no reverse conversion. `Clone()` and arithmetic return independent mutable
results. The matrix arithmetic implementation takes read-only operands; mutable
operators forward to it. The interfaces are the public numerical interoperability surface.

## Geometry

```csharp
var rotation = QuaternionD.FromAxisAngle(Vector3D.UnitZ, Math.PI / 2);
var transform = new Isometry3D(rotation, new Vector3D(1, 2, 3));
ReadOnlyIsometry3D pose = transform;

Vector3D point = pose * Vector3D.UnitX; // Approximately [1,3,3].
ReadOnlyVector3D translation = pose.Translation;
ReadOnlyMatrix3D orientation = pose.RotationMatrix;
var writableTranslation = transform.Translation;
writableTranslation.Z = 10;          // translation.Z now reads 10.
transform.SetTranslation(translation);
```

`Rotation` computes an independent quaternion from the matrix, even on a read-only
isometry. It is not a borrowed subview. `SetTranslation`, `SetRotation`, and
`SetRotationMatrix` accept read-only inputs and copy values into the existing
transform, including overlapping inputs. Mutable properties also have setters.

Angles are radians, quaternions use X/Y/Z/W, and transforms act on column vectors.
`a * b` applies b first. Rotation operations normalize quaternion inputs locally;
Hamilton multiplication itself preserves algebraic coefficients. Callers must
preserve orthonormal rotations and the homogeneous last row [0,0,0,1] in isometries.

`Matrix3D.FromMatrix` and `Vector3D.FromMatrix` specialize and share storage.
`Isometry3D.View` also shares storage, while `Isometry3D.FromMatrix` explicitly copies.

## Backing storage and limits

One layout descriptor handles dimensions and positive row/column strides.
Owned matrices are column-major. Mapping also supports padded and row-major storage:

```csharp
double[] coefficients = [1, 2, 3, 4, 5, 6];
var matrix = MatrixXD.Map(coefficients, 2, 3, columnStride: 1, rowStride: 3);
```

Ordinary descriptors retain memory and layout metadata; their tensor views have
rank two and logical [row, column] indexing. View construction converts singleton
dimension strides to zero as required by System.Numerics.Tensors. Mapped layouts
are validated against the tensor API when the descriptor is created: standard
column-major, row-major, padded, strided-vector, block, and transpose layouts are
supported; arbitrary overlapping strides are not promised. Descriptors use positive
strides. Map external memory into a descriptor before passing it to public operations.
General matrix multiplication is implemented here because TensorPrimitives does
not supply that operation.

Default descriptors have no storage; initialize them before use. Dimensions may
be zero to represent empty native results; numerical kernels require nonempty inputs. `new Isometry3D()` creates identity and `new Matrix3D()` creates a zero
matrix; their `default` values do not run those constructors. General matrix solvers
and decompositions remain outside this prototype.

Ordinary descriptors retain their memory provider. A native memory manager must
in turn retain the native allocation. Scalar getters/setters and public numerical
operations keep that provider alive internally; no callback or caller-side
`GC.KeepAlive` is needed for ordinary geometry operations. Scalar access returns
values rather than naked references into native memory. For mutable subviews,
store the view in a local before assigning its properties.

Mapping arbitrary native memory does not establish ownership by itself, prevent
explicit disposal/reallocation by external code, or synchronize access. The memory
provider/caller must guarantee validity against those actions. Read-only access
controls mutation through this API; it does not establish immutability.

An explicitly extracted tensor span does not retain a native memory manager.
The caller must keep the descriptor/owner alive for that span's complete use.
Array-backed spans retain their managed array automatically. This advanced escape
hatch is separate from the owner-aware operations; previously returned spans
cannot be revoked by disposing a wrapper.

The [generated native bindings](../../Darp.Tesseract.Native/README.md) demonstrate
retained native result views and call-scoped input pinning. Their transforms,
translations and rotations use the same ordinary geometry operations.
