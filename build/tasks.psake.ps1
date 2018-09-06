<#
#>

Task "Publish-Packages" -alias "push" -description "This task executes a full deployment of the application." `
-depends @("version", "msbuild", "test", "pack", "push-nuget", "push-vsix");

# -----

Task "MSBuild-Solution" -alias "msbuild" -description "" `
-depends @("restore") -action {
	$msbuild = Get-MSBuild;
	[string]$sln = Resolve-Path "$RootDir/*.sln";
	Exec { &$msbuild $sln /p:Configuration=$Configuration /verbosity:minimal; }
}

Task "Package-Solution" -alias "pack" -description "This task generates all delployment packages." `
-depends @("restore") -action {
    $version = Get-NcrementManifest $ManifestJson | Convert-NcrementVersionNumberToString $Branch -AppendSuffix;
	if (Test-Path $ArtifactsDir) { Remove-Item $ArtifactsDir -Recurse -Force; }
	New-Item $ArtifactsDir -ItemType Directory | Out-Null;

	# Powershell Gallery
	$proj = Get-Item "$RootDir\src\*.CLI\*.csproj";
	Write-Header "dotnet: publish '$($proj.Basename)' (FDD)";
	Exec { &dotnet publish $proj.FullName --configuration $Configuration; }
	$psd1 = Get-Item (Join-Path $proj.DirectoryName "bin\$Configuration\*\NShellit\*\*\*.psd1");
	Copy-Item $psd1.DirectoryName -Destination $ArtifactsDir -Recurse;

	# Nuget
	$proj = Get-Item "$RootDir\src\$SolutionName\*.csproj";
	Write-Header "dotnet: pack '$($proj.BaseName)'";
	Exec { &dotnet pack $proj.FullName --output $ArtifactsDir --configuration $Configuration /p:PackageVersion=$version; }

	# VSIX
	$msbuild = Get-MSBuild;
	$proj = Get-Item "$RootDir/src/*.VSIX/*.csproj";
	Write-Header "msbuild: '$($proj.BaseName)'";
	Exec { &$msbuild /p:"Configuration=$Configuration;Platform=AnyCPU" /verbosity:minimal $proj.FullName; }

	$vsix = Get-Item "$RootDir/src/*.VSIX/bin/$Configuration/*.vsix";
	Copy-Item $vsix -Destination $ArtifactsDir -Force;

	# Un-packing .nupkg for testing.
	[string]$nupkg = Resolve-Path "$ArtifactsDir\*.nupkg";
	$zip = [IO.Path]::ChangeExtension($nupkg, ".zip");
	Copy-Item $nupkg -Destination $zip;
	Expand-Archive $zip -DestinationPath "$ArtifactsDir\msbuild";
	if (Test-Path $zip) { Remove-Item $zip; }
}

Task "Stage-VSIXPackage" -alias "push-vsix" -description "This task publish all .vsix packages to vsixgallery.com." `
-depends @("restore") -action {
	$script = Join-Path $ToolsDir "vsix/1.0/vsix.ps1";

	if (-not (Test-Path $script))
	{
		$dir  = Split-Path $script -Parent;
		if (-not (Test-Path $dir -PathType Container)) { New-Item $dir -ItemType Directory | Out-Null; }
		Invoke-WebRequest "https://raw.github.com/madskristensen/ExtensionScripts/master/AppVeyor/vsix.ps1" -OutFile $script;
	}
	Import-Module $script -Force;

	foreach ($package in (Get-ChildItem $ArtifactsDir -Filter "*.vsix"))
	{
		Write-Header "vsix: publish '$($package.Basename)'";
		Vsix-PublishToGallery $package.FullName;

		if (-not $NonInteractive)
		{
			Start-Process "http://vsixgallery.com/";
		}
	}
}