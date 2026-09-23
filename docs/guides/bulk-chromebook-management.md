## Bulk Chromebook Management

AdminTrayTool includes a dedicated **Bulk Chromebook Management** tool for processing multiple Chromebooks in one operation.

Access it through:

**Tray icon → Chromebook Management → Bulk Management**

Bulk Management allows supported Chromebook actions to be performed against multiple devices using CSV files.

The bulk workflow is:

**Select Action → Download Template → Import CSV → Select Devices → Review Action → Execute**

Checking devices against Google Workspace is available as an optional step when current device information is required.

---

## Available Actions

Bulk Chromebook Management currently supports:

* **Disable Chromebook**
* **Re-enable Chromebook**
* **Move to OU**
* **Powerwash**
* **Clear Profiles**
* **Update Asset ID**

The CSV requirements depend on the selected action.

| Action | Required CSV columns |
|---|---|
| **Disable Chromebook** | `serialNumber` |
| **Re-enable Chromebook** | `serialNumber` |
| **Move to OU** | `serialNumber` |
| **Powerwash** | `serialNumber` |
| **Clear Profiles** | `serialNumber` |
| **Update Asset ID** | `serialNumber`, `AssetId` |

---

## Bulk Management Workflow

The Bulk Chromebook Management window is designed around an action-first workflow.

### 1. Select an Action

Use the **Action** dropdown to select the operation you want to perform.

If **Move to OU** is selected, an **Organisational Unit** selector is also displayed.

The selected action determines which CSV template is generated.

### 2. Download the Template

Click **Download Template** to generate a CSV template for the selected action.

The template automatically contains the columns required for that operation.

### 3. Prepare the CSV

Open the downloaded CSV and add the Chromebook information.

For actions that only require serial numbers:

```text
serialNumber
ABC123456
ABC123457
ABC123458
```

For **Update Asset ID**:

```text
serialNumber,AssetId
ABC123456,ASSET-10001
ABC123457,ASSET-10002
ABC123458,ASSET-10003
```

### 4. Import the CSV

Click **Import CSV** and select the completed CSV file.

AdminTrayTool performs local validation of the CSV before adding devices to the list.

### 5. Select Devices

After the CSV has been imported successfully, the devices are available in the list for selection.

Devices do **not** need to be checked against Google Workspace before they can be selected.

### 6. Optionally Check Devices

Click **Check Devices** if you want AdminTrayTool to look up the devices in Google Workspace using GAM7.

This step is optional and can take time for larger lists.

### 7. Review the Action

Select the devices to process and click **Review Action**.

AdminTrayTool displays the operation that will be performed before any changes are made.

### 8. Execute the Action

After confirming the review, AdminTrayTool processes the selected devices individually.

Each device receives its own result.

### 9. Export Results

Use **Export Results** to save the results of the bulk operation to a CSV file.

---

## Download Template

The **Download Template** button creates a CSV template based on the currently selected action.

Templates are generated dynamically.

### Serial Number Template

The following actions use a template containing only:

```text
serialNumber
```

* Disable Chromebook
* Re-enable Chromebook
* Move to OU
* Powerwash
* Clear Profiles

Example:

```text
serialNumber
ABC123456
ABC123457
ABC123458
```

### Update Asset ID Template

The **Update Asset ID** action uses:

```text
serialNumber,AssetId
```

Example:

```text
serialNumber,AssetId
ABC123456,ASSET-10001
ABC123457,ASSET-10002
ABC123458,ASSET-10003
```

Using the generated template is recommended because it ensures that the correct column names are used.

---

## Importing Chromebooks

Click **Import CSV** and select the CSV file containing the devices.

AdminTrayTool checks the CSV locally before importing it into the device list.

The required columns depend on the selected action.

### Serial Number Actions

For the following actions:

* Disable Chromebook
* Re-enable Chromebook
* Move to OU
* Powerwash
* Clear Profiles

the CSV must contain:

```text
serialNumber
```

For example:

```text
serialNumber
ABC123456
ABC123457
ABC123458
```

### Update Asset ID

For **Update Asset ID**, the CSV must contain:

```text
serialNumber,AssetId
```

For example:

```text
serialNumber,AssetId
ABC123456,ASSET-10001
ABC123457,ASSET-10002
ABC123458,ASSET-10003
```

The Asset ID supplied in the CSV becomes the requested new Asset ID for that device.

---

## CSV Validation

CSV validation occurs locally during import.

This validation is separate from checking the devices against Google Workspace.

AdminTrayTool verifies that the imported CSV contains the information required for the selected action.

For example, an **Update Asset ID** import requires both:

```text
serialNumber,AssetId
```

A CSV containing only:

```text
serialNumber
ABC123456
```

does not contain enough information for an Asset ID update.

Using **Download Template** is the easiest way to ensure that the correct columns are present.

---

## Duplicate Devices

Duplicate serial numbers are detected during import.

Only the first occurrence of a serial number is added to the device list.

Duplicate entries are skipped and the number of duplicates skipped is reported in the import status.

---

## Checking Devices

The **Check Devices** function performs a lookup against Google Workspace using GAM7.

It can be used when the technician wants to confirm the current state of the imported devices before processing them.

Checking can provide information such as:

* Whether the Chromebook exists in Google Workspace.
* The current device status.
* The current Asset ID.

### Checking is Optional

A device does **not** need to be successfully checked before it can be reviewed or processed.

This is an important part of the bulk workflow.

For example, a technician can import:

```text
serialNumber,AssetId
ABC123456,ASSET-10001
ABC123457,ASSET-10002
```

and proceed directly to reviewing the operation without first checking the devices.

If current device information has not been retrieved, the relevant information is displayed as:

**Not checked**

This is particularly useful for **Update Asset ID**, because the current Asset ID is not required to perform the update.

### Checking Large Lists

Checking devices can take time because AdminTrayTool performs Google Workspace/GAM7 lookups.

For large CSV files, the technician can skip the check when current Google Workspace information is not required.

---

## Device Status

When devices are checked, the status column can provide information about the lookup.

Typical statuses include:

| Status | Description |
|---|---|
| **NOT CHECKED** | The device has not been checked against Google Workspace. |
| **CHECKING...** | The device is currently being checked. |
| **FOUND** | The Chromebook was found in Google Workspace. |
| **NOT FOUND** | No matching Chromebook was found. |
| **ERROR** | An error occurred while checking the device. |

A **FOUND** status confirms that the lookup found a matching Chromebook.

However, **FOUND is not a prerequisite for reviewing or executing a bulk action**.

The result of the actual bulk operation determines whether the operation succeeds or fails.

---

## Selecting Devices

Bulk Management provides controls for selecting devices from the imported list.

### Select All

Selects all devices in the list.

### Select None

Clears the current device selection.

The interface displays the number of currently selected devices.

Devices can be selected regardless of whether they have been checked against Google Workspace.

---

## Choosing a Bulk Action

Select the required operation from the **Action** dropdown.

The available actions are described below.

---

## Disable Chromebook

Disables each selected Chromebook in Google Admin.

CSV format:

```text
serialNumber
ABC123456
ABC123457
ABC123458
```

Only the `serialNumber` column is required.

---

## Re-enable Chromebook

Re-enables each selected Chromebook in Google Admin.

CSV format:

```text
serialNumber
ABC123456
ABC123457
ABC123458
```

Only the `serialNumber` column is required.

---

## Move to OU

Moves each selected Chromebook to the selected Google Admin organisational unit.

CSV format:

```text
serialNumber
ABC123456
ABC123457
ABC123458
```

The destination OU is selected in AdminTrayTool rather than being included in the CSV.

### Selecting the OU

When **Move to OU** is selected:

1. The **Organisational Unit** selector is displayed.
2. Select the destination OU.
3. Import the CSV.
4. Select the devices.
5. Review the action.
6. Confirm the operation.

The same CSV can therefore be reused for different destination OUs.

---

## Powerwash

Initiates a remote ChromeOS Powerwash for each selected Chromebook.

CSV format:

```text
serialNumber
ABC123456
ABC123457
ABC123458
```

Only the `serialNumber` column is required.

Powerwash is a destructive operation that resets the selected Chromebook.

Carefully review the selected devices before confirming the operation.

---

## Clear Profiles

Clears user profiles from each selected Chromebook.

CSV format:

```text
serialNumber
ABC123456
ABC123457
ABC123458
```

Only the `serialNumber` column is required.

Carefully review the selected devices before confirming the operation.

---

## Update Asset ID

Updates the Google Admin **Annotated Asset ID** for each selected Chromebook.

CSV format:

```text
serialNumber,AssetId
ABC123456,ASSET-10001
ABC123457,ASSET-10002
ABC123458,ASSET-10003
```

Both columns are required.

The `AssetId` value supplied in the CSV is used as the new Asset ID.

### Current Asset ID Is Not Required

The current Asset ID does not need to be retrieved before performing the update.

For example:

```text
serialNumber,AssetId
ABC123456,ASSET-10001
```

can be imported and processed without first running **Check Devices**.

If the current Asset ID has not been retrieved, the device list displays:

**Not checked**

The requested new Asset ID is displayed separately so that the technician can verify what will be written to Google Admin.

---

## Review Before Processing

After selecting the devices and action, click **Review Action**.

AdminTrayTool displays a confirmation showing the operation that will be performed.

The review includes relevant information such as:

* The selected action.
* The number of selected devices.
* The selected serial numbers.
* The target organisational unit when using **Move to OU**.
* The requested Asset IDs when using **Update Asset ID**.
* Appropriate warnings for potentially destructive operations.

No changes are made during the review.

The action is only performed after the technician confirms the operation.

### Review Does Not Require Device Validation

A device does not have to have a **FOUND** status before it can be reviewed.

This allows technicians to prepare bulk operations without waiting for a separate Google Workspace lookup.

---

## Bulk Processing

After confirmation, AdminTrayTool processes each selected Chromebook individually using GAM7.

The interface displays:

* The current device being processed.
* The current device number.
* The total number of devices.
* A progress bar.
* The result for each device.

Each device is processed independently.

A failure affecting one device does not necessarily mean that the other selected devices will fail.

---

## Operation Results

The result of the actual GAM7 operation determines whether the device was successfully processed.

Typical results include:

| Result | Description |
|---|---|
| **PROCESSING...** | The operation is currently being performed. |
| **SUCCESS** | The GAM7 operation completed successfully. |
| **FAILED** | The operation failed. |

If an operation fails, the **Error** column contains the available error information returned by GAM7 or the application.

The final summary displays the number of successful and failed operations.

For example:

```text
Bulk action complete.

Successful: 18
Failed: 2
```

The execution result is authoritative.

A device does not need to have been successfully checked beforehand in order for the actual operation to succeed.

---

## Results Table

The device list contains the following information:

| Column | Description |
|---|---|
| **Serial Number** | Chromebook serial number. |
| **Status** | Import or Google Workspace lookup status. |
| **Current Asset ID** | Current Asset ID retrieved during a device check, when available. |
| **New Asset ID** | Requested Asset ID supplied by the CSV for an Asset ID update. |
| **Result** | Result of the actual bulk operation. |
| **Error** | Error information when an operation fails. |

When a device has not been checked, information that requires a Google Workspace lookup may display:

**Not checked**

---

## Exporting Results

Use **Export Results** to save the current bulk operation results to a CSV file.

The exported results include information such as:

| Column | Description |
|---|---|
| **SerialNumber** | Chromebook serial number. |
| **Status** | Current import or lookup status. |
| **CurrentAssetId** | Current Asset ID when it has been retrieved. |
| **NewAssetId** | Requested new Asset ID when applicable. |
| **Result** | Result of the bulk operation. |
| **Error** | Error information when an operation failed. |

The default filename includes the date and time of the export.

For example:

```text
Chromebook-Bulk-Results-20260923-103015.csv
```

Exported results can be retained for operational records or used to identify devices requiring further investigation.

---

## Clearing the Bulk List

Use **Clear List** to remove all imported devices and their associated status and action results from the Bulk Management window.

This allows the technician to start a new bulk operation without closing the window.

Clearing the list does **not** make any changes to Chromebooks in Google Workspace.

---

## Recommended Bulk Workflow

For a typical bulk operation:

### Step 1 — Select the Action

Select the required action from the **Action** dropdown.

For example:

**Update Asset ID**

### Step 2 — Download the Template

Click **Download Template**.

### Step 3 — Prepare the CSV

Populate the generated template.

For example:

```text
serialNumber,AssetId
ABC123456,ASSET-10001
ABC123457,ASSET-10002
ABC123458,ASSET-10003
```

### Step 4 — Import the CSV

Click **Import CSV** and select the completed CSV.

AdminTrayTool validates the CSV locally.

### Step 5 — Review the Device List

Confirm that the expected devices have been imported.

### Step 6 — Optionally Check Devices

Click **Check Devices** if current Google Workspace information is required.

This step can be skipped.

### Step 7 — Select Devices

Select the devices that should be processed.

### Step 8 — Review the Action

Click **Review Action**.

Confirm the:

* Action.
* Devices.
* Organisational Unit, if applicable.
* Requested Asset IDs, if applicable.

### Step 9 — Execute

Confirm the operation and allow AdminTrayTool to process the selected devices.

### Step 10 — Review Results

Check the **Result** and **Error** columns.

### Step 11 — Export Results

Use **Export Results** if a record of the operation is required.

---

## Example: Bulk Asset ID Update

A technician receives the following CSV:

```text
serialNumber,AssetId
CROS001,CHROME-001
CROS002,CHROME-002
CROS003,CHROME-003
```

The technician:

1. Selects **Update Asset ID**.
2. Imports the CSV.
3. Confirms that the three devices appear.
4. Optionally runs **Check Devices**.
5. Selects the devices to update.
6. Clicks **Review Action**.
7. Confirms the requested Asset IDs.
8. Confirms the operation.
9. Reviews the individual results.
10. Exports the results if required.

The current Asset ID does not need to be known before the update is executed.

---

## Example: Bulk Move to OU

CSV:

```text
serialNumber
CROS001
CROS002
CROS003
```

The technician:

1. Selects **Move to OU**.
2. Selects the destination OU from the **Organisational Unit** selector.
3. Imports the CSV.
4. Confirms the device list.
5. Optionally runs **Check Devices**.
6. Selects the devices.
7. Clicks **Review Action**.
8. Confirms the destination OU.
9. Executes the operation.
10. Reviews the results.

The destination OU is selected in the application and is not required in the CSV.

---

## Safety Considerations

Bulk actions can affect multiple Chromebooks, so review the selected devices carefully before confirming an operation.

In particular:

* Confirm that the correct action is selected.
* Confirm that the correct devices are selected.
* Verify the target organisational unit when using **Move to OU**.
* Verify the requested Asset IDs when using **Update Asset ID**.
* Carefully review the operation before using **Powerwash**.
* Carefully review the operation before using **Clear Profiles**.
* Use **Check Devices** when current Google Workspace information is required.
* Use **Export Results** when an operational record is required.

Bulk operations require the appropriate Google Workspace permissions for the selected action.