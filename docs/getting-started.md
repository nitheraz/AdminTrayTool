# Getting Started

## Installation

1. Download the latest `AdminTrayTool.msi` from the [Releases page](https://github.com/nitheraz/AdminTrayTool/releases/latest).
2. Run the installer. AdminTrayTool installs to `Program Files (x86)\AdminTrayTool` and adds a Start Menu shortcut.
3. Launch AdminTrayTool from the Start Menu, or it will start automatically on next login if configured to do so.
4. Look for the AdminTrayTool icon in your system tray (you may need to click the "show hidden icons" arrow).

## First-Run Setup

The first time you open **Chromebook Management** or **Group Management**, AdminTrayTool checks whether GAM7 is authenticated on your machine. If it isn't, you'll be guided through:

1. **GAM Project setup** — if no `client_secrets.json` is found, you'll be prompted to either create a new GAM project or point to an existing one. See [GAM7 Setup & Troubleshooting](gam-setup.md) for details.
2. **OAuth authentication** — a browser window opens for you to sign in with your Google Workspace admin account and grant the required permissions.

Once authenticated, all features become available immediately.

## Updating

AdminTrayTool checks for updates automatically on startup (silently — you'll only see a prompt if a newer version is available). You can also check manually via **tray icon → About AdminTrayTool → Check for Updates**.

## Upgrading Versions

Running a newer installer over an existing installation automatically upgrades in place — no need to uninstall first. Your configuration files (`config.json`, `groupTemplates.json`) are preserved across upgrades.