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

**Bulk Management** allows administrators to perform supported Chromebook management actions against multiple devices instead of processing each device individually.

Access it through:

**Tray icon → Chromebook Management → Bulk Management**

Bulk Management uses action-specific CSV templates so that the information required for each operation is clear.

### Bulk Management Workflow

The general workflow is:

1. **Select Action**
2. **Download Template** if required
3. **Import CSV**
4. **Review imported devices**
5. **Check Devices** if Google Workspace validation is required
6. **Select Devices**
7. **Review Action**
8. **Confirm and Execute**
9. **Export Results** if required

**Check Devices** is optional. It performs Google Workspace lookups using GAM7 and can take some time when processing a large list.

A valid CSV import can be reviewed without first checking every device against Google Workspace.

### CSV Templates

The required CSV columns depend on the selected action.

#### Disable Chromebook, Re-enable Chromebook, Powerwash and Clear Profiles

These actions require:

```text
serialNumber
```

Example:

```text
serialNumber
ABC123456
ABC123457
ABC123458
```

#### Move to OU

Move to OU uses:

```text
serialNumber
```

The target organizational unit is selected separately in the Bulk Management window.

#### Update Asset ID

Update Asset ID requires:

```text
serialNumber,AssetId
```

Example:

```text
serialNumber,AssetId
ABC123456,ASSET-1001
ABC123457,ASSET-1002
ABC123458,ASSET-1003
```

Use **Download Template** after selecting an action to generate the appropriate CSV template.

### Importing a CSV

Click **Import CSV** and select the prepared CSV file.

AdminTrayTool validates the CSV locally before adding devices to the list.

This validation checks that:

* The required columns are present.
* Required values are provided.
* Serial numbers are valid for import.
* Duplicate serial numbers are identified.

Invalid rows are not treated as valid devices for processing.

Duplicate serial numbers are automatically detected so that the same device is not added multiple times.

### Checking Devices

Click **Check Devices** when you want to verify the imported serial numbers against Google Workspace.

AdminTrayTool uses GAM7 to check the devices and can retrieve information such as the current Asset ID.

Checking devices is particularly useful when you want to confirm that the serial numbers exist in Google Workspace before performing an operation.

For **Update Asset ID**, the current Asset ID does not need to be checked before the action can be executed. The new Asset ID is supplied by the CSV.

If the current Asset ID has not been checked, it may be displayed as **Not checked**.

### Selecting Devices

Use the selection controls in the Bulk Management window to select the devices that should be processed.

Review the device list carefully before continuing.

The Bulk Management window displays information including:

| Column                | Description                                      |
| --------------------- | ------------------------------------------------ |
| **Serial Number**     | Chromebook serial number.                        |
| **Status**            | Current validation/check status.                 |
| **Current Asset ID**  | Current Asset ID when available.                 |
| **New Asset ID**      | Asset ID requested by an Update Asset ID action. |
| **Result**            | Result of the bulk operation.                    |
| **Error**             | Error information when an operation fails.       |

A device does not necessarily need to have been checked against Google Workspace before it can be included in the review.

The actual bulk operation determines whether the requested action succeeds for each device.

### Available Bulk Actions

The available actions are:

* **Disable Chromebook** — disable selected Chromebooks.
* **Re-enable Chromebook** — re-enable selected Chromebooks.
* **Move to OU** — move selected Chromebooks to the organizational unit selected in the interface.
* **Powerwash** — initiate a ChromeOS Powerwash on selected Chromebooks.
* **Clear Profiles** — clear user profiles from selected Chromebooks.
* **Update Asset ID** — update the Google Admin Asset ID for selected Chromebooks using the Asset ID supplied in the CSV.

### Review Action

After selecting the devices and action, click **Review Action**.

AdminTrayTool displays a summary of the operation before any changes are made.

The review includes information such as:

* The selected action
* The number of selected devices
* The selected serial numbers
* The target organizational unit when using **Move to OU**
* The requested Asset IDs when using **Update Asset ID**
* Additional warnings for destructive actions such as **Powerwash** and **Clear Profiles**

No changes are made until the operation is confirmed.

### Bulk Operation Progress

After confirmation, AdminTrayTool processes the selected devices individually.

The interface reports the current progress and the result for each device.

Each device receives a result indicating whether the operation succeeded or failed.

If an individual device fails, the error information is displayed in the **Error** column.

A failure on one device does not necessarily prevent the remaining selected devices from being processed.

### Exporting Results

Use **Export Results** to save the current operation results to a CSV file.

Exported results can be retained for operational records or used to identify devices that require further investigation.

The exported results include information such as:

* Serial number
* Status
* Current Asset ID
* New Asset ID when applicable
* Operation result
* Error information

### Clearing the Bulk List

Use **Clear List** to remove the imported devices and their current results from the Bulk Management window.

This does not make any changes to the Chromebooks in Google Workspace.

## Clearing the Form

Use **Clear Form** to remove the current device information and return the individual Chromebook Management form to its initial state.

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