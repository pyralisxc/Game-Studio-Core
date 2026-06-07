param(
    [string]$ProjectPath = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path,
    [string]$UnityExe = "C:\Program Files\Unity\Hub\Editor\6000.4.0f1\Editor\Unity.exe",
    [int]$TimeoutMinutes = 15,
    [switch]$SkipUnity,
    [switch]$SkipFinalBuild,
    [switch]$SkipResidueScan,
    [switch]$SkipPackagePortability
)

$ErrorActionPreference = "Stop"

function Write-Step {
    param([string]$Message)
    Write-Host ""
    Write-Host "== $Message ==" -ForegroundColor Cyan
}

function Invoke-Checked {
    param(
        [string]$FilePath,
        [string[]]$Arguments,
        [string]$FailureMessage
    )

    & $FilePath @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "$FailureMessage Exit code: $LASTEXITCODE"
    }
}

function Assert-NoUnityProcess {
    $unityProcesses = Get-Process Unity -ErrorAction SilentlyContinue
    if ($unityProcesses) {
        $ids = ($unityProcesses | Select-Object -ExpandProperty Id) -join ", "
        throw "Unity is already running (process id(s): $ids). Close the Editor before running pre-scene validation so batchmode owns the project lock."
    }
}

function Get-UnityResultSummary {
    param([string]$ResultsPath)

    [xml]$xml = Get-Content $ResultsPath
    $run = $xml."test-run"
    $failures = @()
    foreach ($testCase in $xml.SelectNodes('//test-case[@result="Failed"]')) {
        $message = $testCase.failure.message.'#cdata-section'
        if ([string]::IsNullOrWhiteSpace($message)) {
            $message = $testCase.failure.message
        }

        $failures += [pscustomobject]@{
            Name = $testCase.fullname
            Message = ($message -replace "(`r`n|`n|`r)", " ").Trim()
        }
    }

    [pscustomobject]@{
        Result = [string]$run.result
        Total = [int]$run.total
        Passed = [int]$run.passed
        Failed = [int]$run.failed
        Skipped = [int]$run.skipped
        Failures = $failures
    }
}

function Invoke-UnityTestRun {
    param(
        [ValidateSet("EditMode", "PlayMode")]
        [string]$Platform,
        [string]$ProjectPath,
        [string]$UnityExe,
        [string]$LogDirectory,
        [int]$TimeoutMinutes
    )

    Assert-NoUnityProcess

    if (!(Test-Path -LiteralPath $UnityExe)) {
        throw "Unity executable was not found: $UnityExe"
    }

    $stamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $logPath = Join-Path $LogDirectory "pre-scene-$($Platform.ToLowerInvariant())-$stamp.log"
    $resultsPath = Join-Path $LogDirectory "pre-scene-$($Platform.ToLowerInvariant())-$stamp-results.xml"

    Write-Step "Unity $Platform"
    Write-Host "Log: $logPath"
    Write-Host "Results: $resultsPath"
    Write-Host "Running without -quit. Unity Test Framework 1.6 skips command-line tests when -quit is supplied."

    & $UnityExe -batchmode -nographics -projectPath $ProjectPath -runTests -testPlatform $Platform -testResults $resultsPath -logFile $logPath | Out-Host

    $deadline = (Get-Date).AddMinutes($TimeoutMinutes)
    do {
        Start-Sleep -Seconds 5
        $unityRunning = [bool](Get-Process Unity -ErrorAction SilentlyContinue)
        $hasResults = Test-Path -LiteralPath $resultsPath

        if ($hasResults -and !$unityRunning) {
            $summary = Get-UnityResultSummary -ResultsPath $resultsPath
            Write-Host "$Platform result: $($summary.Result) total=$($summary.Total) passed=$($summary.Passed) failed=$($summary.Failed) skipped=$($summary.Skipped)"

            if ($summary.Failed -gt 0 -or $summary.Result -notlike "Passed*") {
                foreach ($failure in $summary.Failures) {
                    Write-Host "FAILED: $($failure.Name) :: $($failure.Message)" -ForegroundColor Red
                }

                throw "$Platform Unity tests failed. See $resultsPath"
            }

            return [pscustomobject]@{
                Platform = $Platform
                LogPath = $logPath
                ResultsPath = $resultsPath
                Summary = $summary
            }
        }

        if (!$unityRunning -and !$hasResults) {
            if (Test-Path -LiteralPath $logPath) {
                Write-Host "Unity exited without writing results. Last relevant log lines:"
                Select-String -Path $logPath -Pattern "Compiler errors|Compilation failed|error CS|Exception|Test run|Exiting|return code" -CaseSensitive:$false |
                    Select-Object -Last 40 |
                    ForEach-Object { Write-Host $_.Line }
            }

            throw "$Platform Unity run exited without a results XML. See $logPath"
        }
    } while ((Get-Date) -lt $deadline)

    $stillRunning = Get-Process Unity -ErrorAction SilentlyContinue
    if ($stillRunning) {
        throw "$Platform Unity run timed out after $TimeoutMinutes minute(s). Unity is still running; inspect it before retrying."
    }

    throw "$Platform Unity run timed out after $TimeoutMinutes minute(s) without results."
}

function Suspend-UnityVersionControlLayout {
    param([string]$ProjectPath)

    $layoutPath = Join-Path $ProjectPath "UserSettings\Layouts\CurrentMaximizeLayout.dwlt"
    if (!(Test-Path -LiteralPath $layoutPath)) {
        return $null
    }

    $layoutText = Get-Content -LiteralPath $layoutPath -Raw
    if ($layoutText -notmatch "Unity\.PlasticSCM\.Editor\.UVCSWindow") {
        return $null
    }

    $stamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $backupPath = Join-Path $ProjectPath "Logs\Codex\layout-backups\CurrentMaximizeLayout-$stamp.dwlt"
    $defaultLayoutPath = Join-Path $ProjectPath "UserSettings\Layouts\default-6000.dwlt"
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $backupPath) | Out-Null
    Copy-Item -LiteralPath $layoutPath -Destination $backupPath -Force

    if (Test-Path -LiteralPath $defaultLayoutPath) {
        Copy-Item -LiteralPath $defaultLayoutPath -Destination $layoutPath -Force
    }
    else {
        Remove-Item -LiteralPath $layoutPath -Force
    }

    Write-Host "Temporarily suspended Unity Version Control layout for batchmode validation."
    Write-Host "Backup: $backupPath"

    [pscustomobject]@{
        LayoutPath = $layoutPath
        BackupPath = $backupPath
    }
}

function Restore-UnityVersionControlLayout {
    param($Guard)

    if ($null -eq $Guard) {
        return
    }

    if (Test-Path -LiteralPath $Guard.BackupPath) {
        Copy-Item -LiteralPath $Guard.BackupPath -Destination $Guard.LayoutPath -Force
        Write-Host "Restored Unity Version Control layout after validation."
    }
}

function Assert-NoValidationResidue {
    param([string]$ProjectPath)

    Write-Step "residue scan"

    $residue = @()
    $assetTemp = Join-Path $ProjectPath "Assets\Temp"
    if (Test-Path -LiteralPath $assetTemp) {
        $residue += $assetTemp
    }

    $generatedScenes = Get-ChildItem -LiteralPath (Join-Path $ProjectPath "Assets") -Filter "InitTestScene*.unity" -File -ErrorAction SilentlyContinue
    foreach ($scene in $generatedScenes) {
        $residue += $scene.FullName
    }

    $sampleRoots = @(
        "Assets\GameplayExamplePack",
        "Assets\GeneratedSamples",
        "Assets\SampleGenerated",
        "Assets\SamplesGenerated"
    )

    foreach ($relativePath in $sampleRoots) {
        $path = Join-Path $ProjectPath $relativePath
        if (Test-Path -LiteralPath $path) {
            $residue += $path
        }
    }

    $packageRoot = Join-Path $ProjectPath "Packages\com.neonblackinteractivellc.neonblackhub\Members\Pyralis\Gameplay"
    if (Test-Path -LiteralPath $packageRoot) {
        $emptyPackageFolders = Get-ChildItem -LiteralPath $packageRoot -Directory -Recurse -ErrorAction SilentlyContinue |
            Where-Object {
                $_.FullName -notmatch "\\(Docs|Samples|Tests)(\\|$)" -and
                -not (Get-ChildItem -LiteralPath $_.FullName -Force -ErrorAction SilentlyContinue | Where-Object { $_.Name -ne ".meta" })
            }

        foreach ($folder in $emptyPackageFolders) {
            $residue += $folder.FullName
        }
    }

    if ($residue.Count -gt 0) {
        Write-Host "Validation residue found:" -ForegroundColor Red
        foreach ($path in $residue) {
            Write-Host "  $path" -ForegroundColor Red
        }

        throw "Validation residue scan failed. Archive or remove generated/test leftovers before declaring scene or prefab readiness."
    }

    Write-Host "No validation residue found."
}

function Assert-PackagePortability {
    param([string]$ProjectPath)

    Write-Step "package portability"

    $packageId = "com.neonblackinteractivellc.neonblackhub"
    $expectedVersion = "0.1.2"
    $packageRoot = Join-Path $ProjectPath "Packages\$packageId"
    $packageJsonPath = Join-Path $packageRoot "package.json"
    $manifestPath = Join-Path $ProjectPath "Packages\manifest.json"
    $lockPath = Join-Path $ProjectPath "Packages\packages-lock.json"
    $runtimeMarkerPath = Join-Path $packageRoot "Runtime\RuntimeExample.cs"
    $editorMarkerPath = Join-Path $packageRoot "Editor\EditorExample.cs"
    $legacyRuntimeMembersPath = Join-Path $packageRoot "Runtime\Members"

    if (!(Test-Path -LiteralPath $packageJsonPath)) {
        throw "Embedded package metadata is missing: $packageJsonPath"
    }

    $packageJson = Get-Content -LiteralPath $packageJsonPath -Raw | ConvertFrom-Json
    if ($packageJson.name -ne $packageId) {
        throw "Embedded package name mismatch. Expected '$packageId' but found '$($packageJson.name)'."
    }

    if ($packageJson.version -ne $expectedVersion) {
        throw "Embedded package version mismatch. Expected '$expectedVersion' but found '$($packageJson.version)'."
    }

    if (Test-Path -LiteralPath $legacyRuntimeMembersPath) {
        throw "Stale legacy package content found at $legacyRuntimeMembersPath. Current package source must live under Members\Pyralis\Gameplay."
    }

    if (!(Test-Path -LiteralPath $runtimeMarkerPath) -or !(Test-Path -LiteralPath $editorMarkerPath)) {
        throw "Package marker scripts are missing. Expected Runtime\RuntimeExample.cs and Editor\EditorExample.cs."
    }

    $runtimeMarker = Get-Content -LiteralPath $runtimeMarkerPath -Raw
    $editorMarker = Get-Content -LiteralPath $editorMarkerPath -Raw
    if ($runtimeMarker -notmatch 'Version\s*=\s*"0\.1\.2"' -or $editorMarker -notmatch 'Version\s*=\s*"0\.1\.2"') {
        throw "Package marker scripts are not stamped with version $expectedVersion."
    }

    if (Test-Path -LiteralPath $manifestPath) {
        $manifestText = Get-Content -LiteralPath $manifestPath -Raw
        if ($manifestText -match "com\.studiotools\.core") {
            throw "Packages\manifest.json still references com.studiotools.core. Remove the old local package dependency before handoff."
        }
    }

    if (Test-Path -LiteralPath $lockPath) {
        $lockText = Get-Content -LiteralPath $lockPath -Raw
        if (-not ($lockText -match '"com\.neonblackinteractivellc\.neonblackhub"\s*:\s*\{[\s\S]*?"source"\s*:\s*"embedded"')) {
            throw "Packages\packages-lock.json does not resolve $packageId as an embedded package. Reopen Unity or remove/re-add the embedded package."
        }
    }

    Write-Host "$packageId is embedded and stamped $expectedVersion."
}

$ProjectPath = (Resolve-Path $ProjectPath).Path
$solutionPath = Join-Path $ProjectPath "Game Studio Core.slnx"
$logDirectory = Join-Path $ProjectPath "Logs\Codex"
New-Item -ItemType Directory -Force -Path $logDirectory | Out-Null

Write-Host "Pre-scene validation project: $ProjectPath"

if (!$SkipPackagePortability) {
    Assert-PackagePortability -ProjectPath $ProjectPath
}

Write-Step "dotnet restore"
Invoke-Checked -FilePath "dotnet" -Arguments @("restore", $solutionPath) -FailureMessage "dotnet restore failed."

Write-Step "dotnet build"
Invoke-Checked -FilePath "dotnet" -Arguments @("build", $solutionPath, "--no-restore") -FailureMessage "dotnet build failed."

$unityResults = @()
$layoutGuard = $null
try {
    if (!$SkipUnity) {
        $layoutGuard = Suspend-UnityVersionControlLayout -ProjectPath $ProjectPath
        $unityResults += Invoke-UnityTestRun -Platform "EditMode" -ProjectPath $ProjectPath -UnityExe $UnityExe -LogDirectory $logDirectory -TimeoutMinutes $TimeoutMinutes
        $unityResults += Invoke-UnityTestRun -Platform "PlayMode" -ProjectPath $ProjectPath -UnityExe $UnityExe -LogDirectory $logDirectory -TimeoutMinutes $TimeoutMinutes
    }
}
finally {
    Restore-UnityVersionControlLayout -Guard $layoutGuard
}

if (!$SkipFinalBuild) {
    Write-Step "final dotnet restore"
    Invoke-Checked -FilePath "dotnet" -Arguments @("restore", $solutionPath) -FailureMessage "final dotnet restore failed."

    Write-Step "final dotnet build"
    Invoke-Checked -FilePath "dotnet" -Arguments @("build", $solutionPath, "--no-restore") -FailureMessage "final dotnet build failed."
}

if (!$SkipResidueScan) {
    Assert-NoValidationResidue -ProjectPath $ProjectPath
}

Write-Step "summary"
Write-Host "Pre-scene validation passed."
foreach ($result in $unityResults) {
    Write-Host "$($result.Platform): $($result.Summary.Passed)/$($result.Summary.Total) passed"
    Write-Host "  XML: $($result.ResultsPath)"
    Write-Host "  Log: $($result.LogPath)"
}
