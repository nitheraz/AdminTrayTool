# Chromebook Management

Access Chromebook Management through:

**Tray icon → Chromebook Management**

AdminTrayTool uses GAM7 to communicate with Google Workspace and manage ChromeOS devices.

## Searching for a Device

Use the **Search By** dropdown to choose how the device should be located.

### Serial Number

Searches directly for the Chromebook's hardware serial number.

This is normally the fastest way to locate a specific device.

### Asset ID

Searches your Google Workspace ChromeOS device inventory for a matching Asset ID.

The first Asset ID search may take several seconds on larger Google Workspace domains.

Search results are cached locally for approximately **4 hours**, making repeat searches significantly faster.

Enter the value and click **Look Up**, or press **Enter**.

## Device Information

When a Chromebook is found, AdminTrayTool displays information about the device.

| Field                 | Description                                                                                   |
| --------------------- | --------------------------------------------------------------------------------------------- |
| **Serial Number**     | The Chromebook's hardware serial number.                                                      |
| **Asset ID**          | The device's Google Admin asset identifier. This can be edited and saved.                     |
| **Google Device ID**  | The unique device ID assigned by Google Admin.                                                |
| **Model**             | The Chromebook model. AdminTrayTool normalizes common model naming variations where possible. |
| **Wi-Fi MAC**         | The device's Wi-Fi MAC address. Use **Copy** to copy it to the clipboard.                     |
| **Organisation Unit** | The device's current Google Admin organizational unit.                                        |
| **Recent User**       | The most recent user associated with the Chromebook, when available.                          |
| **Last Sync**         | The last time the Chromebook synchronized with Google Admin.                                  |
| **Status**            | The current device status reported by Google Admin.                                           |

## Individual Device Actions

After locating a Chromebook, the available management actions are shown in the action area.

Some actions may require specific Google Workspace administrator permissions.

### Refresh

Re-fetches the latest device information from Google Workspace.

Use **Refresh** after making a change or when you want to confirm the current device state.

### Save Asset ID

After changing the Asset ID, click **Save** to submit the change to Google Workspace.

### Disable Chromebook

Disables the Chromebook in Google Admin.

A confirmation is required before the action is performed.

### Re-enable Chromebook

Re-enables a previously disabled Chromebook.

A confirmation is required before the action is performed.

### Move OU

Moves the Chromebook to a different Google Admin organizational unit.

Select the target OU and confirm the operation before the device is moved.

This can be useful when preparing devices for different schools, year levels, deployment groups, or management policies.

### Clear Profiles

Clears locally stored user profiles from the Chromebook.

Use this when a Chromebook needs its existing user profile data removed before being reassigned or returned to service.

A confirmation is required before the action is performed.

> **Important:** Clearing profiles affects user data stored locally on the Chromebook. Ensure any required user data has been synchronized or otherwise preserved before performing the action.

### Powerwash

Initiates a ChromeOS Powerwash on the Chromebook.

Powerwashing resets the device and removes locally stored user data and settings.

A confirmation is required before the action is performed.

> **Important:** Powerwash is a destructive operation. Verify that the correct Chromebook has been located before confirming the action.

## Bulk Management

**Bulk Management** allows administrators to perform Chromebook management actions against multiple devices instead of processing each device individually.

This is useful for large-scale device administration, such as preparing a group of Chromebooks for deployment, changing their organizational unit, or performing maintenance actions across multiple devices.

### Selecting Devices

Add or select the Chromebooks that should be included in the bulk operation.

Review the selected devices before starting an action to ensure the correct devices are included.

### Available Bulk Actions

Depending on the current version and available Google Workspace permissions, bulk management can be used for supported Chromebook actions such as:

* **Move OU** — move multiple Chromebooks to a selected organizational unit.
* **Disable Chromebook** — disable multiple Chromebooks.
* **Re-enable Chromebook** — re-enable multiple Chromebooks.
* **Clear Profiles** — clear profiles from multiple Chromebooks.
* **Powerwash** — initiate a Powerwash on multiple Chromebooks.

Bulk operations should be reviewed carefully before confirmation because an action may affect every selected device.

### Bulk Operation Progress

When a bulk action is started, AdminTrayTool processes the selected devices and reports the results.

The operation output can be used to identify:

* Successfully processed devices
* Devices that failed
* GAM7 or Google Workspace errors
* Devices requiring further investigation

If an individual device fails during a bulk operation, the remaining devices may still be processed depending on the operation and error encountered.

## Clearing the Form

Use **Clear Form** to remove the current device information and return the form to its initial state.

This does not make any changes to the Chromebook in Google Workspace.

## GAM Output

All GAM7 operations are displayed in the **GAM Output** panel at the bottom of the window.

This output is useful when troubleshooting failed commands, authentication problems, individual device actions, or bulk operations.

The output may contain technical information returned directly by GAM7 and Google Workspace.

## Permissions

Chromebook management actions depend on the Google Workspace administrator permissions available to the authenticated GAM7 account.

Bulk operations use the same underlying Google Workspace permissions as the corresponding individual actions.

If an action fails, check the **GAM Output** panel for the GAM7 or Google Workspace error message.

For GAM7 project configuration and authentication troubleshooting, see [GAM7 Setup & Troubleshooting](../gam-setup.md).
