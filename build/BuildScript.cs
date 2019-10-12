using System;
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
            context.Properties.Set(BuildProps.BuildConfiguration, "Release");
        }

        protected override void ConfigureTargets(ITaskContext context)
        {
            var buildVersion = context.CreateTarget("build.version")
                .SetAsHidden()
                .SetDescription("Fetches build version from 'EasyCaching.ProjectVersion.txt' file.")
                .AddTask(x => x.FetchBuildVersionFromFileTask());
      
            var build = context.CreateTarget("Build")
                .SetDescription("Build's the solution.")
                .AddCoreTask(x => x.Restore())
                .AddCoreTask(x => x.UpdateNetCoreVersionTask(_easyCachingProjects))
                .AddCoreTask(x => x.Build())
                .DependsOn(buildVersion);

            var runTests = context.CreateTarget("Run.Tests")
                .SetDescription("Run's all Easy caching tests.")
                .AddTask(X => X.RunProgramTask("docker")
                    .WithArguments("ps", "-a"))
                .AddCoreTask(x => x.Test().Project("test/EasyCaching.UnitTests/EasyCaching.UnitTests.csproj").NoBuild());              

           var nugetPublish = context.CreateTarget("Nuget.Publish")
                .SetDescription("Packs and publishes nuget package.")
                .DependsOn(buildVersion)
                .AddTasks(PackProjects)
                .Do(PublishNuGetPackage);

           var rebuild = context.CreateTarget("Rebuild")
                .SetAsDefault()
                .DependsOn(buildVersion, build, runTests);

            context.CreateTarget("Rebuild.Server")
                .SetAsDefault()
                .DependsOn(rebuild)
                .DependsOn(nugetPublish);
        }

        private void PackProjects(ITarget target)
        {
            target
                .AddCoreTask(x => x.Pack()
                    .Project("src/EasyCaching.Core")
                    .IncludeSymbols()
                    .OutputDirectory("output"))
                .AddCoreTask(x => x.Pack()
                    .Project("src/EasyCaching.Bus.CSRedis")
                    .IncludeSymbols()
                    .OutputDirectory("output"))
                .AddCoreTask(x => x.Pack()
                    .Project("src/EasyCaching.Bus.RabbitMQ")
                    .IncludeSymbols()
                    .OutputDirectory("output"))
                .AddCoreTask(x => x.Pack()
                    .Project("src/EasyCaching.Bus.Redis")
                    .IncludeSymbols()
                    .OutputDirectory("output"))
                .AddCoreTask(x => x.Pack()
                    .Project("src/EasyCaching.Disk")
                    .IncludeSymbols()
                    .OutputDirectory("output"))
                .AddCoreTask(x => x.Pack()
                    .Project("src/EasyCaching.HybridCache")
                    .IncludeSymbols()
                    .OutputDirectory("output"))
                .AddCoreTask(x => x.Pack()
                    .Project("src/EasyCaching.InMemory")
                    .IncludeSymbols()
                    .OutputDirectory("output"))
                .AddCoreTask(x => x.Pack()
                    .Project("src/EasyCaching.Interceptor.AspectCore")
                    .IncludeSymbols()
                    .OutputDirectory("output"))
                .AddCoreTask(x => x.Pack()
                    .Project("src/EasyCaching.Interceptor.Castle")
                    .IncludeSymbols()
                    .OutputDirectory("output"))
                .AddCoreTask(x => x.Pack()
                    .Project("src/EasyCaching.Memcached")
                    .IncludeSymbols()
                    .OutputDirectory("output"))
                .AddCoreTask(x => x.Pack()
                    .Project("src/EasyCaching.Redis")
                    .IncludeSymbols()
                    .OutputDirectory("output"))
                .AddCoreTask(x => x.Pack()
                    .Project("src/EasyCaching.ResponseCaching")
                    .IncludeSymbols()
                    .OutputDirectory("output"))
                .AddCoreTask(x => x.Pack()
                    .Project("src/EasyCaching.Serialization.Json")
                    .IncludeSymbols()
                    .OutputDirectory("output"))
                .AddCoreTask(x => x.Pack()
                    .Project("src/EasyCaching.Serialization.MessagePack")
                    .IncludeSymbols()
                    .OutputDirectory("output"))
                .AddCoreTask(x => x.Pack()
                    .Project("src/EasyCaching.Serialization.Protobuf")
                    .IncludeSymbols()
                    .OutputDirectory("output"))
                .AddCoreTask(x => x.Pack()
                    .Project("src/EasyCaching.SQLite")
                    .IncludeSymbols()
                    .OutputDirectory("output"));
        }

        private void PublishNuGetPackage(ITaskContext context)
        {
            var version = context.Properties.GetBuildVersion();
            var nugetVersion = version.ToString(3);

            context.CoreTasks().NugetPush($"output/EasyCaching.{nugetVersion}.nupkg")
                .DoNotFailOnError((e) => { context.LogInfo($"Failed to publish Nuget package."); })
                .ServerUrl("https://www.nuget.org/api/v2/package")
                .ApiKey(NugetApiKey)
                .Execute(context);

            context.CoreTasks().NugetPush($"output/EasyCaching.Bus.CSRedis.{nugetVersion}.nupkg")
                .DoNotFailOnError((e) => { context.LogInfo($"Failed to publish Nuget package."); })
                .ServerUrl("https://www.nuget.org/api/v2/package")
                .ApiKey(NugetApiKey)
                .Execute(context);

            context.CoreTasks().NugetPush($"output/EasyCaching.Bus.RabbitMQ.{nugetVersion}.nupkg")
                .DoNotFailOnError((e) => { context.LogInfo($"Failed to publish Nuget package."); })
                .ServerUrl("https://www.nuget.org/api/v2/package")
                .ApiKey(NugetApiKey)
                .Execute(context);

            context.CoreTasks().NugetPush($"output/EasyCaching.Bus.Redis.{nugetVersion}.nupkg")
                .DoNotFailOnError((e) => { context.LogInfo($"Failed to publish Nuget package."); })
                .ServerUrl("https://www.nuget.org/api/v2/package")
                .ApiKey(NugetApiKey)
                .Execute(context);

            context.CoreTasks().NugetPush($"output/EasyCaching.CSRedis.{nugetVersion}.nupkg")
                .DoNotFailOnError((e) => { context.LogInfo($"Failed to publish Nuget package."); })
                .ServerUrl("https://www.nuget.org/api/v2/package")
                .ApiKey(NugetApiKey)
                .Execute(context);

            context.CoreTasks().NugetPush($"output/EasyCaching.Disk.{nugetVersion}.nupkg")
                .DoNotFailOnError((e) => { context.LogInfo($"Failed to publish Nuget package."); })
                .ServerUrl("https://www.nuget.org/api/v2/package")
                .ApiKey(NugetApiKey)
                .Execute(context);

            context.CoreTasks().NugetPush($"output/EasyCaching.HybridCache.{nugetVersion}.nupkg")
                .DoNotFailOnError((e) => { context.LogInfo($"Failed to publish Nuget package."); })
                .ServerUrl("https://www.nuget.org/api/v2/package")
                .ApiKey(NugetApiKey)
                .Execute(context);

            context.CoreTasks().NugetPush($"output/EasyCaching.InMemory.{nugetVersion}.nupkg")
                .DoNotFailOnError((e) => { context.LogInfo($"Failed to publish Nuget package."); })
                .ServerUrl("https://www.nuget.org/api/v2/package")
                .ApiKey(NugetApiKey)
                .Execute(context);

            context.CoreTasks().NugetPush($"output/EasyCaching.Interceptor.AspectCore.{nugetVersion}.nupkg")
                .DoNotFailOnError((e) => { context.LogInfo($"Failed to publish Nuget package."); })
                .ServerUrl("https://www.nuget.org/api/v2/package")
                .ApiKey(NugetApiKey)
                .Execute(context);

            context.CoreTasks().NugetPush($"output/EasyCaching.Interceptor.Castle.{nugetVersion}.nupkg")
                .DoNotFailOnError((e) => { context.LogInfo($"Failed to publish Nuget package."); })
                .ServerUrl("https://www.nuget.org/api/v2/package")
                .ApiKey(NugetApiKey)
                .Execute(context);

            context.CoreTasks().NugetPush($"output/EasyCaching.Memcached.{nugetVersion}.nupkg")
                .DoNotFailOnError((e) => { context.LogInfo($"Failed to publish Nuget package."); })
                .ServerUrl("https://www.nuget.org/api/v2/package")
                .ApiKey(NugetApiKey)
                .Execute(context);

            context.CoreTasks().NugetPush($"output/EasyCaching.Redis.{nugetVersion}.nupkg")
                .DoNotFailOnError((e) => { context.LogInfo($"Failed to publish Nuget package."); })
                .ServerUrl("https://www.nuget.org/api/v2/package")
                .ApiKey(NugetApiKey)
                .Execute(context);

            context.CoreTasks().NugetPush($"output/EasyCaching.ResponseCaching.{nugetVersion}.nupkg")
                .DoNotFailOnError((e) => { context.LogInfo($"Failed to publish Nuget package."); })
                .ServerUrl("https://www.nuget.org/api/v2/package")
                .ApiKey(NugetApiKey)
                .Execute(context);

            context.CoreTasks().NugetPush($"output/EasyCaching.Serialization.Json.{nugetVersion}.nupkg")
                .DoNotFailOnError((e) => { context.LogInfo($"Failed to publish Nuget package."); })
                .ServerUrl("https://www.nuget.org/api/v2/package")
                .ApiKey(NugetApiKey)
                .Execute(context);

            context.CoreTasks().NugetPush($"output/EasyCaching.Serialization.MessagePack.{nugetVersion}.nupkg")
                .DoNotFailOnError((e) => { context.LogInfo($"Failed to publish Nuget package."); })
                .ServerUrl("https://www.nuget.org/api/v2/package")
                .ApiKey(NugetApiKey)
                .Execute(context);

            context.CoreTasks().NugetPush($"output/EasyCaching.Serialization.Protobuf.{nugetVersion}.nupkg")
                .DoNotFailOnError((e) => { context.LogInfo($"Failed to publish Nuget package."); })
                .ServerUrl("https://www.nuget.org/api/v2/package")
                .ApiKey(NugetApiKey)
                .Execute(context);

            context.CoreTasks().NugetPush($"output/EasyCaching.SQLite.{nugetVersion}.nupkg")
                .DoNotFailOnError((e) => { context.LogInfo($"Failed to publish Nuget package."); })
                .ServerUrl("https://www.nuget.org/api/v2/package")
                .ApiKey(NugetApiKey)
                .Execute(context);
        }

        private string[] _easyCachingProjects = new string[]
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
