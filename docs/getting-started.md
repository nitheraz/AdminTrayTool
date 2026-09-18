# Getting Started

## Installation

1. Download the latest `AdminTrayTool.msi` from the [GitHub Releases page](https://github.com/nitheraz/AdminTrayTool/releases/latest).

2. Run the installer.

3. AdminTrayTool installs to:

   ```text
   C:\Program Files (x86)\AdminTrayTool
   ```

4. A Start Menu shortcut is created during installation.

5. Launch AdminTrayTool from the Start Menu.

6. Look for the AdminTrayTool icon in the Windows system tray. You may need to click the **Show hidden icons** arrow.

## First-Run Setup

The first time you open **Chromebook Management** or **Group Management**, AdminTrayTool checks whether GAM7 is configured and authenticated on the machine.

If GAM7 is not configured, you will be guided through the setup process.

### 1. GAM7 Project Setup

AdminTrayTool checks for an existing GAM7 project and its `client_secrets.json`.

If your organization already has a working GAM7 project, you should use the existing project rather than creating another Google Cloud project.

If this is the first GAM7 machine for your organization, you can create a new GAM7 project.

See [GAM7 Setup & Troubleshooting](gam-setup.md) for detailed instructions.

### 2. OAuth Authentication

Once the GAM7 project is available, AdminTrayTool checks for an existing OAuth session.

If authentication is required:

1. GAM7 starts the OAuth process.
2. A browser window opens.
3. Sign in using your Google Workspace administrator account.
4. Grant the requested permissions.
5. Return to AdminTrayTool.

Once authentication is complete, the GAM7-powered features are available.

> **Security:** Each machine maintains its own GAM7 authentication session. Do not copy another technician's OAuth tokens or authenticated GAM7 credentials to your machine.

## Updating

AdminTrayTool can check for new versions automatically when the application starts.

When a newer version is available, you will be prompted to update.

You can also check manually through:

**Tray icon → About AdminTrayTool → Check for Updates**

## Upgrading Versions

AdminTrayTool supports in-place upgrades.

Run the newer installer over the existing installation. You do **not** need to uninstall the previous version first.

Your application configuration is stored outside the installation directory and is preserved during upgrades:

```text
%ProgramData%\AdminTrayTool\
├── config.json
└── groupTemplates.json
```

Your GAM7 user configuration is also stored separately under your Windows user profile and is not replaced by the AdminTrayTool application upgrade.
