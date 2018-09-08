Param(
	[string]$Configuration = "Release",
	[switch]$Clean,
	[switch]$NoBuild
)
Clear-Host;

[string]$proj = Resolve-Path "../../src/*.CLI/*.*proj";
if (-not $NoBuild.IsPresent) { &dotnet build $proj --configuration $Configuration; }

[string]$cwd = Resolve-Path "../WebFlow.Sample";
[string]$config = Resolve-Path "../*.Sample/webflow.config";
[string]$cli = Resolve-Path "../../src/*.CLI/bin/$Configuration/*/*.CLI.dll";

if ($Clean)
{
	Get-ChildItem "$cwd\wwwroot" -Recurse -Filter "*.js*"  | Remove-Item -Force;
	Get-ChildItem "$cwd\wwwroot" -Recurse -Filter "*.css*" | Remove-Item -Force;
}

&dotnet $cli build $config -w;