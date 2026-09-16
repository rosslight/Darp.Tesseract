param(
  [Parameter(Mandatory)][ValidateSet('win-x64', 'linux-x64', 'linux-arm64', 'osx-x64', 'osx-arm64')][string] $RuntimeId
)

$ErrorActionPreference = 'Stop'
$repositoryDir = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$runtimeDir = Join-Path $repositoryDir "artifacts/native/$RuntimeId"
$outputDir = Join-Path $repositoryDir "artifacts/notices/$RuntimeId"
if ([string]::IsNullOrWhiteSpace($env:CONDA_PREFIX)) {
  throw 'Collect notices inside the pinned Pixi environment.'
}
$noticesRoot = [IO.Path]::GetFullPath((Join-Path $repositoryDir 'artifacts/notices'))
if (-not [IO.Path]::GetFullPath($outputDir).StartsWith($noticesRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
  throw 'Notice output must remain within artifacts/notices.'
}
if (Test-Path -LiteralPath $outputDir) { Remove-Item -LiteralPath $outputDir -Recurse -Force }
New-Item -ItemType Directory -Path $outputDir -Force | Out-Null

$runtimeNames = @(Get-ChildItem -LiteralPath $runtimeDir -File | ForEach-Object { $_.Name.ToLowerInvariant() })
# Include libraries incorporated through headers or static linking, as well as
# owners of every adjacent runtime binary. Build-only tools are not distributed.
$nativeInputs = @('eigen', 'cereal', 'bullet-cpp', 'console_bridge', 'orocos-kdl',
  'libboost-headers', 'boost-cpp', 'libboost-devel')
$inventory = @()
$attributedRuntimes = @('tesseract_csharp.dll', 'libtesseract_csharp.so', 'libtesseract_csharp.dylib')
foreach ($metadataFile in Get-ChildItem (Join-Path $env:CONDA_PREFIX 'conda-meta') -Filter '*.json' | Sort-Object Name) {
  $metadata = Get-Content -LiteralPath $metadataFile.FullName -Raw | ConvertFrom-Json
  $ownedRuntimes = @($metadata.files | Where-Object {
    $fileName = $_.Replace('\', '/').Split('/')[-1].ToLowerInvariant()
    $runtimeNames -contains $fileName
  })
  if ($nativeInputs -notcontains $metadata.name -and $ownedRuntimes.Count -eq 0) {
    continue
  }

  $packageDir = $metadata.extracted_package_dir
  if ([string]::IsNullOrWhiteSpace($packageDir)) { $packageDir = $metadata.link.source }
  $licenseDir = Join-Path $packageDir 'info/licenses'
  $licenseFiles = @(Get-ChildItem -LiteralPath $licenseDir -Recurse -File -ErrorAction SilentlyContinue)
  if ($licenseFiles.Count -eq 0) {
    throw "No packaged license texts found for '$($metadata.name)' in '$licenseDir'."
  }
  $dependencyDir = Join-Path $outputDir $metadata.name
  foreach ($licenseFile in $licenseFiles) {
    $destination = Join-Path $dependencyDir ([IO.Path]::GetRelativePath($licenseDir, $licenseFile.FullName))
    New-Item -ItemType Directory -Path (Split-Path -Parent $destination) -Force | Out-Null
    Copy-Item -LiteralPath $licenseFile.FullName -Destination $destination
  }
  # Header packages can contain further third-party license texts outside
  # info/licenses (for example Cereal's bundled RapidJSON and RapidXML).
  foreach ($installedLicense in $metadata.files | Where-Object {
    $_.Replace('\', '/').Split('/')[-1] -match '^(LICENSE|COPYING|NOTICE)([._-].*)?$'
  }) {
    $source = Join-Path $env:CONDA_PREFIX $installedLicense
    $destination = Join-Path $dependencyDir "installed/$installedLicense"
    New-Item -ItemType Directory -Path (Split-Path -Parent $destination) -Force | Out-Null
    Copy-Item -LiteralPath $source -Destination $destination
  }
  $recipeDir = Join-Path $packageDir 'info/recipe'
  if (Test-Path -LiteralPath $recipeDir) {
    Copy-Item -LiteralPath $recipeDir -Destination (Join-Path $dependencyDir 'source-recipe') -Recurse -Force
  }
  $inventory += [ordered]@{
    name = $metadata.name
    version = $metadata.version
    build = $metadata.build
    license = $metadata.license
    packageUrl = $metadata.url
    sha256 = $metadata.sha256
    runtimeFiles = $ownedRuntimes
  }
  $attributedRuntimes += $ownedRuntimes | ForEach-Object { $_.Replace('\', '/').Split('/')[-1].ToLowerInvariant() }
}
if ($RuntimeId -eq 'win-x64') { $attributedRuntimes += 'orocos-kdl.dll' }
foreach ($runtimeName in $runtimeNames) {
  if ($attributedRuntimes -notcontains $runtimeName) { throw "No license attribution found for '$runtimeName'." }
}
$inventory | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $outputDir 'dependencies.json') -Encoding utf8

# Preserve concrete copyright notices from the pinned, incorporated upstream
# sources, including the BSD-licensed UR solver and headers.
$copyrightLines = foreach ($source in @('tesseract', 'boost_plugin_loader', 'opw_kinematics')) {
  $sourceDir = Join-Path $repositoryDir "native/$source"
  Get-ChildItem -LiteralPath $sourceDir -Recurse -File |
    Where-Object Extension -In '.h', '.hpp', '.cpp', '.cxx', '.inl' |
    Select-String -Pattern '(?i)copyright\s+(\(c\)|[0-9]|©)' |
    ForEach-Object { "$source`: $($_.Line.Trim())" }
}
$copyrightLines | Sort-Object -Unique | Set-Content -LiteralPath (Join-Path $outputDir 'COPYRIGHTS.txt') -Encoding utf8
Write-Host "Collected notices for $($inventory.Count) native dependencies ($RuntimeId)."
