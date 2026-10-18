$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe'
if (-not (Test-Path $csc)) { throw 'The .NET Framework C# compiler was not found.' }
$bin = Join-Path $PSScriptRoot 'bin'
New-Item -ItemType Directory -Force -Path $bin | Out-Null
$names = @('RoutingEngine.cs','RoadGeometry.cs','AutoDepotSearch.cs','OsrmDataProvider.cs','AutoSearchFiles.cs')
$sources = @($names | ForEach-Object { Join-Path (Join-Path $root 'DepoBul') $_ })
foreach ($test in @('EngineChecks','AutoSearchChecks')) {
    $exe = Join-Path $bin ($test + '.exe')
    $testSource = Join-Path $PSScriptRoot ($test + '.cs')
    & $csc /nologo /target:exe "/out:$exe" /r:System.Core.dll /r:System.Runtime.Serialization.dll @sources $testSource
    if ($LASTEXITCODE -ne 0) { throw "Compilation failed: $test" }
    & $exe
    if ($LASTEXITCODE -ne 0) { throw "Checks failed: $test" }
}
