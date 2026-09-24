# Darp.Tesseract.Native

.NET 10 bindings for [Tesseract Robotics](https://github.com/tesseract-robotics/tesseract).
Use them to load robot descriptions, query scene graphs, compute forward and inverse
kinematics, and work with collision managers.

The C# API follows the upstream C++ API, including names such as `calcFwdKin` and
`getKinematicGroup`. SWIG generates the bindings.

## Where to start

| If you want to... | Read |
| --- | --- |
| Load a robot and call FK or IK | [Native binding usage](src/Darp.Tesseract.Native/README.md) |
| Run tests against source or a NuGet package | [Test instructions](tests/Darp.Tesseract.Native.IntegrationTests/README.md) |
| Build or change the bindings | The instructions below |

`Darp.Tesseract.Native` uses `Aardvark.Base` for fixed-size geometry and packages
the native wrapper and its runtime dependencies. Applications consuming the
package do not need Pixi, SWIG or a C++ build environment.

## Supported platforms and scope

The build and package-test workflow targets these runtimes on matching hosts:

| Platform | Runtime identifiers | Baseline |
| --- | --- | --- |
| Windows | `win-x64` | x64 only |
| Linux | `linux-x64`, `linux-arm64` | glibc 2.28 |
| macOS | `osx-x64`, `osx-arm64` | macOS 11 |

Linux also needs the distribution's C/C++ runtimes and zlib. Windows ARM64 is not supported.

The bindings include resources, geometry, scene graphs, URDF/SRDF parsing, state
solvers, environments, collision-manager operations and kinematics. The native
wrapper embeds Bullet, FCL, KDL, OPW and UR plugin factories.

Visualization, ROS integration, PCL point-cloud parsing and robot-specific IKFast
solvers are outside this package. Some upstream signatures are excluded explicitly
in [bindings/components](bindings/components). Treat the generated C# declarations
as the reference for what is available.

## Build from source

Run these commands from the repository root. You need:

- The .NET SDK selected by [global.json](global.json), currently .NET 10.
- PowerShell 7, available as `pwsh`, including on Linux and macOS.
- Pixi 0.70.x, as required by [pixi.toml](pixi.toml).
- A host C++ toolchain supported by the Pixi environment. The Windows configuration uses Visual Studio 2026.

Initialize all pinned upstream sources, then build the host's native runtime:

```powershell
git submodule update --init --recursive
pixi run build-native
dotnet build Darp.Tesseract.Native.slnx
```

On Windows, [scripts/install_pixi.ps1](scripts/install_pixi.ps1) can install the
repository's Pixi version. The native build writes to `artifacts/native/<rid>/`.
The managed project copies those files to its output directory.

Generated sources are committed. Regenerate them when changing SWIG declarations
or upstream headers:

```powershell
pixi run -e bindings generate-bindings
pixi run build-native
dotnet build Darp.Tesseract.Native.slnx
```

Keep the generated C# and C++ changes together. Do not edit generated files by hand.

Run the integration tests after building the native runtime:

```powershell
dotnet test --project tests/Darp.Tesseract.Native.IntegrationTests/Darp.Tesseract.Native.IntegrationTests.csproj
```

## Repository layout

| Path | Contents |
| --- | --- |
| `src/Darp.Tesseract.Native/Generated/` | SWIG-generated C# API |
| `src/Darp.Tesseract.Native/Runtime/` | Managed geometry copying and native container support |
| `bindings/components/` | Upstream headers to expose and signatures to exclude |
| `bindings/geometry/` | Eigen mappings, collection mappings and native conversion code |
| `bindings/support/` | Shared SWIG rules for ownership, exceptions and other C++ types |
| `bindings/generated/` | Generated C++ wrapper |
| `native/` | Pinned upstream submodules |
| `tests/Darp.Tesseract.Native.IntegrationTests/` | Native interop and package tests |

The root CMake build copies Tesseract into an ignored build directory and applies
[the runtime dependency patch](patches/tesseract-runtime-dependencies.patch) there.
It leaves the submodule checkout unchanged. The patch disables PCL-backed URDF
point-cloud parsing and removes an unnecessary compiled Boost.Graph dependency.

Tesseract components and plugin factories link into `tesseract_csharp`. Other
required native libraries ship alongside it. KDL remains dynamically linked.
The plugin bootstrap registers the wrapper with Tesseract's plugin loader so
existing SRDF/YAML factory names and search-library entries can be used.

## Pack and release

After building native assets, create the package:

```powershell
dotnet pack src/Darp.Tesseract.Native/Darp.Tesseract.Native.csproj -c Release -o artifacts/packages
```

Packing does not run CMake or Pixi. It includes the runtime directories already
present under `artifacts/native/`. A local build normally provides only the host's
runtime. [CI](.github/workflows/build-package.yml) builds all five runtimes, combines
them into a package, and tests that package on each platform without the native
build environment.

[Release automation](.github/workflows/release.yml) uses conventional commits on
`main` to maintain a release-please PR. Merging it creates a version tag, builds
and tests the package, publishes it to NuGet.org, and attaches package and
symbol files to the GitHub release.

Maintainers must configure NuGet Trusted Publishing for `Darp.Tesseract.Native`,
using the `rosslight/Darp.Tesseract` repository and
`release.yml` workflow. Set the Actions secret or variable `NUGET_USER` to the
NuGet profile username associated with that policy. GitHub Actions also needs
permission to create release PRs.

See [LICENSE](LICENSE) and [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md) for
licensing. Native packages include upstream notices and dependency license materials.
