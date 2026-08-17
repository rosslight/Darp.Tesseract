param(
  [string]$SwigPath = ""
)

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repositoryDir = [System.IO.Path]::GetFullPath((Join-Path $scriptDir ".."))
$upstreamCommonInterface = Join-Path $repositoryDir "native/tesseract_python/tesseract_python/swig/tesseract_common_python.i"
$upstreamKinematicsInterface = Join-Path $repositoryDir "native/tesseract_python/tesseract_python/swig/tesseract_kinematics_python.i"
$upstreamInverseKinematicsHeader = Join-Path $repositoryDir "native/tesseract/kinematics/core/include/tesseract/kinematics/inverse_kinematics.h"
$upstreamOpwHeader = Join-Path $repositoryDir "native/tesseract/kinematics/opw/include/tesseract/kinematics/opw/opw_inv_kin.h"
$upstreamOpwParametersHeader = Join-Path $repositoryDir "native/opw_kinematics/include/opw_kinematics/opw_parameters.h"
$tesseractCommonIncludeDir = Join-Path $repositoryDir "native/tesseract/common/include"
$nativeOutputDir = Join-Path $repositoryDir "bindings/generated"
$managedOutputDir = Join-Path $repositoryDir "src/Darp.Tesseract.Native/Generated"

$requiredUpstreamFiles = @(
  $upstreamCommonInterface,
  $upstreamKinematicsInterface,
  $upstreamInverseKinematicsHeader,
  $upstreamOpwHeader,
  $upstreamOpwParametersHeader
)
if ($requiredUpstreamFiles.Where({ -not (Test-Path -LiteralPath $_) }).Count -ne 0) {
  throw "An upstream submodule is missing. Run 'git submodule update --init --recursive'."
}

$upstreamContracts = @(
  @{ Path = $upstreamCommonInterface; Declarations = @('%include "tesseract/common/timer.h"') },
  @{ Path = $upstreamKinematicsInterface; Declarations = @('%include "tesseract/kinematics/inverse_kinematics.h"', '//%shared_ptr(tesseract::kinematics::OPWInvKin)') },
  @{ Path = $upstreamInverseKinematicsHeader; Declarations = @('IKSolutions calcInvKin(const tesseract::common::TransformMap& tip_link_poses,', 'const Eigen::Ref<const Eigen::VectorXd>& seed) const;') },
  @{ Path = $upstreamOpwHeader; Declarations = @('class OPWInvKin : public InverseKinematics', 'OPWInvKin(opw_kinematics::Parameters<double> params,') },
  @{ Path = $upstreamOpwParametersHeader; Declarations = @('std::array<T, 6> offsets;', 'std::array<signed char, 6> sign_corrections;') }
)
foreach ($contract in $upstreamContracts) {
  $upstreamText = Get-Content -Raw -LiteralPath $contract.Path
  foreach ($declaration in $contract.Declarations) {
    if (-not $upstreamText.Contains($declaration)) {
      throw "The upstream SWIG contract changed: expected declaration '$declaration' in '$($contract.Path)'. Review the C# overlay before regenerating."
    }
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
$modules = @(
  @{ Interface = "tesseract_common_csharp.i"; Wrapper = "TesseractCommon_wrap.cxx"; Library = "tesseract_common_csharp"; Includes = @($tesseractCommonIncludeDir) },
  @{ Interface = "tesseract_kinematics_csharp.i"; Wrapper = "TesseractKinematics_wrap.cxx"; Library = "tesseract_kinematics_csharp"; Includes = @() }
)
foreach ($module in $modules) {
  $interfacePath = Join-Path $repositoryDir "bindings/$($module.Interface)"
  $wrapperPath = Join-Path $nativeOutputDir $module.Wrapper
  $arguments = @(
    "-c++",
    "-std=c++17",
    "-csharp",
    "-namespace", "Darp.Tesseract.Native",
    "-dllimport", $module.Library,
    "-outdir", $managedOutputDir,
    "-o", $wrapperPath
  )
  $arguments += $module.Includes | ForEach-Object { "-I$_" }
  $arguments += $interfacePath

  & $SwigPath @arguments

  if ($LASTEXITCODE -ne 0) {
    throw "SWIG failed for '$($module.Interface)' with exit code $LASTEXITCODE."
  }
}

Write-Host "Generated managed sources in $managedOutputDir"
Write-Host "Generated native wrappers in $nativeOutputDir"
