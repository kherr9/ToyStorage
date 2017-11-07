//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var version = new Version(Argument("build_version", "0.0.0.0"));
var versionSuffix = Argument("version_suffix", "beta");

Information("Arguments:");
Information($"target:={target}");
Information($"configuration:={configuration}");
Information($"build_version:={version}");
Information($"version_suffix:={versionSuffix}");
Information("\n");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var buildDir = Directory("./src/ToyStorage/bin") + Directory(configuration);
var artifactDir = Directory("./artifacts");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(buildDir);
    CleanDirectory(artifactDir);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
	DotNetCoreRestore("./src/ToyStorage.sln");
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
	Information($"Building version {version}");

	DotNetCoreBuild("./src/ToyStorage.sln", new DotNetCoreBuildSettings
    {
        Configuration = configuration,
		ArgumentCustomization  = args => args.Append($"/p:VersionPrefix={version.ToString(3)}")
    });
});

Task("Run-Integration-Tests")
    .Does(() =>
{
	DotNetCoreTest("./src/ToyStorage.IntegrationTests", new DotNetCoreTestSettings
    {
        Configuration = configuration
    });
});

Task("Pack")
	.Does(() => 
{
	Information($"Packing version {version}");

	var settings = new DotNetCorePackSettings
	{
		Configuration = configuration,
		OutputDirectory = artifactDir,
		NoBuild = true,
		ArgumentCustomization  = args => args.Append($"/p:VersionPrefix={version.ToString(3)}")
	};

	if(!string.IsNullOrEmpty(versionSuffix))
	{
		settings.VersionSuffix = $"{versionSuffix}-{version.Revision}";
		Information($"VersionSuffix: {settings.VersionSuffix}");
	}

	DotNetCorePack("./src/ToyStorage", settings);
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
	.IsDependentOn("Build")
    .IsDependentOn("Run-Integration-Tests")
	.IsDependentOn("Pack");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);