param(
  [string]$Version = "0.70.1"
)

$ErrorActionPreference = "Stop"

if (-not $IsWindows) {
  throw "This bootstrap script currently installs the Windows x64 Pixi binary. On Linux/macOS, install Pixi with the platform package manager."
}
if ([System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture -ne "X64") {
  throw "The pinned Tesseract packages currently support Windows x64 only."
}

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repositoryDir = [System.IO.Path]::GetFullPath((Join-Path $scriptDir ".."))
$installDir = Join-Path $repositoryDir "artifacts/tools/pixi"
$archivePath = Join-Path $repositoryDir "artifacts/tools/pixi-$Version.zip"
$pixiPath = Join-Path $installDir "pixi.exe"
$downloadUri = "https://github.com/prefix-dev/pixi/releases/download/v$Version/pixi-x86_64-pc-windows-msvc.zip"

if (Test-Path -LiteralPath $pixiPath) {
  Write-Output $pixiPath
  exit 0
}

New-Item -ItemType Directory -Path (Split-Path -Parent $archivePath) -Force | Out-Null
Invoke-WebRequest -Uri $downloadUri -OutFile $archivePath
New-Item -ItemType Directory -Path $installDir -Force | Out-Null
Expand-Archive -LiteralPath $archivePath -DestinationPath $installDir -Force

if (-not (Test-Path -LiteralPath $pixiPath)) {
  throw "Pixi executable was not found after extracting '$archivePath'."
}

Write-Output $pixiPath
