# Group Management

Access Group Management through:

**Tray icon → Group Management**

Group Management uses GAM7 to manage Google Workspace group memberships.

## Adding a Single Staff Member

To add a staff member using a predefined template:

1. Select a **Staff Type** from the dropdown.
2. Enter the staff member's Google Workspace email address.
3. Autocomplete will suggest known users as you type.
4. Click **Add to Group(s)**.
5. A confirmation dialog displays all groups that will be updated.
6. Confirm the operation.
7. A summary displays which additions succeeded or failed.

## Custom Group

Select **Custom** from the **Staff Type** dropdown when the staff member needs to be added to a specific group that is not included in a predefined template.

The **Group Email** field becomes available and supports autocomplete.

## Group Templates

Group templates allow a staff role to be mapped to multiple Google Groups.

For example:

```text
Primary Staff
├── primary-staff@example.com
├── staff-announcements@example.com
└── primary-resources@example.com
```

Instead of manually adding a new staff member to each group, the technician can select the appropriate staff type and perform the operation once.

### Editing Templates

Templates can be managed through:

**Tray icon → Config Settings → Edit Group Templates**

Each row represents one group.

Multiple rows with the same **Template Name** are treated as a single template containing multiple groups.

For example:

| Template Name | Group Email                                                               |
| ------------- | ------------------------------------------------------------------------- |
| Primary Staff | [primary-staff@example.com](mailto:primary-staff@example.com)             |
| Primary Staff | [staff-announcements@example.com](mailto:staff-announcements@example.com) |
| Primary Staff | [primary-resources@example.com](mailto:primary-resources@example.com)     |

These rows become one **Primary Staff** template.

## Autocomplete

Staff and group email fields provide autocomplete suggestions.

The information is retrieved from Google Workspace through GAM7 and cached locally for approximately **4 hours** to keep the interface responsive and reduce repeated queries.
