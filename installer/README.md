# AdminTrayTool Installer

This directory contains the WiX Toolset files used to build the Windows MSI installer for AdminTrayTool.

The installer is built using **WiX Toolset 3.x** and packages the published AdminTrayTool application together with the bundled GAM7 files.

## Installer Structure

The main installer file is:

```text
installer/
├── Product.wxs
├── License.rtf
└── README.md
```

`Product.wxs` defines the MSI package, installation directories, application icon, Start Menu shortcut, and references to the application and GAM7 component groups.

## Build Requirements

To build the MSI locally, you need:

- Windows
- WiX Toolset 3.x
- A published AdminTrayTool application
- The bundled GAM7 files
- `adminTray.ico`
- `License.rtf`

The current local build process uses the WiX Toolset command-line tools:

```text
candle.exe
light.exe
```

## Publish Directory

The MSI build expects the published AdminTrayTool files to be available in the repository's `publish` directory.

The GitHub Actions workflow passes this directory to WiX through the `SourceDir` variable.

Conceptually:

```text
AdminTrayTool/
├── publish/
│   ├── AdminTrayTool.exe
│   ├── AdminTrayTool.dll
│   ├── adminTray.ico
│   └── GAM7/
│       └── gam.exe
│
└── installer/
    ├── Product.wxs
    ├── License.rtf
    └── README.md
```

## Building the MSI Locally

First, publish the AdminTrayTool application into the expected `publish` directory.

Then open a command prompt or PowerShell window in the `installer` directory:

```powershell
cd installer
```

Build the WiX object file:

```powershell
candle.exe -dSourceDir="..\publish" Product.wxs
```

Then link the MSI:

```powershell
light.exe -ext WixUtilExtension Product.wixobj -o AdminTrayTool.msi
```

The resulting MSI will be created as:

```text
installer\AdminTrayTool.msi
```

### Product Version

The WiX project expects the product version to be supplied through the `ProductVersion` preprocessor variable:

```text
$(var.ProductVersion)
```

When building manually, provide the version to `candle.exe`, for example:

```powershell
candle.exe -dSourceDir="..\publish" -dProductVersion="1.5.1" Product.wxs
```

Then link the resulting object:

```powershell
light.exe -ext WixUtilExtension Product.wixobj -o AdminTrayTool.msi
```

The normal project build scripts may provide this value automatically.

## Application Installation

The MSI is configured as a **per-machine** installation:

```xml
InstallScope="perMachine"
```

The application is installed under the Windows Program Files directory:

```text
C:\Program Files (x86)\AdminTrayTool
```

The bundled GAM7 files are installed in:

```text
C:\Program Files (x86)\AdminTrayTool\GAM7
```

The application expects the GAM7 executable at:

```text
GAM7\gam.exe
```

relative to the AdminTrayTool installation directory.

## Start Menu Shortcut

The installer creates an **Admin Tray Tool** shortcut in the Start Menu under:

```text
Start Menu\Programs\AdminTrayTool
```

The shortcut launches:

```text
AdminTrayTool.exe
```

The AdminTrayTool application icon is used for the shortcut.

The shortcut is removed when the application is uninstalled.

## Application Icon

The MSI uses:

```text
adminTray.ico
```

as the application icon.

The icon is also registered as the MSI's Add/Remove Programs product icon.

## Major Upgrades

The installer uses WiX `MajorUpgrade`:

```xml
<MajorUpgrade
    DowngradeErrorMessage="A newer version of AdminTrayTool is already installed."
    Schedule="afterInstallInitialize" />
```

This allows a newer AdminTrayTool MSI to upgrade an existing installation.

A downgrade to an older version is prevented.

The MSI uses the following upgrade code:

```text
244eca79-078b-40c5-8c1c-2b7fc36ba934
```

This value should remain unchanged between releases.

## Configuration and Upgrades

AdminTrayTool stores its application configuration outside the installation directory:

```text
%ProgramData%\AdminTrayTool\
```

For example:

```text
config.json
groupTemplates.json
```

These files are not part of the application files installed by the MSI.

Keeping the configuration outside the installation directory allows normal application upgrades to replace the application without replacing the technician's configuration.

GAM7 user authentication is also maintained separately under the Windows user's profile.

## License

The installer uses:

```text
installer\License.rtf
```

as the license displayed by the WiX installation interface.

The license file is referenced by `Product.wxs` through:

```xml
<WixVariable
    Id="WixUILicenseRtf"
    Value="installer\License.rtf" />
```

## Component Groups

`Product.wxs` references two component groups for the application files:

```xml
<ComponentGroupRef Id="AppFiles" />
<ComponentGroupRef Id="GAM7Files" />
```

These component groups are generated or maintained separately from the main `Product.wxs` file.

The application component group contains the published AdminTrayTool files.

The GAM7 component group contains the bundled GAM7 files.

## Using Heat.exe

WiX `heat.exe` can be used to harvest files from the publish directory into a WiX component group.

For example:

```powershell
heat dir ..\publish `
    -cg AppFiles `
    -dr INSTALLFOLDER `
    -gg `
    -sfrag `
    -srd `
    -var var.SourceDir `
    -out components.wxs
```

If using harvested components, include the generated `.wxs` file in the WiX build and reference the corresponding `ComponentGroup`.

### Important

GAM7 may require special handling during harvesting because certain files or components can conflict with other harvested files.

If the project contains manually maintained GAM7 component definitions, do not replace them blindly with automatically harvested components.

## MSI Compression

The installer uses:

```xml
<MediaTemplate EmbedCab="yes" />
```

This embeds the installation cabinet into the MSI.

The resulting MSI can therefore contain the packaged application files without requiring a separate external cabinet file.

## Code Signing

The MSI should be digitally signed before being distributed to users.

A Windows code-signing certificate and Microsoft's `signtool.exe` can be used to sign the final MSI.

Example:

```powershell
signtool sign /fd SHA256 /a AdminTrayTool.msi
```

The exact signing command depends on the certificate and signing infrastructure used by the organization.

Signing should be performed **after** the MSI has been built.

## GitHub Actions

The repository includes a GitHub Actions workflow for building the application and MSI.

The CI workflow:

1. Checks out the repository.
2. Installs the required .NET SDK.
3. Builds/publishes AdminTrayTool.
4. Installs WiX Toolset.
5. Builds the WiX MSI.
6. Produces the installer artifact.

The workflow passes the published application directory to WiX through:

```text
-dSourceDir
```

The WiX build must therefore remain consistent with the repository's `publish` directory structure.

## Troubleshooting

### `candle.exe` is not recognized

WiX is either not installed or its `bin` directory is not in the current PATH.

Verify the WiX installation and run the command from the WiX `bin` directory, or add that directory to PATH.

### `SourceDir` files cannot be found

Confirm that the application has been published into:

```text
publish\
```

relative to the repository root.

Also verify that the command uses:

```text
-dSourceDir="..\publish"
```

when executed from the `installer` directory.

### `ProductVersion` is undefined

`Product.wxs` expects:

```text
$(var.ProductVersion)
```

Provide the value when invoking `candle.exe`:

```powershell
-dProductVersion="1.5.1"
```

or use the project's build script, which supplies the version automatically.

### GAM7 files are missing from the MSI

Verify that:

```text
publish\GAM7\
```

contains the required GAM7 files and that the `GAM7Files` component group is being generated or included correctly.

### The MSI cannot be rebuilt

Make sure an existing MSI or WiX object file is not being used by another process.

It can also help to remove generated WiX build files before rebuilding:

```text
Product.wixobj
AdminTrayTool.msi
```

Do not delete source files such as `Product.wxs` or `License.rtf`.

## Release Checklist

Before distributing a new AdminTrayTool MSI:

- [ ] Update the application version.
- [ ] Build the application successfully.
- [ ] Publish the application.
- [ ] Confirm the `publish` directory contains the expected application files.
- [ ] Confirm the bundled `GAM7` directory is present.
- [ ] Build the MSI successfully.
- [ ] Install the MSI on a test machine.
- [ ] Confirm the Start Menu shortcut works.
- [ ] Confirm AdminTrayTool starts correctly.
- [ ] Test GAM7-powered features.
- [ ] Confirm application configuration is preserved during an upgrade.
- [ ] Test the upgrade over the previous release.
- [ ] Sign the MSI if code signing is available.
- [ ] Test the signed MSI.
- [ ] Upload the final MSI to the GitHub release.

## Installer Development Notes

The installer is intentionally kept separate from the application source.

Application functionality should be implemented in the main AdminTrayTool project.

Installer changes should normally be limited to:

- MSI packaging
- Installation paths
- Shortcuts
- Application metadata
- Versioning
- Included files
- Upgrade behavior
- Installer UI
- Signing and release packaging