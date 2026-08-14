AdminTrayTool Portable
======================

This folder contains a portable build of AdminTrayTool. Users can unzip the archive and run the included `Run-AdminTrayTool.cmd` to start the application.

Behavior
- The script attempts to copy the default `config.json` to `%ProgramData%\AdminTrayTool` if the current user can create and write to that folder.
- If ProgramData is not writable, the app will fall back to the per-user config located under `%APPDATA%\AdminTrayTool\config.json`.
- The portable ZIP includes the published files and a `config.json` sample. Edit the `config.json` before running if you want to customize defaults.

Usage
1. Unzip the portable archive to a folder.
2. Edit `config.json` if desired.
3. Run `Run-AdminTrayTool.cmd` (double-click or run from a console). The command will try to deploy the config to ProgramData and launch the .exe.

Security note
- The script may copy config.json into ProgramData making it machine-wide. If you don't want this behavior, remove `config.json` from the extracted folder before running.

Support
- For installer packaging and machine-wide installs, consider using the MSI produced by the installer workflow.
