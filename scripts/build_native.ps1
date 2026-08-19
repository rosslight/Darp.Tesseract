$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repositoryDir = [System.IO.Path]::GetFullPath((Join-Path $scriptDir ".."))
$wrapperPath = Join-Path $repositoryDir "bindings/generated/TesseractNative_wrap.cxx"
$runtimeCollectorPath = Join-Path $scriptDir "collect_runtime_dependencies.cmake"

if ([string]::IsNullOrWhiteSpace($env:CONDA_PREFIX)) {
  throw "The native environment is not active. Run 'pixi run build-native'."
}
if (-not (Test-Path -LiteralPath $wrapperPath)) {
  throw "Generated wrapper '$wrapperPath' is missing. Run 'pixi run -e bindings generate-bindings'."
}

$architecture = [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture
$runtimeId = if ($IsWindows) {
  if ($architecture -ne [System.Runtime.InteropServices.Architecture]::X64) {
    throw "Windows ARM64 is not supported yet."
  }
  "win-x64"
} elseif ($IsMacOS) {
  switch ($architecture) {
    ([System.Runtime.InteropServices.Architecture]::X64) { "osx-x64" }
    ([System.Runtime.InteropServices.Architecture]::Arm64) { "osx-arm64" }
    default { throw "Unsupported macOS host architecture: $architecture" }
  }
} elseif ($IsLinux) {
  switch ($architecture) {
    ([System.Runtime.InteropServices.Architecture]::X64) { "linux-x64" }
    ([System.Runtime.InteropServices.Architecture]::Arm64) { "linux-arm64" }
    default { throw "Unsupported Linux host architecture: $architecture" }
  }
} else {
  throw "Unsupported native build host: $([System.Runtime.InteropServices.RuntimeInformation]::OSDescription)"
}

$buildDir = Join-Path $repositoryDir "artifacts/build/$runtimeId"
$outputDir = Join-Path $repositoryDir "artifacts/native/$runtimeId"

function Set-PortableWrapperRuntimePath([string] $libraryPath) {
  if ($IsLinux) {
    & patchelf --set-rpath '$ORIGIN' $libraryPath
    if ($LASTEXITCODE -ne 0) {
      throw "Could not normalize the Linux runtime path in '$libraryPath'."
    }
    return
  }

  if ($IsMacOS) {
    $loadCommands = & otool -l $libraryPath
    if ($LASTEXITCODE -ne 0) {
      throw "Could not inspect the macOS runtime paths in '$libraryPath'."
    }

    $runtimePaths = [System.Collections.Generic.List[string]]::new()
    for ($index = 0; $index -lt $loadCommands.Count; $index++) {
      if ($loadCommands[$index].Trim() -ne 'cmd LC_RPATH') {
        continue
      }
      for ($detailIndex = $index + 1; $detailIndex -lt [Math]::Min($index + 5, $loadCommands.Count); $detailIndex++) {
        if ($loadCommands[$detailIndex] -match '^\s*path\s+(.+?)\s+\(offset\s+\d+\)$') {
          $runtimePaths.Add($Matches[1])
          break
        }
      }
    }

    foreach ($runtimePath in ($runtimePaths | Select-Object -Unique)) {
      if ($runtimePath -ne '@loader_path') {
        & install_name_tool -delete_rpath $runtimePath $libraryPath
        if ($LASTEXITCODE -ne 0) {
          throw "Could not remove macOS runtime path '$runtimePath' from '$libraryPath'."
        }
      }
    }
    if (-not $runtimePaths.Contains('@loader_path')) {
      & install_name_tool -add_rpath '@loader_path' $libraryPath
      if ($LASTEXITCODE -ne 0) {
        throw "Could not add the relative macOS runtime path to '$libraryPath'."
      }
    }
  }
}

function Assert-PortableRuntimePaths([string[]] $libraryPaths, [switch] $RequireWrapperRelativePath) {
  if (-not ($IsLinux -or $IsMacOS)) {
    return
  }

  $forbiddenPaths = @($env:CONDA_PREFIX, $repositoryDir, $buildDir) |
    Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
    ForEach-Object { [System.IO.Path]::GetFullPath($_) }

  foreach ($candidatePath in $libraryPaths) {
    $runtimeMetadata = if ($IsLinux) {
      & readelf -d $candidatePath
    } else {
      & otool -l $candidatePath | Select-Object -Skip 1
    }
    if ($LASTEXITCODE -ne 0) {
      throw "Could not inspect native runtime paths in '$candidatePath'."
    }

    $runtimeMetadataText = $runtimeMetadata -join "`n"
    foreach ($forbiddenPath in $forbiddenPaths) {
      if ($runtimeMetadataText.Contains($forbiddenPath, [System.StringComparison]::Ordinal)) {
        $matchingMetadata = $runtimeMetadata |
          Where-Object { $_.Contains($forbiddenPath, [System.StringComparison]::Ordinal) }
        throw "Native library '$candidatePath' retains build path '$forbiddenPath' in: $($matchingMetadata -join '; ')"
      }
    }

    if ($RequireWrapperRelativePath) {
      $expectedRuntimePath = if ($IsLinux) { '$ORIGIN' } else { '@loader_path' }
      if (-not $runtimeMetadataText.Contains($expectedRuntimePath, [System.StringComparison]::Ordinal)) {
        throw "Native wrapper '$candidatePath' does not use the relative runtime path '$expectedRuntimePath'."
      }
    }
  }
}

if (Test-Path -LiteralPath $buildDir) {
  Remove-Item -LiteralPath $buildDir -Recurse -Force
}
if (Test-Path -LiteralPath $outputDir) {
  Remove-Item -LiteralPath $outputDir -Recurse -Force
}
New-Item -ItemType Directory -Path $buildDir -Force | Out-Null

& cmake -S $repositoryDir -B $buildDir -G Ninja -DCMAKE_BUILD_TYPE=Release
if ($LASTEXITCODE -ne 0) { throw "CMake superbuild configure failed with exit code $LASTEXITCODE." }

& cmake --build $buildDir --config Release --parallel
if ($LASTEXITCODE -ne 0) { throw "CMake superbuild failed with exit code $LASTEXITCODE." }

$installPrefixPath = Join-Path $buildDir "wrapper-install-prefix.txt"
if (-not (Test-Path -LiteralPath $installPrefixPath)) {
  throw "The wrapper install-prefix marker is missing: '$installPrefixPath'."
}
$installPrefix = (Get-Content -Raw -LiteralPath $installPrefixPath).Trim()
$libraryPath = switch ($runtimeId) {
  "win-x64" { Join-Path $installPrefix "tesseract_csharp.dll" }
  "linux-x64" { Join-Path $installPrefix "libtesseract_csharp.so" }
  "linux-arm64" { Join-Path $installPrefix "libtesseract_csharp.so" }
  "osx-x64" { Join-Path $installPrefix "libtesseract_csharp.dylib" }
  "osx-arm64" { Join-Path $installPrefix "libtesseract_csharp.dylib" }
}
if (-not (Test-Path -LiteralPath $libraryPath)) {
  throw "The installed native wrapper was not found at '$libraryPath'."
}
if (-not $IsMacOS) {
  Set-PortableWrapperRuntimePath -libraryPath $libraryPath
  Assert-PortableRuntimePaths -libraryPaths @($libraryPath) -RequireWrapperRelativePath
}

$collectorArgs = @(
  "-DWRAPPER_LIBRARY=$libraryPath"
  "-DOUTPUT_DIRECTORY=$outputDir"
)
if ($IsWindows) {
  $linkerEntry = Get-Content -LiteralPath (Join-Path $buildDir "wrapper-build/CMakeCache.txt") |
    Where-Object { $_ -match '^CMAKE_LINKER:FILEPATH=' } |
    Select-Object -First 1
  if ($null -eq $linkerEntry) {
    throw "CMAKE_LINKER was not found in the wrapper build cache."
  }
  $linkerPath = $linkerEntry.Substring($linkerEntry.IndexOf('=') + 1)
  $dumpbinPath = Join-Path (Split-Path -Parent $linkerPath) "dumpbin.exe"
  if (-not (Test-Path -LiteralPath $dumpbinPath)) {
    throw "dumpbin.exe was not found next to the configured linker at '$dumpbinPath'."
  }
  $collectorArgs += "-DRUNTIME_DEPENDENCY_TOOL=$dumpbinPath"
}
$collectorArgs += @("-P", $runtimeCollectorPath)

& cmake @collectorArgs
if ($LASTEXITCODE -ne 0) { throw "Native runtime collection failed with exit code $LASTEXITCODE." }

$packagedLibraries = Get-ChildItem -LiteralPath $outputDir -File | Select-Object -ExpandProperty FullName
if ($IsMacOS) {
  $packagedWrapperPath = Join-Path $outputDir ([System.IO.Path]::GetFileName($libraryPath))
  Set-PortableWrapperRuntimePath -libraryPath $packagedWrapperPath
  Assert-PortableRuntimePaths -libraryPaths @($packagedWrapperPath) -RequireWrapperRelativePath
}
Assert-PortableRuntimePaths -libraryPaths $packagedLibraries

Write-Host "Built $runtimeId native runtime in $outputDir"
