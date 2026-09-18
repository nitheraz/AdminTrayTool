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

## Actions

Depending on the device and the available permissions, AdminTrayTool provides the following actions:

### Refresh

Re-fetches the latest device information from Google Workspace.

### Save Asset ID

After changing the Asset ID, click **Save** to submit the change to Google Workspace.

### Disable Chromebook

Disables the Chromebook in Google Admin.

A confirmation is required before the action is performed.

### Re-enable Chromebook

Re-enables a previously disabled Chromebook.

A confirmation is required before the action is performed.

## GAM Output

All GAM7 operations are displayed in the **GAM Output** panel at the bottom of the window.

This output is useful when troubleshooting failed commands or authentication problems.

The output may contain technical information returned directly by GAM7 and Google Workspace.
