# Darp.Tesseract.Native

Generated .NET bindings for [Tesseract Robotics](https://github.com/tesseract-robotics/tesseract). The managed API is produced directly from Tesseract's public C++ headers with SWIG; there is no handwritten native façade or managed forwarding layer.

## Current scope

The first binding slice covers the foundations needed for a robot kinematics workflow:

- common value types, resources, plugin metadata, and reusable containers;
- Eigen vectors, matrices, quaternions, and transforms;
- primitive geometry and scene graphs;
- URDF parsing and SRDF models;
- state solvers;
- environments, joint groups, forward kinematics, Jacobians, and generic inverse kinematics.

Collision remains an internal dependency of `Environment`, but its public API is intentionally not generated yet. Mesh construction, Task Composer, serialization, managed callbacks, and multi-tip IK are also deferred until their reusable container or lifetime rules are in place.

The upstream sources remain unmodified and are pinned as submodules:

- `native/tesseract`: Tesseract C++ `0.35.0`;
- `native/tesseract_nanobind`: nanobind API reference and dependency lock `0.35.0.7`;
- `native/eigen`: Eigen headers used while SWIG parses the public API `3.4.0`.

The nanobind wrapper is the API coverage oracle, not a runtime dependency. Generation checks representative declarations in the pinned nanobind modules and then parses the corresponding original Tesseract headers. The native wrapper links the official Tesseract packages from the exact Pixi lock committed by nanobind.

## Kinematics example

```csharp
var urdf = File.ReadAllText("robot.urdf");
var srdf = File.ReadAllText("robot.srdf");

using var locator = new GeneralResourceLocator();
locator.addPath(Path.GetFullPath("resources"));

using var sceneGraph = TesseractNative.parseURDFString(urdf, locator);
using var srdfModel = new SRDFModel();
srdfModel.initString(sceneGraph, srdf, locator);

using var environment = new Darp.Tesseract.Native.Environment();
if (!environment.init(sceneGraph, srdfModel))
    throw new InvalidOperationException("Could not initialize the environment.");

using var group = environment.getKinematicGroup("manipulator");
using var seed = new VectorXd(checked((int)group.numJoints()));
seed.AsSpan().Clear();

var tip = group.getActiveLinkNames()[^1];
using var transforms = group.calcFwdKin(seed);
using var target = transforms.get(tip);
using var input = new KinGroupIKInput(target, group.getBaseLinkName(), tip);
using var solutions = group.calcInvKin(input, seed);

for (var index = 0; index < solutions.size(); index++)
{
    ReadOnlySpan<double> joints = solutions.GetSolutionSpan(index);
    // The span views C++-owned Eigen storage and is valid while solutions lives.
}
```

`VectorXd.AsSpan()`, `MatrixXd.AsSpan()`, and `IKSolutions.GetSolutionSpan()` view native Eigen storage directly. The native algorithm owns any result allocation it requires; the generated C# layer does not copy that data into managed arrays. A view must not outlive its owner and must not be retained across an operation that can resize the underlying native container.

## Design

- One generated native library owns the complete binding surface. This avoids allocator, RTTI, and plugin-lifetime boundaries between generated modules.
- Reusable SWIG typemaps handle ownership, standard containers, files, and Eigen types. Method-specific P/Invoke entry points are generated from upstream declarations.
- Upstream `std::unique_ptr<T>` results are transferred into the same generated `shared_ptr<T>` proxy used elsewhere, so ownership crosses the ABI once.
- Kinematic groups returned by an environment retain their environment owner on the managed side, keeping loaded solver plugins alive.
- The managed project targets `netstandard2.1` and `net10.0` for built-in span support.

## Develop locally

Initialize the pinned sources:

```powershell
git submodule update --init --recursive
```

Install the pinned Windows tools and generate the wrapper:

```powershell
$swig = ./scripts/install_swig.ps1
$pixi = ./scripts/install_pixi.ps1
./scripts/generate_bindings.ps1 -SwigPath $swig
```

Build and test on Windows x64:

```powershell
./scripts/build_native.ps1 -RuntimeId win-x64 -PixiPath $pixi

$pixiPrefix = (Resolve-Path native/tesseract_nanobind/.pixi/envs/default).Path
$env:CONDA_PREFIX = $pixiPrefix
$env:Path = (Join-Path $pixiPrefix "Library/bin") + ";" + $env:Path
dotnet run --project tests/Darp.Tesseract.Native.IntegrationTests -c Release
```

The official Tesseract 0.35 packages currently provide native dependency sets for `win-x64`, `linux-x64`, `linux-arm64`, and `osx-arm64`. Each RID must be built on a matching host. Linux and macOS require Pixi to be installed separately; pass its path through `-PixiPath` when it is not on `PATH`.

## Extending the binding

Add upstream headers to `bindings/tesseract_csharp.i` in dependency-coherent slices. Put only generally reusable ownership, container, or value-type rules under `bindings/support/`. Do not patch a method with a handwritten native forwarding function just to improve its C# shape, and never edit files inside the Tesseract or nanobind submodules.

When updating Tesseract, advance `native/tesseract` and `native/tesseract_nanobind` to compatible releases together, review the nanobind coverage sources, regenerate, compile the native wrapper, and run the integration test against a real robot model.

Packaging self-contained RID assets is a separate follow-up. The current developer build intentionally relies on the pinned Pixi environment for Tesseract's transitive native libraries.
