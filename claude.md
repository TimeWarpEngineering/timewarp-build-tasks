# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

TimeWarp.Build.Tasks is an MSBuild tasks NuGet package that provides build-time automation for TimeWarp projects. The primary functionality is automatic git metadata injection into assemblies.

## Architecture

### MSBuild Task Package Structure

This is a **build-time dependency package** (not a runtime library):
- `IncludeBuildOutput=false` - The DLL is not added as a lib reference
- `DevelopmentDependency=true` - Package is consumed only at build time
- Tasks are automatically imported via `build/TimeWarp.Build.Tasks.props`

### Key Components

1. **GitMetadataTask** ([git-metadata-task.cs](source/timewarp-build-tasks/git-metadata-task.cs))
   - Executes `git rev-parse HEAD` to get commit hash
   - Executes `git log -1 --format=%ct` to get commit timestamp
   - Injects data as AssemblyMetadata attributes: `CommitHash` and `CommitDate`
   - Runs `BeforeTargets="GetAssemblyAttributes;GenerateAssemblyInfo"`
   - Can be disabled by setting `TimeWarpEnableGitMetadata=false`

2. **Task Registration** ([timewarp-build-tasks.props](source/timewarp-build-tasks/build/timewarp-build-tasks.props))
   - Defines the `UsingTask` declaration
   - Configures automatic execution target
   - Emits error `TWBT0001` when MSBuild is not running on .NET 11+ (Visual Studio, `MSBuild.exe`, older SDKs)
   - Automatically applied to consuming projects via `build/` and `buildTransitive/` paths

### Package Layout
```
build/
  net11.0/TimeWarp.Build.Tasks.dll  (task assembly)
  TimeWarp.Build.Tasks.props        (auto-imported)
buildTransitive/
  net11.0/TimeWarp.Build.Tasks.dll  (task assembly)
  TimeWarp.Build.Tasks.props        (transitive imports)
```

## Build Commands

### Build the package
```bash
dotnet build source/timewarp-build-tasks/timewarp-build-tasks.csproj
```

### Pack for NuGet
```bash
dotnet pack source/timewarp-build-tasks/timewarp-build-tasks.csproj -c Release
```

### Install locally for testing
```bash
# Build and pack
dotnet pack source/timewarp-build-tasks/timewarp-build-tasks.csproj -c Release -o artifacts/packages

# In consuming project, add package source
dotnet nuget add source /path/to/artifacts/packages

# Reference in consuming project
dotnet add package TimeWarp.Build.Tasks --version 2.0.0
```

## Solution Structure

- **timewarp-build-tasks.slnx** - Modern XML solution file format (VS 2022+)
- **Directory.Build.props** - Repository-level MSBuild properties
- **Directory.Packages.props** - Central Package Management (CPM) configuration
- **source/timewarp-build-tasks/** - Single project containing the MSBuild task

## MSBuild Configuration

### Central Package Management
All package versions are centrally managed in [Directory.Packages.props](Directory.Packages.props):
- Microsoft.Build.Framework: 18.12.2
- Microsoft.Build.Utilities.Core: 18.12.2

These must not be newer than the MSBuild in the minimum supported SDK (.NET SDK 11.0.100 ships
MSBuild 18.12.2); a task compiled against a newer MSBuild fails to load.

### Target Framework
- **net11.0** - From 2.0.0 the task runs only in MSBuild on .NET 11+ (`dotnet build` with the .NET 11 SDK).
  Visual Studio and `MSBuild.exe` (.NET Framework MSBuild) are not supported; consumers on those stay on 1.x.

### Important Properties
- `Nullable=enable` - Nullable reference types enabled
- `TreatWarningsAsErrors=true` - All warnings are errors

## Adding New MSBuild Tasks

1. Create task class inheriting from `Microsoft.Build.Utilities.Task`
2. Define `[Output]` properties for task results
3. Implement `Execute()` method
4. Register in `source/timewarp-build-tasks/build/timewarp-build-tasks.props` (packed as `build/TimeWarp.Build.Tasks.props`):
   ```xml
   <UsingTask TaskName="TimeWarp.Build.Tasks.YourTask"
              AssemblyFile="$(MSBuildThisFileDirectory)net11.0/TimeWarp.Build.Tasks.dll"
              Condition="'$(MSBuildRuntimeType)' == 'Core'" />

   <Target Name="YourTarget" BeforeTargets="CoreCompile">
     <YourTask />
   </Target>
   ```

## Package Metadata

- **License**: Unlicense
- **Author**: Steven T. Cramer
- **PackageVersion**: 2.0.0 (defined in source/Directory.Build.props)
- **Repository**: https://github.com/TimeWarpEngineering/timewarp-build-tasks

## SourceLink Configuration

Enabled for deterministic builds:
- `PublishRepositoryUrl=true`
- `EmbedUntrackedSources=true`
- Symbol packages generated as `.snupkg` format
