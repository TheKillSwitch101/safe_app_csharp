#tool "nuget:?package=OpenCover"
#addin Cake.Coveralls

// --------------------------------------------------------------------------------
// Desktop Build and Test
// --------------------------------------------------------------------------------

var coreTestProject = File("SafeApp.Tests.Core/SafeApp.Tests.Core.csproj");
var coreTestBin = Directory("SafeApp.Tests.Core/bin/Release");
var codeCoverageFilePath = "SafeApp.Tests.Core/TestResults/CodeCoverResult.xml";
var Desktop_TESTS_RESULT_PATH = "SafeApp.Tests.Core/TestResults/DesktopTestResult.xml";
coveralls_token = EnvironmentVariable("coveralls_access_token");

Task("Build-Desktop-Project")
  .IsDependentOn("Restore-NuGet")
  .Does(() => {
    var dotnetBuildArgument = string.Empty;
    var osFamily = (int)Context.Environment.Platform.Family;
    if(osFamily == 1)
      dotnetBuildArgument = @"--runtime win10-x64";
    else if(osFamily == 2)
      dotnetBuildArgument = @"--runtime linux-x64";
    else if (osFamily == 3)
      dotnetBuildArgument = @"--runtime win-x64";
    
    DotNetCoreBuild(coreTestProject,
    new DotNetCoreBuildSettings() {
      Configuration = configuration,
      ArgumentCustomization = args => args.Append(dotnetBuildArgument)
      });
  });

Task("Run-Desktop-Tests")
  .IsDependentOn("Build-Desktop-Project")
  .Does(() => {
    OpenCover(tool => {
        tool.DotNetCoreTest(
            coreTestProject.Path.FullPath,
            new DotNetCoreTestSettings()
            {
              Configuration = configuration,
              ArgumentCustomization = args => args.Append("--logger \"trx;LogFileName=DesktopTestResult.xml\"")
            });
        },
        new FilePath(codeCoverageFilePath),
        new OpenCoverSettings()
        { 
            SkipAutoProps = true,
            Register = "user",
            OldStyle = true
        }
        .WithFilter("+[*]*")
        .WithFilter("-[SafeApp.Tests*]*")
        .WithFilter("-[NUnit3.*]*"));
  });

Task("Upload-Coverage-Report")
  .IsDependentOn("Run-Desktop-Tests")
	.Does(() =>
	{
		CoverallsIo(codeCoverageFilePath, new CoverallsIoSettings()
		{
			RepoToken = coveralls_token
		});
	})
  .Finally(() =>
  {
    if(AppVeyor.IsRunningOnAppVeyor)
    {
      var resultsFile = File(Desktop_TESTS_RESULT_PATH);
      AppVeyor.UploadTestResults(resultsFile.Path.FullPath, AppVeyorTestResultsType.MSTest);
    }
  });
