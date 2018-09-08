Param(
	[Alias('t')]
	[int]$Timeout = 10000
)

try
{
	Push-Location $PSScriptRoot;

	Clear-Host;
	$resultsDir = Join-Path $env:TEMP "webflow";
	if (Test-Path $resultsDir) { Remove-Item $resultsDir -Recurse -Force; }

	[string]$mocha = Resolve-Path "$PSScriptRoot\node_modules\mocha\bin\mocha";
	&node $mocha --timeout $Timeout;
}
finally { Pop-Location; }