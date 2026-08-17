param(
  [Parameter(Mandatory = $true)]
  [ValidateSet("win-x64", "win-arm64", "linux-x64", "linux-arm64", "osx-x64", "osx-arm64")]
  [string]$RuntimeId,
  [Parameter(Mandatory = $true)]
  [string]$Generator,
  [string]$Toolset = "",
  [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$parts = $RuntimeId -split '-', 2
$os = $parts[0]
$arch = $parts[1]
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repositoryDir = [System.IO.Path]::GetFullPath((Join-Path $scriptDir ".."))
$sourceDir = Join-Path $repositoryDir "native"
$buildDir = Join-Path $repositoryDir "artifacts/build/$RuntimeId"
$outputDir = Join-Path $repositoryDir "artifacts/native/$RuntimeId"
$wrapperPaths = @(
  (Join-Path $repositoryDir "bindings/generated/TesseractCommon_wrap.cxx"),
  (Join-Path $repositoryDir "bindings/generated/TesseractKinematics_wrap.cxx")
)

foreach ($wrapperPath in $wrapperPaths) {
  if (-not (Test-Path -LiteralPath $wrapperPath)) {
    throw "Generated wrapper '$wrapperPath' is missing. Run './scripts/generate_bindings.ps1' first."
  }
}

if (Test-Path -LiteralPath $buildDir) {
  Remove-Item -LiteralPath $buildDir -Recurse -Force
}
New-Item -ItemType Directory -Path $buildDir -Force | Out-Null
New-Item -ItemType Directory -Path $outputDir -Force | Out-Null

$cmakeArgs = @("-S", $sourceDir, "-B", $buildDir, "-G", $Generator)
if (-not [string]::IsNullOrWhiteSpace($Toolset)) {
  $cmakeArgs += "-T", $Toolset
}
switch ($os) {
  "win" {
    $msvcArch = if ($arch -eq "arm64") { "ARM64" } else { "x64" }
    $cmakeArgs += "-A", $msvcArch
  }
  "linux" {
    $cmakeArgs += "-DCMAKE_BUILD_TYPE=$Configuration"
  }
  "osx" {
    $osxArch = if ($arch -eq "x64") { "x86_64" } else { "arm64" }
    $cmakeArgs += "-DCMAKE_BUILD_TYPE=$Configuration", "-DCMAKE_OSX_ARCHITECTURES=$osxArch", "-DCMAKE_OSX_DEPLOYMENT_TARGET=11.0"
  }
}

& cmake @cmakeArgs
if ($LASTEXITCODE -ne 0) { throw "CMake configure failed with exit code $LASTEXITCODE." }

& cmake --build $buildDir --config $Configuration --parallel
if ($LASTEXITCODE -ne 0) { throw "CMake build failed with exit code $LASTEXITCODE." }

$libraryNames = @("tesseract_common_csharp", "tesseract_kinematics_csharp")
foreach ($libraryName in $libraryNames) {
  switch ($os) {
    "win" { $libraryPath = Join-Path $buildDir "$Configuration/$libraryName.dll" }
    "linux" { $libraryPath = Join-Path $buildDir "lib$libraryName.so" }
    "osx" { $libraryPath = Join-Path $buildDir "lib$libraryName.dylib" }
  }

  if (-not (Test-Path -LiteralPath $libraryPath)) {
    throw "Native library was not found at '$libraryPath'."
  }

  Copy-Item -LiteralPath $libraryPath -Destination $outputDir -Force
  Write-Host "Copied $libraryPath to $outputDir"
}
