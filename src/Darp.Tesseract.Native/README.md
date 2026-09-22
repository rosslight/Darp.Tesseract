# Darp.Tesseract.Native

Generated .NET 10 bindings for Tesseract Robotics. The package includes the native
runtime and depends on [Darp.Geometry](../Darp.Geometry/README.md) for vectors,
matrices, quaternions and transforms.

Method names follow Tesseract's C++ API. Look in [Generated](Generated) for the
available C# signatures. For build prerequisites and platform support, see the
[repository README](../../README.md).

## Load a robot and compute kinematics

This example uses the ABB robot fixture included in the repository. Run it from
the repository root in an application referencing the binding package or project.

```csharp
using Darp.Geometry;
using Darp.Tesseract.Native;
using TesseractEnvironment = Darp.Tesseract.Native.Environment;

var assets = Path.GetFullPath(
    "tests/Darp.Tesseract.Native.IntegrationTests/Assets");
var robot = Path.Combine(assets, "darp_test");

using var locator = new GeneralResourceLocator();
locator.addPath(assets);

using var sceneGraph = TesseractNative.parseURDFString(
    File.ReadAllText(Path.Combine(robot, "abb_irb2400.urdf")), locator);
using var srdf = new SRDFModel();
srdf.initString(
    sceneGraph,
    File.ReadAllText(Path.Combine(robot, "abb_irb2400.srdf")),
    locator);

using var environment = new TesseractEnvironment();
if (!environment.init(sceneGraph, srdf))
    throw new InvalidOperationException("Could not initialize the robot.");

using var group = environment.getKinematicGroup("manipulator");
using var seed = new VectorXD(checked((int)group.numJoints()));
using var poses = group.calcFwdKin(seed);
using var tool = poses["tool0"];
using var position = tool.Translation;
using var jacobian = group.calcJacobian(seed, "tool0");

Console.WriteLine(position);
Console.WriteLine($"Jacobian: {jacobian.Rows} by {jacobian.Columns}");

using var target = new KinGroupIKInput(
    tool, group.getBaseLinkName(), "tool0");
using var solutions = group.calcInvKin(target, seed);
foreach (var solution in solutions)
{
    using (solution)
        Console.WriteLine(solution);
}
```

For your robot, change the URDF, SRDF, resource path, group name and tip link.
The SRDF and referenced YAML configure the kinematics plugins.
[The integration tests](../../tests/Darp.Tesseract.Native.IntegrationTests/KinematicsTests.cs)
also show collision-manager setup, environment commands and joint-state access.

## Geometry inputs and results

Generated geometry inputs accept `IReadOnlyMatrixD` where the native signature
allows read-only access. The binding checks the required shape. You can supply
a geometry object backed by managed memory or a result from an earlier native call.

Results use the matching read-only interfaces, such as `IReadOnlyVectorXD`,
`IReadOnlyMatrixD` and `IReadOnlyIsometry3D`. Call `Clone()` for a writable copy.
Results own a reference to native storage and remain usable after the originating
proxy is disposed.

Transform maps and geometry sequences are disposable too. Each lookup or
enumeration produces an independently retained element. Dispose both the
collection and the elements you take from it, as in the example above.

## What copies, and what shares memory?

| Native signature or result | Binding behavior |
| --- | --- |
| `const Eigen::Ref` input | Pins compatible inner-contiguous memory. Packs other layouts into a temporary native value. |
| Eigen value or `const T&` input | Creates a native value from the supplied coefficients. |
| Eigen value result | Moves the result into owned native storage and returns a read-only view. |
| Reference or pointer getter | Returns an independent snapshot, so later proxy changes do not invalidate it. |
| Element of a returned geometry map or sequence | Shares the collection's native allocation without copying coefficients. |
| Writable `Eigen::Ref` | Copies back into the supplied mutable object after success. The shape stays fixed. |
| Writable `T&` | Uses a managed `ref` parameter and replaces the result object, allowing its shape to change. |

A `ref` replacement does not dispose the previous managed object. Keep a separate
reference to the old value if you need to dispose it after the call. Existing
aliases remain valid.

Creating native containers from managed dictionaries or lists copies their
elements. Empty vectors, matrices and containers are supported.

Do not dispose or mutate an input concurrently with a native call. Raw tensor
spans follow the [geometry lifetime rules](../Darp.Geometry/README.md#tensor-spans).

## Change or extend a binding

Edit the SWIG inputs, then regenerate both managed and native output:

```powershell
pixi run -e bindings generate-bindings
pixi run build-native
dotnet build src/Darp.Tesseract.Native/Darp.Tesseract.Native.csproj
```

Run these commands from the repository root. Rebuilding the native wrapper matters
when a change alters the generated interop signatures.

| Change | Source |
| --- | --- |
| Expose an upstream header or exclude a signature | [bindings/components](../../bindings/components) |
| Add an Eigen type or geometry container mapping | [mappings.i](../../bindings/geometry/mappings.i) |
| Change generated C# conversion or ownership code | [typemaps.i](../../bindings/geometry/typemaps.i) |
| Change native Eigen conversion behavior | [runtime.h](../../bindings/geometry/runtime.h) |
| Change managed pinning, memory ownership or containers | [Runtime](Runtime) |
| Change shared SWIG behavior for other C++ types | [bindings/support](../../bindings/support) |

SWIG writes C# files to `src/Darp.Tesseract.Native/Generated/` and the C++ wrapper
to `bindings/generated/`. Review and commit both outputs with the input changes.
The generated files are not the place to fix conversion behavior.
