<#
.SYNOPSIS
Restore npm packages.
#>

Param(
	[string]$Archive = "$PSScriptRoot\dependencies.zip",
	[string]$ProjectName,
	[switch]$Force
)

try
{
	Push-Location $PSScriptRoot;

	$nodeDir = "$PWD\node_modules";
	[bool]$shouldUpdate = $Force.IsPresent;

	# Restoring node modules.
	$dependencies = Get-Content "$PWD\package.json" | Out-String | ConvertFrom-Json  | Select-Object -ExpandProperty "devDependencies";
	foreach ($prop in $dependencies.PSObject.Properties)
	{
		if (-not (Test-Path "$nodeDir\$($prop.Name)"))
		{
			$shouldUpdate = $true;
			Write-Host "importing $module module ...";
			&npm install --save-dev "$($prop.Name)@`"$($prop.Value)`"";
		}
	}

	# Create resource package.
	$lib = "$PWD\dependencies\*";
	if ($shouldUpdate -or (-not (Test-Path $Archive)) -or ((Get-Item $Archive | Select-Object -ExpandProperty Length) -eq 0))
	{
		if (Test-Path $archive) { Remove-Item $Archive -Force; }
		Compress-Archive $nodeDir -DestinationPath $Archive -CompressionLevel Fastest;
		Compress-Archive $lib -DestinationPath $Archive -CompressionLevel Fastest -Update;
		Write-Host "$ProjectName -> $Archive";
	}
}
finally { Pop-Location; }