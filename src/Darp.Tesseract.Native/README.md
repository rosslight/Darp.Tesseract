# Generated Tensor2 bindings

`Darp.Tesseract.Native` is the single generated binding package. It targets .NET 10
and uses the reusable math types from `Darp.Geometry`. Constructors, methods and
properties are generated from upstream headers using shared type mappings.
There is one native wrapper module and one managed native-object graph.

```csharp
using Darp.Geometry.Tensor2;
using Darp.Tesseract.Native;

// group comes from Darp.Tesseract.Native.Environment.getKinematicGroup(...).
var joints = new VectorXD(0.0, 0.2, -0.3, 0.0, 0.4, 0.0);
TransformMap poses = group.calcFwdKin(joints);
ReadOnlyIsometry3D tool = poses["tool0"];
Vector3D position = tool.Translation.Clone();
ReadOnlyMatrixXD jacobian = group.calcJacobian(joints, "tool0");

using var target = new KinGroupIKInput(tool, group.getBaseLinkName(), "tool0");
IKSolutions solutions = group.calcInvKin(target, joints);
foreach (ReadOnlyVectorXD solution in solutions)
    Console.WriteLine(solution);

// Output references replace the container; earlier element views stay valid.
group.calcFwdKin(ref poses, joints);
group.calcInvKin(ref solutions, target, joints);
```

The editable [playground](../../examples/Darp.Tesseract.Native.Playground/Program.cs)
also covers joint-state constructors/properties, empty optional vectors, limits,
and environment state. Run it yourself with URDF, SRDF, group and tip-link arguments.

## Generated and handwritten parts

| Part | Source |
| --- | --- |
| Public Tesseract API | SWIG, using `bindings/tesseract_csharp.i` and upstream headers |
| Eigen input/output conversion | Reusable macros in `bindings/geometry/typemaps.i` |
| Eight Eigen types and three containers | Explicit declarations in `bindings/geometry/mappings.i` |
| Native container operations and managed container declarations | SWIG declarations in `mappings.i` and reusable collection macros in `typemaps.i` |
| Pinning, memory manager, SafeHandle and generic container implementation | Handwritten shared code under `Runtime/`, plus `bindings/geometry/runtime.h` |

The type rules cover fixed vectors (2/3/4), dynamic vectors, dynamic matrices,
N by 2 matrices, quaternions and isometries. The same rules handle FK/IK, Jacobians,
state solvers, collision transforms, scene-graph fields, joint states, limits and
command constructors. Container rules cover transform maps, transform sequences
and IK solution sequences.

## Ownership and copying rules

- `const Eigen::Ref` inputs borrow pinned inner-contiguous storage. Padded
  column-major matrices are supported. Other layouts are packed locally.
- Concrete Eigen values and `const T&` inputs are materialized as native values;
  those signatures cannot consume an Eigen Map directly.
- Value results move into a retained native allocation and expose read-only
  Tensor2 descriptors. Reference/pointer getters produce independent snapshots,
  because the originating object can be modified or explicitly disposed.
- Maps and sequences are frozen after construction. Indexing creates a tensor
  view that shares ownership of their native allocation, without copying element
  coefficients. A view can outlive the managed collection.
- Writable `Eigen::Ref` parameters use a local native value, then copy back into
  the supplied mutable descriptor after a successful call. Its shape cannot change.
- Writable `T&` parameters use `ref` managed parameters. They return replacement
  descriptors/containers, allowing resizing while preserving earlier views.
  The native in/out value is initialized from the old value, so append/update
  semantics are preserved. There is no reuse promise for output buffers.
- Inputs created from managed dictionaries/lists copy their elements into a native
  container once. Empty containers and empty native vectors/matrices are supported.

The matrix/vector interfaces retain memory managers during ordinary math and
scalar access. Explicit raw tensor spans still require the usual caller-managed
lifetime. Native proxy disposal and concurrent mutation remain the caller's
responsibility. Read-only access is not a security boundary against unsafe code.

## Regeneration

From the repository root:

```powershell
pixi run -e bindings generate-bindings
pixi run build-native
dotnet build src/Darp.Tesseract.Native/Darp.Tesseract.Native.csproj
```

The generation command invokes SWIG directly. Type mappings and collection declarations
live in the SWIG interface files; no separate manifest or support-code generator is
needed. Compilation and integration tests verify the generated bindings.
Do not edit generated files manually.

This covers the selected binding surface; existing `%ignore` declarations still
define which upstream APIs are exposed. Integration tests exercise robot loading,
FK/Jacobian/IK, plugins, commands, strided inputs and retained native views.
