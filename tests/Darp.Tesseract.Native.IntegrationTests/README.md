# Tests

This project tests the generated bindings against the real
native wrapper. It uses xUnit v3 and Microsoft.Testing.Platform, selected by the
repository's [global.json](../../global.json).

## Run against the source projects

From the repository root, build the host's native runtime and run the tests:

```powershell
pixi run build-native
dotnet test --project tests/Darp.Tesseract.Native.IntegrationTests/Darp.Tesseract.Native.IntegrationTests.csproj
```

You only need to rebuild native code after changes that affect the native wrapper
or its dependencies. See [source build setup](../../README.md#build-from-source)
for prerequisites and submodules.

The test project references the source project by default. Native assets come
from `artifacts/native/<rid>/` and copy to the managed output directory. Tests load
their robot files from `Assets/`, which the test project also copies to its output.

## What the tests cover

| File | Checks |
| --- | --- |
| [KinematicsTests.cs](KinematicsTests.cs) | ABB robot loading, FK/Jacobian/IK round trips, collision and OPW plugins, environment commands, copied inputs, shape checks, output replacement and detached result lifetimes |

The ABB IRB 2400 fixture and its plugin configuration live under
[Assets/darp_test](Assets/darp_test). Native result tests include collection disposal and forced GC to check that
copied values remain usable. They do not measure
native leaks or establish thread safety.

## Run against local packages

Build the native assets and pack the library first, following the
[pack instructions](../../README.md#pack-and-release). Then restore and test with
project references disabled:

```powershell
dotnet restore tests/Darp.Tesseract.Native.IntegrationTests/Darp.Tesseract.Native.IntegrationTests.csproj --source artifacts/packages --source https://api.nuget.org/v3/index.json -p:UseLocalProjectReference=false

dotnet test --project tests/Darp.Tesseract.Native.IntegrationTests/Darp.Tesseract.Native.IntegrationTests.csproj -c Release --no-restore -p:UseLocalProjectReference=false
```

Run package tests outside a Pixi shell. This mode adds a deployment check that
requires an empty `CONDA_PREFIX` and no `.pixi` entries in `PATH`. It checks that
the package supplies the wrapper without separate Tesseract component libraries
or the disabled PCL/VTK dependencies.

Use `-p:DarpTesseractNativeVersion=<version>` on both commands to test a version
other than the repository's current version. When rebuilding the same package
version, use a fresh NuGet package cache so restore does not reuse an earlier build.

To return to source-project testing, restore again without
`UseLocalProjectReference=false` before using `--no-restore`.

[The package workflow](../../.github/workflows/build-package.yml) runs this mode
on each supported platform.

## Common setup failures

- A missing `tesseract_csharp` library usually means the host runtime has not been built or copied to the output.
- A native loader error can also indicate a missing adjacent dependency. Keep the files from `artifacts/native/<rid>/` together.
- Resource or plugin errors should be investigated against the files in the output's `Assets/` directory and the group names in the SRDF.
- Package tests rejecting `CONDA_PREFIX` or `PATH` are detecting the build environment. Run them in a normal shell.
