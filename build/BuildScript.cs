using System;
using System.IO;
using FlubuCore.Context;
using FlubuCore.Context.FluentInterface.Interfaces;
using FlubuCore.Scripting;

namespace Build
{
    public class BuildScript : DefaultBuildScript
    {
        [FromArg("apiKey", "Nuget api key for publishing nuget package.")]
        public string NugetApiKey { get; set; }

        protected override void ConfigureBuildProperties(IBuildPropertiesContext context)
        {
            context.Properties.Set(BuildProps.ProductId, "EasyCaching");
            context.Properties.Set(BuildProps.SolutionFileName, "EasyCaching.sln");
            context.Properties.Set(BuildProps.OutputDir, "output");
            context.Properties.Set(BuildProps.BuildConfiguration, "Release");
        }

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
                .AddCoreTask(x => x.Test().Project("test/EasyCaching.UnitTests/EasyCaching.UnitTests.csproj")
                    .NoBuild());

            var nugetPublish = context.CreateTarget("Nuget.Publish")
                .SetDescription("Packs and publishes nuget package.")
                .ForEach(_easyCachingProjectsToPack, (project, target) =>
                {
                    target
                        .AddCoreTask(x => x.Pack()
                            .Project(project)
                            .IncludeSymbols()
                            .OutputDirectory(context.Properties.GetOutputDir()));
                })
                .Do(PublishNuGetPackage);

            var rebuild = context.CreateTarget("Rebuild")
                .SetAsDefault()
                .DependsOn(build, runTests);
            
            var branch = Environment.GetEnvironmentVariable("APPVEYOR_REPO_BRANCH");
            context.CreateTarget("Rebuild.Server")
                .SetAsDefault()
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

        private string[] _easyCachingProjectsToPack = new string[]
        {
            "src/EasyCaching.Core/EasyCaching.Core.csproj",
            "src/EasyCaching.Bus.CSRedis/EasyCaching.Bus.CSRedis.csproj",
            "src/EasyCaching.Bus.RabbitMQ/EasyCaching.Bus.RabbitMQ.csproj",
            "src/EasyCaching.Bus.Redis/EasyCaching.Bus.Redis.csproj",
            "src/EasyCaching.CSRedis/EasyCaching.CSRedis.csproj",
            "src/EasyCaching.Disk/EasyCaching.Disk.csproj",
            "src/EasyCaching.HybridCache/EasyCaching.HybridCache.csproj",
            "src/EasyCaching.InMemory/EasyCaching.InMemory.csproj",
            "src/EasyCaching.Interceptor.AspectCore/EasyCaching.Interceptor.AspectCore.csproj",
            "src/EasyCaching.Interceptor.Castle/EasyCaching.Interceptor.Castle.csproj",
            "src/EasyCaching.Memcached/EasyCaching.Memcached.csproj",
            "src/EasyCaching.Redis/EasyCaching.Redis.csproj",
            "src/EasyCaching.ResponseCaching/EasyCaching.ResponseCaching.csproj",
            "src/EasyCaching.Serialization.Json/EasyCaching.Serialization.Json.csproj",
            "src/EasyCaching.Serialization.MessagePack/EasyCaching.Serialization.MessagePack.csproj",
            "src/EasyCaching.Serialization.Protobuf/EasyCaching.Serialization.Protobuf.csproj",
            "src/EasyCaching.SQLite/EasyCaching.SQLite.csproj",
        };
    }
}
