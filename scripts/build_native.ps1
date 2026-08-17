param(
  [Parameter(Mandatory = $true)]
  [ValidateSet("win-x64", "linux-x64", "linux-arm64", "osx-arm64")]
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
$manifestPath = Join-Path $repositoryDir "native/tesseract_nanobind/pyproject.toml"

if (-not (Test-Path -LiteralPath $wrapperPath)) {
  throw "Generated wrapper '$wrapperPath' is missing. Run './scripts/generate_bindings.ps1' first."
}
if (-not (Test-Path -LiteralPath $manifestPath)) {
  throw "The pinned nanobind Pixi manifest is missing. Run 'git submodule update --init --recursive'."
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
  throw "The official Tesseract conda packages are native builds; '$RuntimeId' must be built on a '$RuntimeId' host (current: '$hostRuntime')."
}

if (Test-Path -LiteralPath $buildDir) {
  Remove-Item -LiteralPath $buildDir -Recurse -Force
}
if (Test-Path -LiteralPath $outputDir) {
  Remove-Item -LiteralPath $outputDir -Recurse -Force
}
New-Item -ItemType Directory -Path $buildDir -Force | Out-Null
New-Item -ItemType Directory -Path $outputDir -Force | Out-Null

$pixiArgs = @("run", "--manifest-path", $manifestPath)
& $PixiPath @pixiArgs cmake -S $sourceDir -B $buildDir -G Ninja "-DCMAKE_BUILD_TYPE=$Configuration"
if ($LASTEXITCODE -ne 0) { throw "CMake configure failed with exit code $LASTEXITCODE." }

& $PixiPath @pixiArgs cmake --build $buildDir --config $Configuration --parallel
if ($LASTEXITCODE -ne 0) { throw "CMake build failed with exit code $LASTEXITCODE." }

$libraryPath = switch ($RuntimeId) {
  "win-x64" { Join-Path $buildDir "tesseract_csharp.dll" }
  "linux-x64" { Join-Path $buildDir "libtesseract_csharp.so" }
  "linux-arm64" { Join-Path $buildDir "libtesseract_csharp.so" }
  "osx-arm64" { Join-Path $buildDir "libtesseract_csharp.dylib" }
}
if (-not (Test-Path -LiteralPath $libraryPath)) {
  throw "Native library was not found at '$libraryPath'."
}

Copy-Item -LiteralPath $libraryPath -Destination $outputDir -Force
Write-Host "Copied $libraryPath to $outputDir"
