# AdminTrayTool MSI Build Guide

## Prerequisites

- **WiX Toolset v3.11.1.2318** - Download from: https://github.com/wixtoolset/wix3/releases
- **.NET 10 SDK** - For building the application
- **PowerShell 5.0+** - To run the build script

## Build Script

Use `Build-MSI.ps1` to create a standalone MSI installer for testing.

### Quick Start

```powershell
# Navigate to project root
cd C:\Users\mdejesus\source\repos\AdminTrayTool

# Run the build script
.\Build-MSI.ps1 -ProductVersion "1.1.4" -Configuration "Release"
```

### Script Parameters

- **`-ProductVersion`** (default: "1.1.4") - Version number for the MSI
- **`-Configuration`** (default: "Release") - Build configuration
- **`-OutputPath`** (default: ".\msi-output") - Output directory for the MSI

### Build Process

The script automat the following steps:

1. **Check Prerequisites**
   - Verifies WiX Toolset installation
   - Checks for adminTray.ico icon

2. **Build .NET Application**
   - Publishes AdminTrayTool in Release mode
   - Outputs to `.\publish` directory

3. **Copy GAM7**
   - Copies GAM7 folder to publish directory
   - Creates placeholder if GAM7 is missing

4. **Generate WiX Components**
   - Uses `heat.exe` to generate component files
   - Creates components for application files
   - Creates components for GAM7 files

5. **Compile WiX**
   - Uses `candle.exe` to compile WiX source files
   - Generates `.wixobj` object files

6. **Link MSI**
   - Uses `light.exe` to link object files
   - Creates final MSI installer

7. **Verify MSI**
   - Confirms MSI was created successfully
   - Shows file size

## Usage

### Testing on VM

1. **Build the MSI:**
   ```powershell
   .\Build-MSI.ps1
   ```

2. **Copy to VM:**
   ```powershell
   # Copy the MSI from msi-output folder to your VM
   # Via RDP file share or USB drive
   ```

3. **Install on VM:**
   ```powershell
   msiexec /i AdminTrayTool-1.1.4.msi
   # Or use GUI installer which will appear
   ```

4. **Test Scenarios:**
   - Launch AdminTrayTool from Start Menu
   - Run OAuth setup
   - Test Chromebook management (lookup, enable, disable)
   - Test configuration editor
   - Verify GAM7 integration

5. **Automatic Shortcut Creation:**
   - Start Menu shortcut is created automatically
   - Application icon is set correctly
   - Uninstall option available

### Troubleshooting

**Issue: "candle.exe not found"**
- Verify WiX Toolset v3.11.1.2318 is installed
- Default path: `C:\Program Files (x86)\WiX Toolset v3.11\bin`
- Update path in script if different

**Issue: "GAM7 folder not found"**
- Ensure GAM7 folder exists in project root
- Script will continue without GAM7, but functionality will be limited
- You can add GAM7 to installed application later

**Issue: Icon not found**
- Ensure `adminTray.ico` exists in project root
- Script will fail if icon is missing

**Issue: WiXUI extensions not found**
- Verify WiX Toolset installation is complete
- Extensions should be in WiX bin directory

## Output

- **MSI File:** `.\msi-output\AdminTrayTool-1.1.4.msi`
- **PDB File:** `.\msi-output\AdminTrayTool-1.1.4.wixpdb` (for debugging)
- **Object Files:** `.\wix-obj\` (can be deleted after build)
- **Published Files:** `.\publish\` (can be deleted after MSI creation)

## Next Steps

### After Successful MSI Build

1. **Test on VM:**
   - Verify installation completes
   - Check all features work
   - Test OAuth flow changes

2. **Commit to GitHub:**
   ```powershell
   git add .
   git commit -m "Add comprehensive OAuth validation and MSI build support

   - Improved OAuth token detection across multiple user profiles
   - Environment variable support (GAM_CONFIG_DIR)
   - WiX v3 standalone MSI build script
   - Version 1.1.4"
   git push origin gam-detection
   ```

3. **Create Release:**
   - Go to GitHub repository
   - Create a new release
   - Attach the MSI file
   - Add release notes

## Customization

### Change Product Version

Update in script or pass parameter:
```powershell
.\Build-MSI.ps1 -ProductVersion "1.1.5"
```

### Change Installation Directory

Edit `installer\Product.wxs`:
```xml
<Directory Id="INSTALLFOLDER" Name="AdminTrayTool">
  <!-- Change "AdminTrayTool" to desired folder name -->
</Directory>
```

### Customize License

Replace `installer\License.rtf` with your license file.

### Add More Files

Run the build once, then examine generated `.wxs` files in `wix-obj` directory and modify component definitions as needed.

## Resources

- WiX Toolset v3: https://wixtoolset.org/
- WiX v3 Manual: https://wixtoolset.org/docs/v3/
- Heat.exe Tool: https://wixtoolset.org/docs/v3/tools/heat/
- Candle.exe Tool: https://wixtoolset.org/docs/v3/tools/candle/
- Light.exe Tool: https://wixtoolset.org/docs/v3/tools/light/

## Support

For issues with:
- **Build Script:** Check BuildErrors  
- **WiX Toolset:** See WiX documentation
- **Application:** Review AdminTrayTool code and logs
