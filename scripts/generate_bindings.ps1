param(
  [string]$SwigPath = ""
)

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repositoryDir = [System.IO.Path]::GetFullPath((Join-Path $scriptDir ".."))
$tesseractDir = Join-Path $repositoryDir "native/tesseract"
$nanobindDir = Join-Path $repositoryDir "native/tesseract_nanobind"
$nativeOutputDir = Join-Path $repositoryDir "bindings/generated"
$managedOutputDir = Join-Path $repositoryDir "src/Darp.Tesseract.Native/Generated"
$interfacePath = Join-Path $repositoryDir "bindings/tesseract_csharp.i"

$coverageContracts = @(
  @{ Path = "src/tesseract_common/tesseract_common_bindings.cpp"; Text = "nb::class_<tesseract::common::JointState>" },
  @{ Path = "src/tesseract_geometry/tesseract_geometry_bindings.cpp"; Text = "nb::class_<tg::Geometry>" },
  @{ Path = "src/tesseract_scene_graph/tesseract_scene_graph_bindings.cpp"; Text = "nb::class_<tsg::SceneGraph>" },
  @{ Path = "src/tesseract_urdf/tesseract_urdf_bindings.cpp"; Text = 'm.def("parseURDFString"' },
  @{ Path = "src/tesseract_srdf/tesseract_srdf_bindings.cpp"; Text = "nb::class_<ts::SRDFModel>" },
  @{ Path = "src/tesseract_state_solver/tesseract_state_solver_bindings.cpp"; Text = "nb::class_<tsg::StateSolver>" },
  @{ Path = "src/tesseract_kinematics/tesseract_kinematics_bindings.cpp"; Text = "nb::class_<tk::KinematicGroup" },
  @{ Path = "src/tesseract_environment/tesseract_environment_bindings.cpp"; Text = "nb::class_<te::Environment>" }
)

foreach ($contract in $coverageContracts) {
  $path = Join-Path $nanobindDir $contract.Path
  if (-not (Test-Path -LiteralPath $path)) {
    throw "The pinned nanobind coverage source is missing: '$path'. Run 'git submodule update --init --recursive'."
  }
  if (-not (Get-Content -Raw -LiteralPath $path).Contains($contract.Text)) {
    throw "The nanobind coverage contract changed: expected '$($contract.Text)' in '$path'. Review bindings/tesseract_csharp.i."
  }
}

$includeDirs = @(
  (Join-Path $repositoryDir "bindings"),
  (Join-Path $repositoryDir "native/eigen"),
  (Join-Path $tesseractDir "common/include"),
  (Join-Path $tesseractDir "geometry/include"),
  (Join-Path $tesseractDir "scene_graph/include"),
  (Join-Path $tesseractDir "urdf/include"),
  (Join-Path $tesseractDir "srdf/include"),
  (Join-Path $tesseractDir "state_solver/include"),
  (Join-Path $tesseractDir "kinematics/core/include"),
  (Join-Path $tesseractDir "environment/include")
)

foreach ($path in @($interfacePath) + $includeDirs) {
  if (-not (Test-Path -LiteralPath $path)) {
    throw "A binding input is missing: '$path'. Run 'git submodule update --init --recursive'."
  }
}

if ([string]::IsNullOrWhiteSpace($SwigPath)) {
  $localSwig = Join-Path $repositoryDir "artifacts/tools/swigwin-4.5.0/swig.exe"
  if (Test-Path -LiteralPath $localSwig) {
    $SwigPath = $localSwig
  } else {
    $swigCommand = Get-Command swig -ErrorAction SilentlyContinue
    if ($null -eq $swigCommand) {
      throw "SWIG was not found. On Windows run './scripts/install_swig.ps1' or pass -SwigPath."
    }
    $SwigPath = $swigCommand.Source
  }
}

if (-not (Test-Path -LiteralPath $SwigPath)) {
  throw "SWIG executable was not found at '$SwigPath'."
}

New-Item -ItemType Directory -Path $nativeOutputDir -Force | Out-Null
New-Item -ItemType Directory -Path $managedOutputDir -Force | Out-Null

Get-ChildItem -LiteralPath $managedOutputDir -Filter "*.cs" -File -ErrorAction SilentlyContinue | Remove-Item -Force
Get-ChildItem -LiteralPath $nativeOutputDir -Filter "*_wrap.cxx" -File -ErrorAction SilentlyContinue | Remove-Item -Force

$wrapperPath = Join-Path $nativeOutputDir "TesseractNative_wrap.cxx"
$arguments = @(
  "-c++",
  "-std=c++17",
  "-csharp",
  "-namespace", "Darp.Tesseract.Native",
  "-dllimport", "tesseract_csharp",
  "-outdir", $managedOutputDir,
  "-o", $wrapperPath
)
$arguments += $includeDirs | ForEach-Object { "-I$_" }
$arguments += $interfacePath

Write-Host "Generating the native Tesseract API with $SwigPath"
& $SwigPath @arguments
if ($LASTEXITCODE -ne 0) {
  throw "SWIG failed with exit code $LASTEXITCODE."
}

Write-Host "Generated managed sources in $managedOutputDir"
Write-Host "Generated native wrapper in $wrapperPath"
