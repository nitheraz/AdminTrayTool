# Chromebook Management

Access via the tray icon → **Chromebook Management**.

## Searching for a Device

Use the **Search By** dropdown to choose between:

- **Serial Number** — direct lookup, fastest.
- **Asset ID** — searches your domain's full device list for a match. The first search may take a few seconds on large domains; results are cached locally for 4 hours, so repeat searches are near-instant.

Enter the value and click **Look Up**, or press Enter.

## Device Information

Once a device is found, you'll see:

| Field | Description |
|---|---|
| Serial Number | The device's hardware serial |
| Asset ID | Editable — click **Save** after typing a new value |
| Google Device ID | The device's unique ID in Google Admin |
| Model | Device model (normalized for common naming quirks) |
| Wi-Fi MAC | Click **Copy** to copy to clipboard |
| Organisation Unit | The device's current OU in Google Admin |
| Last Sync | Last time the device checked in |

## Actions

- **Refresh** — re-fetches the latest info for the currently loaded device.
- **Disable Chromebook** — disables the device in Google Admin. Requires confirmation.
- **Re-enable Chromebook** — re-enables a previously disabled device. Requires confirmation.

All actions are logged in the **GAM Output** panel at the bottom of the window, which shows the raw GAM7 command output for troubleshooting.