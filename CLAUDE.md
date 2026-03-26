# CLAUDE.md

## Purpose of `features/physhi-updated`

This branch is a focused Roslyn fork for NScript integration.

Its goal is to let `NScript.csc.lib` observe Roslyn's bound C# bodies during compilation and run custom logic before emit, without carrying a broad compiler fork or rewriting Roslyn's pipeline.

In practice, the meaningful comparison base in this fork is `origin/master`.
Upstream `real_origin/main` exists, but this branch is not a small patch over today's upstream tip; it is a targeted fork branch layered on the fork's own mainline history.

## What changed compared to the fork mainline

### 1. Bound-body callback hook in `CSharpCompilation`

Files:
- `src/Compilers/CSharp/Portable/Compilation/CSharpCompilation.cs`
- `src/Compilers/CSharp/Portable/Compiler/MethodCompiler.cs`

The branch adds an internal callback:

- `CSharpCompilation.OnBoundExpressionGenerated`

This callback receives:

- `MethodSymbol method`
- `BoundStatementList boundBody`
- `BoundStatementList? initializers`

`MethodCompiler` invokes it after Roslyn has produced the bound method body and processed initializers, but before later compilation stages continue.

Intent:

- Give NScript access to Roslyn's post-binding representation.
- Allow external analysis or transformation logic to inspect methods at the semantic/bound-tree stage.
- Keep the hook opt-in so there is no extra work when no callback is registered.

### 2. Pre-compilation hook in `CommonCompiler`

File:
- `src/Compilers/Core/Portable/CommandLine/CommonCompiler.cs`

The branch adds:

- `protected virtual void OnBeforeCompilation(Compilation compilation)`

`CompileAndEmit` now calls this hook immediately before the emit pipeline starts.

Intent:

- Let a custom compiler subclass run setup or analysis before compilation output is produced.
- Provide a cleaner extension point than patching the emit path directly.

### 3. Internal access for NScript

File:
- `src/Compilers/CSharp/Portable/Microsoft.CodeAnalysis.CSharp.csproj`

The branch adds:

- `InternalsVisibleTo Include="NScript.csc.lib"`

Intent:

- Allow `NScript.csc.lib` to use Roslyn internals needed to register and consume the new compilation hooks.

### 4. Private-build and unsigned-build adjustments

Files:
- `eng/targets/Settings.props`
- `src/Compilers/Core/Portable/Microsoft.CodeAnalysis.csproj`
- `src/Compilers/CSharp/Portable/Microsoft.CodeAnalysis.CSharp.csproj`
- `src/Tools/Source/CompilerGeneratorTools/Source/CSharpSyntaxGenerator/CSharpSyntaxGenerator.csproj`

The branch disables assembly signing and clears public keys in the affected projects. It also comments out the internals-visible-to key block in `eng/targets/Settings.props`.

Intent:

- Make local/private Roslyn builds succeed without the normal signing infrastructure.
- Remove strong-name friction when producing custom binaries for NScript consumption.
- Keep the custom fork easy to build outside the standard internal signing environment.

The generator project also changes:

- `ExcludeFromSourceOnlyBuild` -> `ExcludeFromSourceBuild`

This appears to be a build-compatibility fix needed for the private/custom build setup.

## Change summary

The branch is not trying to redesign Roslyn.
It makes a narrow set of changes so NScript can:

- get access to bound method bodies,
- run logic before compilation/emit,
- consume Roslyn internals from `NScript.csc.lib`, and
- build the forked compiler without the usual signing requirements.

## Related branch artifact

The repository already contains `PHYSHI-UPDATED.md`, which is a broader usage-oriented document for this fork.
This file is the concise maintenance note explaining the purpose of the code changes themselves.