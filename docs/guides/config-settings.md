# Config Settings

Access via the tray icon → **Config Settings**.

## App Config

Controls the **Admin Web Portals**, **Remote Desktop Connections**, and **Admin Tools** sections of the tray menu.

- **Edit App Config...** — opens an in-app editor with a tab per section. Use **Add**, **Edit**, and **Delete** to manage entries; changes save immediately and the tray menu reloads automatically.
- **Open App Config File...** — opens `config.json` directly in your default editor.
- **Open App Config Folder...** — opens the folder containing `config.json` in Explorer.

## Group Templates

Same editing pattern, applied to [Group Templates](group-management.md#group-templates):

- **Edit Group Templates...**
- **Open Group Templates File...**
- **Open Group Templates Folder...**

## Where Config Files Live

Both files live in `%ProgramData%\AdminTrayTool\`:

- `config.json`
- `groupTemplates.json`

This is a machine-wide location, so configuration is shared across all users on the same computer.