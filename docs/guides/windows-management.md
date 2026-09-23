# Windows Management

Windows Management provides a central entry point for Windows administration tasks within AdminTrayTool.

From **Windows Management**, you can access:

* **Active Directory Management**
* **MECM / SCCM Management**

The Windows Management screen provides a consistent interface for Windows administration tools without requiring technicians to open each management function separately.

## Opening Windows Management

From the AdminTrayTool tray menu:

**Tray icon → Windows Management**

The Windows Management window provides separate options for Active Directory and MECM / SCCM.

## Active Directory

Select **Active Directory** to open the Active Directory Management tool.

Active Directory Management allows you to:

* Look up a computer by computer name.
* View computer information.
* View the operating system.
* View the computer's current organisational unit.
* View the computer's distinguished name.
* Search and filter available organisational units.
* Select a target organisational unit.
* Move a computer to another organisational unit.

See [Active Directory Management](active-directory-management.md) for more information.

## MECM / SCCM

Select **MECM / SCCM** to open the MECM management tool.

MECM Management allows you to:

* Look up a computer.
* View computer information.
* Manage MECM collection membership.
* Add a computer to a collection.
* Remove a computer from a collection.
* Review actions through the activity log.

See [MECM / SCCM Management](mecm-management.md) for more information.

## Permissions

Windows Management functions depend on the permissions available to the Windows account running AdminTrayTool.

Active Directory and MECM / SCCM operations may require appropriate administrative permissions and access to the relevant management infrastructure.

AdminTrayTool does not grant additional Windows or MECM permissions by itself.

## Closing the Management Windows

The Windows Management, Active Directory Management, and MECM / SCCM Management windows include a **FORM ACTIONS** section with a **CLOSE** button.

Select **CLOSE** when you have finished working in the management window.