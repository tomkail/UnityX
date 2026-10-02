# UnityX
Lots of helpers I use for working with Unity, split into small UPM packages so a project can take only what it needs.

Requires Unity 6 (6000.0+). The pre-package layout (a single `Assets/UnityX` folder) is preserved at the `legacy` tag.

## Packages
Every package lives in `Packages/com.tomkail.unityx.<name>/`. Run `Tools/unityx list` for the current list, descriptions
and dependencies. Dependencies between packages are declared in each `package.json` (generated from the asmdef
references; see *Maintaining* below).

`legacy` holds code that hasn't been modularised yet. It's unsorted, depends on most of the other packages, and things
will move out of it over time — prefer the focused packages where possible. When something moves out of legacy,
projects that relied on it need the new package: run `unityx scan` after `unityx update`.

Themes are presets that add a group of packages at once (`Tools/themes.json`): `motion`, `camera-all`, `geometry-all`,
`ui`, `input`, `editor-tools`.

## Using UnityX in a project
UnityX is added as a **git submodule** and packages are referenced as local `file:` packages. The code stays editable
inside the project, and improvements can be committed straight back to UnityX.

```bash
# once per project
git submodule add git@github.com:tomkail/UnityX.git UnityX
UnityX/Tools/unityx add core springs timers      # packages, themes, or "all"
git add .gitmodules UnityX Packages/manifest.json
git commit -m "Add UnityX"
```

`add` writes each package and everything it depends on into `Packages/manifest.json`, e.g.
`"com.tomkail.unityx.springs": "file:../UnityX/Packages/com.tomkail.unityx.springs"`.
(Unity doesn't resolve dependencies between local packages itself, so the script does it.)

Cloning a project that uses UnityX: `git clone --recursive …`, or `git submodule update --init` after cloning.

### Commands
| Command | What it does |
|---|---|
| `unityx list` | Packages, their dependencies, and themes |
| `unityx add <pkg\|theme>…` | Add packages plus their dependencies |
| `unityx remove <pkg>…` | Remove packages (refuses if an installed package still needs them) |
| `unityx sync` | Re-add missing dependencies, fix paths (run after pulling UnityX changes that add dependencies) |
| `unityx status` | Installed packages; how far the UnityX checkout is behind/ahead of `origin/master` |
| `unityx update` | Pull the latest UnityX into the submodule, then `sync` |
| `unityx scan` | Packages the project uses but hasn't installed, and installed packages nothing uses (asset GUIDs and package namespaces such as `using UnityX…` are certain; bare type names are hints) |

Run them from inside the Unity project (or pass `--project <path>`).

### Staying up to date
```bash
UnityX/Tools/unityx update
git add UnityX Packages/manifest.json && git commit -m "Update UnityX"
```
Each project pins an exact UnityX commit, so nothing changes under a project until it chooses to update.

### Making improvements from a project
Edit the package files in place (they're ordinary files in `UnityX/Packages/...`). Then:
```bash
cd UnityX
git checkout master          # submodules sit on a detached HEAD by default
git pull --ff-only           # get anything newer first
git commit -am "Springs: fix overshoot"
git push
cd ..
git add UnityX && git commit -m "Update UnityX"
```
Because other projects pin their own commit, a push never breaks them; they pick it up on their next `unityx update`.

### Read-only alternative (no submodule)
For a project that will never edit UnityX, packages can be installed straight from git (Package Manager →
*Install package from git URL*):
```
https://github.com/tomkail/UnityX.git?path=Packages/com.tomkail.unityx.springs#v2.0.0
```
Git packages are immutable in `Library/PackageCache`, and their dependencies must be added the same way.

## Maintaining
The repo is itself a Unity project: open it in Unity 6 and every package is embedded and compiled together.

- **Adding a module:** create `Packages/com.tomkail.unityx.<name>/` with a `package.json` (copy one) and the module
  folder with its asmdef(s). Reference other UnityX assemblies **by name**, not GUID.
- **After changing asmdef references:** `Tools/unityx gen-deps` rewrites every `package.json` `dependencies`.
- **Before pushing:** `Tools/unityx check` (with this project closed in Unity) compiles every package in the Editor
  *and* for a player build, which catches editor-only API used outside `#if UNITY_EDITOR`.
- **Releasing:** all packages share one version. `Tools/unityx set-version 2.1.0`, commit, `git tag v2.1.0`, push tags.
