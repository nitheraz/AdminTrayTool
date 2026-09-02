# 🚀 AdminTrayTool MSI Build - Quick Start Guide

## What Just Got Created

You now have a complete stand alone MSI build system for AdminTrayTool v1.1.4 with WiX v3.11.1.2318.

### Files Created:

1. **`Build-MSI.ps1`** - Main build script (fully automated)
2. **`Build-Helper.ps1`** - Interactive menu (easiest to use)
3. **`Check-Prerequisites.bat`** - Quick prerequisites checker
4. **`MSI-BUILD-GUIDE.md`** - Detailed build documentation
5. **`BUILD-QUICK-START.md`** - This file

## 🎯 Quickest Path: 5 Minutes to MSI

### Step 1: Check Prerequisites
```powershell
# Run this first
.\Check-Prerequisites.bat
```

**If any checks fail:**
- Download WiX: https://github.com/wixtoolset/wix3/releases
- Download .NET SDK: https://dotnet.microsoft.com/download

### Step 2: Build MSI (Interactive)
```powershell
# Easiest way - interactive menu
.\Build-Helper.ps1

# Then select option [2] Build MSI for Testing
```

**OR Run directly:**
```powershell
.\Build-MSI.ps1
```

### Step 3: Copy to VM
- MSI location: `.\msi-output\AdminTrayTool-1.1.4.msi`
- Copy to your VM via RDP/USB/network

### Step 4: Install on VM
```powershell
# On VM
msiexec /i AdminTrayTool-1.1.4.msi
```

### Step 5: Test
- Launch from Start Menu
- Test OAuth setup with your OAuth configuration
- Test Chromebook management features

## 📋 Common Scenarios

### Scenario 1: First Time Build
```powershell
cd C:\Users\mdejesus\source\repos\AdminTrayTool
.\Build-Helper.ps1
# Select option 1, then 2
```

### Scenario 2: Build with Different Version
```powershell
.\Build-Helper.ps1
# Select option 3
# Enter version number (e.g., 1.1.5)
```

### Scenario 3: Rebuild Same Version
```powershell
.\Build-MSI.ps1
```

### Scenario 4: Test on Multiple VMs
```powershell
# Build once
.\Build-MSI.ps1

# Then copy from msi-output to as many VMs as needed
# File: AdminTrayTool-1.1.4.msi
```

## 🎨 Interactive Menu (Recommended)

Use this for the best user experience:

```powershell
.\Build-Helper.ps1
```

Menu options:
1. **Check Prerequisites** - Verify WiX and .NET are installed
2. **Build MSI for Testing** - Build with default version 1.1.4
3. **Build MSI with Custom Version** - Build 1.1.5, 2.0.0, etc.
4. **Open MSI Output Folder** - Browse finished MSI files
5. **Show Build Guide** - View detailed documentation
6. **Commit & Push to GitHub** - Automatic Git workflow
7. **Exit** - Quit menu

## 📦 Normal Direct Build

If you prefer command line without menu:

```powershell
# Default (v1.1.4)
.\Build-MSI.ps1

# Custom version
.\Build-MSI.ps1 -ProductVersion "1.1.5"

# Release or Debug
.\Build-MSI.ps1 -Configuration "Release"

# Custom output path
.\Build-MSI.ps1 -OutputPath "C:\builds\msi"
```

## 📂 Output Files

After build, you'll find:
```
.\msi-output\
├── AdminTrayTool-1.1.4.msi       ← Use this to install
├── AdminTrayTool-1.1.4.wixpdb    ← Debug info
└── [other WiX artifacts]

.\publish\                         ← Published .NET app
├── AdminTrayTool.exe
├── AdminTrayTool.dll
├── config.json
└── GAM7\                         ← GAM tools

.\wix-obj\                        ← WiX compilation files
└── [component .wixobj files]    ← Can be deleted
```

## 🧪 Testing Workflow

### On Your Development Machine

```powershell
# 1. Make code changes (you already did!)
# 2. Build MSI
.\Build-MSI.ps1

# 3. Test locally (if you have VS 2026 debugger ready)
# Or copy to VM...
```

### On Test VM

```powershell
# 1. Copy AdminTrayTool-1.1.4.msi to VM

# 2. Install
msiexec /i AdminTrayTool-1.1.4.msi

# 3. Test OAuth Setup
# Click Start Menu > Admin Tray Tool
# Should see your OAuth validation improvements

# 4. Test Features
# - Chromebook Management
# - Device Lookup
# - Configuration Editor

# 5. Uninstall (if needed)
msiexec /x AdminTrayTool-1.1.4.msi
```

## 🐛 Troubleshooting

### Issue: "heat.exe not found"
**Solution:** Install WiX Toolset v3.11.1.2318
- Download: https://github.com/wixtoolset/wix3/releases
- Install to default location: `C:\Program Files (x86)\WiX Toolset v3.11\`

### Issue: "dotnet not found"
**Solution:** Install .NET 10 SDK
- Download: https://dotnet.microsoft.com/download
- Verify: `dotnet --version` in PowerShell

### Issue: Build fails with permission error
**Solution:** Run PowerShell as Administrator
```powershell
# Right-click PowerShell > Run as Administrator
```

### Issue: MSI file is too large (>500MB)
**Solution:** This is normal if GAM7 is included (it's ~300MB+)
- Remove GAM7 folder before build if you want smaller MSI
- Or distribute GAM7 separately

### Issue: Can't install on VM (access denied)
**Solution:** Ensure you have admin rights on VM
```powershell
# On VM, run as Administrator
msiexec /i AdminTrayTool-1.1.4.msi
```

## ✅ Verification Checklist

After successful MSI build:
- [ ] MSI file exists in `.\msi-output\`
- [ ] File size is ~10-50MB (or ~400MB if includes GAM7)
- [ ] Can copy MSI to VM successfully
- [ ] Installation starts on VM (UI appears)
- [ ] Application runs after installation
- [ ] Start Menu shortcut created
- [ ] Can uninstall cleanly

## 🔄 Next Steps After Testing

Once you've tested on VM and everything works:

### 1. Commit to GitHub
```powershell
.\Build-Helper.ps1
# Select option [6] Commit & Push to GitHub
```

Or manually:
```powershell
git add .
git commit -m "Add MSI build support - v1.1.4

- OAuth validation improvements
- Multi-location GAM config detection
- WiX v3 standalone build script
- Automated MSI generation"
git push origin gam-detection
```

### 2. Create GitHub Release
- Go to: https://github.com/nitheraz/AdminTrayTool/releases
- Click "Create a new release"
- Tag: `v1.1.4`
- Title: `Release v1.1.4 - OAuth Improvements`
- Upload: `AdminTrayTool-1.1.4.msi`
- Publish

### 3. Set Up CI/CD (Optional)
The existing `.github/workflows/build-msi.yml` can create releases automatically.

## 📖 Detailed Documentation

For complete information, see:
- **`MSI-BUILD-GUIDE.md`** - Full technical guide
- **`Build-MSI.ps1`** - Inline script comments
- **`Build-Helper.ps1`** - Menu descriptions

## 🎓 What This Enables

✓ Standalone MSI for easy distribution
✓ No build pipeline needed (works offline)
✓ Test before committing to GitHub
✓ Version management (1.1.4, 1.1.5, etc.)
✓ Clean uninstall experience
✓ Start Menu integration
✓ Silent install option possible
✓ All-in-one deployment package

## Summary

You're set up to build production-ready MSI installers!

**Next action:** Run `.\Build-Helper.ps1` and select option 2 to build your first test MSI.

Good luck! 🚀
