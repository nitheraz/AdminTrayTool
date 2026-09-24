# AdminTrayTool - User Guide

AdminTrayTool is a Windows system-tray utility that provides quick access to administrative web portals, RDP targets, Windows administration tools, and Chromebook management tools.

The application is configured using a JSON configuration file. The preferred machine-wide configuration location is:

```text
%PROGRAMDATA%\AdminTrayTool\config.json
```

A per-user configuration location is also supported:

```text
%APPDATA%\AdminTrayTool\config.json
```

This guide covers:

* Installation using the MSI
* Portable usage using the ZIP package
* First-run behavior and configuration locations
* Editing configuration using the built-in editor
* Managing browser profiles and launching URLs
* Adding RDP servers and administrative tools
* Windows and Chromebook management
* Configuration permissions
* Troubleshooting
* GitHub Actions builds and releases

---

# Installation

## MSI Installation

The MSI installer is the recommended installation method for managed or regularly used computers.

### 1. Download the MSI

For official releases, download the MSI from the GitHub Release for the required version.

GitHub Actions also produces the MSI as a build artifact for testing.

The file is named:

```text
AdminTrayTool-<version>.msi
```

For example:

```text
AdminTrayTool-<version>.msi
```

### 2. Run the installer

Run the MSI and follow the installation prompts.

The MSI performs a per-machine installation and may require administrator privileges.

### 3. Configuration

The application uses:

```text
%PROGRAMDATA%\AdminTrayTool\config.json
```

as the preferred machine-wide configuration file.

The exact configuration supplied with an installation depends on the version and build being deployed.

### 4. Start AdminTrayTool

After installation, launch AdminTrayTool.

When running, the application appears in the Windows system tray.

Right-click the tray icon to access the available administrative tools.

---

# Portable Usage

AdminTrayTool can also be distributed as a portable ZIP package.

The portable package is useful for:

* Testing
* Temporary use
* VM validation
* Development
* Environments where MSI installation is not required

## Using the Portable Package

### 1. Download the ZIP

The release package is named:

```text
AdminTrayTool-portable.zip
```

### 2. Extract the ZIP

Extract the contents to a suitable folder.

### 3. Run the application

Start:

```text
AdminTrayTool.exe
```

or use any launcher included with the particular release.

The portable package does not require an MSI installation.

> Configuration behavior is still based on the application's normal configuration lookup rules. The application prefers the machine-wide ProgramData configuration when it is available.

---

# First Run and Configuration Locations

AdminTrayTool supports both machine-wide and per-user configuration.

## Machine-Wide Configuration

Preferred location:

```text
%PROGRAMDATA%\AdminTrayTool\config.json
```

This configuration is intended for computers where the same AdminTrayTool configuration should be available to users of the machine.

## Per-User Configuration

Fallback location:

```text
%APPDATA%\AdminTrayTool\config.json
```

This allows a user-specific configuration when a machine-wide configuration is unavailable or cannot be used.

## Configuration Priority

The application checks the machine-wide configuration first.

The general lookup order is:

```text
%PROGRAMDATA%\AdminTrayTool\config.json
        ↓
%APPDATA%\AdminTrayTool\config.json
```

The application uses the available configuration according to this lookup process.

---

# Editing Configuration

AdminTrayTool includes a built-in configuration editor.

## Opening the Editor

1. Right-click the AdminTrayTool system-tray icon.
2. Select **Edit config...**
3. The configuration editor opens.

The editor provides separate sections for:

* Web Portals
* RDP Servers
* Admin Tools

Use the available **Add**, **Edit**, and **Delete** controls to manage entries.

## Saving Changes

Select **Save** after making changes.

The application writes the updated configuration to the appropriate configuration file.

On managed installations, the machine-wide configuration may require administrator permissions to modify.

If Windows displays an elevation prompt when saving configuration, provide appropriate administrator credentials.

---

# Web Portals

Web portals allow frequently used administrative websites to be launched directly from the AdminTrayTool menu.

A web portal can include:

* Display name
* URL
* Browser profile

Example:

```json
{
  "name": "Admin Console",
  "url": "https://admin.example.com",
  "profile": "Profile 1"
}
```

## Browser Profiles

The `profile` value can be used to launch a specific Chrome or Edge browser profile where supported by the configured browser.

Common Chrome profile directory names include:

```text
Default
Profile 1
Profile 2
```

The configured profile must match the profile directory used by the browser.

If a profile does not exist, the browser may open using another available profile or fail to launch as expected.

---

# RDP Servers

RDP entries provide quick access to Remote Desktop targets.

Example:

```json
{
  "name": "Server1",
  "host": "server1.example.com"
}
```

The display name is shown in the AdminTrayTool menu while the `host` value identifies the remote computer.

Select an RDP entry from the tray menu to launch the Remote Desktop connection.

---

# Admin Tools

Admin Tools provide shortcuts to locally installed administrative applications.

Example:

```json
{
  "name": "PuTTY",
  "exe": "putty.exe",
  "args": "",
  "elevated": true
}
```

An administrative tool can specify:

* Name
* Executable
* Command-line arguments
* Whether it should be launched elevated

## Elevated Tools

Tools configured with:

```json
"elevated": true
```

are launched with administrator elevation.

Windows may display a UAC prompt depending on the current security context.

---

# Windows Management

AdminTrayTool provides access to Windows administration functions through the **Windows Management** section.

Depending on the installed version and configuration, this can include tools such as:

* Active Directory management
* Microsoft Endpoint Configuration Manager (MECM)

These functions are intended for IT administration and may require appropriate Windows or management-system permissions.

---

# Chromebook Management

AdminTrayTool can provide Chromebook management functionality through the bundled GAM7 tools.

Chromebook management can include actions such as:

* Chromebook lookup
* Refreshing Chromebook information
* Disabling a Chromebook
* Re-enabling a Chromebook
* Clearing Chromebook form information
* Moving Chromebooks between organisational units
* Powerwashing Chromebooks
* Clearing Chromebook profiles

The exact actions available depend on the installed version and the configured GAM7 environment.

## GAM7

The application expects GAM7 in the application directory when Chromebook management is being used.

The bundled structure is generally:

```text
GAM7\
└── gam.exe
```

GAM7 authentication must be configured separately.

The application can check the GAM7 authentication state and provide status information when Chromebook management is opened.

For GAM7 configuration and authentication instructions, refer to the project's GAM setup documentation.

---

# Configuration JSON

A simplified configuration example is:

```json
{
  "editor": "notepad.exe",
  "webPortals": [
    {
      "name": "Admin Console",
      "url": "https://admin.example.com",
      "profile": "Profile 1"
    }
  ],
  "rdpServers": [
    {
      "name": "Server1",
      "host": "server1.example.com"
    }
  ],
  "adminTools": [
    {
      "name": "PuTTY",
      "exe": "putty.exe",
      "args": "",
      "elevated": true
    }
  ]
}
```

The exact available properties may change as new features are added.

For the current configuration structure, use the configuration editor where possible rather than manually editing JSON.

---

# Configuration Permissions

The machine-wide configuration is stored under:

```text
%PROGRAMDATA%\AdminTrayTool
```

Because this is a machine-wide configuration, write access may be restricted.

This is intentional on managed computers where standard users should not be able to modify administrative shortcuts or configuration.

If Windows requests administrator credentials when saving configuration, this indicates that the current user does not have sufficient permission to modify the machine-wide configuration.

Administrators can modify the configuration as required.

## Per-User Configuration

When a machine-wide configuration is unavailable, AdminTrayTool can use:

```text
%APPDATA%\AdminTrayTool\config.json
```

This provides a user-specific configuration without requiring changes to the machine-wide configuration.

---

# Troubleshooting

## The Tray Icon Is Not Visible

Check the Windows system tray and the hidden-icons area.

If AdminTrayTool is running, its tray icon should be available there.

You can also check Task Manager for the AdminTrayTool process.

---

## A Menu Item Is Missing

The tray menu is generated from the configured items.

Check:

1. Open **Edit config...**
2. Verify that the required entry exists.
3. Verify that the configuration was saved successfully.
4. Restart AdminTrayTool if necessary.

You can also use **Open config file...** from the tray menu to inspect the active configuration file.

---

## Configuration Will Not Save

If the Save operation requires administrator elevation, the current user may not have permission to modify the machine-wide configuration.

Try saving with appropriate administrator credentials.

If the JSON file has been manually modified, also check that the JSON remains valid.

---

## A Web Portal Does Not Open

Check:

* The URL begins with `http://` or `https://`.
* The configured browser is installed.
* The browser profile name is correct.
* The computer has network access to the destination.

---

## An RDP Connection Does Not Start

Check:

* The hostname is correct.
* The target computer is reachable.
* Remote Desktop is enabled on the target.
* The current account has permission to connect.
* Network or firewall restrictions are not blocking the connection.

---

## An Admin Tool Does Not Launch

Check:

* The executable is installed.
* The configured executable name or path is correct.
* Command-line arguments are valid.
* The tool does not require additional configuration.
* If `elevated` is enabled, respond to the Windows UAC prompt when required.

---

# Chromebook Management Troubleshooting

If Chromebook management is unavailable or GAM7 is not authenticated:

1. Confirm the `GAM7` folder exists.
2. Confirm `GAM7\gam.exe` exists.
3. Open Chromebook Management.
4. Check the displayed GAM7 authentication status.
5. Verify the GAM7 OAuth configuration.
6. Confirm the Google Workspace account has the required permissions.

For detailed GAM7 setup instructions, refer to the project's GAM setup documentation.

---

# Release Packages

Official releases can contain two primary downloadable packages:

```text
AdminTrayTool-<version>.msi
AdminTrayTool-portable.zip
```

The MSI is intended for normal installation.

The portable ZIP is intended for portable use, testing, and environments where an MSI installation is not required.

---

# GitHub Actions Builds

AdminTrayTool uses GitHub Actions to automate the build process.

The workflow:

1. Checks out the source code.
2. Installs the .NET 10 SDK.
3. Determines the build version.
4. Builds the application.
5. Publishes a self-contained `win-x64` application.
6. Verifies GAM7.
7. Generates WiX installer components.
8. Builds the MSI.
9. Creates the portable ZIP.
10. Uploads build artifacts.

Release builds are created when a version tag is pushed.

For example:

```text
v<version>
```

produces release packages using version:

```text
<version>
```

---

# GitHub Actions Artifacts

For workflow runs, build artifacts can be available under the run's **Artifacts** section.

Current artifact packages include:

```text
AdminTrayTool-MSI
AdminTrayTool-portable
AdminTrayTool-publish
```

The MSI and portable ZIP are also attached to the GitHub Release when the workflow is triggered by a version tag.

---

# Creating a Release

The recommended release process is:

```text
Update code
    ↓
Update VERSION.txt
    ↓
Build and test locally
    ↓
Commit changes
    ↓
Merge into main
    ↓
Create version tag
    ↓
Push tag
    ↓
GitHub Actions builds release
    ↓
GitHub Release published
```

The interactive helper can assist with this process:

```powershell
.\Build-Helper.ps1
```

Select:

```text
5. Commit, Merge to Main, Push & Trigger GitHub Release
```

The workflow is tag-driven, so pushing the version tag causes GitHub Actions to build and publish the release.

---

# Version Numbers

AdminTrayTool uses standard three-part version numbers:

```text
MAJOR.MINOR.PATCH
```

For example:

```text
<version>
```

The version is maintained in:

```text
VERSION.txt
```

The build helper can update the version and synchronise the relevant project version information.

Release tags use:

```text
v<version>
```

For example:

```text
v<version>
```

---

# Documentation

Additional documentation is available in the repository:

| Document                           | Purpose                                            |
| ---------------------------------- | -------------------------------------------------- |
| **MSI-IMPLEMENTATION-COMPLETE.md** | Overview of the MSI build and release system       |
| **MSI-BUILD-GUIDE.md**             | Detailed MSI build and troubleshooting information |
| **BUILD-QUICK-START.md**           | Quick instructions for building and releasing      |
| **BUILD-QUICK-REFERENCE.txt**      | One-page build reference                           |

---

# Support and Issues

For application issues, configuration problems, or feature requests, use the project's GitHub Issues.

When reporting a problem, include:

* AdminTrayTool version
* Windows version
* What operation was being performed
* Error message
* Relevant log information
* Whether the MSI or portable version was being used

Avoid including passwords, OAuth credentials, API keys, or other sensitive configuration information in issue reports.

---

# Quick Reference

### Start AdminTrayTool

Launch:

```text
AdminTrayTool.exe
```

### Open Configuration Editor

```text
Tray Icon
   ↓
Edit config...
```

### Machine Configuration

```text
%PROGRAMDATA%\AdminTrayTool\config.json
```

### Per-User Configuration

```text
%APPDATA%\AdminTrayTool\config.json
```

### Build MSI Locally

```powershell
.\Build-MSI.ps1
```

### Use Build Helper

```powershell
.\Build-Helper.ps1
```

### Build a Specific Version

```powershell
.\Build-MSI.ps1 -ProductVersion "<version>"
```

### Release

```text
Update Version
      ↓
Build & Test
      ↓
Commit
      ↓
Merge to main
      ↓
Create v<version> tag
      ↓
Push
      ↓
GitHub Actions
```

---

**AdminTrayTool**

Windows IT Administration Utility

**Current installer technology:** WiX Toolset 3.11.1.2318
**Application framework:** .NET 10
**CI/CD:** GitHub Actions
**Installer:** Windows MSI
**Portable package:** ZIP
