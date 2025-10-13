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

1. **GitMetadataTask** ([GitMetadataTask.cs](source/timewarp-build-tasks/GitMetadataTask.cs))
   - Executes `git rev-parse HEAD` to get commit hash
   - Executes `git log -1 --format=%ct` to get commit timestamp
   - Injects data as AssemblyMetadata attributes: `CommitHash` and `CommitDate`
   - Runs during `BeforeTargets="CoreCompile"`
   - Can be disabled by setting `TimeWarpEnableGitMetadata=false`

2. **Task Registration** ([TimeWarp.Build.Tasks.props](source/timewarp-build-tasks/build/TimeWarp.Build.Tasks.props))
   - Defines the `UsingTask` declaration
   - Configures automatic execution target
   - Automatically applied to consuming projects via `build/` and `buildTransitive/` paths

### Package Layout
```
build/
  netstandard2.0/TimeWarp.Build.Tasks.dll  (task assembly)
  TimeWarp.Build.Tasks.props               (auto-imported)
buildTransitive/
  TimeWarp.Build.Tasks.props               (transitive imports)
```

## Build Commands

### Build the package
```bash
dotnet build source/timewarp-build-tasks/TimeWarp.Build.Tasks.csproj
```

### Pack for NuGet
```bash
dotnet pack source/timewarp-build-tasks/TimeWarp.Build.Tasks.csproj -c Release
```

### Install locally for testing
```bash
# Build and pack
dotnet pack source/timewarp-build-tasks/TimeWarp.Build.Tasks.csproj -c Release -o artifacts/packages

# In consuming project, add package source
dotnet nuget add source /path/to/artifacts/packages

# Reference in consuming project
dotnet add package TimeWarp.Build.Tasks --version 1.0.0-beta.1
```

## Solution Structure

- **timewarp-build-tasks.slnx** - Modern XML solution file format (VS 2022+)
- **Directory.Build.props** - Repository-level MSBuild properties
- **Directory.Packages.props** - Central Package Management (CPM) configuration
- **source/timewarp-build-tasks/** - Single project containing the MSBuild task

## MSBuild Configuration

### Central Package Management
All package versions are centrally managed in [Directory.Packages.props](Directory.Packages.props):
- Microsoft.Build.Framework: 17.15.0-preview-25277-114
- Microsoft.Build.Utilities.Core: 17.15.0-preview-25277-114

### Target Framework
- **netstandard2.0** - Required for MSBuild task compatibility across .NET Framework and .NET Core/5+ builds

### Important Properties
- `LangVersion=latest` - Uses latest C# features
- `Nullable=enable` - Nullable reference types enabled
- `TreatWarningsAsErrors=true` - All warnings are errors
- `CopyLocalLockFileAssemblies=true` - Includes dependencies in output

## Adding New MSBuild Tasks

1. Create task class inheriting from `Microsoft.Build.Utilities.Task`
2. Define `[Output]` properties for task results
3. Implement `Execute()` method
4. Register in `build/TimeWarp.Build.Tasks.props`:
   ```xml
   <UsingTask TaskName="TimeWarp.Build.Tasks.YourTask"
              AssemblyFile="$(MSBuildThisFileDirectory)netstandard2.0/TimeWarp.Build.Tasks.dll" />

   <Target Name="YourTarget" BeforeTargets="CoreCompile">
     <YourTask />
   </Target>
   ```

## Package Metadata

- **License**: Unlicense
- **Author**: Steven T. Cramer
- **PackageVersion**: 1.0.0-beta.1 (defined in Directory.Build.props)
- **Repository**: https://github.com/TimeWarpEngineering/timewarp-build-tasks

## SourceLink Configuration

Enabled for deterministic builds:
- `PublishRepositoryUrl=true`
- `EmbedUntrackedSources=true`
- Symbol packages generated as `.snupkg` format
