# AdminTrayTool

**AdminTrayTool** is a lightweight Windows system-tray utility built for IT administrators who manage Google Workspace, Chromebooks, and everyday administrative tasks from a single application.

It is designed to make common technician workflows faster without replacing your existing IT service-management platform.

## What is it for?

AdminTrayTool lives in the Windows system tray and provides one-click access to:

* **Chromebook Management** — look up, disable, re-enable, and update Chromebooks through GAM7 using Serial Number or Asset ID. Device information and search results are cached locally for faster repeat lookups.
* **Group Management** — add staff to Google Groups individually or through role-based templates such as Primary Staff, Secondary Staff, and School Officer.
* **Quick Launch** — configurable shortcuts to administration web portals, Remote Desktop connections, and local administration tools such as PuTTY and PowerShell.
* **Config Settings** — manage application settings and group templates through an in-app editor without manually editing JSON files.

AdminTrayTool is intended for IT staff who already have the appropriate administrative access on their workstation. It is **not a self-service privilege-elevation tool for end users**.

## Requirements

* Windows 10 or Windows 11
* [GAM7](https://github.com/GAM-team/GAM), bundled with the AdminTrayTool installer
* A Google Workspace administrator account with the permissions and API scopes required for the features being used

## Quick Links

* [Getting Started](getting-started.md) — installation and first-run setup
* [Chromebook Management](chromebook-management.md) — device lookup and management
* [Group Management](group-management.md) — staff and Google Group management
* [GAM7 Setup & Troubleshooting](gam-setup.md) — GAM7 authentication, OAuth, projects, and common errors
* [FAQ](faq.md) — frequently asked questions
* [Latest Release](https://github.com/nitheraz/AdminTrayTool/releases/latest)
