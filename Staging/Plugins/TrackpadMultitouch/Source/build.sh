#!/bin/bash
# Builds TrackpadMultitouch.bundle (arm64) next to this script's parent folder.
#
# Unity never unloads a native plugin, so if the Editor is open and has already loaded
# the old bundle, this rebuild lands on disk but the OLD code stays resident until you
# restart the Editor. Rebuild native as rarely as possible.
#
# Ad-hoc signs + strips quarantine so Gatekeeper on macOS 26 won't refuse to load it
# ("bundle is damaged"). For distributable player builds, re-sign with your Dev ID team
# in an Xcode post-process step instead of ad-hoc.

set -euo pipefail

SRC_DIR="$(cd "$(dirname "$0")" && pwd)"
PLUGIN_DIR="$(cd "$SRC_DIR/.." && pwd)"
NAME="TrackpadMultitouch"
BUNDLE="$PLUGIN_DIR/$NAME.bundle"
MACOS_DIR="$BUNDLE/Contents/MacOS"

echo "Building $NAME.bundle (arm64)…"
rm -rf "$BUNDLE"
mkdir -p "$MACOS_DIR"

clang -bundle -arch arm64 \
    -framework CoreFoundation \
    -mmacosx-version-min=11.0 \
    -O2 \
    "$SRC_DIR/$NAME.m" \
    -o "$MACOS_DIR/$NAME"

cat > "$BUNDLE/Contents/Info.plist" <<PLIST
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleExecutable</key><string>$NAME</string>
    <key>CFBundleIdentifier</key><string>com.unityx.trackpadmultitouch</string>
    <key>CFBundleName</key><string>$NAME</string>
    <key>CFBundlePackageType</key><string>BNDL</string>
    <key>CFBundleVersion</key><string>1.0</string>
</dict>
</plist>
PLIST

# Ad-hoc sign, then strip the quarantine xattr Gatekeeper adds.
codesign --force --sign - "$BUNDLE"
xattr -dr com.apple.quarantine "$BUNDLE" 2>/dev/null || true

echo "Done: $BUNDLE"
