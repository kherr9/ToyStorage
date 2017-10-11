//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var version = new Version(Argument("build_version", "0.0.0.0"));

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

Task("Run-Unit-Tests")
    .Does(() =>
{
	DotNetCoreTest("./src/ToyStorage.UnitTests", new DotNetCoreTestSettings
    {
        Configuration = configuration
    });
});

Task("Pack")
	.Does(() => 
{
	Information($"Packing version {version}");

	// beta
	DotNetCorePack("./src/ToyStorage", new DotNetCorePackSettings
	{
		Configuration = configuration,
		OutputDirectory = artifactDir,
		NoBuild = true,
		VersionSuffix = $"ci-{version.Revision}",
		ArgumentCustomization  = args => args.Append($"/p:VersionPrefix={version.ToString(3)}")
	});

	// release
	//DotNetCorePack("./src/ToyStorage", new DotNetCorePackSettings
	//{
	//	Configuration = configuration,
	//	OutputDirectory = artifactDir,
	//	NoBuild = true,
	//	ArgumentCustomization  = args => args.Append($"/p:VersionPrefix={version.ToString(3)}")
	//});
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
	.IsDependentOn("Build")
    ////.IsDependentOn("Run-Unit-Tests")
	.IsDependentOn("Pack");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);