# Integration tests

These tests exercise the public generated API against the real native wrapper.
The ABB fixture covers URDF/SRDF loading, FK/Jacobian/IK round trips, embedded
collision and OPW factories, and environment commands. Geometry checks cover
strided vector inputs, row-major matrix properties, shape rejection, empty vectors,
output replacement, snapshots after disposal, and container views after explicit disposal and collection. Focused managed geometry
tests cover final-owner release, retained views and pins, alias disposal, failed
ownership transfer, read-only views, representative transform algebra, empty shapes
and stable normalization across layouts.
They deliberately avoid exhaustive overload and coefficient-by-coefficient math tests.

Build the host runtime first with `pixi run build-native`, then run:

```powershell
dotnet test --project tests/Darp.Tesseract.Native.IntegrationTests/Darp.Tesseract.Native.IntegrationTests.csproj
```

CI packs `Darp.Geometry` and `Darp.Tesseract.Native`, restores this executable with
`-p:UseLocalProjectReference=false`, and runs the same tests against those packages.
That mode adds a packaging check and requires `CONDA_PREFIX` and Pixi PATH entries
to be absent. See `.github/workflows/build-package.yml` for the exact commands.

Forced collection tests use non-inlined helpers so the originating native proxy
or collection is out of scope before collection. They verify retained views remain
usable; they are not an exhaustive native leak or concurrency test.
