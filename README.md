# Darp.Tesseract.Native

Managed .NET bindings for [Tesseract Robotics](https://github.com/tesseract-robotics/tesseract), generated with SWIG and distributed with RID-specific native runtimes.

## Current scope

This repository establishes a maintainable end-to-end binding and packaging path. It currently wraps `tesseract_common`'s `Timer` and `Stopwatch` APIs and provides analytical inverse kinematics for configurable six-axis robots through Tesseract's `OPWInvKin`. `Timer.start` is intentionally deferred until the managed callback lifetime contract is designed.

All upstream sources remain unmodified and are pinned as submodules:

- `native/tesseract`: Tesseract C++ source (`0.35.0`)
- `native/tesseract_python`: canonical upstream SWIG definitions (`0.35.0`)
- `native/opw_kinematics`: Tesseract's pinned analytical solver dependency (`0.5.3`)
- `native/eigen`: pinned linear algebra dependency (`3.4.0`)
- `native/console_bridge`: Tesseract's logging dependency (`1.0.2`)

The upstream definitions target Python and contain NumPy/Python-only typemaps. The files under `bindings/` are dependency-coherent C# overlays. Binding generation checks that reused declarations are still present upstream, so an upstream change fails loudly instead of silently changing the managed ABI.

## Inverse kinematics

The kinematics API is generated directly from Tesseract, Eigen, and OPW types. There is no handwritten managed model and no native façade. The small `%extend` blocks in the SWIG overlay only add constructors and indexed access to C++ aliases that SWIG cannot otherwise represent.

```csharp
using var parameters = new OPWParameters
{
    // ABB IRB 2400 parameters used by Tesseract's own OPW test.
    a1 = 0.100,
    a2 = -0.135,
    b = 0,
    c1 = 0.615,
    c2 = 0.705,
    c3 = 0.755,
    c4 = 0.085,
};
parameters.setOffset(2, -Math.PI / 2);

using var jointNames = new StringVector(new[]
{
    "joint_1", "joint_2", "joint_3", "joint_4", "joint_5", "joint_6",
});
using var solver = new OPWInvKin(parameters, "base_link", "tool0", jointNames);
using var target = new Isometry3d();
target.setTranslation(1, 0, 1.306);
target.setQuaternion(0, 0, 0, 1);
using var targets = new TransformMap();
targets.set("tool0", target);
using var seed = new VectorXd(6);
using IKSolutions solutions = solver.calcInvKin(targets, seed);
```

This is the direct-binding foundation for the group-oriented pipeline. `Environment.getKinematicGroup()`, `KinGroupIKInput`, and `KinematicGroup.calcInvKin()` are the intended public workflow. Adding them requires the upstream scene graph, resource locator, plugin loader, URDF/SRDF, geometry, collision, YAML, Boost Graph, and Orocos KDL dependency closure, and remains the next dependency-coherent module.

## Intended native runtimes

- `win-x64`
- `win-arm64`
- `linux-x64`
- `linux-arm64`
- `osx-x64`
- `osx-arm64`

MSVC uses the static C/C++ runtime, Linux links `libgcc` and `libstdc++` statically, and macOS relies only on OS-provided system libraries.
Linux artifacts are built on Ubuntu 22.04 to keep the glibc floor stable, and macOS artifacts set a macOS 11.0 deployment target (the first macOS release supporting Apple Silicon).

## Develop locally

Initialize upstream sources:

```powershell
git submodule update --init --recursive
```

Generate bindings on Windows using the pinned SWIG bootstrap:

```powershell
$swig = ./scripts/install_swig.ps1
./scripts/generate_bindings.ps1 -SwigPath $swig
```

Build the native library and run the integration executable:

```powershell
./scripts/build_native.ps1 -RuntimeId win-x64 -Generator "Visual Studio 18 2026"
dotnet run --project tests/Darp.Tesseract.Native.IntegrationTests -c Release
```

Pack all native artifacts currently present below `artifacts/native`:

```powershell
dotnet pack src/Darp.Tesseract.Native/Darp.Tesseract.Native.csproj -c Release -o artifacts/packages
```

## Extending the binding

Add declarations to the local C# overlay in small dependency-coherent modules, mirror the corresponding upstream SWIG declarations, and extend the native CMake target only with the required upstream implementation files and libraries. Never patch files inside either submodule. If a language-neutral SWIG fix is useful upstream, contribute it there and then advance both pinned submodules together.

## Updating Tesseract

Tesseract and `tesseract_python` should always move to the same released tag. Fetch both submodules, check that the desired tag exists in each, check out the tag in both, then update `TesseractVersion` and the build metadata in `Directory.Build.props`. Regenerate bindings and run the package workflow locally before committing the two gitlink changes. The regeneration guard intentionally stops if an upstream SWIG declaration used by the C# overlay disappears.
