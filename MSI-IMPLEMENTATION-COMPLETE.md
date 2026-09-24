# 🎉 AdminTrayTool MSI Build System - Implementation Complete

## ✅ Implementation Overview

The AdminTrayTool project now has a complete MSI build and release workflow supporting both **local MSI builds** and **automated GitHub Actions releases**.

The system is designed to make development builds, testing, version management, and official releases consistent and repeatable.

### Build System Components

| File                                | Purpose                                                         |
| ----------------------------------- | --------------------------------------------------------------- |
| **Build-MSI.ps1**                   | Direct PowerShell script for building an MSI locally            |
| **Build-Helper.ps1**                | Interactive menu for building, version management, and releases |
| **MSI-BUILD-GUIDE.md**              | Detailed technical documentation                                |
| **BUILD-QUICK-START.md**            | User-friendly build and release instructions                    |
| **BUILD-QUICK-REFERENCE.txt**       | Quick command/reference sheet                                   |
| **.github/workflows/build-msi.yml** | Automated GitHub Actions MSI and release workflow               |
| **VERSION.txt**                     | Current application version                                     |

---

## 🚀 What Is Implemented

### Automated Local MSI Build

`Build-MSI.ps1` provides a direct way to create an MSI without relying on GitHub Actions.

The build process:

1. Publishes the .NET application.
2. Copies GAM7 when available.
3. Separates GAM7 from the normal application harvest.
4. Generates WiX components.
5. Generates the GAM7 WiX components separately.
6. Compiles the WiX sources.
7. Links the final MSI.
8. Verifies that the MSI was created successfully.

The local build uses **WiX Toolset 3.11.1.2318**.

Example:

```powershell
.\Build-MSI.ps1
```

A specific version can also be supplied:

```powershell
.\Build-MSI.ps1 -ProductVersion "<version>"
```

Additional parameters are available for configuration and output location:

```powershell
.\Build-MSI.ps1 `
    -ProductVersion "<version>" `
    -Configuration "Release" `
    -OutputPath ".\msi-output"
```

---

## 🧰 Interactive Build Helper

`Build-Helper.ps1` provides the preferred interactive workflow for everyday development.

### Current Menu

```text
1. Check Prerequisites
2. Build MSI (Current Version)
3. Update Version Number
4. Open MSI Output Folder
5. Commit, Merge to Main, Push & Trigger GitHub Release
6. Exit
```

### Option 1 — Check Prerequisites

Checks for the tools required by the local build process, including:

* .NET SDK
* WiX Toolset
* WiX `heat.exe`
* WiX `candle.exe`
* WiX `light.exe`
* GitHub CLI
* GitHub CLI authentication
* `installer\Product.wxs`

### Option 2 — Build MSI

Builds the MSI using the version currently stored in `VERSION.txt`.

The resulting MSI is placed in:

```text
msi-output\
```

### Option 3 — Update Version Number

The helper can update the application version using:

* Patch
* Minor
* Major
* Custom version

The version is stored in:

```text
VERSION.txt
```

The project file version properties are also updated where applicable.

### Option 4 — Open MSI Output Folder

Opens the local:

```text
msi-output\
```

folder in Windows Explorer.

### Option 5 — Release Workflow

The helper can guide the user through:

1. Checking the current Git status.
2. Committing changes.
3. Merging a feature branch into `main`.
4. Pushing the changes.
5. Creating a version tag.
6. Pushing the tag.

Pushing the version tag starts the GitHub Actions release workflow.

---

# ☁️ GitHub Actions Release System

AdminTrayTool now has an automated GitHub Actions workflow for builds and releases.

The workflow runs on:

* Pushes to `main`
* Version tags matching `v*`
* Pull requests targeting `main`
* Manual workflow dispatches

The workflow runs on a Windows GitHub Actions runner.

---

## 📦 Automated Build Process

The GitHub Actions workflow performs the following major steps:

### 1. Checkout

Retrieves the current repository source code.

### 2. Install .NET

Uses the .NET 10 SDK.

### 3. Determine Version

For release builds, the version is taken from the Git tag.

For example:

```text
v<version>
```

produces:

```text
<version>
```

### 4. Build

The application is built in Release configuration with the appropriate version information.

### 5. Publish

The application is published as:

```text
win-x64
self-contained
```

This allows the published application to run without requiring a separate .NET runtime installation.

### 6. Include Configuration

The workflow includes the repository's `config.json` in the published application when it is present.

**Do not place passwords, OAuth secrets, API keys, or other sensitive information in a tracked `config.json`.**

### 7. Verify GAM7

The CI build expects:

```text
GAM7\
└── gam.exe
```

The workflow verifies that GAM7 is present before creating the release package.

### 8. Generate WiX Components

The normal application files are harvested separately from GAM7.

GAM7 receives its own WiX component definition so that it can be included correctly in the MSI without creating duplicate component/file entries.

### 9. Build MSI

The final MSI is generated using WiX Toolset.

The release MSI follows this naming convention:

```text
AdminTrayTool-<version>.msi
```

For example:

```text
AdminTrayTool-<version>.msi
```

### 10. Create Portable ZIP

The published application is also packaged as:

```text
AdminTrayTool-portable.zip
```

### 11. Upload Build Artifacts

The workflow makes the following artifacts available from the GitHub Actions run:

```text
AdminTrayTool-MSI
AdminTrayTool-portable
AdminTrayTool-publish
```

### 12. Create GitHub Release

When a version tag is pushed, GitHub Actions automatically creates the GitHub Release and attaches:

```text
AdminTrayTool-<version>.msi
AdminTrayTool-portable.zip
```

GitHub can also generate release notes automatically.

---

# 📁 Build Output

## Local Build

A typical local build produces:

```text
AdminTrayTool\
├── msi-output\
│   ├── AdminTrayTool-<version>.msi
│   └── AdminTrayTool-<version>.wixpdb
│
├── publish\
│   ├── AdminTrayTool.exe
│   ├── AdminTrayTool.dll
│   ├── config.json
│   └── GAM7\
│       └── gam.exe
│
├── wix-obj\
│   └── [generated WiX files]
│
└── AppSourceDir\
    └── [temporary harvested application files]
```

The generated working directories are recreated as part of the build process.

The important local output is:

```text
msi-output\AdminTrayTool-<version>.msi
```

---

# 🔐 GAM7 Handling

GAM7 is handled separately from the main application files.

This is important because GAM7 contains its own executable and supporting files that need to be included in the MSI without being duplicated by the normal application file harvest.

### Local Build

The local build can continue without GAM7 when the `GAM7` directory is unavailable.

A warning is displayed when GAM7 is missing.

### GitHub Actions

The CI workflow expects GAM7 to be present and verifies:

```text
GAM7\gam.exe
```

before continuing with the MSI build.

---

# 🔢 Version Management

Version information is managed through:

```text
VERSION.txt
```

For example:

```text
<version>
```

The build helper can update the version and synchronise the relevant project version properties.

Release tags use the following format:

```text
v<version>
```

The tag determines the version used by the GitHub Actions release build.

This provides a consistent relationship between:

```text
VERSION.txt
        ↓
Application version
        ↓
Release tag
        ↓
MSI version
        ↓
GitHub Release
```

---

# 🧪 Testing Workflow

The recommended development workflow is:

### Local Development

```text
Make changes
     ↓
Update version if required
     ↓
Build MSI locally
     ↓
Install/test on VM
     ↓
Commit changes
```

### Official Release

```text
Changes tested
     ↓
Merge into main
     ↓
Create version tag
     ↓
Push tag
     ↓
GitHub Actions builds MSI
     ↓
GitHub Release created
     ↓
MSI + Portable ZIP published
```

This separates **local testing** from the **official release process**.

---

# 📚 Documentation

The build system documentation is divided into separate documents so each file has a specific purpose.

| Document                           | Purpose                                            |
| ---------------------------------- | -------------------------------------------------- |
| **MSI-IMPLEMENTATION-COMPLETE.md** | High-level overview of the completed MSI system    |
| **MSI-BUILD-GUIDE.md**             | Detailed technical build and troubleshooting guide |
| **BUILD-QUICK-START.md**           | Simple instructions for building and releasing     |
| **BUILD-QUICK-REFERENCE.txt**      | Short command/reference sheet                      |

The implementation document should remain a summary rather than duplicating all of the technical instructions from the build guide.

---

# ✅ Current Capabilities

The AdminTrayTool build system now supports:

* ✅ Local MSI generation
* ✅ WiX Toolset integration
* ✅ .NET 10 Release builds
* ✅ Self-contained win-x64 CI publishing
* ✅ GAM7 packaging
* ✅ Separate GAM7 WiX components
* ✅ Version management
* ✅ Interactive build helper
* ✅ Prerequisite checking
* ✅ MSI output verification
* ✅ Portable ZIP generation
* ✅ GitHub Actions CI
* ✅ Automated GitHub Releases
* ✅ Version-tagged releases
* ✅ MSI and portable release downloads

---

# 🔄 Future Improvements

The current system provides the core build and release workflow. Possible future improvements include:

* Code signing for the MSI and application binaries
* Automated MSI installation testing
* Automated upgrade/uninstall testing
* More extensive release validation
* Improved version synchronisation between `VERSION.txt`, the project file, and CI
* Additional installer configuration
* Multi-language installer support

These can be added independently without changing the basic local build and tag-driven release workflow.

---

# 🎯 Implementation Status

The MSI build system is **implemented and operational** for both local development builds and GitHub Actions releases.

### Local Build

```text
Build-MSI.ps1
      ↓
.NET Publish
      ↓
GAM7 Packaging
      ↓
WiX Harvest
      ↓
WiX Compile
      ↓
WiX Link
      ↓
MSI
```

### Release Build

```text
Git Commit
      ↓
Merge to main
      ↓
Version Tag
      ↓
GitHub Actions
      ↓
.NET Build & Publish
      ↓
GAM7 Validation
      ↓
WiX MSI Build
      ↓
Portable ZIP
      ↓
GitHub Release
```

---

## 🎉 Result

AdminTrayTool now has a repeatable build and release process that can be used throughout future development.

For normal development, use:

```powershell
.\Build-Helper.ps1
```

For a direct MSI build:

```powershell
.\Build-MSI.ps1
```

For an official release, use the version/tag workflow and allow GitHub Actions to create the release packages.

**Current implementation status: ✅ Complete**

**MSI technology:** WiX Toolset 3.11.1.2318
**Application:** .NET 10 / Windows
**Release platform:** GitHub Actions
**Installer format:** Windows MSI
**Portable format:** ZIP
