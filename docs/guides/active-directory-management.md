# Active Directory Management

Active Directory Management allows IT administrators to look up Windows computers and move them between organisational units.

## Opening Active Directory Management

From the AdminTrayTool tray menu:

**Windows Management → Active Directory**

## Computer Lookup

Enter a computer name in the **Computer Name** field and select **LOOK UP**.

You can also press **Enter** after entering the computer name.

After a successful lookup, AdminTrayTool displays:

* Computer name
* Operating system
* Current organisational unit
* Distinguished name

The activity log records the lookup operation and the computer's current organisational unit.

## Organisational Units

After a successful computer lookup, AdminTrayTool loads the available organisational units.

Use **Filter OUs** to narrow the list of available organisational units.

The filter searches the organisational unit names and updates the list as you type.

## Moving a Computer

To move a computer to another organisational unit:

1. Look up the computer.
2. Enter text into **Filter OUs** if required.
3. Select the target organisational unit.
4. Select **MOVE COMPUTER**.
5. Review the confirmation message.
6. Select **Yes** to continue.

AdminTrayTool records the result of the move in the activity log.

After a successful move, the computer is looked up again so the displayed Active Directory information can be refreshed.

## Confirmation

Computer moves require confirmation before the operation is performed.

The confirmation displays:

* The computer name
* The target organisational unit
* A request to confirm the operation

This helps prevent accidental moves to the wrong organisational unit.

## Activity Log

The **ACTIVITY LOG** displays operations performed during the current session, including:

* Computer lookups
* Lookup failures
* Organisational unit loading
* Computer moves
* Errors returned by Active Directory operations

Each entry includes the time the event occurred.

## Permissions

Active Directory operations require appropriate permissions in the Windows domain environment.

AdminTrayTool uses the Windows management service available to the application and does not bypass Active Directory permissions.

## Closing the Window

When finished, select:

**FORM ACTIONS → CLOSE**

The window closes without exiting AdminTrayTool.
