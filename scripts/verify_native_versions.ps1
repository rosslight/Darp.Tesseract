$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repositoryDir = [System.IO.Path]::GetFullPath((Join-Path $scriptDir ".."))
$packageManifestPath = Join-Path $repositoryDir "packaging/tesseract/pixi.toml"
$tesseractPackagePath = Join-Path $repositoryDir "native/tesseract/package.xml"
$cmakePath = Join-Path $repositoryDir "native/CMakeLists.txt"

foreach ($path in @($packageManifestPath, $tesseractPackagePath, $cmakePath)) {
  if (-not (Test-Path -LiteralPath $path)) {
    throw "Native version input '$path' is missing. Run 'git submodule update --init --recursive'."
  }
}

$packageManifest = Get-Content -Raw -LiteralPath $packageManifestPath
$packageVersionMatch = [regex]::Match(
  $packageManifest,
  '(?ms)^\[package\]\s+.*?^version\s*=\s*"(?<version>[^"]+)"')
if (-not $packageVersionMatch.Success) {
  throw "Could not read the Tesseract version from '$packageManifestPath'."
}
$packageVersion = $packageVersionMatch.Groups["version"].Value

[xml]$tesseractPackage = Get-Content -Raw -LiteralPath $tesseractPackagePath
$sourceVersion = [string]$tesseractPackage.package.version
if ([string]::IsNullOrWhiteSpace($sourceVersion)) {
  throw "Could not read the Tesseract version from '$tesseractPackagePath'."
}

$cmake = Get-Content -Raw -LiteralPath $cmakePath
$cmakeVersionMatches = [regex]::Matches(
  $cmake,
  'find_package\(tesseract\s+(?<version>[0-9]+\.[0-9]+\.[0-9]+)\s+REQUIRED')
if ($cmakeVersionMatches.Count -eq 0) {
  throw "Could not read the required Tesseract version from '$cmakePath'."
}
$cmakeVersions = @($cmakeVersionMatches | ForEach-Object { $_.Groups["version"].Value } | Select-Object -Unique)

$versions = @($packageVersion, $sourceVersion) + $cmakeVersions
if (@($versions | Select-Object -Unique).Count -ne 1) {
  throw "Tesseract versions disagree: package=$packageVersion, source=$sourceVersion, CMake=$($cmakeVersions -join ', ')."
}

Write-Host "Tesseract version $packageVersion is consistent across the source submodule, Pixi package, and native wrapper."
