Param(
	[int]$Timeout = 10000
)

try
{
	Push-Location $PSScriptRoot;

	Clear-Host;
	[string]$mocha = Resolve-Path "$PSScriptRoot\node_modules\mocha\bin\mocha";
	&node $mocha --timeout $Timeout;
}
finally { Pop-Location; }