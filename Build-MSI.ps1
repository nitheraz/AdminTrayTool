# =====================================================================
# AdminTrayTool MSI Build Script
# WiX Toolset v3.11.1.2318
# This script builds a deployable MSI installer for testing
# =====================================================================

param(
    [string]$ProductVersion = "1.1.4",
    [string]$Configuration = "Release",
    [string]$OutputPath = ".\msi-output"
)

# =====================================================================
# Configuration
# =====================================================================

$ErrorActionPreference = "Stop"

$PSScriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $PSScriptRoot

# WiX Toolset paths
$WiXBinPath = "C:\Program Files (x86)\WiX Toolset v3.11\bin"

$Heat = Join-Path $WiXBinPath "heat.exe"
$Candle = Join-Path $WiXBinPath "candle.exe"
$Light = Join-Path $WiXBinPath "light.exe"

# Project paths
$PublishDir = ".\publish"
$ISS = ".\installer"
$ProductWxs = "$ISS\Product.wxs"
$AdminTrayIcon = ".\adminTray.ico"

# Output
$WixObjDir = ".\wix-obj"
$MSIOutput = Join-Path $OutputPath "AdminTrayTool-$ProductVersion.msi"
$MSIPdb = Join-Path $OutputPath "AdminTrayTool-$ProductVersion.wixpdb"

# =====================================================================
# Utility Functions
# =====================================================================

function Test-Tool {
    param(
        [string]$ToolPath,
        [string]$ToolName
    )

    if (-not (Test-Path $ToolPath)) {
        Write-Error "ERROR: $ToolName not found at: $ToolPath"
        Write-Error "Please install WiX Toolset v3.11.1.2318"
        exit 1
    }
}

function Test-Prerequisites {

    Write-Host "`nChecking prerequisites..." -ForegroundColor Cyan

    Test-Tool $Heat "heat.exe"
    Test-Tool $Candle "candle.exe"
    Test-Tool $Light "light.exe"

    if (-not (Test-Path $AdminTrayIcon)) {
        Write-Error "ERROR: adminTray.ico not found"
        exit 1
    }

    if (-not (Test-Path $ProductWxs)) {
        Write-Error "ERROR: $ProductWxs not found"
        exit 1
    }

    Write-Host "All prerequisites found." -ForegroundColor Green
}

function Build-DotNet {

    Write-Host "`nBuilding .NET application..." -ForegroundColor Cyan

    # Clean previous publish
    if (Test-Path $PublishDir) {
        Remove-Item $PublishDir -Recurse -Force
    }

    # Publish
    & dotnet publish AdminTrayTool.csproj `
        -c $Configuration `
        -o $PublishDir `
        --property:DebugType=None `
        --property:DebugSymbols=false

    if ($LASTEXITCODE -ne 0) {
        Write-Error "ERROR: dotnet publish failed"
        exit 1
    }

    Write-Host ".NET build successful." -ForegroundColor Green
}

function Copy-GAM7 {

    Write-Host "`nChecking GAM7 folder..." -ForegroundColor Cyan

    $GamSource = ".\GAM7"
    $GamDest = "$PublishDir\GAM7"

    if (Test-Path $GamDest) {
        Write-Host "GAM7 already exists in publish folder." -ForegroundColor Green
        return
    }

    if (-not (Test-Path $GamSource)) {
        Write-Warning "WARNING: GAM7 folder not found at $GamSource"
        Write-Warning "Continuing without GAM7."
        return
    }

    Write-Host "GAM7 not found in publish folder. Copying..."

    Copy-Item $GamSource -Destination $GamDest -Recurse -Force

    Write-Host "GAM7 copied successfully." -ForegroundColor Green
}

function Generate-Components {

    Write-Host "`nGenerating WiX component files..." -ForegroundColor Cyan

    if (Test-Path $WixObjDir) {
        Remove-Item $WixObjDir -Recurse -Force
    }

    New-Item -ItemType Directory -Path $WixObjDir | Out-Null

    # ---------------------------------------------------------------
    # Application files
    # ---------------------------------------------------------------

    Write-Host "  Processing application files..."

    $AppSourceDir = ".\AppSourceDir"

    if (Test-Path $AppSourceDir) {
        Remove-Item $AppSourceDir -Recurse -Force
    }

    New-Item -ItemType Directory -Path $AppSourceDir -Force | Out-Null

    Get-ChildItem $PublishDir -Force |
        Where-Object { $_.Name -ne "GAM7" } |
        ForEach-Object {
            Copy-Item $_.FullName $AppSourceDir -Recurse -Force
    }
    
    & $Heat dir $AppSourceDir `
        -o "$WixObjDir\components-app.wxs" `
        -gg `
        -sf `
        -srd `
        -sreg `
        -dr INSTALLFOLDER `
        -cg AppFiles `
        -var var.AdminTrayToolOutputDir

    if ($LASTEXITCODE -ne 0) {
        Write-Error "ERROR: heat.exe failed for application files"
        exit 1
    }

    # ---------------------------------------------------------------
    # GAM7 files
    # ---------------------------------------------------------------

    $GamDest = "$PublishDir\GAM7"

    if (Test-Path $GamDest) {

        Write-Host "  Processing GAM7 files..."

        & $Heat dir $GamDest `
            -o "$WixObjDir\components-gam.wxs" `
            -gg `
            -sf `
            -srd `
            -sreg `
            -dr GAM7Folder `
            -cg GAM7Files `
            -var var.GAM7OutputDir

        if ($LASTEXITCODE -ne 0) {
            Write-Error "ERROR: heat.exe failed for GAM7 files"
            exit 1
        }
    }
    else {

        Write-Host "  Creating empty GAM7 component group..."

        @"
<?xml version="1.0" encoding="UTF-8"?>
<Wix xmlns="http://schemas.microsoft.com/wix/2006/wi">
    <Fragment>
        <ComponentGroup Id="GAM7Files">
        </ComponentGroup>
    </Fragment>
</Wix>
"@ | Out-File "$WixObjDir\components-gam.wxs" -Encoding UTF8
    }

    Write-Host "Component files generated." -ForegroundColor Green
}

function Compile-WiX {

    Write-Host "`nCompiling WiX files..." -ForegroundColor Cyan

    $WixFiles = @(
        $ProductWxs
        "$WixObjDir\components-app.wxs"
        "$WixObjDir\components-gam.wxs"
    )

    $CandleArgs = @(
        "-out", "$WixObjDir\"
        "-dProductVersion=$ProductVersion"
        "-dAdminTrayToolOutputDir=$PublishDir"
        "-dGAM7OutputDir=$PublishDir\GAM7"
        "-dWixUILicenseRtf=$ISS\License.rtf"
        "-dAdminTrayIcon=$AdminTrayIcon"
    )

    $CandleArgs += $WixFiles

    & $Candle @CandleArgs

    if ($LASTEXITCODE -ne 0) {
        Write-Error "ERROR: candle.exe compilation failed"
        exit 1
    }

    Write-Host "WiX compilation successful." -ForegroundColor Green
}

function Link-MSI {

    Write-Host "`nLinking MSI..." -ForegroundColor Cyan

    if (-not (Test-Path $OutputPath)) {
        New-Item -ItemType Directory -Path $OutputPath | Out-Null
    }

    $LightArgs = @(
        "-o", $MSIOutput
        "-pdbout", $MSIPdb
        "-sice:ICE61"
        "-ext", "WixUIExtension"
    )

    if (Test-Path "$WixObjDir\Product.wixobj") {
        $LightArgs += "$WixObjDir\Product.wixobj"
    }

    if (Test-Path "$WixObjDir\components-app.wixobj") {
        $LightArgs += "$WixObjDir\components-app.wixobj"
    }

    if (Test-Path "$WixObjDir\components-gam.wixobj") {
        $LightArgs += "$WixObjDir\components-gam.wixobj"
    }

    & $Light @LightArgs

    if ($LASTEXITCODE -ne 0) {
        Write-Error "ERROR: light.exe linking failed"
        exit 1
    }

    Write-Host "MSI linking successful." -ForegroundColor Green
}

function Verify-MSI {

    Write-Host "`nVerifying MSI..." -ForegroundColor Cyan

    if (-not (Test-Path $MSIOutput)) {
        Write-Error "ERROR: MSI file not found at $MSIOutput"
        exit 1
    }

    $FileSize = (Get-Item $MSIOutput).Length
    $FileSizeMB = [math]::Round($FileSize / 1MB, 2)

    Write-Host "  MSI File: $MSIOutput"
    Write-Host "  Size: $FileSizeMB MB"

    Write-Host "MSI verification successful." -ForegroundColor Green
}

# =====================================================================
# Main Build Process
# =====================================================================

Write-Host @"
╔═══════════════════════════════════════════════════════════════════╗
║           AdminTrayTool MSI Build Script v1.1.4                   ║
║                    WiX Toolset v3.11.1.2318                       ║
╚═══════════════════════════════════════════════════════════════════╝

Configuration:   $Configuration
Product Version: $ProductVersion
Output Path:     $OutputPath
"@ -ForegroundColor Cyan

try {

    Test-Prerequisites
    Build-DotNet
    Copy-GAM7
    Generate-Components
    Compile-WiX
    Link-MSI
    Verify-MSI

    Write-Host @"
╔═══════════════════════════════════════════════════════════════════╗
║                   ✓ MSI BUILD SUCCESSFUL!                        ║
╚═══════════════════════════════════════════════════════════════════╝

Next Steps:

1. Copy the MSI to your VM
2. Run:
   msiexec /i $((Split-Path $MSIOutput -Leaf))

3. Test the OAuth flow
4. Test Chromebook management features

MSI Location:
$MSIOutput
"@ -ForegroundColor Green

}
catch {

    Write-Host "`n✗ BUILD FAILED" -ForegroundColor Red
    Write-Error $_
    exit 1
}
finally {

    # Cleanup
    # Uncomment to auto-cleanup WiX object files
    #
    # if (Test-Path $WixObjDir) {
    #     Remove-Item $WixObjDir -Recurse -Force
    # }
}