# MSI Build Guide

This guide explains how to build, test, and release **AdminTrayTool** using the project's local PowerShell scripts and GitHub Actions workflow.

## Build and Release Overview

AdminTrayTool has two build paths:

### Local build

`Build-MSI.ps1` builds an MSI on a Windows development machine using the locally installed WiX Toolset.

### GitHub Actions release build

The GitHub Actions workflow builds the official release when a version tag such as `v<version>` is pushed.

The GitHub Actions workflow produces:

* `AdminTrayTool-<version>.msi`
* `AdminTrayTool-portable.zip`
* A GitHub Release containing both files

For official releases, the GitHub Actions build should be treated as the release build.

---

# Prerequisites

## .NET SDK

AdminTrayTool requires the .NET 10 SDK.

Check the installed version:

```powershell
dotnet --version
```

The GitHub Actions workflow installs .NET 10 automatically.

---

## WiX Toolset

The local MSI build script currently uses:

**WiX Toolset v3.11**

The expected installation directory is:

```text
C:\Program Files (x86)\WiX Toolset v3.11\bin
```

The following executables must exist:

```text
heat.exe
candle.exe
light.exe
```

You can check:

```powershell
Test-Path "C:\Program Files (x86)\WiX Toolset v3.11\bin\heat.exe"
Test-Path "C:\Program Files (x86)\WiX Toolset v3.11\bin\candle.exe"
Test-Path "C:\Program Files (x86)\WiX Toolset v3.11\bin\light.exe"
```

The GitHub Actions workflow searches the Windows runner for WiX rather than assuming the exact local installation path.

---

## Required project files

The following files/directories are required for a complete build:

```text
AdminTrayTool.csproj
Build-MSI.ps1
Build-Helper.ps1
adminTray.ico
config.json
installer\
GAM7\
```

The installer directory contains the WiX source files, including:

```text
installer\Product.wxs
installer\GAM7Components.wxs
```

The GAM7 directory must contain:

```text
GAM7\gam.exe
```

GAM7 is required for Chromebook management functionality.

---

# Local MSI Build

The local build is performed using:

```powershell
.\Build-MSI.ps1
```

The script defaults to:

```text
ProductVersion = 1.1.4
Configuration  = Release
OutputPath     = .\msi-output
```

## Build a specific version

For example:

```powershell
.\Build-MSI.ps1 -ProductVersion "<version>"
```

You can also explicitly specify Release configuration:

```powershell
.\Build-MSI.ps1 `
    -ProductVersion "<version>" `
    -Configuration "Release"
```

To specify a different output directory:

```powershell
.\Build-MSI.ps1 `
    -ProductVersion "<version>" `
    -Configuration "Release" `
    -OutputPath ".\msi-output"
```

---

# What Build-MSI.ps1 Does

The local build performs the following operations.

## 1. Build the .NET application

The script publishes the application using:

```powershell
dotnet publish AdminTrayTool.csproj `
    -c Release `
    -o .\publish `
    --property:DebugType=None `
    --property:DebugSymbols=false
```

The existing `publish` directory is removed before publishing.

---

## 2. Copy GAM7

The script checks:

```text
.\GAM7
```

If it exists, it copies the directory into:

```text
.\publish\GAM7
```

The resulting application therefore contains:

```text
publish\
└── GAM7\
    └── gam.exe
```

If GAM7 is missing, the local script continues with a warning. However, Chromebook functionality will not be available in that build.

---

## 3. Prepare application files

The application files are copied into:

```text
.\AppSourceDir
```

GAM7 is deliberately excluded from this directory.

This is important because GAM7 is packaged separately by the installer.

The resulting structure is approximately:

```text
AppSourceDir\
├── AdminTrayTool.exe
├── AdminTrayTool.dll
├── config.json
└── ...
```

GAM7 is kept separately:

```text
publish\
└── GAM7\
    └── ...
```

---

## 4. Harvest application files

WiX Heat generates:

```text
wix-obj\components-app.wxs
```

This contains the application files from `AppSourceDir`.

GAM7 is not included in this harvest.

---

## 5. Generate GAM7 components

If GAM7 is available, the build generates:

```text
wix-obj\components-gam.wxs
```

The generated file contains the GAM7 components required by the installer.

If GAM7 is unavailable, an empty component group is generated so the installer can still compile.

---

## 6. Compile WiX

The following WiX files are compiled:

```text
installer\Product.wxs
wix-obj\components-app.wxs
wix-obj\components-gam.wxs
```

The build defines the following values:

```text
ProductVersion
AdminTrayToolOutputDir
GAM7OutputDir
WixUILicenseRtf
AdminTrayIcon
```

---

## 7. Link the MSI

WiX Light links the compiled objects into the final MSI.

The default output is:

```text
msi-output\AdminTrayTool-<version>.msi
```

For example:

```text
msi-output\AdminTrayTool-<version>.msi
```

A WiX PDB is also produced:

```text
msi-output\AdminTrayTool-<version>.wixpdb
```

---

# Local Build Verification

`Build-MSI.ps1` verifies that the MSI was created and reports its file size.

It does **not** automatically install the MSI or perform a full application test.

After building, test the installer separately.

For example:

```powershell
msiexec /i ".\msi-output\AdminTrayTool-<version>.msi"
```

Recommended manual tests include:

* Application starts correctly
* Tray icon appears
* Configuration loads correctly
* Admin tools open
* RDP functionality works
* Chromebook Management opens
* GAM7 authentication works
* Chromebook lookup works
* Chromebook actions work
* MSI upgrade behaviour works

---

# Build Helper

`Build-Helper.ps1` provides an interactive menu for the normal development and release workflow.

Run:

```powershell
.\Build-Helper.ps1
```

The menu provides:

```text
[1] Check Prerequisites
[2] Build MSI
[3] Update Version Number
[4] Open MSI Output Folder
[5] Commit, Merge to Main, Push & Trigger GitHub Release
[6] Exit
```

## Option 1 — Check Prerequisites

The helper checks:

* WiX Toolset
* .NET SDK
* `installer\Product.wxs`
* GitHub CLI
* GitHub CLI authentication

If GitHub CLI is not authenticated, run:

```powershell
gh auth login
```

---

## Option 2 — Build MSI

This calls:

```powershell
.\Build-MSI.ps1 `
    -ProductVersion $CurrentVersion `
    -Configuration "Release"
```

The resulting MSI is placed in:

```text
.\msi-output\
```

---

# Version Management

The project uses:

```text
VERSION.txt
```

as the version source for the interactive build helper.

The helper supports:

* Patch version
* Minor version
* Major version
* Custom version

For example:

```text
<version>
```

A patch bump produces:

```text
1.5.2
```

A minor bump produces:

```text
1.6.0
```

A major bump produces:

```text
2.0.0
```

Custom versions must use:

```text
MAJOR.MINOR.PATCH
```

For example:

```text
<version>
```

When the version is changed, `Build-Helper.ps1` updates the following project properties when they exist:

```xml
<Version><version></Version>
<AssemblyVersion><version>.0</AssemblyVersion>
<FileVersion><version>.0</FileVersion>
```

---

# GitHub Actions Release

The official release workflow is located at:

```text
.github/workflows/build-msi.yml
```

The workflow runs on:

* Pushes to `main`
* Version tags matching `v*`
* Pull requests targeting `main`
* Manual workflow dispatch

For an official release, use a version tag.

For example:

```powershell
git tag v<version>
git push origin v<version>
```

GitHub Actions then determines the release version from the tag:

```text
v<version>
```

becomes:

```text
<version>
```

---

# GitHub Actions Build Process

The GitHub Actions workflow performs the complete release build.

## 1. Checkout

The repository is checked out using:

```text
actions/checkout@v4
```

## 2. Install .NET 10

The workflow installs:

```text
.NET 10
```

using:

```text
actions/setup-dotnet@v4
```

## 3. Determine version

For a version tag:

```text
v<version>
```

the MSI version becomes:

```text
<version>
```

Non-tag builds use:

```text
1.0.0
```

as the workflow's fallback version.

## 4. Restore and build

The workflow runs:

```powershell
dotnet restore
```

and then:

```powershell
dotnet build -c Release --no-restore
```

with the release version supplied through MSBuild properties.

## 5. Publish

The application is published as a self-contained `win-x64` application.

The publish output is:

```text
publish\
```

## 6. Copy config.json

If the repository contains:

```text
config.json
```

it is copied into:

```text
publish\config.json
```

## 7. Verify GAM7

The workflow requires:

```text
GAM7\gam.exe
```

The build fails if GAM7 or `gam.exe` is missing.

This differs from the local `Build-MSI.ps1`, which can continue without GAM7.

---

# GitHub Actions Installer Packaging

The workflow creates:

```text
AppSourceDir\
```

from the published application.

GAM7 is then removed from `AppSourceDir`.

This ensures the application files and GAM7 are packaged separately.

The workflow verifies that GAM7 is not present in the application harvest.

---

# GAM7 WiX Generation

The GitHub Actions workflow generates:

```text
installer\GAM7Components.wxs
```

directly from the contents of:

```text
GAM7\
```

GAM7 is not harvested using WiX Heat.

This packaging approach is intentional because GAM7 contains a large number of files and dependencies that can cause duplicate-file, component-reference, and self-registration problems when harvested automatically.

The generated WiX file creates the required directory structure and file components.

---

# GitHub Actions MSI Output

The workflow creates:

```text
installer-output\AdminTrayTool-<version>.msi
```

For example:

```text
installer-output\AdminTrayTool-<version>.msi
```

It also creates:

```text
installer-output\AdminTrayTool-portable.zip
```

The workflow uploads both as GitHub Actions artifacts.

The published application is also uploaded as:

```text
AdminTrayTool-publish
```

---

# GitHub Release

When the workflow is triggered by a version tag beginning with `v`, it creates a GitHub Release.

For example:

```text
v<version>
```

produces a release containing:

```text
AdminTrayTool-<version>.msi
AdminTrayTool-portable.zip
```

GitHub automatically generates release notes.

The local `Build-Helper.ps1` does **not** upload the MSI.

Instead, after the version tag is pushed, GitHub Actions performs the release build and uploads the release files.

---

# Recommended Release Workflow

For a normal release:

## 1. Start the build helper

```powershell
.\Build-Helper.ps1
```

## 2. Update the version

Choose:

```text
[3] Update Version Number
```

Select the appropriate version bump or enter a custom version.

## 3. Build and test the MSI

Choose:

```text
[2] Build MSI
```

Test the resulting MSI locally.

## 4. Commit the changes

Choose:

```text
[5] Commit, Merge to Main, Push & Trigger GitHub Release
```

Enter a commit message or accept:

```text
Release v<version>
```

## 5. Merge to main

If working on a feature branch, the helper can:

* Push the feature branch
* Checkout `main`
* Pull the latest `main`
* Merge the feature branch

## 6. Create the version tag

Confirm:

```text
Tag this commit as v<version>?
```

For example:

```text
v<version>
```

## 7. Push main and the tag

Confirm the push.

The helper pushes:

```text
main
```

and:

```text
v<version>
```

## 8. GitHub Actions builds the release

Once the tag reaches GitHub, the workflow builds:

```text
AdminTrayTool-<version>.msi
AdminTrayTool-portable.zip
```

and creates the GitHub Release.

---

# Release Checklist

Before creating a release, verify:

* [ ] Application builds successfully
* [ ] Version has been updated
* [ ] `VERSION.txt` contains the intended version
* [ ] `AdminTrayTool.csproj` version properties are correct
* [ ] GAM7 exists
* [ ] `GAM7\gam.exe` exists
* [ ] MSI builds successfully
* [ ] MSI installs successfully
* [ ] Application starts after installation
* [ ] Chromebook functionality works
* [ ] OAuth functionality works
* [ ] Upgrade behaviour has been tested
* [ ] Changes are committed
* [ ] Feature branch has been merged into `main`
* [ ] Version tag has been pushed
* [ ] GitHub Actions completed successfully
* [ ] GitHub Release contains the MSI
* [ ] GitHub Release contains the portable ZIP

---

# Build Directories

The build process creates several working directories.

| Directory           | Purpose                                    |
| ------------------- | ------------------------------------------ |
| `publish\`          | Published application files                |
| `AppSourceDir\`     | Application files prepared for WiX         |
| `wix-obj\`          | Local WiX generated files and objects      |
| `msi-output\`       | Local MSI build output                     |
| `installer-output\` | GitHub Actions MSI and portable ZIP output |

The local `Build-MSI.ps1` automatically recreates:

```text
publish\
AppSourceDir\
wix-obj\
```

as part of its build process.

The GitHub Actions workflow uses:

```text
installer-output\
```

for its release artifacts.

---

# Troubleshooting

## WiX not found

Verify:

```powershell
Test-Path "C:\Program Files (x86)\WiX Toolset v3.11\bin\candle.exe"
```

Also check:

```powershell
Test-Path "C:\Program Files (x86)\WiX Toolset v3.11\bin\heat.exe"
Test-Path "C:\Program Files (x86)\WiX Toolset v3.11\bin\light.exe"
```

---

## .NET not found

Run:

```powershell
dotnet --version
```

Install the .NET 10 SDK if necessary.

---

## GAM7 not found

Verify:

```text
GAM7\
└── gam.exe
```

For GitHub Actions, GAM7 is required and the workflow will fail if it is missing.

---

## Product.wxs not found

Verify:

```text
installer\Product.wxs
```

exists in the repository.

---

## GitHub CLI not authenticated

Run:

```powershell
gh auth login
```

Then verify:

```powershell
gh auth status
```

---

## PowerShell script execution is blocked

If Windows reports that scripts cannot be loaded because script execution is disabled, check the current policy:

```powershell
Get-ExecutionPolicy
```

You can run the script without permanently changing the policy by using:

```powershell
powershell.exe -ExecutionPolicy Bypass -File .\Build-Helper.ps1
```

or:

```powershell
powershell.exe -ExecutionPolicy Bypass -File .\Build-MSI.ps1 -ProductVersion "<version>"
```

Follow your organization's PowerShell security policy when choosing an execution method.

---

# Summary

For development:

```powershell
.\Build-MSI.ps1 -ProductVersion "<version>"
```

For the interactive workflow:

```powershell
.\Build-Helper.ps1
```

For an official GitHub release:

```powershell
git tag v<version>
git push origin v<version>
```

The version-specific examples above use `<version>`; replace it with the version being released.
