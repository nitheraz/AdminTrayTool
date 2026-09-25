<div align="center">

<img src="assets/Admintraytool.png" alt="AdminTrayTool" width="180">

# IT Admin Quick Tools

</div>

**AdminTrayTool** is a lightweight Windows system-tray utility built for IT administrators who manage Google Workspace, Chromebooks, Windows computers, and everyday administrative tasks from a single application.

It is designed to make common technician workflows faster without replacing your existing IT service-management platform.

<div class="grid cards" markdown>

-   :material-rocket-launch:{ .lg .middle } **Get Started**

    ---

    Install AdminTrayTool, complete first-run setup, configure GAM7 authentication, and learn about application updates.

    [:octicons-arrow-right-24: Getting Started](getting-started.md)

-   :material-google-chrome:{ .lg .middle } **Chromebook Management**

    ---

    Look up and manage individual Chromebooks using Serial Number or Asset ID.

    [:octicons-arrow-right-24: Chromebook Management](guides/chromebook-management.md)

-   :material-devices:{ .lg .middle } **Bulk Chromebook Management**

    ---

    Import, validate, review and process Chromebook operations using action-specific CSV templates.

    [:octicons-arrow-right-24: Bulk Management](guides/bulk-chromebook-management.md)

-   :material-microsoft-windows:{ .lg .middle } **Windows Management**

    ---

    Access Windows administration tools for Active Directory and Microsoft Endpoint Configuration Manager.

    [:octicons-arrow-right-24: Windows Management](guides/windows-management.md)

-   :material-account-group:{ .lg .middle } **Group Management**

    ---

    Manage staff membership in Google Groups individually or through role-based templates.

    [:octicons-arrow-right-24: Group Management](guides/group-management.md)

-   :material-cog:{ .lg .middle } **Configuration**

    ---

    Configure application settings and group templates through the in-app editor.

    [:octicons-arrow-right-24: Config Settings](guides/config-settings.md)

</div>

---

## What is it for?

AdminTrayTool lives in the Windows system tray and provides one-click access to common IT administration tasks.

### Chromebook Management

Look up and manage individual Chromebooks through GAM7 using **Serial Number or Asset ID**.

Available operations include:

- View device information
- Update Asset IDs
- Disable or re-enable devices
- Move devices between organisational units
- Clear profiles
- Initiate Powerwash operations

[Open Chromebook Management Guide](guides/chromebook-management.md){ .md-button }

### Bulk Chromebook Management

Bulk Chromebook Management provides a structured workflow for processing multiple devices.

You can:

- Import Chromebook information from action-specific CSV templates
- Validate CSV data locally
- Optionally check devices against Google Workspace
- Select devices for processing
- Review operations before execution
- Monitor processing progress
- Export results

[Open Bulk Management Guide](guides/bulk-chromebook-management.md){ .md-button }

### Windows Management

Windows Management provides access to administration tools for:

- Active Directory
- Microsoft Endpoint Configuration Manager (MECM/SCCM)

[Open Windows Management Guide](guides/windows-management.md){ .md-button }

### Active Directory Management

Use Active Directory Management to:

- Look up Windows computers
- View computer information
- Search organisational units
- Move computers between OUs

[Open Active Directory Guide](guides/active-directory-management.md){ .md-button }

### MECM / SCCM Management

Use MECM / SCCM Management to:

- Look up Windows computers
- View device information
- Add devices to collections
- Remove devices from collections

[Open MECM / SCCM Guide](guides/mecm-management.md){ .md-button }

### Group Management

Manage staff membership in Google Groups individually or through role-based templates such as:

- Primary Staff
- Secondary Staff
- School Officer

[Open Group Management Guide](guides/group-management.md){ .md-button }

### Quick Launch

AdminTrayTool provides configurable shortcuts to:

- Administration web portals
- Remote Desktop connections
- Local administration tools such as PuTTY
- PowerShell

These shortcuts can be configured through the application's settings.

### Config Settings

Configure application settings and group templates through the in-app editor without manually editing JSON files.

[Open Configuration Guide](guides/config-settings.md){ .md-button }

---

## Requirements

AdminTrayTool requires:

- **Windows 10 or Windows 11**
- **GAM7**
- A Google Workspace administrator account with the permissions and API scopes required for the Google Workspace features being used
- Appropriate Windows permissions for Active Directory and MECM/SCCM management features
- Network access to the relevant Google Workspace and Windows management services

GAM7 is bundled with the AdminTrayTool installer.

---

!!! warning "Administrative Access"

    AdminTrayTool is intended for IT staff who already have the appropriate administrative access on their workstation.

    It is **not a self-service privilege-elevation tool for end users**.

---

## Quick Links

### Getting Started

- [Getting Started](getting-started.md) — installation, first-run setup, GAM7 authentication, and application updates.

### Chromebook Management

- [Chromebook Management](guides/chromebook-management.md) — look up and manage individual Chromebooks.
- [Bulk Chromebook Management](guides/bulk-chromebook-management.md) — use action-specific CSV templates to import, validate, select, review, process, and export results for multiple Chromebooks.
- [GAM7 Setup & Troubleshooting](gam-setup.md) — GAM7 authentication, OAuth, projects, and common errors.

### Windows Management

- [Windows Management](guides/windows-management.md) — access Active Directory and MECM/SCCM administration.
- [Active Directory Management](guides/active-directory-management.md) — look up computers, view information, search OUs, and move computers between OUs.
- [MECM / SCCM Management](guides/mecm-management.md) — look up computers and manage device collections.

### Other Tools

- [Group Management](guides/group-management.md) — manage staff membership in Google Groups.
- [Config Settings](guides/config-settings.md) — configure AdminTrayTool settings and group templates.
- [FAQ](faq.md) — frequently asked questions.

---

## Latest Release

Get the current installer and release information from the latest AdminTrayTool release.

[View Latest Release](https://github.com/nitheraz/AdminTrayTool/releases/latest){ .md-button .md-button--primary }

---

## Project

AdminTrayTool is designed to make common IT administration workflows faster while continuing to work alongside existing IT service-management platforms.

[View AdminTrayTool on GitHub](https://github.com/nitheraz/AdminTrayTool){ .md-button }
