#addin "Cake.FileHelpers"
//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var version = Argument("build_version", "0.0.0");

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

Task("SetVersion")
	.Does(() => 
{
	/*
	var file = "./src/ToyStorage.SolutionInfo.cs";
	var version = "0.0.1";
	var buildNo = "123";
	var semVersion = string.Concat(version + "-" + buildNo);
	CreateAssemblyInfo(file, new AssemblyInfoSettings {
		Product = "SampleProject",
		Version = version,
		FileVersion = version,
		InformationalVersion = semVersion,
		Copyright = string.Format("Copyright (c) Contoso 2014 - {0}", DateTime.Now.Year)
	});
	*/

	ReplaceRegexInFiles("./src/ToyStorage/AssemblyInfo.cs", 
                    "(?<=AssemblyVersion\\(\")(.+?)(?=\"\\))", 
                    version);

	ReplaceRegexInFiles("./src/ToyStorage/AssemblyInfo.cs", 
                    "(?<=AssemblyFileVersion\\(\")(.+?)(?=\"\\))", 
                    version);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .IsDependentOn("SetVersion")
    .Does(() =>
{
	DotNetCoreRestore("./src/ToyStorage.sln");
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
	Information("Got verions...");
	Information(version);

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