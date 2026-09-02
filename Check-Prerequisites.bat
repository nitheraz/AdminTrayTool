@echo off
REM Quick test script to verify MSI build prerequisites

echo.
echo ========================================
echo AdminTrayTool MSI Build - Prerequisites Check
echo ========================================
echo.

REM Check WiX Installation
echo [1/3] Checking WiX Toolset v3.11.1.2318...
if exist "C:\Program Files (x86)\WiX Toolset v3.11\bin\heat.exe" (
	echo     ✓ heat.exe found
) else (
	echo     ✗ heat.exe NOT found - WiX may not be installed
	goto :missing
)

if exist "C:\Program Files (x86)\WiX Toolset v3.11\bin\candle.exe" (
	echo     ✓ candle.exe found
) else (
	echo     ✗ candle.exe NOT found - WiX may not be installed
	goto :missing
)

if exist "C:\Program Files (x86)\WiX Toolset v3.11\bin\light.exe" (
	echo     ✓ light.exe found
) else (
	echo     ✗ light.exe NOT found - WiX may not be installed
	goto :missing
)

REM Check .NET SDK
echo.
echo [2/3] Checking .NET SDK...
for /f "tokens=*" %%i in ('dotnet --version 2^>nul') do set DOTNET_VERSION=%%i
if defined DOTNET_VERSION (
	echo     ✓ .NET SDK found: %DOTNET_VERSION%
) else (
	echo     ✗ .NET SDK NOT found
	goto :missing
)

REM Check Icon
echo.
echo [3/3] Checking required files...
if exist "adminTray.ico" (
	echo     ✓ adminTray.ico found
) else (
	echo     ✗ adminTray.ico NOT found (optional, build will continue)
)

if exist "installer\Product.wxs" (
	echo     ✓ installer\Product.wxs found
) else (
	echo     ✗ installer\Product.wxs NOT found
	goto :missing
)

if exist "Build-MSI.ps1" (
	echo     ✓ Build-MSI.ps1 found
) else (
	echo     ✗ Build-MSI.ps1 NOT found
	goto :missing
)

echo.
echo ========================================
echo ✓ All prerequisites are installed!
echo ========================================
echo.
echo Ready to build. Run the following:
echo.
echo   PowerShell -ExecutionPolicy Bypass -File "Build-MSI.ps1"
echo.
pause
goto :end

:missing
echo.
echo ========================================
echo ✗ Some prerequisites are missing
echo ========================================
echo.
echo Missing components detected. Please install:
echo.
echo 1. WiX Toolset v3.11.1.2318
echo    Download: https://github.com/wixtoolset/wix3/releases
echo.
echo 2. .NET 10 SDK
echo    Download: https://dotnet.microsoft.com/download
echo.
pause
goto :end

:end
