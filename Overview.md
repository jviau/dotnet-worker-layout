# Engineering Flow

The goals of this engineering flow improvements is to provide an intuitive, consistent, and up-to-date experience for an engineers day-to-day development experience. We should be using existing tooling as much as possible and avoiding custom build scripts. Visual Studio, VSCode, or `dotnet` CLI is the only tooling you should need to build this repository. Builds should be as consistent as possible between local development and the CI machines. What happens locally, should happen on the CI machine - no PR build failure surprises. Additionally, we should be set up for rapid adoption of new dotnet and C# features as they are released.

This proposal breaks up into the following areas: Structure, Build & Release, Analyzers & Tooling.

## Structure

Part of this proposal is to adjust the repo structure. The goal behind the repo structure is to strike a balance between convenient and predictable folder navigation along with efficient grouping to leverage MSBuild `Directory.Build.props/targets` to apply common properties to "sections". One of the goals was when navigating via command line, you should be able to easily tab-complete to the folder you want. Having all project folders start with the same name makes this inconvenient. (ie: `Worker.Extensions.*` for all folder names in `extensions`).

### Goals

- Layout makes "sense". There should be a mental mapping between a projects name and location to its built assembly name.
- Projects are logically grouped and structured together into discrete shippable units.
  - A 'shippable unit' is one or many projects that release & ship together.
  - A shippable unit ideally maps to a folder cone.
- Navigation by CLI should be convenient - easy tab completion.
- Repo root gives immediate idea of where stuff goes.

### Layout from a glance

``` json
|- doc                  // doc: contains all doc files
|- eng                  // eng: contains all our engineering related items: common targets, ci templates, signing key
|  |- ci                // eng/ci: CI build files and templates.
|  |- targets           // eng/targets: common msbuild .props and .targets and their supporting files.
|- misc                 // misc: this is a special csproj which collects files, leveraging globbing, to show in Visual Studio, without needing to add one by one to sln.
|- out                  // out: all project output goes here
|  |- bin               // out/bin: final output
|  |- obj               // out/obj: intermediate output
|  |- pkg               // out/pkg: all packages (nuget or otherwise) go here
|- protos               // protos: proto subdirectory *NOTE*: we should consider consuming these as nuget packages.
|- samples              // samples: all sample projects.
|- src                  // src: all source files go here
|  |- Extensions        // src/Extensions: All worker extensions (including ApplicationInsights)
|  |- Host              // src/Host: Functions custom dotnet host projects
|  |- Shared            // src/Shared: "SharedReference" projects, giving us a way to directly share code without using InternalsVisibleTo
|  |- Worker            // src/Worker: All the projects that make up our 'framework'
|     |- Sdk            // src/Worker/Sdk: All the SDK projects
|     |  |- Sdk         // src/Worker/Sdk/Sdk: Microsoft.Azure.Functions.Worker.Sdk
|     |  |- Analyzers   // src/Worker/Sdk/Analyzers: Microsoft.Azure.Functions.Worker.Sdk.Analyzers
|     |  |- Generators  // src/Worker/Sdk/Generators: Microsoft.Azure.Functions.Worker.Sdk.Generators
|     |- Worker         // src/Worker/Worker: Microsoft.Azure.Functions.Worker
|     |- Core           // src/Worker/Core: Microsoft.Azure.Functions.Worker.Core
|     |- Grpc           // src/Worker/Grpc: Microsoft.Azure.Functions.Worker.Grpc
|- test                 // test: all test code here. Mirrors src structure.
```

The repository has been restructured as follows:
- **All shipping** source code is under `src` folder
  - `sdk` -> `src/Worker/Sdk`
  - `extensions` -> `src/Extensions`
  - Worker projects -> `src/Worker`
  - Code under src follows this general format:
    1. start with a repo-wide prefix "`TopLevelNamespace`": `Microsoft.Azure.Functions`.
    2. Look at package names: `Microsoft.Azure.Functions.Worker.Core`
    3. Trim prefix from package name: `Worker.Core`
    4. Split on `"."` into folders: `Worker/Core`
    5. Name csproj with 3: `Worker.Core.csproj`
    6. End result: `src/Worker/Core/Worker.csproj`
  - There are exceptions to the above.
    1. When assembly name matches folder: `Microsoft.Azure.Functions.Worker`, repeat the last section: `src/Worker/Worker/Worker.csproj`.
    2. "Sections" may have their own rules. `src/Extensions` is slightly different: `src/Extensions/{ShortName}/src/Worker.Extensions.{ShortName}.csproj`
  - Exceptions are allowed. `src/Extensions/ApplicationInsights/src/Worker.ApplicationInsights.csproj` (omits `Extensions` from assembly name).
- Samples go under `samples` still. This is non-shipping code.
- The `test` section follows an identical structure, just with `.Tests` added to folders and csproj as appropriate.
- All projects build to `out/bin/{AssemblyName}/{Configuration}/{TargetFramework}`
  - Samples builds to `out/samples/...`, just to keep them separate.
- `misc` is a helper project for Visual Studio. Lets us use globbing instead of explicit solution files for showing random files in VS.
- `doc` any doc files we want available that don't fit elsewhere (eg. msdocs). Such as analyzer rules.

## Build & Release

To improve our build experience, this design aims to make the local build and CI experience as identical as possible. Additionally, it aims to make the build experience per project consistent and predictable. Adding new projects should be very simple, with so long as you place it in the right location with the right name, things should "just work".

### Goals

- To build, you should never need anything beyond `dotnet build` or your respective IDE's build command.
  - No custom environment, tools, or scripts.
- Builds are consistent and predictable between local and CI.
  - Want to fully mirror ci build? `dotnet pack -p:CI=true`
- Can build at any level, see `dirs.proj` files sprinkled about.
  - `cd src/extensions && dotnet build` -> builds all extensions.
- Simplified versioning. Only use `<VersionPrefix>` and `<VersionSuffix>`.
- Per-package release notes and readme.
- Well defined and _automated_ (where possible) release flow.

### MSBuild props and targets

Building off the repo structure, we extensively leverage `Directory.Build` files to set all common props and targets. The goal is that all MSBuild semantics are unified across our projects, avoiding accidental diverging later on. In general, you will see individual `.csproj` files have very minimal properties in them, if any at all. This is because all of the properties come in via `Directory.Build.{props|targets}`. Though those files combined with the repo structure, we can have all common msbuild extracted out of the csproj itself.

- Configures all projects build to the `out`
  - Have a problem with stale build artifacts? delete `out` directory. No need to find individual `bin` and `obj` folders all over.
  - Predictable output and package locations. Can be leveraged in build & ci scripts.
- Applies common CI versioning semantics
- Common packaging properties and customization
- Custom targets, ie: `InternalsVisibleTo.targets`, `SharedReferences.targets`

## Dirs.proj

Throughout the repo you will see `dirs.proj` files spread about. These files let you build and test that folder section the `dirs.proj` lives in. These can also be used for CI. The `.sln` file is only needed for your IDE.

#### NuGet Packages

- Every package now has its own `README.md` and `release_notes.md`. These will be included in the package, flushing out the `nuget.org` experience.
  - Example result: https://www.nuget.org/packages/Microsoft.DurableTask.Abstractions
  - Per-package `README.md` should include only information about that specific package, and how it relates to the rest of the Worker ecosystem. Can be brief or descriptive, up to use to choose.
  - Per-package `release_notes.md` - these are the changes for *only* that package. Release notes will also automatically include a link to the github release tag for the package.

### CI

We will continue to use ADO. We want all CI to be **minimal** additional logic on top of our build logic. In essence, all CI should amount to the following steps:

1. `restore`
   1. Performs `dotnet restore {Target}`
   2. Installs any additional tools (CosmosDB Emulator, Azurite)
2. `build`
   1. Performs `dotnet build {Target} -c release --no-restore --no-incremental`
   2. Signs outputs, verifies signatures
3. `test`
   1. Performs `dotnet test {Target} -c release --no-build`
   2. Uploads test and code-coverage results.
4. `pack`
   1. Performs `dotnet pack {Target} -c release --no-build`
   2. Signs outputs, verifies signatures

`out/pkg/**` will always be the build artifacts.

Any additional CI behavior will be inferred and performed within our build targets, no need for passing extra build properties.

**TODO:** We should investigate moving onto OneBranch pipelines.

### Pull Requests

Instead of trying to kick off specific builds based on what the PR actually touches, all PRs build the entire repo and run all tests. This simplifies cross-cutting PR concerns and reduces changes which miss anything.

### Versioning

We release a lot of things from this repo. To make sense of it, we structure the repo to organize discrete releases into their own 'sections'. Anywhere you see a `ci.yml` file means that directory and all sub directories are an independent release. The primary change is that anything within a discrete release **will be versioned the same**. The biggest change here is that all Worker (non-extension) and SDK packages will now always have the same version number. This also simplifies the customer experience: simpler matrix of package to package version.

- Since SDK and Worker packages are all built together, we will version them consistently. Even if one of the packages has no changes, we will still rev version and release when another one is changed.
  - This lets us use a single git tag for tagging and publishing these releases.
  - Dev does not need to think too much about what is all changing. When we want to cut a release we don't need to think about what has and hasn't changed, just release them all.
  - This will prevent a meta package version slowly getting away from the packages it references.
- Extensions are still versioned and released independently. They will be tagged independently as well.
- Nit change: `-preview1` -> `-preview.1`. Use a dot separator for preview version, align with dotnet team.
- In the project/props files: we only use `<VersionPrefix>` and `<VersionSuffix>` (no leading "`-`").
  - The dotnet SDK common props already understand and combine these values for you. We don't need all the custom versioning logic we have beyond setting these two properties.

#### CI Versioning scheme

- from PR: `{VersionSuffix}` -> `{VersionSuffix}.{BuildReason}.{BuildNumber}.{BuildRevision}`
- `{BuildReason}`:
  - `pr` - for PR builds
  - `ci` - for CI builds off `main` and `feature/*`
  - `dev` - for all builds on dev machine.
- `{BuildNumber}`: `{yyMMdd}`
- `{BuildRevision}`: this is the `{rev:r}` part of the ADO `Build.BuildNumber`. Ensures all CI/PR builds are uniquely versioned.
  - For `dev`, this is always `0`.
- example: `preview.1` -> `preview.1.pr.230616.1`
- When releasing from a tag, we do not modify `{VersionSuffix}`.

### Release Flow

The release flow is still a work in progress. The goal is to have an automated as possible system. In general, we want the release flow to be kicked off with pushing a new tag to git. This should start the dominoes: draft github release created (done automatically with tag push), appropriate CI build started.

#### Worker & Sdk Release Flow

- Rev common version in `src/Worker/Directory.Build.props`, PR & commit
- Tag `v{version}`, push tag
- Kick off CI build, validate
- Release CI build, publish to ADO. Validate as necessary
- Create draft github release for tag `v{version}`, consolidate release notes into release
- Publish release to nuget.org (following SDK partner steps)
- Publish github release
- Reset release notes

#### Extensions Release Flow

- Same flow as above with the following changes:
  - Version is controlled directly in the project.
  - we tag with `{extension_short_name}-v{version}`

**NOTE:** for the steps below, it would be great if we can automate as much of it as possible.
- Reset release notes.
- Rev the patch version and set `VersionSuffix` to `preview.1`.

## Analyzers and Tooling

This design modernizes all the analyzers and tooling we use.

### SDK

Dotnet SDK is pinned via `global.json`. Each dev and CI machine will use the same dotnet SDK version. We also have additional msbuild SDKs listed there, which are used for `misc.csproj` and `dirs.proj`.

### Language features

We will be using all the latest and greatest language features:

- explicit nullability
- implicit usings
- file scoped namespaces

### Analyzers

Editorconfig has been updated to emit warnings for both style and quality. We are using the 'recommended' analyzer set, as well as the _mostly_ default style rules from `dotnet new editorconfig`.

Some naming rules have changed from what we have, primarily non-readonly static field must now start with `s_`.

#### Style Cop

Style cop analyzers have been left out for now. Editorconfig covers almost all of the same functionality, making them less useful these days. The only additional benefit Style cop would offer is stricter XML doc contents verification.

- Standard text requirement for some xmldoc summaries:
  - Constructors: `Initializes a new instance of the <see cref={Name} /> {class|struct}.`.
  - Properties: `Gets {or sets} ...`
  - `bool` properties: `Gets {or sets} a value indicating whether ...`
- `<param>`, `<typeparam>` values all present, in order, not empty, not duplicated.
- `<return>` value present, not empty.
- All xmldoc sections wend with a period.

#### Incremental builds

You will notice warnings 'disappear' on a rebuild. This is due to incremental builds. What is happening is on the rebuild, with nothing else changing in your build, the entire compile step is skipped (nothing to do as nothing changed), and such analyzers are not ran. Thus no warnings emitted, even though it still exists. Your IDE should continue to display the warning though.

This will not be an issue on CI builds as we will explicitly pass in `--no-incremental`.

### Directory.Package.props

This has not been added yet, still evaluating. This system allows us to centrally control all `<PackageReference>` versions. So within a .csproj you only need to fill out the `Include` portion, and the `Version` portion is controlled in the `Directory.Package.props` file. This allows us to ensure all our dependencies are consistent. I am still evaluating this as it may be too restrictive for an SDK. Per-package control of our dependencies may be important for us. We may be able to at least use this in the `src/Worker` directory, but probably not `src/Extensions`.

### Testing

- Every project has its own dedicated unit test project.
  - `[InternalsVisibleTo]` automatically added for Moq and the respective source project.
- All unit tests will be ran on all platforms / OS. We will use [Xunit.SkippableFact](https://github.com/AArnott/Xunit.SkippableFact) to opt out tests at runtime if the platform is not supported:
  ``` CSharp
    [SkippableFact]
    public void SomeTest()
    {
        // Skip can also be placed in the ctor of this class.
        Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows));

        // Test code. Runs only if on windows.
    }
  ```

## Other

- Deprecate `Microsoft.Azure.Functions.Worker.Extensions.Storage` meta-package? Customers can import Blob and Queues independently. This aligns with track 2 SDKs.
- ApplicationInsights move to `src/extensions/ApplicationInsights` this is an optional "extension" package after all.
  - This sets a precedence of packages in `extensions` without `.Extensions.` in name. Concerns?