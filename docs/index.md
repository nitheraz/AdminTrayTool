# AdminTrayTool

**AdminTrayTool** is a lightweight Windows system-tray utility built for IT administrators who manage Google Workspace, Chromebooks, Windows computers, and everyday administrative tasks from a single application.

It is designed to make common technician workflows faster without replacing your existing IT service-management platform.

## What is it for?

AdminTrayTool lives in the Windows system tray and provides one-click access to:

* **Chromebook Management** — look up and manage individual Chromebooks through GAM7 using Serial Number or Asset ID. View device information, update Asset IDs, disable or re-enable devices, move devices between organisational units, clear profiles, and initiate Powerwash operations.
* **Bulk Chromebook Management** — import Chromebook serial numbers from CSV files, validate devices against Google Workspace, select devices for bulk actions, review operations before execution, monitor progress, and export results.
* **Windows Management** — access Windows administration tools for Active Directory and Microsoft Endpoint Configuration Manager (MECM/SCCM).
* **Active Directory Management** — look up Windows computers, view computer information, search organisational units, and move computers between OUs.
* **MECM / SCCM Management** — look up Windows computers, view device information, and add or remove devices from collections.
* **Group Management** — add staff to Google Groups individually or through role-based templates such as Primary Staff, Secondary Staff, and School Officer.
* **Quick Launch** — configurable shortcuts to administration web portals, Remote Desktop connections, and local administration tools such as PuTTY and PowerShell.
* **Config Settings** — manage application settings and group templates through an in-app editor without manually editing JSON files.

AdminTrayTool is intended for IT staff who already have the appropriate administrative access on their workstation. It is **not a self-service privilege-elevation tool for end users**.

## Requirements

* Windows 10 or Windows 11
* [GAM7](https://github.com/GAM-team/GAM)
* A Google Workspace administrator account with the permissions and API scopes required for the Google Workspace features being used
* Appropriate Windows permissions for Active Directory and MECM/SCCM management features
* Network access to the relevant Google Workspace and Windows management services

GAM7 is bundled with the AdminTrayTool installer.

## Quick Links

### Getting Started

* [Getting Started](getting-started.md) — installation, first-run setup, GAM7 authentication, and application updates.

### Chromebook Management

* [Chromebook Management](guides/chromebook-management.md) — look up and manage individual Chromebooks.
* [Bulk Chromebook Management](guides/bulk-chromebook-management.md) — import, validate, select, process, and export results for multiple Chromebooks.
* [GAM7 Setup & Troubleshooting](gam-setup.md) — GAM7 authentication, OAuth, projects, and common errors.

### Windows Management

* [Windows Management](guides/windows-management.md) — access Active Directory and MECM/SCCM administration.
* [Active Directory Management](guides/active-directory-management.md) — look up computers, view information, search OUs, and move computers between OUs.
* [MECM / SCCM Management](guides/mecm-management.md) — look up computers and manage device collections.

### Other Tools

* [Group Management](guides/group-management.md) — manage staff membership in Google Groups.
* [Config Settings](guides/config-settings.md) — configure AdminTrayTool settings and group templates.
* [FAQ](faq.md) — frequently asked questions.

## Latest Release

See the [latest AdminTrayTool release](https://github.com/nitheraz/AdminTrayTool/releases/latest) for the current installer and release information.
