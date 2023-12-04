using System.Diagnostics;
using System.Reflection;
using System.Runtime.Intrinsics;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Perfolizer.Horology;

if (Debugger.IsAttached)
{
    BenchmarkSwitcher.FromAssembly(Assembly.GetExecutingAssembly()).Run(args, new DebugInProcessConfig());
    return;
}

Job baseJob = Job.Default
    .WithRuntime(CoreRuntime.Latest)
    .WithIterationTime(TimeInterval.FromSeconds(0.25));

IConfig config = DefaultConfig.Instance
    .HideColumns(Column.EnvironmentVariables, Column.RatioSD, Column.Error,
        Column.Categories, Column.IterationCount, Column.WarmupCount,
        Column.Toolchain, Column.CompletedWorkItems, Column.LockContentions)
    .AddLogicalGroupRules(BenchmarkLogicalGroupRule.ByCategory)
    .AddDiagnoser(new DisassemblyDiagnoser(new(exportGithubMarkdown: true, printInstructionAddresses: false)))
    .AddJob(baseJob.WithId("Scalar").WithEnvironmentVariable("DOTNET_EnableHWIntrinsic", "0"));

if (Vector512.IsHardwareAccelerated)
{
    config = config
        .AddJob(baseJob.WithId("Vector128").WithEnvironmentVariable("DOTNET_EnableAVX2", "0"))
        .AddJob(baseJob.WithId("Vector256").WithEnvironmentVariable("DOTNET_PreferredVectorBitWidth", "256"))
        .AddJob(baseJob.WithId("Vector512").WithEnvironmentVariable("DOTNET_PreferredVectorBitWidth", "512"));
}
else if (Vector256.IsHardwareAccelerated)
{
    config = config
        .AddJob(baseJob.WithId("Vector128").WithEnvironmentVariable("DOTNET_EnableAVX2", "0"))
        .AddJob(baseJob.WithId("Vector256"));
}
else if (Vector128.IsHardwareAccelerated)
{
    config = config
        .AddJob(baseJob.WithId("Vector128"));
}

BenchmarkSwitcher.FromAssembly(Assembly.GetExecutingAssembly()).Run(args, config);
