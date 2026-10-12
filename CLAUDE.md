# CLAUDE.md

## Checking the Unity editor

The editor is often open while we work. To check whether it's running, list process **names only**:

```bash
pgrep -l Unity
```

Don't use `ps aux`, `ps -ef`, `pgrep -f`/`-lf` or anything else that prints full command lines. Unity Hub launches the editor with the account's sign-in token on its command line, so printing full arguments puts the token into the transcript.

## Compiling without the editor

Use the C# compiler bundled with the editor version in `ProjectSettings/ProjectVersion.txt`, if it's installed. The `.csproj` files at the repo root (generated when the editor opens the project) list every source file, reference and define, so build a response file from one and pass it to `csc`:

- Compiler: `/Applications/Unity/Hub/Editor/<version>/Unity.app/Contents/Resources/Scripting/DotNetSdk/dotnet <same folder>/DotNetSdk/sdk/<sdk version>/Roslyn/bincore/csc.dll @file.rsp`
- From the csproj, take `<Compile Include>` (sources), `<HintPath>` (references), `<ProjectReference>` (resolve to `Library/ScriptAssemblies/<name>.dll`) and `<DefineConstants>`. Add `-target:library -nostdlib+ -noconfig -unsafe+`.
- Write the output to the scratchpad, never into `Library/`.
