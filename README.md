# Darp.Tesseract.Native

Generated .NET bindings for [Tesseract Robotics](https://github.com/tesseract-robotics/tesseract). The package exposes the SWIG-generated API directly and ships ready-to-use native runtimes for Windows, Linux, and macOS.

## Architecture

The repository keeps the native and managed build boundaries separate:

- Pixi locks the compiler and third-party native dependency environment.
- A root CMake superbuild copies the pinned Tesseract submodule into an ignored build directory, applies the repository's focused runtime-dependency patch, builds Tesseract statically, and builds the generated wrapper against that private installation.
- `dotnet pack` consumes the resulting `artifacts/native/<rid>/` directories without invoking Pixi or CMake.
- NuGet's standard `runtimes/<rid>/native/` assets select and deploy the matching wrapper and its adjacent runtime dependencies.

The Tesseract checkout stays pristine. `patches/tesseract-runtime-dependencies.patch` contains two focused build-graph changes: it makes PCL-backed URDF point-cloud parsing optional and avoids linking the compiled Boost.Graph library when Tesseract only consumes its header API. This removes the PCL/VTK and Boost.Regex/ICU runtime graphs while retaining normal URDF geometry, meshes, octomap files, and scene-graph functionality.

The single `tesseract_csharp` wrapper contains statically linked Tesseract components and the Bullet, FCL, KDL, OPW, and UR plugin factories. A generated internal bootstrap registers that already-loaded wrapper with Tesseract's normal plugin loader. Existing SRDF/YAML class aliases and search-library entries therefore continue to work without separate generic factory libraries.

The native build targets `win-x64`, `linux-x64`, `linux-arm64`, `osx-x64`, and `osx-arm64` on matching native hosts. Windows ARM64 is deferred. Linux targets glibc 2.28 and expects the distribution's standard C/C++ runtimes and zlib; the package supplies the adjacent robotics dependency closure. macOS targets 11.0.

## Current binding surface

The modular SWIG inputs cover feasible non-visual APIs from:

- common values, resources, containers, and plugin metadata;
- Eigen vectors, matrices, quaternions, and transforms;
- geometry and scene graphs;
- URDF/SRDF parsing and state solvers;
- collision-manager lifecycle and stable operations;
- environments, joint groups, FK, Jacobians, KDL/OPW/UR IK, and generic IKFast-facing types.

Unsupported C++ shapes are ignored explicitly in the component interface files. A curated managed façade, visualization, ROS integration, PCL point-cloud parsing, and robot-specific IKFast solvers are outside this package.

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
```

Native-backed spans such as `VectorXd.AsSpan()` and `IKSolutions.GetSolutionSpan()` remain valid only while their owning proxy is alive and unchanged.

## Develop locally

Initialize the pinned Tesseract source:

```powershell
git submodule update --init native/tesseract
```

Install Pixi on Windows when it is not already available:

```powershell
$pixi = ./scripts/install_pixi.ps1
```

Generate committed binding sources explicitly:

```powershell
pixi run -e bindings generate-bindings
```

Build the current host's native runtime:

```powershell
pixi run build-native
```

The command produces only `artifacts/native/<host-rid>/`. Managed packaging and tests remain ordinary .NET operations:

```powershell
dotnet pack src/Darp.Tesseract.Native/Darp.Tesseract.Native.csproj -c Release -o artifacts/packages
dotnet test --project tests/Darp.Tesseract.Native.IntegrationTests/Darp.Tesseract.Native.IntegrationTests.csproj -c Release
```

CI builds the five native RIDs independently, merges their artifacts into one NuGet package, and runs the smoke tests from that package without Pixi or native build paths.

## Extend the bindings

Add public headers deliberately to the relevant file under `bindings/components/`. Put only reusable ownership, container, filesystem, Eigen, or exception behavior under `bindings/support/`. Run `pixi run -e bindings generate-bindings` and review the generated C# and C++ diffs before committing them.
