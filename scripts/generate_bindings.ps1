param(
  [string]$SwigPath = ""
)

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repositoryDir = [System.IO.Path]::GetFullPath((Join-Path $scriptDir ".."))
$upstreamInterface = Join-Path $repositoryDir "native/tesseract_python/tesseract_python/swig/tesseract_common_python.i"
$tesseractIncludeDir = Join-Path $repositoryDir "native/tesseract/common/include"
$interfacePath = Join-Path $repositoryDir "bindings/tesseract_common_csharp.i"
$nativeOutputDir = Join-Path $repositoryDir "bindings/generated"
$managedOutputDir = Join-Path $repositoryDir "src/Darp.Tesseract.Native/Generated"
$wrapperPath = Join-Path $nativeOutputDir "TesseractCommon_wrap.cxx"

if (-not (Test-Path -LiteralPath $upstreamInterface)) {
  throw "The tesseract_python submodule is missing. Run 'git submodule update --init --recursive'."
}

$upstreamText = Get-Content -Raw -LiteralPath $upstreamInterface
$requiredDeclarations = @('%include "tesseract/common/timer.h"')
foreach ($declaration in $requiredDeclarations) {
  if (-not $upstreamText.Contains($declaration)) {
    throw "The upstream SWIG contract changed: expected declaration '$declaration'. Review the C# overlay before regenerating."
  }
}

if ([string]::IsNullOrWhiteSpace($SwigPath)) {
  $swigCommand = Get-Command swig -ErrorAction SilentlyContinue
  if ($null -eq $swigCommand) {
    throw "SWIG was not found. On Windows run './scripts/install_swig.ps1' and pass its output as -SwigPath."
  }
  $SwigPath = $swigCommand.Source
}

if (-not (Test-Path -LiteralPath $SwigPath)) {
  throw "SWIG executable was not found at '$SwigPath'."
}

New-Item -ItemType Directory -Path $nativeOutputDir -Force | Out-Null
New-Item -ItemType Directory -Path $managedOutputDir -Force | Out-Null

Get-ChildItem -LiteralPath $managedOutputDir -Filter "*.cs" -File -ErrorAction SilentlyContinue | Remove-Item -Force

Write-Host "Generating C# bindings with $SwigPath"
& $SwigPath `
  -c++ `
  -std=c++17 `
  -csharp `
  -namespace "Darp.Tesseract.Native" `
  -dllimport "tesseract_common_csharp" `
  -outdir $managedOutputDir `
  -o $wrapperPath `
  "-I$tesseractIncludeDir" `
  $interfacePath

if ($LASTEXITCODE -ne 0) {
  throw "SWIG failed with exit code $LASTEXITCODE."
}

Write-Host "Generated managed sources in $managedOutputDir"
Write-Host "Generated native wrapper at $wrapperPath"
