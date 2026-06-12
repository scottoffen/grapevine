#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Runs all tests across the repository's solution(s) and produces a single,
    combined code coverage report.

.DESCRIPTION
    This script lives at the repository root. It autodiscovers every solution
    file under src/ (the repo convention is that all .sln and .slnx files live there),
    builds and tests each one, then merges the coverage output from every test
    project into one HTML + text report.

    It is intended for local use: by default it opens the HTML report in your
    browser when finished. Pass -NoOpen to skip that step, which also makes the
    script safe to run in CI (it will not try to launch a browser on a build
    agent).

    Source and updates: https://github.com/scottoffen/util-powershell-test-coverage

.PARAMETER Solution
    Optional. One or more solution paths to use instead of autodiscovery.
    Leave this empty (the default) to let the script find src/*.sln and src/*.slnx itself.

.PARAMETER NoOpen
    Suppresses opening the report in a browser when the run finishes.
    Use this in CI, or any time you just want the files on disk.

.EXAMPLE
    ./test-coverage.ps1
    Autodiscovers src/*.sln and src/*.slnx, runs everything, and opens the report locally.

.EXAMPLE
    ./test-coverage.ps1 -NoOpen
    Same as above but does not open a browser (suitable for CI).

.LINK
    https://github.com/scottoffen/util-powershell-test-coverage
#>

param(
    # Optional explicit solution list. When empty, the script discovers src/*.sln and src/*.slnx.
    [string[]]$Solution,

    # When set, do not open the HTML report at the end.
    [switch]$NoOpen
)

# Stop on PowerShell cmdlet errors (for example a failed Get-ChildItem or
# Remove-Item). NOTE: this does NOT catch failures from native commands like
# dotnet, so we check $LASTEXITCODE ourselves after each dotnet call.
$ErrorActionPreference = "Stop"

# Helper: throw if the most recent native command returned a non-zero exit code.
# We use this for the build, restore, and report steps, where any failure should
# abort the run. We deliberately do NOT use it after 'dotnet test', because
# failing tests are an expected outcome that should still produce a report.
function Assert-LastExitCode {
    param([string]$What)
    if ($LASTEXITCODE -ne 0) {
        throw "$What failed with exit code $LASTEXITCODE"
    }
}

# Anchor everything to the repository root (this script's own folder) so the
# script behaves identically no matter what directory you invoke it from.
# Push-Location plus the finally block at the very bottom restores your original
# location when the script exits, even on error.
Push-Location $PSScriptRoot
try {
    # --- Discover the solution(s) -------------------------------------------
    # Convention: all solution files live under src/. If the caller passed an
    # explicit -Solution list, honor it; otherwise find every src/*.sln and src/*.slnx.
    if ($Solution) {
        $solutionFiles = $Solution | ForEach-Object { Get-Item $_ }
    }
    else {
        $solutionFiles = Get-ChildItem -Path (Join-Path $PSScriptRoot "src") -Include "*.sln","*.slnx" -Recurse
    }

    if (@($solutionFiles).Count -eq 0) {
        throw "No solution files found under src/. Nothing to build or test."
    }

    Write-Host "Found $(@($solutionFiles).Count) solution(s):"
    $solutionFiles | ForEach-Object { Write-Host "  $($_.Name)" }

    # --- Prepare folders -----------------------------------------------------
    # Put test results in a known folder so we never accidentally pick up
    # coverage files from an older run. Wipe it first.
    $resultsDir = Join-Path $PSScriptRoot "TestResults"
    if (Test-Path $resultsDir) { Remove-Item $resultsDir -Recurse -Force }

    # The coverlet run settings file, shared by all solutions and anchored to the
    # repo root. If it is missing we warn and carry on without it rather than
    # failing the whole run.
    $runSettings = Join-Path $PSScriptRoot "src/coverlet.runsettings"
    $hasRunSettings = Test-Path $runSettings
    if (-not $hasRunSettings) {
        Write-Warning "Run settings not found at $runSettings. Continuing without --settings."
    }

    # --- Build every solution ------------------------------------------------
    # Build all solutions first, then test with --no-build, so the test pass
    # never triggers a rebuild.
    foreach ($sln in $solutionFiles) {
        Write-Host "`nBuilding $($sln.Name)..."
        dotnet build $sln.FullName
        Assert-LastExitCode "dotnet build ($($sln.Name))"
    }

    # --- Test every solution -------------------------------------------------
    # Capture the test exit code instead of throwing on it. Failing tests are a
    # normal result and we still want the coverage report. We remember the last
    # non-zero code so the script can exit non-zero at the very end (which is
    # what makes the CI case behave correctly).
    $testExitCode = 0
    foreach ($sln in $solutionFiles) {
        Write-Host "`nTesting $($sln.Name)..."

        # Build the argument list so we can conditionally include --settings.
        # Splatting (@testArgs) passes each array element as one argument, so the
        # space inside "XPlat Code Coverage" is preserved without extra quoting.
        $testArgs = @(
            "test", $sln.FullName,
            "--no-build",
            "--collect:XPlat Code Coverage",
            "--results-directory", $resultsDir
        )
        if ($hasRunSettings) {
            $testArgs += @("--settings", $runSettings)
        }

        dotnet @testArgs
        if ($LASTEXITCODE -ne 0) { $testExitCode = $LASTEXITCODE }
    }

    # --- Collect coverage ----------------------------------------------------
    # Every test project drops a coverage.cobertura.xml under a GUID-named
    # subfolder of $resultsDir. Grab them all, from every solution, so the report
    # covers the entire repository in one pass.
    $coverageFiles = Get-ChildItem -Path $resultsDir -Recurse -Filter "coverage.cobertura.xml"
    if (@($coverageFiles).Count -eq 0) {
        throw "No coverage files found under $resultsDir. The test run likely failed to start."
    }

    Write-Host "`nFound $(@($coverageFiles).Count) coverage file(s)."

    # ReportGenerator takes a semicolon-separated list of report paths.
    $reportsArg = ($coverageFiles | ForEach-Object { $_.FullName }) -join ';'

    # --- Generate the report -------------------------------------------------
    # Clean the output folder first so classes removed since the last run do not
    # linger as stale HTML pages.
    $coverageDir = Join-Path $PSScriptRoot "coverage"
    if (Test-Path $coverageDir) { Remove-Item $coverageDir -Recurse -Force }

    # The arguments are the same however ReportGenerator is invoked. targetdir is
    # an absolute path so it does not depend on the current working directory.
    $reportArgs = @(
        "-reports:$reportsArg",
        "-targetdir:$coverageDir",
        "-reporttypes:HtmlInline_AzurePipelines;TextSummary"
    )

    # ReportGenerator can come from one of two places. Prefer the local tool
    # manifest when it is present (pinned and reproducible, no global install
    # required); otherwise fall back to a globally installed tool.
    $toolManifest = Join-Path $PSScriptRoot "src/.config/dotnet-tools.json"
    if (Test-Path $toolManifest) {
        dotnet tool restore --tool-manifest $toolManifest
        Assert-LastExitCode "dotnet tool restore"

        # 'dotnet tool run' finds the manifest by walking up from the current
        # directory, so run it from ./src where src/.config/dotnet-tools.json lives.
        Push-Location ./src
        try {
            dotnet tool run reportgenerator @reportArgs
            Assert-LastExitCode "reportgenerator"
        }
        finally {
            Pop-Location
        }
    }
    elseif (Get-Command reportgenerator -ErrorAction SilentlyContinue) {
        # No manifest: use the globally installed ReportGenerator from PATH.
        reportgenerator @reportArgs
        Assert-LastExitCode "reportgenerator"
    }
    else {
        throw "ReportGenerator not found. Either keep src/.config/dotnet-tools.json in the repository, or install the tool globally with: dotnet tool install --global dotnet-reportgenerator-globaltool"
    }

    # --- Open the report (local convenience) ---------------------------------
    # Skipped when -NoOpen is set, which is what makes this safe for CI.
    $indexPath = Join-Path $coverageDir "index.html"
    if (-not $NoOpen) {
        if ($IsWindows) {
            Start-Process $indexPath
        } elseif ($IsMacOS) {
            open $indexPath
        } elseif ($IsLinux) {
            xdg-open $indexPath
        } else {
            Write-Warning "Platform not detected. Open coverage/index.html manually."
        }
    }

    # --- Surface the test result last ----------------------------------------
    # You always get the report above. But if any tests failed, exit non-zero so
    # the failure is not silently swallowed (and so CI would go red).
    if ($testExitCode -ne 0) {
        Write-Warning "Some tests failed (dotnet test exit code $testExitCode). Coverage report was still generated."
        exit $testExitCode
    }
}
finally {
    # Always restore the caller's original directory.
    Pop-Location
}