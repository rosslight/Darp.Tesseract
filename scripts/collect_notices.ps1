param(
  [Parameter(Mandatory)][string] $RuntimeId,
  [Parameter(Mandatory)][string] $BuildDirectory
)

$ErrorActionPreference = 'Stop'
$repositoryDir = Split-Path -Parent $PSScriptRoot
$outputDir = Join-Path $repositoryDir "artifacts/notices/$RuntimeId"
# The RID is supplied by build_native.ps1. Keep deletion inside artifacts/notices.
$noticesRoot = [IO.Path]::GetFullPath((Join-Path $repositoryDir 'artifacts/notices'))
if (-not [IO.Path]::GetFullPath($outputDir).StartsWith($noticesRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
  throw 'Notice output must remain within artifacts/notices.'
}
if (Test-Path -LiteralPath $outputDir) { Remove-Item -LiteralPath $outputDir -Recurse -Force }
New-Item -ItemType Directory -Path $outputDir -Force | Out-Null

$runtimeNames = @(Get-ChildItem "$repositoryDir/artifacts/native/$RuntimeId" -File | ForEach-Object { $_.Name.ToLowerInvariant() })
# Include header/static inputs and owners of the distributed runtime binaries.
$nativeInputs = @('eigen', 'cereal', 'bullet-cpp', 'console_bridge', 'orocos-kdl',
  'libboost-headers', 'boost-cpp', 'libboost-devel')
$attributedRuntimes = @('tesseract_csharp.dll', 'libtesseract_csharp.so', 'libtesseract_csharp.dylib')
foreach ($metadataFile in Get-ChildItem "$env:CONDA_PREFIX/conda-meta" -Filter '*.json') {
  $metadata = Get-Content -LiteralPath $metadataFile.FullName -Raw | ConvertFrom-Json
  $ownedRuntimes = @($metadata.files | ForEach-Object { $_.Replace('\', '/').Split('/')[-1].ToLowerInvariant() } |
    Where-Object { $runtimeNames -contains $_ })
  if ($nativeInputs -notcontains $metadata.name -and $ownedRuntimes.Count -eq 0) { continue }

  $packageDir = $metadata.extracted_package_dir
  if (-not $packageDir) { $packageDir = $metadata.link.source }
  $dependencyDir = Join-Path $outputDir $metadata.name
  $licenseDirectory = "$packageDir/info/licenses"
  if (Test-Path -LiteralPath $licenseDirectory) {
    Copy-Item -LiteralPath $licenseDirectory -Destination $dependencyDir -Recurse
  } else {
    if ([string]::IsNullOrWhiteSpace($metadata.license)) {
      throw "No license attribution found for package '$($metadata.name)'."
    }
    New-Item -ItemType Directory -Path $dependencyDir -Force | Out-Null
    $metadata.license | Set-Content "$dependencyDir/LICENSE.spdx" -Encoding utf8
    Copy-Item -LiteralPath "$packageDir/info/about.json" -Destination $dependencyDir
  }
  if (Test-Path -LiteralPath "$packageDir/info/recipe") {
    Copy-Item -LiteralPath "$packageDir/info/recipe" -Destination "$dependencyDir/source-recipe" -Recurse
  }
  # Cereal's embedded dependencies have additional licenses in the installed headers.
  foreach ($license in $metadata.files | Where-Object { $_.Split('/')[-1] -match '^(LICENSE|COPYING|NOTICE)([._-].*)?$' }) {
    $destination = Join-Path $dependencyDir "installed/$license"
    New-Item -ItemType Directory -Path (Split-Path -Parent $destination) -Force | Out-Null
    Copy-Item -LiteralPath (Join-Path $env:CONDA_PREFIX $license) -Destination $destination
  }
  $attributedRuntimes += $ownedRuntimes
}
if ($RuntimeId -eq 'win-x64') {
  $sourceDir = New-Item -ItemType Directory -Path "$outputDir/orocos-kdl/sources" -Force
  Copy-Item -LiteralPath "$BuildDirectory/kdl-download/orocos_kdl-1.5.3.tar.gz", "$PSScriptRoot/prepare_kdl_shared.cmake" -Destination $sourceDir
  $attributedRuntimes += 'orocos-kdl.dll'
}
foreach ($runtimeName in $runtimeNames) {
  if ($attributedRuntimes -notcontains $runtimeName) { throw "No license attribution found for '$runtimeName'." }
}

# Preserve copyright notices from the incorporated upstream sources.
$copyrightLines = foreach ($source in @('tesseract', 'trajopt', 'boost_plugin_loader', 'opw_kinematics')) {
  Get-ChildItem "$repositoryDir/native/$source" -Recurse -File |
    Where-Object Extension -In '.h', '.hpp', '.cpp', '.cxx', '.inl' |
    Select-String -Pattern '(?i)copyright\s+(\(c\)|[0-9]|©)' |
    ForEach-Object { "$source`: $($_.Line.Trim())" }
}
$copyrightLines | Sort-Object -Unique | Set-Content "$outputDir/COPYRIGHTS.txt" -Encoding utf8
