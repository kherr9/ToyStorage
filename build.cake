//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var buildDir = Directory("./src/ToyStorage/bin") + Directory(configuration);

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(buildDir);
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
	DotNetCoreBuild("./src/ToyStorage.sln", new DotNetCoreBuildSettings
    {
        Configuration = configuration
    });
});

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
	DotNetCoreTest("./src/ToyStorage.UnitTests", new DotNetCoreTestSettings
    {
        Configuration = configuration
    });
});

Task("Pack")
	////.IsDependentOn("Build")
	.Does(() => 
{
	DotNetCorePack("./src/ToyStorage", new DotNetCorePackSettings
	{
		Configuration = configuration,
		OutputDirectory = "./artifacts/"
	});
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
	.IsDependentOn("Build")
    .IsDependentOn("Run-Unit-Tests")
	.IsDependentOn("Pack");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);