#!/bin/bash

# Forsaken Power Overhaul - Build and Deploy Script
# This script builds the mod in Release mode and deploys it to the Valheim plugins folder

set -e  # Exit on any error

# Configuration
PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_NAME="ForsakenPowerOverhaul"
VALHEIM_INSTALL="C:/Program Files (x86)/Steam/steamapps/common/Valheim"
PLUGIN_DIR="$VALHEIM_INSTALL/BepInEx/plugins/ForsakenPowerOverhaul"
DLL_SOURCE="$PROJECT_DIR/bin/Release/$PROJECT_NAME.dll"

echo "=========================================="
echo "Forsaken Power Overhaul - Build & Deploy"
echo "=========================================="

# Step 1: Clean previous build
echo "🧹 Cleaning previous build..."
if [ -d "$PROJECT_DIR/bin" ]; then
    rm -rf "$PROJECT_DIR/bin"
    echo "   Cleaned bin directory"
fi

if [ -d "$PROJECT_DIR/obj" ]; then
    rm -rf "$PROJECT_DIR/obj"
    echo "   Cleaned obj directory"
fi

# Step 2: Build the project in Release mode
echo ""
echo "🔨 Building project in Release mode..."
cd "$PROJECT_DIR"

if ! dotnet build --configuration Release --verbosity minimal; then
    echo "❌ Build failed! Please fix compilation errors and try again."
    exit 1
fi

echo "✅ Build completed successfully!"

# Step 3: Verify DLL exists
echo ""
echo "🔍 Verifying build output..."
if [ ! -f "$DLL_SOURCE" ]; then
    echo "❌ Error: Built DLL not found at: $DLL_SOURCE"
    echo "   Expected file: $PROJECT_NAME.dll"
    exit 1
fi

echo "✅ DLL found: $DLL_SOURCE"

# Step 4: Check if Valheim is running (this could interfere with file copy)
echo ""
echo "🎮 Checking if Valheim is running..."
if tasklist.exe 2>/dev/null | grep -qi "valheim.exe"; then
    echo "⚠️  WARNING: Valheim appears to be running!"
    echo "   This may prevent the DLL from being copied."
    echo "   Consider closing Valheim before deployment."
    echo ""
    read -p "Continue anyway? (y/N): " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        echo "❌ Deployment cancelled by user."
        exit 1
    fi
fi

# Step 5: Create plugin directory if it doesn't exist
echo ""
echo "📁 Preparing plugin directory..."
if [ ! -d "$PLUGIN_DIR" ]; then
    echo "   Creating directory: $PLUGIN_DIR"
    mkdir -p "$PLUGIN_DIR"
fi

# Step 6: Check if target DLL is locked (Valheim using it)
TARGET_DLL="$PLUGIN_DIR/$PROJECT_NAME.dll"
if [ -f "$TARGET_DLL" ]; then
    echo "🔒 Checking if target DLL is locked..."
    # Try to move the existing DLL to a backup location to test if it's locked
    BACKUP_DLL="$PLUGIN_DIR/${PROJECT_NAME}_backup_$(date +%s).dll"
    if ! mv "$TARGET_DLL" "$BACKUP_DLL" 2>/dev/null; then
        echo "❌ ERROR: Cannot replace existing DLL - file appears to be locked!"
        echo "   This usually means Valheim is running and using the mod."
        echo "   Please close Valheim completely and try again."
        echo "   Target file: $TARGET_DLL"
        exit 1
    else
        # Move was successful, clean up the backup
        rm -f "$BACKUP_DLL"
        echo "✅ Target location is writable"
    fi
fi

# Step 7: Copy the DLL to Valheim plugins folder
echo ""
echo "📦 Deploying to Valheim..."
echo "   Source: $DLL_SOURCE"
echo "   Target: $TARGET_DLL"

if ! cp "$DLL_SOURCE" "$TARGET_DLL"; then
    echo "❌ ERROR: Failed to copy DLL to Valheim plugins folder!"
    echo "   This could be due to:"
    echo "   - Valheim is running and has the mod loaded"
    echo "   - Insufficient permissions"
    echo "   - File system issues"
    echo ""
    echo "   Try:"
    echo "   1. Close Valheim completely"
    echo "   2. Run this script as administrator"
    echo "   3. Check that the Valheim path is correct"
    exit 1
fi

# Step 8: Verify deployment
echo ""
echo "✅ Deployment successful!"
echo ""
echo "📋 Deployment Summary:"
echo "   📄 DLL: $PROJECT_NAME.dll"
echo "   📍 Location: $PLUGIN_DIR"
echo "   📏 Size: $(du -h "$TARGET_DLL" | cut -f1)"
echo "   🕒 Modified: $(date -r "$TARGET_DLL" '+%Y-%m-%d %H:%M:%S')"

echo ""
echo "🎉 Forsaken Power Overhaul is ready!"
echo "   You can now start Valheim to test the mod."
echo "=========================================="
