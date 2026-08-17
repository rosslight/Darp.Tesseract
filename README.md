# Darp.Tesseract.Native

Managed .NET bindings for [Tesseract Robotics](https://github.com/tesseract-robotics/tesseract), generated with SWIG and distributed with RID-specific native runtimes.

## Current scope

This repository establishes a maintainable end-to-end binding and packaging path. The first intentionally small slice wraps `tesseract_common`'s `Timer` and `Stopwatch` APIs. It validates C# proxy generation, C++ compilation, native object ownership, method calls, numeric return marshalling, native loading, and SWIG cleanup without requiring Tesseract's complete motion-planning dependency graph. `Timer.start` is intentionally deferred until the managed callback lifetime contract is designed.

Both upstream repositories are unmodified submodules pinned to matching releases:

- `native/tesseract`: Tesseract C++ source (`0.35.0`)
- `native/tesseract_python`: canonical upstream SWIG definitions (`0.35.0`)

The upstream definitions target Python and contain NumPy/Python-only typemaps. `bindings/tesseract_common_csharp.i` is the C#-portable overlay. Binding generation checks that the reused declarations are still present upstream, so an upstream change fails loudly instead of silently changing the managed ABI.

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
