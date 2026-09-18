# Frequently Asked Questions

## Does every technician need their own Google Workspace administrator credentials?

Normally, yes.

Each technician's machine should authenticate its own GAM7 OAuth session using the Google Workspace account authorized to perform the required operations.

AdminTrayTool does not act as a credential proxy and does not intentionally share one technician's authenticated session with another technician.

## Do I need to create a new Google Cloud project for every computer?

**No.**

If your organization already has an established GAM7 project, use the existing project's `client_secrets.json` when configuring another trusted, organization-managed computer.

Creating a separate project for every technician or computer is generally unnecessary and can make administration more complicated.

See [GAM7 Setup & Troubleshooting](gam-setup.md).

## Is `client_secrets.json` safe to copy?

The file contains OAuth client configuration rather than a technician's authenticated session.

It can be copied between trusted, organization-managed computers when setting up GAM7.

However, it should **not** be committed to a public GitHub repository or distributed through public file-sharing services.

OAuth tokens and other authenticated GAM7 credentials are separate and should be protected accordingly.

## Will upgrading AdminTrayTool delete my configuration?

No.

AdminTrayTool stores its application configuration outside the installation directory:

```text
%ProgramData%\AdminTrayTool\
```

The following files are preserved during normal upgrades:

```text
config.json
groupTemplates.json
```

## Will upgrading AdminTrayTool delete my GAM7 authentication?

Normally, no.

GAM7 maintains its configuration under the current Windows user's profile:

```text
%USERPROFILE%\.gam\
```

The AdminTrayTool application installer does not normally replace this user-specific GAM7 configuration.

## Can I use AdminTrayTool without GAM7?

Yes, for the features that do not depend on Google Workspace.

The following features can operate independently of GAM7:

* Admin Web Portals
* Remote Desktop Connections
* Admin Tools
* General application configuration

The following features require GAM7 and appropriate Google Workspace permissions:

* Chromebook Management
* Group Management

## Does AdminTrayTool replace ServiceNow?

No.

AdminTrayTool is intended as a technician-focused utility for routine administrative tasks.

It can complement an organization's existing service-management, ticketing, asset-management, and knowledge-management systems.

## Where can I report a bug or request a feature?

Open an issue on the [AdminTrayTool GitHub repository](https://github.com/nitheraz/AdminTrayTool/issues).

## How do I check my AdminTrayTool version?

Open:

**Tray icon → About AdminTrayTool**

The About window displays the installed version and provides an option to check for updates.
