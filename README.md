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

## Geometry surface

[Darp.Tesseract.Native](src/Darp.Tesseract.Native/README.md) uses the disposable
matrix, vector, quaternion and isometry classes from [Darp.Geometry](src/Darp.Geometry/README.md).
Views retain shared storage independently; dispose every result and view when done.

```csharp
using Darp.Geometry;
using Darp.Tesseract.Native;

using var joints = new VectorXD(6);
using var poses = group.calcFwdKin(joints);
using var tool = poses["tool0"];
using var translation = tool.Translation;
Console.WriteLine(translation);
```

Ordinary scalar access and math manage storage lifetime internally. Tensor spans
provide explicit access; retained `AsMatrix()` / `AsReadOnlyMatrix()` views keep storage alive until disposed.

## Kinematics example

```csharp
using Darp.Geometry;
using Darp.Tesseract.Native;

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
using var seed = new VectorXD(checked((int)group.numJoints()));

using var activeLinks = group.getActiveLinkNames();
var tip = activeLinks[^1];
using var transforms = group.calcFwdKin(seed);
using var target = transforms[tip];
using var input = new KinGroupIKInput(target, group.getBaseLinkName(), tip);
using var solutions = group.calcInvKin(input, seed);
```

Returned geometry objects retain their native storage independently of the originating proxy or container. Dispose them when finished.

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
dotnet pack src/Darp.Geometry/Darp.Geometry.csproj -c Release -o artifacts/packages
dotnet pack src/Darp.Tesseract.Native/Darp.Tesseract.Native.csproj -c Release -o artifacts/packages
dotnet test --project tests/Darp.Tesseract.Native.IntegrationTests/Darp.Tesseract.Native.IntegrationTests.csproj -c Release
```

CI builds the five native RIDs independently, merges their artifacts into one NuGet package, and runs the smoke tests from that package without Pixi or native build paths.

## Releases

Releases follow the same release-please flow as `Darp.Luau.Native`. Conventional
commits on `main` create or update a release PR containing `CHANGELOG.md`,
`version.txt`, `.release-please-manifest.json` and `Directory.Build.props`.
Merging that PR creates a `v<version>` GitHub release, builds all
five packaged runtimes, tests package consumers, then publishes
`Darp.Geometry`, `Darp.Tesseract.Native` and their symbol packages to NuGet.org and attaches them to
the GitHub release.

Configure NuGet Trusted Publishing for `rosslight/Darp.Tesseract`, workflow
`release.yml`, package owner `rosslight`, and package pattern `Darp.Tesseract*`.
Set the repository Actions secret or variable `NUGET_USER` to the NuGet profile username
that created the policy, and allow GitHub Actions to create pull requests.
`NuGet/login` exchanges the workflow's OIDC token for a temporary publishing key;
no long-lived API key secret is needed.

The package includes `LICENSE`, `THIRD-PARTY-NOTICES.md`, upstream license texts
and platform-specific native dependency licenses and
source materials. KDL is kept dynamically linked, including on Windows.

## Extend the bindings

Add public headers deliberately to the relevant file under `bindings/components/`. Put only reusable ownership, container, filesystem, Eigen, or exception behavior under `bindings/support/`. Run `pixi run -e bindings generate-bindings` and review the generated C# and C++ diffs before committing them.
