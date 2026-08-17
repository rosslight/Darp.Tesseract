param(
  [string]$Version = "4.5.0"
)

$ErrorActionPreference = "Stop"

if (-not $IsWindows) {
  throw "The bootstrap script uses the official swigwin archive and therefore only supports Windows. Install SWIG $Version with the platform package manager instead."
}

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repositoryDir = [System.IO.Path]::GetFullPath((Join-Path $scriptDir ".."))
$toolsDir = Join-Path $repositoryDir "artifacts/tools"
$archivePath = Join-Path $toolsDir "swigwin-$Version.zip"
$installDir = Join-Path $toolsDir "swigwin-$Version"
$swigPath = Join-Path $installDir "swig.exe"
$expectedSha256ByVersion = @{
  "4.5.0" = "D08A5B5CFD3F285CCC13B9EE0667F6E05D07433AAAE89E8AE24850E05E62E04E"
}

if (-not $expectedSha256ByVersion.ContainsKey($Version)) {
  throw "No trusted SHA-256 is configured for SWIG $Version. Add the official archive hash before changing versions."
}

New-Item -ItemType Directory -Path $toolsDir -Force | Out-Null

function Test-ZipArchive([string]$Path) {
  if (-not (Test-Path -LiteralPath $Path)) { return $false }

  $stream = [System.IO.File]::OpenRead($Path)
  try {
    return $stream.ReadByte() -eq 0x50 -and $stream.ReadByte() -eq 0x4B
  }
  finally {
    $stream.Dispose()
  }
}

if (-not (Test-ZipArchive $archivePath)) {
  if (Test-Path -LiteralPath $archivePath) {
    Remove-Item -LiteralPath $archivePath -Force
  }

  $downloadUri = "https://downloads.sourceforge.net/project/swig/swigwin/swigwin-$Version/swigwin-$Version.zip"
  Write-Host "Downloading SWIG $Version from $downloadUri"
  & curl.exe --fail --location --retry 3 --output $archivePath $downloadUri
  if ($LASTEXITCODE -ne 0) {
    throw "Failed to download SWIG $Version (curl exit code $LASTEXITCODE)."
  }
}

if (-not (Test-ZipArchive $archivePath)) {
  throw "Downloaded file '$archivePath' is not a ZIP archive."
}

$actualSha256 = (Get-FileHash -LiteralPath $archivePath -Algorithm SHA256).Hash
if ($actualSha256 -ne $expectedSha256ByVersion[$Version]) {
  throw "SWIG archive checksum mismatch. Expected $($expectedSha256ByVersion[$Version]), got $actualSha256."
}

if (Test-Path -LiteralPath $swigPath) {
  Write-Output $swigPath
  exit 0
}

Expand-Archive -LiteralPath $archivePath -DestinationPath $toolsDir -Force

if (-not (Test-Path -LiteralPath $swigPath)) {
  throw "SWIG executable was not found at '$swigPath' after extraction."
}

Write-Output $swigPath
