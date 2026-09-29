#!/usr/bin/env python3
"""
unityx — manage UnityX packages in a Unity project.

Consumer projects keep UnityX as a git submodule (default: <project>/UnityX) and reference the packages they
want from Packages/manifest.json as local `file:` packages, so the code is editable in place and changes can be
committed straight back to UnityX.

Unity does not resolve dependencies between local/git packages on its own, so `add` / `sync` write the full
dependency closure into the manifest.

Consumer commands (run from anywhere inside a Unity project, or pass --project):
  unityx list                        packages, their UnityX dependencies, and themes
  unityx add <package|theme>...      add packages (plus everything they depend on)
  unityx remove <package>...         remove packages (refuses if another installed package needs them)
  unityx sync                        re-add any missing dependencies and fix manifest paths
  unityx status                      installed packages + how far the UnityX checkout is behind origin
  unityx update                      pull the latest UnityX into the submodule, then sync

Maintainer commands (run inside the UnityX repo):
  unityx gen-deps                    regenerate every package.json "dependencies" from the asmdef references
  unityx set-version <x.y.z>         set the (shared) version of every package
"""
import argparse, json, os, re, subprocess, sys
from collections import OrderedDict

UNITYX_ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
PACKAGES_DIR = os.path.join(UNITYX_ROOT, "Packages")
PREFIX = "com.tomkail.unityx."

# asmdef reference -> Unity registry package that provides it (minimum version for Unity 6)
UNITY_PACKAGE_FOR_ASSEMBLY = {
    "UnityEngine.UI": ("com.unity.ugui", "2.0.0"),
    "UnityEditor.UI": ("com.unity.ugui", "2.0.0"),
    "Unity.TextMeshPro": ("com.unity.ugui", "2.0.0"),
    "Unity.TextMeshPro.Editor": ("com.unity.ugui", "2.0.0"),
    "Unity.InputSystem": ("com.unity.inputsystem", "1.7.0"),
}


# ---------------------------------------------------------------------------------------------- package data

def load_packages():
    pkgs = OrderedDict()
    for name in sorted(os.listdir(PACKAGES_DIR)):
        pj = os.path.join(PACKAGES_DIR, name, "package.json")
        if name.startswith(PREFIX) and os.path.isfile(pj):
            with open(pj) as f:
                pkgs[name] = json.load(f, object_pairs_hook=OrderedDict)
    return pkgs


def load_themes():
    with open(os.path.join(UNITYX_ROOT, "Tools", "themes.json")) as f:
        return json.load(f, object_pairs_hook=OrderedDict)


def short(name):
    return name[len(PREFIX):] if name.startswith(PREFIX) else name


def full(name):
    return name if name.startswith(PREFIX) else PREFIX + name


def unityx_deps(pkgs, name):
    return [d for d in pkgs[name].get("dependencies", {}) if d.startswith(PREFIX)]


def closure(pkgs, names):
    out, stack = [], list(names)
    while stack:
        n = stack.pop()
        if n in out:
            continue
        if n not in pkgs:
            sys.exit(f"unityx: unknown package '{short(n)}' (see `unityx list`)")
        out.append(n)
        stack.extend(unityx_deps(pkgs, n))
    return out


def expand(pkgs, args):
    themes = load_themes()
    names = []
    for a in args:
        if a in themes:
            names += [full(p) for p in themes[a]["packages"]]
        elif a == "all":
            names += list(pkgs)
        else:
            names.append(full(a))
    return names


# ---------------------------------------------------------------------------------------------- project

def find_project(start):
    d = os.path.abspath(start)
    while True:
        if os.path.isfile(os.path.join(d, "Packages", "manifest.json")) and os.path.isdir(os.path.join(d, "Assets")):
            return d
        parent = os.path.dirname(d)
        if parent == d:
            sys.exit("unityx: not inside a Unity project (no Packages/manifest.json found); pass --project")
        d = parent


def read_manifest(project):
    with open(os.path.join(project, "Packages", "manifest.json")) as f:
        return json.load(f, object_pairs_hook=OrderedDict)


def write_manifest(project, manifest):
    deps = manifest["dependencies"]
    manifest["dependencies"] = OrderedDict(sorted(deps.items()))
    with open(os.path.join(project, "Packages", "manifest.json"), "w") as f:
        json.dump(manifest, f, indent=2)
        f.write("\n")


def manifest_ref(project, name):
    rel = os.path.relpath(os.path.join(PACKAGES_DIR, name), os.path.join(project, "Packages"))
    return "file:" + rel.replace(os.sep, "/")


def installed(manifest):
    return [n for n in manifest["dependencies"] if n.startswith(PREFIX)]


def embedded_here(project):
    # Inside the UnityX repo itself the packages are embedded (they live in its own Packages/ folder).
    return os.path.realpath(project) == os.path.realpath(UNITYX_ROOT)


# ---------------------------------------------------------------------------------------------- commands

def cmd_list(a):
    pkgs = load_packages()
    width = max(len(short(n)) for n in pkgs)
    print("Packages:")
    for n, p in pkgs.items():
        deps = ", ".join(short(d) for d in unityx_deps(pkgs, n))
        print(f"  {short(n):<{width}}  {p.get('description', '')}" + (f"  [needs: {deps}]" if deps else ""))
    print("\nThemes (add a whole group at once):")
    for t, v in load_themes().items():
        print(f"  {t:<{width}}  {', '.join(v['packages'])}")


def cmd_add(a):
    pkgs = load_packages()
    project = find_project(a.project)
    if embedded_here(project):
        sys.exit("unityx: this is the UnityX repo — its packages are already embedded")
    manifest = read_manifest(project)
    wanted = closure(pkgs, expand(pkgs, a.packages))
    for n in wanted:
        manifest["dependencies"][n] = manifest_ref(project, n)
    write_manifest(project, manifest)
    print("Added: " + ", ".join(short(n) for n in wanted))


def cmd_remove(a):
    pkgs = load_packages()
    project = find_project(a.project)
    manifest = read_manifest(project)
    removing = {full(n) for n in a.packages}
    for n in installed(manifest):
        if n in removing:
            continue
        needed = removing.intersection(unityx_deps(pkgs, n)) if n in pkgs else set()
        if needed:
            sys.exit(f"unityx: {short(n)} still needs {', '.join(short(x) for x in needed)}; remove it too or keep them")
    for n in removing:
        manifest["dependencies"].pop(n, None)
    write_manifest(project, manifest)
    print("Removed: " + ", ".join(short(n) for n in sorted(removing)))


def cmd_sync(a):
    pkgs = load_packages()
    project = find_project(a.project)
    manifest = read_manifest(project)
    current = installed(manifest)
    unknown = [n for n in current if n not in pkgs]
    for n in unknown:
        print(f"warning: {short(n)} is in the manifest but no longer exists in UnityX — remove it with `unityx remove {short(n)}`")
    wanted = closure(pkgs, [n for n in current if n in pkgs])
    added = [n for n in wanted if n not in current]
    for n in wanted:
        manifest["dependencies"][n] = manifest_ref(project, n)
    write_manifest(project, manifest)
    print("In sync." + (" Added missing dependencies: " + ", ".join(short(n) for n in added) if added else ""))


def git(*args, check=True):
    return subprocess.run(["git", "-C", UNITYX_ROOT, *args], capture_output=True, text=True, check=check).stdout.strip()


def cmd_status(a):
    project = find_project(a.project)
    manifest = read_manifest(project)
    names = installed(manifest)
    print(f"UnityX checkout: {UNITYX_ROOT}")
    print(f"Installed ({len(names)}): " + (", ".join(short(n) for n in names) or "none"))
    git("fetch", "--quiet", check=False)
    head = git("rev-parse", "--short", "HEAD")
    behind = git("rev-list", "--count", "HEAD..origin/master", check=False) or "?"
    ahead = git("rev-list", "--count", "origin/master..HEAD", check=False) or "?"
    dirty = git("status", "--porcelain")
    print(f"UnityX at {head}: {behind} commit(s) behind origin/master, {ahead} ahead"
          + (" — has uncommitted changes" if dirty else ""))


def cmd_update(a):
    project = find_project(a.project)
    if git("status", "--porcelain"):
        sys.exit("unityx: UnityX has uncommitted changes — commit/push (or stash) them first")
    branch = git("rev-parse", "--abbrev-ref", "HEAD")
    if branch == "HEAD":  # detached, the normal state for a submodule
        git("checkout", "--quiet", "master")
    subprocess.run(["git", "-C", UNITYX_ROOT, "pull", "--ff-only"], check=True)
    print(f"UnityX now at {git('rev-parse', '--short', 'HEAD')}")
    cmd_sync(a)
    print("Commit the submodule bump in your project:  git add UnityX Packages/manifest.json")


def cmd_gen_deps(a):
    pkgs = load_packages()
    asm_to_pkg, pkg_refs = {}, {n: set() for n in pkgs}
    for n in pkgs:
        for dirpath, _, files in os.walk(os.path.join(PACKAGES_DIR, n)):
            for f in files:
                if f.endswith(".asmdef"):
                    with open(os.path.join(dirpath, f)) as fh:
                        d = json.load(fh)
                    asm_to_pkg[d["name"]] = n
                    if "UNITY_INCLUDE_TESTS" in d.get("defineConstraints", []):
                        continue  # test assemblies don't make runtime dependencies
                    pkg_refs[n].update(d.get("references", []))
    version_of = {n: p["version"] for n, p in pkgs.items()}
    for n, p in pkgs.items():
        deps = {}
        for r in sorted(pkg_refs[n]):
            if r.startswith("GUID:"):
                print(f"warning: {short(n)} references an assembly by GUID ({r}); use names so deps can be derived")
            elif r in asm_to_pkg and asm_to_pkg[r] != n:
                deps[asm_to_pkg[r]] = version_of[asm_to_pkg[r]]
            elif r in UNITY_PACKAGE_FOR_ASSEMBLY:
                pkg, ver = UNITY_PACKAGE_FOR_ASSEMBLY[r]
                deps[pkg] = ver
            elif r not in asm_to_pkg:
                print(f"warning: {short(n)} references unknown assembly {r}")
        p["dependencies"] = OrderedDict(sorted(deps.items()))
        with open(os.path.join(PACKAGES_DIR, n, "package.json"), "w") as f:
            json.dump(p, f, indent=2)
            f.write("\n")
    # cycle check
    for n in pkgs:
        if n in closure(load_packages(), unityx_deps(load_packages(), n)):
            print(f"warning: dependency cycle through {short(n)}")
    print(f"Regenerated dependencies for {len(pkgs)} packages.")


def cmd_set_version(a):
    if not re.fullmatch(r"\d+\.\d+\.\d+(-[0-9A-Za-z.]+)?", a.version):
        sys.exit("unityx: version must be x.y.z")
    for n, p in load_packages().items():
        p["version"] = a.version
        with open(os.path.join(PACKAGES_DIR, n, "package.json"), "w") as f:
            json.dump(p, f, indent=2)
            f.write("\n")
    cmd_gen_deps(a)


def main():
    ap = argparse.ArgumentParser(prog="unityx", description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--project", default=os.getcwd(), help="Unity project path (default: current directory)")
    sub = ap.add_subparsers(dest="cmd", required=True)
    sub.add_parser("list").set_defaults(fn=cmd_list)
    p = sub.add_parser("add"); p.add_argument("packages", nargs="+"); p.set_defaults(fn=cmd_add)
    p = sub.add_parser("remove"); p.add_argument("packages", nargs="+"); p.set_defaults(fn=cmd_remove)
    sub.add_parser("sync").set_defaults(fn=cmd_sync)
    sub.add_parser("status").set_defaults(fn=cmd_status)
    sub.add_parser("update").set_defaults(fn=cmd_update)
    sub.add_parser("gen-deps").set_defaults(fn=cmd_gen_deps)
    p = sub.add_parser("set-version"); p.add_argument("version"); p.set_defaults(fn=cmd_set_version)
    a = ap.parse_args()
    a.fn(a)


if __name__ == "__main__":
    main()
