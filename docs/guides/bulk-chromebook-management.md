## Bulk Chromebook Management

AdminTrayTool includes a dedicated **Bulk Chromebook Management** tool for processing multiple Chromebooks in one operation.

Access it through:

**Tray icon → Chromebook Management → Bulk Management**

Bulk Management works from a list of Chromebook serial numbers and allows supported management actions to be performed across multiple devices.

### Importing Chromebooks

Chromebooks can be imported from a CSV file.

Click **Import CSV** and select the file containing the Chromebook list.

The CSV must contain a column named:

```text
serialNumber
```

The column can be part of a larger Google Admin export. AdminTrayTool searches the CSV header for the `serialNumber` column and extracts the serial numbers from that column.

For example:

```text
serialNumber
ABC123456
ABC123457
ABC123458
```

Other columns in the CSV are ignored during import.

### Import Template

Use **Import Template** to create a blank CSV template.

The generated template contains the required:

```text
serialNumber
```

column.

This can be used as a starting point when creating a custom bulk device list.

### Duplicate Devices

Duplicate serial numbers are automatically detected during import.

Only the first occurrence is added to the device list. Duplicate entries are skipped and the number of duplicates skipped is displayed in the status message.

## Validating Devices

Before performing a bulk action, the imported Chromebooks should be validated.

Click **Validate Devices** to check each serial number against Google Workspace using GAM7.

Each device is given a validation status:

| Status            | Description                                                |
| ----------------- | ---------------------------------------------------------- |
| **NOT VALIDATED** | The device has been imported but has not yet been checked. |
| **CHECKING...**   | The device is currently being checked.                     |
| **FOUND**         | The Chromebook was found in Google Workspace.              |
| **NOT FOUND**     | No matching Chromebook was found.                          |
| **ERROR**         | An error occurred while attempting to validate the device. |

When a Chromebook is found, its Asset ID is also displayed.

Only devices with a **FOUND** status can be selected for bulk actions.

## Selecting Devices

Bulk Management provides several selection controls:

### Select All

Selects all devices in the list.

### Select Found

Selects only devices that were successfully validated and have a **FOUND** status.

This is useful when an imported list contains serial numbers that are no longer present in Google Workspace.

### Select None

Clears the current device selection.

The interface displays the number of currently selected devices.

## Choosing a Bulk Action

Select an action from the **Action** dropdown.

The available actions are:

### Disable Chromebook

Disables each selected Chromebook in Google Admin.

### Re-enable Chromebook

Re-enables each selected Chromebook.

### Move to OU

Moves each selected Chromebook to a specified Google Admin organisational unit.

When **Move to OU** is selected, AdminTrayTool loads the available organisational units from Google Workspace.

Select the target organisational unit before reviewing the action.

### Powerwash

Initiates a remote ChromeOS Powerwash for each selected Chromebook.

Powerwash resets the selected Chromebooks and removes locally stored user data and settings.

Because this is a destructive operation, AdminTrayTool displays an additional warning during the review step.

### Clear Profiles

Clears user profiles from each selected Chromebook.

Because this can remove locally stored user data, AdminTrayTool displays an additional warning during the review step.

## Review Before Processing

After selecting the devices and action, click **Review Action**.

AdminTrayTool displays a confirmation showing:

* The selected action
* The number of devices
* The serial numbers of the selected devices
* The target organisational unit when using **Move to OU**
* Additional warnings for **Powerwash** and **Clear Profiles**

No changes are made until the action is confirmed.

## Bulk Processing

After confirmation, AdminTrayTool processes the selected Chromebooks individually.

The interface displays:

* The current device being processed
* The current device number
* Total number of devices
* A progress bar
* The result for each device

Each device receives one of the following results:

| Result            | Description                                |
| ----------------- | ------------------------------------------ |
| **PROCESSING...** | The action is currently being performed.   |
| **SUCCESS**       | The GAM7 operation completed successfully. |
| **FAILED**        | The operation failed.                      |

If an operation fails, the **Error** column contains the available error information returned by GAM7 or the application.

A final summary displays the number of successful and failed operations.

For example:

```text
Bulk action complete.

Successful: 18
Failed: 2
```

## Exporting Results

Use **Export Results** to save the current bulk operation results to a CSV file.

The exported file contains:

| Column           | Description                                 |
| ---------------- | ------------------------------------------- |
| **SerialNumber** | Chromebook serial number.                   |
| **Status**       | Validation status.                          |
| **AssetId**      | Asset ID returned during validation.        |
| **Result**       | Result of the bulk action.                  |
| **Error**        | Error information when an operation failed. |

The default filename includes the date and time of the export.

For example:

```text
Chromebook-Bulk-Results-20260923-103015.csv
```

The exported results can be retained for operational records or used to identify devices that require further investigation.

## Clearing the Bulk List

Use **Clear List** to remove all imported devices and their validation and action results from the Bulk Management window.

This does not make any changes to the Chromebooks in Google Workspace.

## Bulk Management Safety

Bulk actions can affect multiple Chromebooks, so review the selected devices carefully before confirming an operation.

In particular:

* Confirm that the correct devices are selected.
* Check the validation status before performing an action.
* Verify the target organisational unit when using **Move to OU**.
* Carefully review the warning before using **Powerwash**.
* Carefully review the warning before using **Clear Profiles**.
* Use **Export Results** when you need a record of the operation.

Bulk operations require the appropriate Google Workspace permissions for the selected action.
