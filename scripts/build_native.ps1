param(
  [Parameter(Mandatory = $true)]
  [ValidateSet("win-x64", "linux-x64", "linux-arm64", "osx-x64", "osx-arm64")]
  [string]$RuntimeId,
  [string]$PixiPath = "",
  [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repositoryDir = [System.IO.Path]::GetFullPath((Join-Path $scriptDir ".."))
$sourceDir = Join-Path $repositoryDir "native"
$buildDir = Join-Path $repositoryDir "artifacts/build/$RuntimeId"
$outputDir = Join-Path $repositoryDir "artifacts/native/$RuntimeId"
$wrapperPath = Join-Path $repositoryDir "bindings/generated/TesseractNative_wrap.cxx"
$manifestPath = Join-Path $repositoryDir "pixi.toml"
$runtimeCollectorPath = Join-Path $scriptDir "collect_runtime_dependencies.cmake"

& (Join-Path $scriptDir "verify_native_versions.ps1")

if (-not (Test-Path -LiteralPath $wrapperPath)) {
  throw "Generated wrapper '$wrapperPath' is missing. Run './scripts/generate_bindings.ps1' first."
}
if (-not (Test-Path -LiteralPath $manifestPath)) {
  throw "The repository Pixi manifest '$manifestPath' is missing."
}
if (-not (Test-Path -LiteralPath $runtimeCollectorPath)) {
  throw "The runtime dependency collector '$runtimeCollectorPath' is missing."
}

if ([string]::IsNullOrWhiteSpace($PixiPath)) {
  $localPixi = Join-Path $repositoryDir "artifacts/tools/pixi/pixi.exe"
  if (Test-Path -LiteralPath $localPixi) {
    $PixiPath = $localPixi
  } else {
    $pixiCommand = Get-Command pixi -ErrorAction SilentlyContinue
    if ($null -eq $pixiCommand) {
      throw "Pixi was not found. Run './scripts/install_pixi.ps1' or pass -PixiPath."
    }
    $PixiPath = $pixiCommand.Source
  }
}

$hostRuntime = if ($IsWindows) {
  "win-x64"
} elseif ($IsMacOS) {
  if ([System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture -eq "Arm64") { "osx-arm64" } else { "osx-x64" }
} else {
  if ([System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture -eq "Arm64") { "linux-arm64" } else { "linux-x64" }
}
if ($RuntimeId -ne $hostRuntime) {
  throw "Tesseract and the generated wrapper are native builds; '$RuntimeId' must be built on a '$RuntimeId' host (current: '$hostRuntime')."
}

if (Test-Path -LiteralPath $buildDir) {
  Remove-Item -LiteralPath $buildDir -Recurse -Force
}
if (Test-Path -LiteralPath $outputDir) {
  Remove-Item -LiteralPath $outputDir -Recurse -Force
}
New-Item -ItemType Directory -Path $buildDir -Force | Out-Null
New-Item -ItemType Directory -Path $outputDir -Force | Out-Null

$pixiArgs = @("run", "--manifest-path", $manifestPath, "--environment", "default")
& $PixiPath @pixiArgs cmake -S $sourceDir -B $buildDir -G Ninja `
  "-DCMAKE_BUILD_TYPE=$Configuration" `
  "-DCMAKE_TRY_COMPILE_CONFIGURATION=$Configuration"
if ($LASTEXITCODE -ne 0) { throw "CMake configure failed with exit code $LASTEXITCODE." }

& $PixiPath @pixiArgs cmake --build $buildDir --config $Configuration --parallel
if ($LASTEXITCODE -ne 0) { throw "CMake build failed with exit code $LASTEXITCODE." }

$libraryPath = switch ($RuntimeId) {
  "win-x64" { Join-Path $buildDir "tesseract_csharp.dll" }
  "linux-x64" { Join-Path $buildDir "libtesseract_csharp.so" }
  "linux-arm64" { Join-Path $buildDir "libtesseract_csharp.so" }
  "osx-x64" { Join-Path $buildDir "libtesseract_csharp.dylib" }
  "osx-arm64" { Join-Path $buildDir "libtesseract_csharp.dylib" }
}
if (-not (Test-Path -LiteralPath $libraryPath)) {
  throw "Native library was not found at '$libraryPath'."
}

$collectorArgs = @(
  "-DWRAPPER_LIBRARY=$libraryPath"
  "-DOUTPUT_DIRECTORY=$outputDir"
)
if ($RuntimeId -eq "win-x64") {
  $linkerEntry = Get-Content -LiteralPath (Join-Path $buildDir "CMakeCache.txt") |
    Where-Object { $_ -match '^CMAKE_LINKER:FILEPATH=' } |
    Select-Object -First 1
  if ($null -eq $linkerEntry) {
    throw "CMAKE_LINKER was not found in the native build cache."
  }
  $linkerPath = $linkerEntry.Substring($linkerEntry.IndexOf('=') + 1)
  $dumpbinPath = Join-Path (Split-Path -Parent $linkerPath) "dumpbin.exe"
  if (-not (Test-Path -LiteralPath $dumpbinPath)) {
    throw "dumpbin.exe was not found next to the configured linker at '$dumpbinPath'."
  }
  $collectorArgs += "-DRUNTIME_DEPENDENCY_TOOL=$dumpbinPath"
}
$collectorArgs += @("-P", $runtimeCollectorPath)

& $PixiPath @pixiArgs cmake @collectorArgs
if ($LASTEXITCODE -ne 0) { throw "Native runtime collection failed with exit code $LASTEXITCODE." }
