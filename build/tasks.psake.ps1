#

Task "Package-Solution" -alias "pack" -description "This task generates all delployment packages." `
-depends @("restore") -action {
    $version = Get-NcrementManifest $ManifestJson | Convert-NcrementVersionNumberToString $Branch -AppendSuffix;
	if (Test-Path $ArtifactsDir) { Remove-Item $ArtifactsDir -Recurse -Force; }
	New-Item $ArtifactsDir -ItemType Directory | Out-Null;
	
	$proj = Get-Item "$RootDir\src\*\*.csproj";
	Write-Header "dotnet: pack '$($proj.BaseName)'";
	Exec { &dotnet pack $proj.FullName --output $ArtifactsDir --configuration $Configuration /p:PackageVersion=$version; }
}