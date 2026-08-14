AdminTrayTool - User Guide
==========================

Overview
--------
AdminTrayTool is a small Windows system-tray utility that provides quick access to administrative web portals, RDP targets, and admin tools. The app is configurable via a JSON file (config.json) located by default at %PROGRAMDATA%\AdminTrayTool\config.json. A per-user fallback is available at %APPDATA%\AdminTrayTool\config.json.

This guide covers:
- Installation (MSI)
- Portable usage (ZIP)
- First-run behavior and config locations
- Editing configuration via the built-in editor
- Managing browser profiles and launching URLs
- Adding RDP and Admin Tools
- Troubleshooting and permissions
- Release notes and changelog guidance

Install (MSI)
--------------
1. Download AdminTrayTool.msi from the Releases page or from the built CI artifacts.
2. Run the MSI (requires elevation for per-machine install).
3. The installer will place the executable in Program Files and create %PROGRAMDATA%\AdminTrayTool with a default config.json. The installer may also create Start Menu and Desktop shortcuts.
4. If you provided signing certificates during build, the MSI will be signed.

Portable (ZIP)
---------------
1. Download AdminTrayTool-portable.zip and extract it to a folder.
2. The ZIP includes a Run-AdminTrayTool.cmd launcher and a sample config.json.
3. Run Run-AdminTrayTool.cmd. It attempts to copy config.json into %PROGRAMDATA%\AdminTrayTool if writable; otherwise, the app will use per-user config under %APPDATA%.
4. If you prefer not to copy config.json into ProgramData, remove the config.json file before running, or edit it in-place after extraction.

First-run / Config locations
----------------------------
- Machine-wide config: %PROGRAMDATA%\AdminTrayTool\config.json (preferred)
- Per-user config fallback: %APPDATA%\AdminTrayTool\config.json

On first run, the app will:
- Read the machine-wide config if present.
- If missing, read per-user config if present.
- If both missing, and if installed, the MSI will have created a default config.json in ProgramData.

Editing configuration (in-app editor)
-------------------------------------
- Right-click the system tray icon and choose "Edit config...".
- The editor exposes three tabs: Web Portals, RDP Servers, Admin Tools.
- Use Add/Edit/Delete buttons to manage items; Save writes changes to the canonical config file.
- The editor attempts to validate entries (e.g., URLs must start with http/https) to avoid malformed JSON.
- The editor also avoids inline grid edits; use the modal dialogs to add or edit items to ensure proper disk persistence.

Config JSON structure
---------------------
Minimal structure:

{
  "editor": "notepad.exe",
  "webPortals": [ { "name": "Admin Console", "url": "https://admin.example.com", "profile": "Profile 1" } ],
  "rdpServers": [ { "name": "Server1", "host": "server1.example.com" } ],
  "adminTools": [ { "name": "PuTTY", "exe": "putty.exe", "args": "", "elevated": true } ]
}

Tips
----
- For Chrome/Edge profile launching, set "profile" to the profile directory name (e.g., "Default" or "Profile 1"). The app will pass --profile-directory to chrome.exe.
- For PuTTY, the editor automatically marks putty.exe as elevated in adminTools by default.

Permissions & ProgramData
-------------------------
- The MSI installer can set ACLs so non-admin users may edit the config.json in ProgramData.
- Best practice: grant Modify only on the specific config file or AdminTrayTool folder instead of global ProgramData.
- If the app cannot write to ProgramData, it falls back to per-user config in %APPDATA%.

Troubleshooting
---------------
- If a menu item is missing in the tray, check which config file the app is using (tray menu includes "Open config file...").
- If Save or Add is disabled in the editor, the on-disk JSON is likely malformed—open the file with "Open config file..." and fix JSON syntax.
- If a URL fails to open, verify the configured browser (chrome.exe) is installed and that the profile name is valid.
- Logs: the installer/workflow creates artifacts; the app logs actions to an application log (admintraytool.log) in ProgramData or %TEMP% if not writable.

Release notes template
----------------------
Include a concise changelog in Releases. Example:

Release v1.0.0 - YYYY-MM-DD
- Initial release: system tray app, config-driven menus for web/RDP/tools
- MSI installer and portable ZIP provided
- Editor UI for managing config

Publishing via CI
-----------------
- The GitHub Actions workflow builds the app, creates a portable ZIP, and builds an MSI using WiX.
- To sign the MSI, provide CERT_PFX (base64 pfx) and CERT_PASSWORD as GitHub Secrets.
- The workflow can create a GitHub Release and upload the MSI and portable ZIP as assets.

Where to find artifacts on GitHub
--------------------------------
1. Actions -> select the workflow run -> Artifacts section -> download AdminTrayTool-msi or AdminTrayTool-publish.
2. Releases -> the workflow can create a Release that contains the MSI and portable ZIP as assets.

Support & Contribution
----------------------
- For issues, open GitHub Issues in the repo.
- For code contributions, open a PR; CI will build MSI and ZIP.

Contact
-------
- Maintain a small README in the repo with contact/ownership details.


---
Generated by AdminTrayTool build automation.
