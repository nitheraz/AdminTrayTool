# MECM / SCCM Management

MECM / SCCM Management provides IT administrators with a central interface for looking up Windows computers and managing MECM collection membership.

MECM is also commonly referred to as **SCCM** or **Configuration Manager**.

## Opening MECM Management

From the AdminTrayTool tray menu:

**Tray icon → Windows Management → MECM / SCCM**

## Computer Lookup

Enter a computer name in the **Computer Name** field and select **LOOK UP**.

You can also press **Enter** after entering the computer name.

After a successful lookup, AdminTrayTool displays available computer information.

The activity log records the lookup operation and its result.

## Computer Information

The MECM management screen provides computer information such as:

* Computer name
* Operating system
* Manufacturer
* Model
* Serial number
* Collection information when available

This provides technicians with the information needed to identify the correct Windows device before performing collection actions.

## MECM Collections

The **MECM COLLECTION** section provides collection management actions.

Select the required collection and use:

* **ADD** — add the computer to the selected MECM collection.
* **REMOVE** — remove the computer from the selected MECM collection.

Collection changes require confirmation before the operation is performed.

## Adding a Computer to a Collection

1. Look up the computer.
2. Select the required collection.
3. Select **ADD**.
4. Review the confirmation message.
5. Select **Yes** to continue.

The result is recorded in the activity log.

## Removing a Computer from a Collection

1. Look up the computer.
2. Select the required collection.
3. Select **REMOVE**.
4. Review the confirmation message.
5. Select **Yes** to continue.

The result is recorded in the activity log.

## Activity Log

The **ACTIVITY LOG** provides a record of actions performed during the current session.

It can include:

* Computer lookups
* Lookup failures
* Collection additions
* Collection removals
* Errors returned by MECM

Each entry includes the time the event occurred.

## Confirmation

Collection changes require confirmation before the operation is performed.

The confirmation identifies:

* The computer
* The selected collection
* The action that will be performed

This provides an opportunity to verify the target before making a collection membership change.

## Permissions

MECM operations require appropriate access to the organisation's Configuration Manager infrastructure.

AdminTrayTool does not grant additional MECM permissions.

If an operation fails, review the activity log and the error message returned by MECM.

## Closing the Window

When finished, select:

**FORM ACTIONS → CLOSE**

The window closes without exiting AdminTrayTool.