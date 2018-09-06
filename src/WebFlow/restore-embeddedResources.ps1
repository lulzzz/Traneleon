<#
.SYNOPSIS
Restore npm packages.
#>

[CmdletBinding(SupportsShouldProcess)]
Param(
	[string]$Archive = "$PSScriptRoot\dependencies.zip",
	[string]$ProjectName = (Split-Path $PSScriptRoot -Leaf)
)

try
{
	Push-Location $PSScriptRoot;

	# Package lib resources.
    $lib = Join-Path $PWD ([System.IO.Path]::GetFileNameWithoutExtension($Archive));
	if ($PSCmdlet.ShouldProcess($lib, "Compress"))
	{
		if (Test-Path $Archive) { Remove-Item $Archive -Force; }
		Compress-Archive "$lib/*" -DestinationPath $Archive -CompressionLevel Fastest;
		Write-Host "$ProjectName -> $Archive";
	}

	# Restore node packages.
	if ($PSCmdlet.ShouldProcess($PSScriptRoot, "npm restore"))
	{
		$moduleDir = "$PSScriptRoot\node_modules";
		Write-Host "restoring node packages ...";
		if (Test-Path $moduleDir) { Remove-Item $moduleDir -Recurse -Force; }
		&npm install --save-dev | Out-Null;
		Get-ChildItem $moduleDir -Recurse -Include @("*.md", "LICENSE") | Remove-Item -Force;

		$Archive = Join-Path $PWD "$($env:OS)-node_modules.zip";
		if (Test-Path $Archive) { Remove-Item $Archive -Force; }
		Compress-Archive $moduleDir -DestinationPath $Archive -CompressionLevel Fastest;
		Write-Host "$ProjectName -> $Archive";
	}
}
finally
{
	[string]$npmLog = "$PWD\npm-debug.log";
	if (Test-Path $npmLog) { Remove-Item $npmLog; }
	Pop-Location;
}