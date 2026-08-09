#!/usr/bin/env bash
#
# Publish -> Distribute -> Ad Hoc for the Habits Android app.
#
# This is the command-line equivalent of the Visual Studio flow:
#   Right-click project -> Publish -> Distribute -> Ad Hoc
#
# It produces a signed .apk that can be sideloaded onto Android devices.
# Docs: https://learn.microsoft.com/en-us/dotnet/maui/android/deployment/publish-ad-hoc?view=net-maui-10.0
#
set -euo pipefail

# ----------------------------------------------------------------------------
# Configuration (override via environment variables if desired)
# ----------------------------------------------------------------------------
PROJECT="${PROJECT:-Habits.csproj}"
CONFIGURATION="${CONFIGURATION:-Release}"
FRAMEWORK="${FRAMEWORK:-net10.0-android}"

# Signing keystore. Defaults to the existing Xamarin/VS-managed keystore.
# Override with the KEYSTORE / KEY_ALIAS environment variables if needed.
# To create a new one:
#   keytool -genkeypair -v -keystore habits.keystore -alias habits \
#           -keyalg RSA -keysize 2048 -validity 10000
KEYSTORE="${KEYSTORE:-/c/Users/Erik/AppData/Local/Xamarin/Mono for Android/Keystore/ErikAndroidKeystore/ErikAndroidKeystore.keystore}"
KEY_ALIAS="${KEY_ALIAS:-ErikAndroidKeystore}"

# Passwords: prefer environment variables; prompt if not set.
KEYSTORE_PASS="${KEYSTORE_PASS:-}"
KEY_PASS="${KEY_PASS:-}"

# Where the signed .apk should be copied after publishing.
OUTPUT_DIR="${OUTPUT_DIR:-./dist}"

# Set BUMP_VERSION=false to skip automatic version incrementing.
BUMP_VERSION="${BUMP_VERSION:-true}"

# ----------------------------------------------------------------------------
# Pre-flight checks
# ----------------------------------------------------------------------------
if [[ ! -f "$PROJECT" ]]; then
	echo "ERROR: Project '$PROJECT' not found. Run this from the repo root." >&2
	exit 1
fi

if [[ ! -f "$KEYSTORE" ]]; then
	echo "ERROR: Keystore '$KEYSTORE' not found." >&2
	echo "Create one with:" >&2
	echo "  keytool -genkeypair -v -keystore \"$KEYSTORE\" -alias \"$KEY_ALIAS\" \\" >&2
	echo "          -keyalg RSA -keysize 2048 -validity 10000" >&2
	exit 1
fi

if [[ -z "$KEYSTORE_PASS" ]]; then
	read -rsp "Keystore password: " KEYSTORE_PASS; echo
fi
if [[ -z "$KEY_PASS" ]]; then
	read -rsp "Key alias password (press Enter to reuse keystore password): " KEY_PASS; echo
	KEY_PASS="${KEY_PASS:-$KEYSTORE_PASS}"
fi

# ----------------------------------------------------------------------------
# Bump version tags (versionCode + versionName)
# ----------------------------------------------------------------------------
if [[ "$BUMP_VERSION" == "true" ]]; then
	# versionCode: <ApplicationVersion> integer -> must increment for Android to
	# treat the install as an upgrade.
	CURRENT_CODE="$(sed -n 's:.*<ApplicationVersion>\(.*\)</ApplicationVersion>.*:\1:p' "$PROJECT")"
	if [[ -z "$CURRENT_CODE" || ! "$CURRENT_CODE" =~ ^[0-9]+$ ]]; then
		echo "ERROR: Could not read a numeric <ApplicationVersion> from '$PROJECT'." >&2
		exit 1
	fi
	NEW_CODE=$((CURRENT_CODE + 1))

	# versionName: <ApplicationDisplayVersion> -> bump the last dotted segment.
	CURRENT_NAME="$(sed -n 's:.*<ApplicationDisplayVersion>\(.*\)</ApplicationDisplayVersion>.*:\1:p' "$PROJECT")"
	if [[ "$CURRENT_NAME" =~ ^(.*[.])?([0-9]+)$ ]]; then
		NEW_NAME="${BASH_REMATCH[1]}$(( ${BASH_REMATCH[2]} + 1 ))"
	else
		NEW_NAME="$CURRENT_NAME"
		echo "WARNING: Could not auto-bump <ApplicationDisplayVersion> ('$CURRENT_NAME'); leaving as-is." >&2
	fi

	sed -i.bak \
		-e "s:<ApplicationVersion>$CURRENT_CODE</ApplicationVersion>:<ApplicationVersion>$NEW_CODE</ApplicationVersion>:" \
		-e "s:<ApplicationDisplayVersion>$CURRENT_NAME</ApplicationDisplayVersion>:<ApplicationDisplayVersion>$NEW_NAME</ApplicationDisplayVersion>:" \
		"$PROJECT"
	rm -f "$PROJECT.bak"

	echo "==> Version bumped: $CURRENT_NAME ($CURRENT_CODE) -> $NEW_NAME ($NEW_CODE)"
fi

# ----------------------------------------------------------------------------
# Publish (signed ad hoc APK)
# ----------------------------------------------------------------------------
echo "==> Publishing $PROJECT ($CONFIGURATION / $FRAMEWORK)..."

dotnet publish "$PROJECT" \
	-f "$FRAMEWORK" \
	-c "$CONFIGURATION" \
	-p:AndroidPackageFormat=apk \
	-p:AndroidKeyStore=true \
	-p:AndroidSigningKeyStore="$KEYSTORE" \
	-p:AndroidSigningKeyAlias="$KEY_ALIAS" \
	-p:AndroidSigningKeyPass="$KEY_PASS" \
	-p:AndroidSigningStorePass="$KEYSTORE_PASS"

# ----------------------------------------------------------------------------
# Collect the signed APK
# ----------------------------------------------------------------------------
PUBLISH_DIR="bin/$CONFIGURATION/$FRAMEWORK/publish"

# The signed, ready-to-distribute APK ends with '-Signed.apk'.
SIGNED_APK="$(find "$PUBLISH_DIR" -name '*-Signed.apk' 2>/dev/null | head -n 1 || true)"
if [[ -z "$SIGNED_APK" ]]; then
	SIGNED_APK="$(find "$PUBLISH_DIR" -name '*.apk' 2>/dev/null | head -n 1 || true)"
fi

if [[ -z "$SIGNED_APK" ]]; then
	echo "ERROR: No .apk found in '$PUBLISH_DIR'." >&2
	exit 1
fi

mkdir -p "$OUTPUT_DIR"
cp "$SIGNED_APK" "$OUTPUT_DIR/"

echo
echo "==> Done. Ad hoc APK ready for distribution:"
echo "    $OUTPUT_DIR/$(basename "$SIGNED_APK")"
echo
echo "Install on a connected device with:"
echo "    adb install -r \"$OUTPUT_DIR/$(basename "$SIGNED_APK")\""
