# Windows Management Setup

Windows Management provides quick access to common Windows administration tools from AdminTrayTool.

The current Windows Management section provides access to:

- Active Directory
- Microsoft Endpoint Configuration Manager (MECM)

AdminTrayTool does not install or configure these Microsoft management tools. The required tools and access must already be available on the technician's Windows computer.

---

## Prerequisites

Before using Windows Management, make sure the technician's computer has the appropriate management tools installed.

### Active Directory

To use the Active Directory option, the computer must have the required **Active Directory administration tools** available.

On supported Windows editions, these tools are normally provided through:

**Remote Server Administration Tools (RSAT)**

The Active Directory Users and Computers console is commonly available as:

```text
dsa.msc
```

You can test whether it is available by opening PowerShell or Command Prompt and running:

```powershell
dsa.msc
```

If Active Directory Users and Computers opens successfully, the Active Directory component required by AdminTrayTool is available.

If Windows reports that `dsa.msc` cannot be found, install the appropriate RSAT Active Directory tools before using this option.

---

## Installing RSAT Active Directory Tools

On supported versions of Windows 10 and Windows 11, RSAT components can be installed through Windows Settings or PowerShell.

### Using Windows Settings

1. Open **Settings**.
2. Go to **System**.
3. Select **Optional features**.
4. Select **View features**.
5. Search for `RSAT`.
6. Locate the Active Directory-related RSAT component.
7. Install the component.
8. Restart Windows if requested.

The exact RSAT component names can vary between Windows versions.

### Using PowerShell

On supported systems, the Active Directory tools can also be installed using PowerShell.

Run PowerShell as Administrator and check the available RSAT components:

```powershell
Get-WindowsCapability -Online -Name RSAT*
```

Look for the Active Directory Domain Services and Lightweight Directory Services tools.

To install the Active Directory tools, use:

```powershell
Get-WindowsCapability -Online -Name Rsat.ActiveDirectory.DS-LDS.Tools~~~~0.0.1.0 |
    Add-WindowsCapability -Online
```

After installation, test the Active Directory console:

```powershell
dsa.msc
```

---

# Microsoft Endpoint Configuration Manager (MECM)

The MECM option requires the **Microsoft Endpoint Configuration Manager console** to already be installed on the technician's computer.

AdminTrayTool does not install the MECM console.

The console is commonly referred to as:

- Configuration Manager Console
- MECM Console
- Microsoft Endpoint Configuration Manager Console
- SCCM Console

The terminology used by an organisation may vary.

---

## MECM Console Access

Installing the console is not enough by itself.

The technician also needs appropriate permissions to access the organisation's Configuration Manager environment.

Depending on the organisation's configuration, this may include:

- Network connectivity to the MECM infrastructure
- VPN access when working remotely
- Access to the Configuration Manager site
- Appropriate Configuration Manager security roles
- Access to the required collections and devices
- Appropriate Windows/domain credentials

If the MECM console opens but the technician cannot see the expected devices, collections, or actions, this is normally a Configuration Manager permissions or connectivity issue rather than an AdminTrayTool issue.

---

# Verify MECM Before Using AdminTrayTool

Before testing the AdminTrayTool shortcut, open the MECM console directly on the computer.

Confirm that:

1. The console starts successfully.
2. The organisation's Configuration Manager site is available.
3. The technician can sign in or connect successfully.
4. The expected devices and collections are visible.
5. The technician can perform the administrative actions they normally require.

Once the MECM console is working normally, the Windows Management option in AdminTrayTool can be used as a shortcut to launch it.

---

# Using Windows Management

Once the prerequisites are installed:

1. Start **AdminTrayTool**.
2. Open the AdminTrayTool tray menu.
3. Select **Windows Management**.
4. The Windows Management window will appear.
5. Select the required management tool.

The available options currently include:

### Active Directory

Opens the Active Directory management tool used for managing directory objects such as users and computers.

The computer must have the required Active Directory administration tools installed.

### MECM

Opens the Microsoft Endpoint Configuration Manager administration console.

The MECM console must already be installed and configured on the computer.

---

# Permissions

AdminTrayTool does not grant Active Directory or MECM permissions.

The technician must already have the required permissions in the relevant management system.

For example:

| Tool | Required access |
|---|---|
| Active Directory | Appropriate Active Directory/domain permissions |
| MECM | Appropriate Configuration Manager security permissions |
| Windows | Permission to launch the installed management applications |

Having administrator rights on the local Windows computer does **not necessarily** provide administrative access to Active Directory or MECM.

Access is controlled by the organisation's existing Windows, Active Directory, and Configuration Manager security configuration.

---

# Remote and Off-Site Use

When working away from the organisation's network, additional connectivity may be required.

For example:

- VPN connection
- Corporate network connection
- Remote access infrastructure
- DNS/domain connectivity

If the management application launches but cannot connect to the organisation's infrastructure, verify network or VPN connectivity first.

---

# Troubleshooting

## Active Directory does not open

### Check `dsa.msc`

Open PowerShell and run:

```powershell
dsa.msc
```

If it does not open, the required RSAT Active Directory tools may not be installed.

Install the appropriate RSAT component and try again.

---

## Active Directory opens but cannot connect

If Active Directory Users and Computers opens but the expected domain or objects are unavailable:

1. Confirm the computer has network connectivity.
2. Confirm the computer can communicate with the domain.
3. Confirm VPN connectivity if working remotely.
4. Confirm the technician's account has the required permissions.
5. Test Active Directory directly without AdminTrayTool.

If the problem also occurs when launching Active Directory directly, the issue is outside AdminTrayTool.

---

## MECM does not open

First try opening the MECM console directly.

If the console is not installed, install the appropriate Microsoft Endpoint Configuration Manager console for your organisation.

If the console is installed but AdminTrayTool cannot launch it, verify that the MECM console installation is functioning correctly.

---

## MECM opens but cannot connect

Check:

1. Network connectivity.
2. VPN connection if required.
3. Configuration Manager site availability.
4. The technician's Configuration Manager permissions.
5. Whether the MECM console works when launched directly.

If the console works directly but behaves differently when launched through AdminTrayTool, record the error message and check the AdminTrayTool version being used.

---

# AdminTrayTool Does Not Configure Windows Management

There is intentionally no separate Windows Management configuration file.

The Windows Management feature acts as a convenient launcher for the management tools already available on the technician's computer.

This means:

- AdminTrayTool does not install RSAT.
- AdminTrayTool does not install MECM.
- AdminTrayTool does not create Active Directory permissions.
- AdminTrayTool does not create MECM permissions.
- AdminTrayTool does not configure VPN or network connectivity.

The organisation's existing Windows, Active Directory, and MECM environment remains responsible for authentication, connectivity, and permissions.

---

# Recommended Technician Setup

For a technician who needs the full Windows Management functionality, verify the following before deployment:

- [ ] AdminTrayTool is installed.
- [ ] AdminTrayTool starts successfully.
- [ ] Active Directory administration tools are installed.
- [ ] `dsa.msc` opens successfully.
- [ ] The technician can access the required Active Directory resources.
- [ ] MECM Console is installed.
- [ ] MECM Console opens successfully.
- [ ] The technician can connect to the MECM site.
- [ ] The technician has the required MECM permissions.
- [ ] VPN/network connectivity is available when required.

Once these checks are complete, Windows Management should be ready to use from AdminTrayTool.