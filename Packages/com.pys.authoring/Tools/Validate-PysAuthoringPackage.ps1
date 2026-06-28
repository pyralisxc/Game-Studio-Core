$ErrorActionPreference = "Stop"

$PackageRoot = Split-Path -Parent $PSScriptRoot

Write-Host "Validating PYS Authoring package at $PackageRoot"

$usingMatches = Select-String `
    -Path (Join-Path $PackageRoot "*.cs"), (Join-Path $PackageRoot "**/*.cs") `
    -Pattern "^\s*using\s+([^;]+);" `
    -ErrorAction SilentlyContinue

$allowedUsingPattern = "^(System(\.|$)|Unity(Editor|Engine)(\.|$)|NUnit(\.|$)|Pys\.Authoring(\.|$))"
$blockedUsings = @()
foreach ($match in $usingMatches) {
    $namespace = $match.Matches[0].Groups[1].Value.Trim()
    if ($namespace -notmatch $allowedUsingPattern) {
        $blockedUsings += [pscustomobject]@{
            Path = $match.Path
            LineNumber = $match.LineNumber
            Namespace = $namespace
            Line = $match.Line
        }
    }
}

if ($blockedUsings.Count -gt 0) {
    Write-Host "Package C# files import namespaces outside the generic authoring boundary:" -ForegroundColor Red
    $blockedUsings | ForEach-Object { Write-Host "$($_.Path):$($_.LineNumber): $($_.Namespace) :: $($_.Line)" }
    exit 1
}

$contractsAsmdef = Join-Path $PackageRoot "Runtime/Pys.Authoring.Contracts.asmdef"
$editorAsmdef = Join-Path $PackageRoot "Editor/Pys.Authoring.Editor.asmdef"

if (!(Test-Path $contractsAsmdef)) {
    throw "Missing contracts asmdef: $contractsAsmdef"
}

if (!(Test-Path $editorAsmdef)) {
    throw "Missing editor asmdef: $editorAsmdef"
}

$editorReferences = Get-Content -Raw $editorAsmdef
if ($editorReferences -notmatch "Pys.Authoring.Contracts") {
    throw "Editor asmdef must reference Pys.Authoring.Contracts."
}

Write-Host "PYS Authoring package validation passed." -ForegroundColor Green
