param(
  [string] $PackageDirectory = 'artifacts/packages',
  [string] $ReleaseTag = ''
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem
$packages = @(Get-ChildItem -LiteralPath $PackageDirectory -Filter '*.nupkg' -File)
if ($packages.Count -ne 1) { throw 'Expected exactly one NuGet package.' }
if ($packages[0].Length -ge 250MB) { throw 'The package exceeds the NuGet.org size limit.' }
$archive = [IO.Compression.ZipFile]::OpenRead($packages[0].FullName)
try {
  $names = @($archive.Entries.FullName)
  foreach ($required in @('LICENSE', 'THIRD-PARTY-NOTICES.md', 'README.md',
    'licenses/upstream/opw_kinematics/LICENSE',
    'licenses/upstream/tesseract/LICENSE',
    'licenses/sources/orocos_kdl-1.5.3.tar.gz', 'licenses/sources/prepare_kdl_shared.cmake')) {
    if ($names -notcontains $required) { throw "Missing package material: $required" }
  }
  $wrappers = [ordered]@{
    'win-x64' = 'tesseract_csharp.dll'
    'linux-x64' = 'libtesseract_csharp.so'
    'linux-arm64' = 'libtesseract_csharp.so'
    'osx-x64' = 'libtesseract_csharp.dylib'
    'osx-arm64' = 'libtesseract_csharp.dylib'
  }
  foreach ($rid in $wrappers.Keys) {
    if ($names -notcontains "runtimes/$rid/native/$($wrappers[$rid])") { throw "Missing wrapper for $rid" }
    if ($names -notcontains "licenses/$rid/dependencies.json") { throw "Missing dependency inventory for $rid" }
    if ($names -notcontains "licenses/$rid/COPYRIGHTS.txt") { throw "Missing upstream copyrights for $rid" }
  }
  if ($names -notcontains 'runtimes/win-x64/native/orocos-kdl.dll') {
    throw 'Windows must ship independently replaceable LGPL KDL.'
  }
  $nuspec = $archive.Entries | Where-Object FullName -Like '*.nuspec' | Select-Object -First 1
  $reader = [IO.StreamReader]::new($nuspec.Open())
  try { [xml]$metadata = $reader.ReadToEnd() } finally { $reader.Dispose() }
  if ($metadata.package.metadata.id -ne 'Darp.Tesseract.Native') { throw 'Unexpected NuGet package ID.' }
  $version = [string]$metadata.package.metadata.version
  if ($ReleaseTag -and $ReleaseTag.TrimStart('v') -ne $version) {
    throw "Release tag '$ReleaseTag' does not match package version '$version'."
  }
  Write-Host "Validated Darp.Tesseract.Native $version with five runtimes, licenses and source materials."
} finally {
  $archive.Dispose()
}
