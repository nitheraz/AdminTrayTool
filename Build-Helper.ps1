#!/usr/bin/env powershell
# AdminTrayTool Build Helper - Interactive menu
# Version 1.1.4

Write-Host "`nAdminTrayTool Build & Deployment Workflow (v1.1.4)`n" -ForegroundColor Cyan

while ($true) {
	Write-Host "What would you like to do?" -ForegroundColor Yellow
	Write-Host "[1] Check Prerequisites"
	Write-Host "[2] Build MSI for Testing"
	Write-Host "[3] Build MSI with Custom Version"
	Write-Host "[4] Open MSI Output Folder"
	Write-Host "[5] Commit & Push to GitHub"
	Write-Host "[6] Exit`n"

	$choice = Read-Host "Enter choice (1-6)"

	switch ($choice) {
		"1" {
			Write-Host "`nChecking prerequisites...`n" -ForegroundColor Cyan
			$WiXPath = "C:\Program Files (x86)\WiX Toolset v3.11\bin"
			if ((Test-Path "$WiXPath\heat.exe") -and (Test-Path "$WiXPath\candle.exe") -and (Test-Path "$WiXPath\light.exe")) {
				Write-Host "  [OK] WiX Toolset v3.11`n" -ForegroundColor Green
			} else {
				Write-Host "  [FAIL] WiX Toolset v3.11 not found`n" -ForegroundColor Red
			}
			if (Get-Command dotnet -ErrorAction SilentlyContinue) {
				$dotnetVer = & dotnet --version 2>$null
				Write-Host "  [OK] .NET SDK ($dotnetVer)`n" -ForegroundColor Green
			} else {
				Write-Host "  [FAIL] .NET SDK not found`n" -ForegroundColor Red
			}
			if (Test-Path "installer\Product.wxs") {
				Write-Host "  [OK] installer\Product.wxs`n" -ForegroundColor Green
			} else {
				Write-Host "  [FAIL] installer\Product.wxs not found`n" -ForegroundColor Red
			}
		}
		"2" {
			Write-Host "`nBuilding MSI version 1.1.4...`n" -ForegroundColor Cyan
			& .\Build-MSI.ps1 -ProductVersion "1.1.4" -Configuration "Release"
			if ($LASTEXITCODE -eq 0) {
				Write-Host "`nMSI build complete at .\msi-output\AdminTrayTool-1.1.4.msi`n" -ForegroundColor Green
			}
		}
		"3" {
			Write-Host "`nEnter custom version (e.g., 1.1.5):" -ForegroundColor Yellow
			$version = Read-Host
			if ($version) {
				Write-Host "`nBuilding MSI version $version...`n" -ForegroundColor Cyan
				& .\Build-MSI.ps1 -ProductVersion $version -Configuration "Release"
				if ($LASTEXITCODE -eq 0) {
					Write-Host "`nMSI build complete at .\msi-output\AdminTrayTool-$version.msi`n" -ForegroundColor Green
				}
			}
		}
		"4" {
			if (Test-Path ".\msi-output") {
				Start-Process "explorer.exe" ".\msi-output"
				Write-Host "`nOpened msi-output folder`n" -ForegroundColor Green
			} else {
				Write-Host "`nmsi-output not found. Build MSI first.`n" -ForegroundColor Yellow
			}
		}
		"5" {
			Write-Host "`nPreparing to commit...`n" -ForegroundColor Cyan
			Write-Host "Default message: 'Update: OAuth validation and MSI build support v1.1.4'"
			$message = Read-Host "Enter message (or press Enter for default)"
			if (-not $message) {
				$message = "Update: OAuth validation and MSI build support v1.1.4"
			}
			git add .
			git commit -m $message
			$pushResp = Read-Host "`nPush to remote (y/n)?"
			if ($pushResp -eq "y") {
				git push origin gam-detection
				Write-Host "`nPushed to GitHub!`n" -ForegroundColor Green
			}
		}
		"6" {
			Write-Host "`nGoodbye!`n" -ForegroundColor Cyan
			exit 0
		}
		default {
			Write-Host "`nInvalid choice. Try again.`n" -ForegroundColor Red
		}
	}
}
