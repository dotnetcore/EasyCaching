using System;
using System.IO;
using FlubuCore.Context;
using FlubuCore.Context.Attributes.BuildProperties;
using FlubuCore.Context.FluentInterface;
using FlubuCore.Context.FluentInterface.Interfaces;
using FlubuCore.IO;
using FlubuCore.Scripting;
using FlubuCore.Targeting;

namespace Build
{
    public class BuildScript : DefaultBuildScript
    {
        [FromArg("apiKey", "Nuget api key for publishing nuget package.")]
        public string NugetApiKey { get; set; }

        [ProductId] public string ProductId { get; set; } = "ProductId";

        [SolutionFileName] public string SolutionFileName { get; set; } = "EasyCaching.sln";

        [BuildConfiguration] public string BuildConfiguration { get; set; } = "Release";

        public FullPath OutputDir => RootDirectory.CombineWith("output");

        public FullPath SourceDir => RootDirectory.CombineWith("src");

        public FullPath TestDir => RootDirectory.CombineWith("test");

        protected override void ConfigureTargets(ITaskContext context)
        {
            var build = context.CreateTarget("Build")
                .SetDescription("Build's the solution.")
                .AddCoreTask(x => x.Clean()
                    .CleanOutputDir())
                .AddCoreTask(x => x.Restore())
                .AddCoreTask(x => x.Build());

            var runTests = context.CreateTarget("Run.Tests")
                .SetDescription("Run's all EasyCaching tests.")
                .AddTask(X => X.RunProgramTask("docker")
                    .WithArguments("ps", "-a"))
                .AddCoreTask(x => x.Test().Project(TestDir.CombineWith("EasyCaching.UnitTests/EasyCaching.UnitTests.csproj"))
                    .NoBuild());

            var projectsToPack = context.GetFiles(SourceDir, "*/*.csproj");
            var nugetPublish = context.CreateTarget("Nuget.Publish")
                .SetDescription("Packs and publishes nuget package.")
                .Requires(() => NugetApiKey)
                .ForEach(projectsToPack, (project, target) =>
                {
                    target
                        .AddCoreTask(x => x.Pack()
                            .Project(project)
                            .IncludeSymbols()
                            .OutputDirectory(OutputDir));
                })
                .Do(PublishNuGetPackage);

            var rebuild = context.CreateTarget("Rebuild")
                .SetAsDefault()
                .SetDescription("Build's the solution and run's all tests.")
                .DependsOn(build, runTests);

            var branch = context.BuildSystems().AppVeyor().BranchName;

            context.CreateTarget("Rebuild.Server")
                .SetDescription("Build's the solution, run's all tests and publishes nuget package when running on Appveyor.")
                .DependsOn(rebuild)
                .DependsOn(nugetPublish)
                   .When((c) => c.BuildSystems().RunningOn == BuildSystemType.AppVeyor && branch != null && branch.Equals("master", StringComparison.OrdinalIgnoreCase));
        }

        private void PublishNuGetPackage(ITaskContext context)
        {
            var packageFiles = Directory.GetFiles(context.Properties.GetOutputDir(), "*.nupkg");

            foreach (var packageFile in packageFiles)
            {
                if (packageFile.EndsWith("symbols.nupkg", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                context.CoreTasks().NugetPush(packageFile)
                    .DoNotFailOnError((e) => { context.LogInfo($"Failed to publish Nuget package."); })
                    .ServerUrl("https://www.nuget.org/api/v2/package")
                    .ApiKey(NugetApiKey)
                    .Execute(context);
            }
        }
    }
}
