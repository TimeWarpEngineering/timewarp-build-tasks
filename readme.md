[![Stars](https://img.shields.io/github/stars/TimeWarpEngineering/timewarp-build-tasks?logo=github)](https://github.com/TimeWarpEngineering/timewarp-build-tasks)
[![Forks](https://img.shields.io/github/forks/TimeWarpEngineering/timewarp-build-tasks)](https://github.com/TimeWarpEngineering/timewarp-build-tasks)
[![License](https://img.shields.io/github/license/TimeWarpEngineering/timewarp-build-tasks.svg?style=flat-square&logo=github)](https://github.com/TimeWarpEngineering/timewarp-build-tasks/issues)
[![Issues Open](https://img.shields.io/github/issues/TimeWarpEngineering/timewarp-build-tasks.svg?logo=github)](https://github.com/TimeWarpEngineering/timewarp-build-tasks/issues)

[![nuget](https://img.shields.io/nuget/v/TimeWarp.Build.Tasks?logo=nuget)](https://www.nuget.org/packages/TimeWarp.Build.Tasks/)
[![nuget](https://img.shields.io/nuget/dt/TimeWarp.Build.Tasks?logo=nuget)](https://www.nuget.org/packages/TimeWarp.Build.Tasks/)

[![Twitter](https://img.shields.io/twitter/url?style=social&url=https%3A%2F%2Fgithub.com%2FTimeWarpEngineering%2Ftimewarp-build-tasks)](https://twitter.com/intent/tweet?url=https://github.com/TimeWarpEngineering/timewarp-build-tasks)
[![Dotnet](https://img.shields.io/badge/dotnet-10.0-blue)](https://dotnet.microsoft.com)

[![Discord](https://img.shields.io/discord/715274085940199487?logo=discord)](https://discord.gg/7F4bS2T)
[![Twitter](https://img.shields.io/twitter/follow/StevenTCramer.svg)](https://twitter.com/intent/follow?screen_name=StevenTCramer)
[![Twitter](https://img.shields.io/twitter/follow/TheFreezeTeam1.svg)](https://twitter.com/intent/follow?screen_name=TheFreezeTeam1)

<img src="https://raw.githubusercontent.com/TimeWarpEngineering/timewarpengineering.github.io/refs/heads/master/images/LogoNoMarginNoShadow.svg" alt="logo" height="120" style="float: right" />

# TimeWarp.Build.Tasks

**TimeWarp.Build.Tasks** is an MSBuild tasks library that provides build-time automation for .NET projects. The library automatically injects git metadata (commit hash and timestamp) into your assemblies at build time, enabling better traceability and versioning in production deployments.

This build-time dependency integrates seamlessly into your build pipeline without adding runtime overhead, making it ideal for CI/CD workflows and production builds where tracking exact source versions is critical.

## Give a Star! :star:

If you find this project useful, please give it a star. Thanks!

## Features

- **Automatic Git Metadata Injection** - Embeds commit hash and timestamp into assembly metadata
- **Build-Time Only** - No runtime dependencies or performance impact
- **Transitive Support** - Automatically applied to projects that reference packages using this library
- **Configurable** - Can be disabled per-project with `TimeWarpEnableGitMetadata=false`
- **Fallback Handling** - Gracefully handles non-git repositories without breaking builds

## Installation

```console
dotnet add package TimeWarp.Build.Tasks
```

Check out the latest NuGet packages on the [TimeWarp Enterprises NuGet page](https://www.nuget.org/profiles/TimeWarp.Enterprises).

* [TimeWarp.Build.Tasks](https://www.nuget.org/packages/TimeWarp.Build.Tasks/) [![nuget](https://img.shields.io/nuget/v/TimeWarp.Build.Tasks?logo=nuget)](https://www.nuget.org/packages/TimeWarp.Build.Tasks/)

## Usage

Once installed, git metadata is automatically injected into your assemblies during build. Access the metadata at runtime:

```csharp
using System.Reflection;

var assembly = typeof(Program).Assembly;
var commitHash = assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
    .FirstOrDefault(a => a.Key == "CommitHash")?.Value;
var commitDate = assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
    .FirstOrDefault(a => a.Key == "CommitDate")?.Value;

Console.WriteLine($"Built from commit: {commitHash}");
Console.WriteLine($"Commit date: {commitDate}");
```

### Disabling Git Metadata

To disable automatic git metadata injection for a specific project:

```xml
<PropertyGroup>
  <TimeWarpEnableGitMetadata>false</TimeWarpEnableGitMetadata>
</PropertyGroup>
```

## Documentation

See full [documentation](https://timewarpengineering.github.io/timewarp-build-tasks/).

## Unlicense

[![License](https://img.shields.io/github/license/TimeWarpEngineering/timewarp-build-tasks.svg?style=flat-square&logo=github)](https://unlicense.org)
This project is licensed under the [Unlicense](https://unlicense.org).

## Contributing

Your contributions are welcome! Before starting any work, please open a [discussion](https://github.com/TimeWarpEngineering/timewarp-build-tasks/discussions).

Help with the [documentation](https://timewarpengineering.github.io/timewarp-build-tasks/) is also greatly appreciated.

## Contact

If you have an issue and don't receive a timely response, feel free to reach out on our [Discord server](https://discord.gg/A55JARGKKP).

[![Discord](https://img.shields.io/discord/715274085940199487?logo=discord)](https://discord.gg/7F4bS2T)
