# Config Settings

Access configuration through:

**Tray icon → Config Settings**

AdminTrayTool provides an in-app configuration editor so common settings can be changed without manually editing JSON files.

## App Config

App Config controls the following tray-menu sections:

* **Admin Web Portals**
* **Remote Desktop Connections**
* **Admin Tools**

### Edit App Config

Select:

**Config Settings → Edit App Config**

The editor provides a separate tab for each configuration section.

Use:

* **Add** — create a new entry
* **Edit** — modify an existing entry
* **Delete** — remove an entry

Changes are saved immediately and the tray menu reloads the updated configuration.

### Open App Config File

Opens the `config.json` file directly in your default text editor.

### Open App Config Folder

Opens the folder containing the application configuration in Windows Explorer.

## Group Templates

Group Templates use the same editing approach.

Access them through:

**Config Settings → Edit Group Templates**

Each row represents one Google Group.

Multiple rows with the same **Template Name** are combined into a single staff template.

See [Group Management](group-management.md#group-templates) for more information.

### Edit Group Templates

Opens the in-app Group Template editor.

### Open Group Templates File

Opens `groupTemplates.json` directly in your default editor.

### Open Group Templates Folder

Opens the folder containing `groupTemplates.json`.

## Configuration File Location

AdminTrayTool stores its application configuration in:

```text
%ProgramData%\AdminTrayTool\
```

The main files are:

```text
config.json
groupTemplates.json
```

Because this is a machine-wide location, the application configuration is shared between Windows users on the same computer.

## GAM7 Configuration

GAM7 maintains its own configuration separately under the current Windows user's profile:

```text
%USERPROFILE%\.gam\
```

This directory may contain files such as:

```text
client_secrets.json
gam.cfg
oauth2.txt
```

These files are **not the same as AdminTrayTool's application configuration**.

> **Security:** Do not copy another user's OAuth tokens or authenticated GAM7 credentials simply to avoid authentication. Each technician should normally authenticate their own GAM7 session.
