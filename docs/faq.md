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
* Bulk Chromebook Management
* Group Management

## Does Bulk Chromebook Management require me to check every device first?

**No.**

The **Check Devices** step is optional.

You can import a valid CSV, select devices, and use **Review Action** without first checking every device against Google Workspace.

Checking devices is useful when you want to verify that serial numbers exist in Google Workspace and, where available, retrieve information such as the current Asset ID.

The actual bulk operation determines whether the requested action succeeds for each selected device.

## What CSV format does Bulk Chromebook Management use?

The required CSV columns depend on the selected action.

For **Disable Chromebook**, **Re-enable Chromebook**, **Powerwash**, and **Clear Profiles**, the CSV requires:

```text
serialNumber
```

For **Move to OU**, the CSV requires:

```text
serialNumber
```

The target organizational unit is selected separately in AdminTrayTool.

For **Update Asset ID**, the CSV requires:

```text
serialNumber,AssetId
```

Use **Download Template** in Bulk Chromebook Management to generate the appropriate template for the selected action.

## Can I update Asset IDs in bulk?

**Yes.**

Select **Update Asset ID** as the bulk action and import a CSV containing:

```text
serialNumber,AssetId
```

For example:

```text
serialNumber,AssetId
ABC123456,ASSET-1001
ABC123457,ASSET-1002
```

The current Asset ID does not need to be checked before performing the update. The new Asset ID is taken from the CSV.

## What happens if one device fails during a bulk operation?

AdminTrayTool processes the selected devices individually and records the result for each device.

A failed device is shown with its error information where available.

Other selected devices can continue to be processed rather than treating the entire operation as a single all-or-nothing transaction.

Use **Export Results** if you need to retain a record of the operation or identify devices that require further investigation.

## Can I undo a bulk Powerwash or Clear Profiles operation?

These actions should be treated as destructive operations.

**Powerwash** resets the Chromebook and removes locally stored user data and settings.

**Clear Profiles** removes locally stored user profiles.

Review the selected devices and the confirmation warning carefully before executing either action.

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