<#
#>

Task "Publish-Packages" -alias "push" -description "This task executes a full deployment of the application." `
-depends @("push-nuget");

# -----

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
	$vsix = Get-Item "$RootDir/src/*.VSIX/bin/$Configuration/*.vsix";
	Copy-Item $vsix -Destination $ArtifactsDir -Force;

	# Un-packing .nupkg for testing.
	[string]$nupkg = Resolve-Path "$ArtifactsDir\*.nupkg";
	$zip = [IO.Path]::ChangeExtension($nupkg, ".zip");
	Copy-Item $nupkg -Destination $zip;
	Expand-Archive $zip -DestinationPath "$ArtifactsDir\msbuild";
	if (Test-Path $zip) { Remove-Item $zip; }
}