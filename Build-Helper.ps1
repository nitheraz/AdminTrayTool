#!/usr/bin/env powershell
# AdminTrayTool Build Helper - Interactive menu

$VersionFile = ".\VERSION.txt"
$CsprojPath = "AdminTrayTool.csproj"   # adjust if your .csproj lives elsewhere/has a different name
$FeatureBranch = "gam-detection"
$MainBranch = "main"

function Get-CurrentVersion {
	if (Test-Path $VersionFile) {
		return (Get-Content $VersionFile -Raw).Trim()
	}
	return "1.2.0"
}

function Set-CurrentVersion($newVersion) {
	Set-Content -Path $VersionFile -Value $newVersion -NoNewline
}

function Test-VersionFormat($version) {
	return $version -match '^\d+\.\d+\.\d+$'
}

function Update-CsprojVersion($newVersion) {
	if (-not (Test-Path $CsprojPath)) {
		Write-Host "  [WARN] $CsprojPath not found - skipped csproj version update`n" -ForegroundColor Yellow
		return
	}

	$fourPartVersion = "$newVersion.0"
	$content = Get-Content $CsprojPath -Raw

	$content = $content -replace '<Version>[\d\.]+</Version>', "<Version>$newVersion</Version>"
	$content = $content -replace '<AssemblyVersion>[\d\.]+</AssemblyVersion>', "<AssemblyVersion>$fourPartVersion</AssemblyVersion>"
	$content = $content -replace '<FileVersion>[\d\.]+</FileVersion>', "<FileVersion>$fourPartVersion</FileVersion>"

	Set-Content -Path $CsprojPath -Value $content -NoNewline
	Write-Host "  [OK] Updated $CsprojPath to $newVersion / $fourPartVersion`n" -ForegroundColor Green
}

function Bump-Version($current, $part) {
	$parts = $current.Split('.') | ForEach-Object { [int]$_ }
	switch ($part) {
		"major" { $parts[0]++; $parts[1] = 0; $parts[2] = 0 }
		"minor" { $parts[1]++; $parts[2] = 0 }
		"patch" { $parts[2]++ }
	}
	return "$($parts[0]).$($parts[1]).$($parts[2])"
}

$CurrentVersion = Get-CurrentVersion

while ($true) {
	Write-Host "`nAdminTrayTool Build & Deployment Workflow (current version: $CurrentVersion)`n" -ForegroundColor Cyan

	Write-Host "What would you like to do?" -ForegroundColor Yellow
	Write-Host "[1] Check Prerequisites"
	Write-Host "[2] Build MSI (current version: $CurrentVersion)"
	Write-Host "[3] Update Version Number"
	Write-Host "[4] Open MSI Output Folder"
	Write-Host "[5] Commit, Merge to Main, Push & Create GitHub Release"
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
			if (Get-Command gh -ErrorAction SilentlyContinue) {
				$ghVer = & gh --version 2>$null | Select-Object -First 1
				Write-Host "  [OK] GitHub CLI ($ghVer)`n" -ForegroundColor Green

				$ghAuthStatus = & gh auth status 2>&1
				if ($LASTEXITCODE -eq 0) {
					Write-Host "  [OK] GitHub CLI authenticated`n" -ForegroundColor Green
				} else {
					Write-Host "  [WARN] GitHub CLI not authenticated - run 'gh auth login'`n" -ForegroundColor Yellow
				}
			} else {
				Write-Host "  [FAIL] GitHub CLI (gh) not found - install from https://cli.github.com`n" -ForegroundColor Red
			}
		}
		"2" {
			Write-Host "`nBuilding MSI version $CurrentVersion...`n" -ForegroundColor Cyan
			& .\Build-MSI.ps1 -ProductVersion $CurrentVersion -Configuration "Release"
			if ($LASTEXITCODE -eq 0) {
				Write-Host "`nMSI build complete at .\msi-output\AdminTrayTool-$CurrentVersion.msi`n" -ForegroundColor Green
			}
		}
		"3" {
			Write-Host "`nCurrent version: $CurrentVersion`n" -ForegroundColor Cyan
			Write-Host "[1] Bump patch (bug fixes)      -> $(Bump-Version $CurrentVersion 'patch')"
			Write-Host "[2] Bump minor (new feature)     -> $(Bump-Version $CurrentVersion 'minor')"
			Write-Host "[3] Bump major (breaking change) -> $(Bump-Version $CurrentVersion 'major')"
			Write-Host "[4] Enter custom version"
			Write-Host "[5] Cancel`n"

			$vChoice = Read-Host "Enter choice (1-5)"

			$newVersion = $null
			switch ($vChoice) {
				"1" { $newVersion = Bump-Version $CurrentVersion "patch" }
				"2" { $newVersion = Bump-Version $CurrentVersion "minor" }
				"3" { $newVersion = Bump-Version $CurrentVersion "major" }
				"4" {
					$input = Read-Host "Enter custom version (e.g., 1.2.0)"
					if (Test-VersionFormat $input) {
						$newVersion = $input
					} else {
						Write-Host "`n[FAIL] Invalid format. Use MAJOR.MINOR.PATCH (e.g., 1.2.0)`n" -ForegroundColor Red
					}
				}
				"5" { Write-Host "`nCancelled.`n" -ForegroundColor Yellow }
				default { Write-Host "`nInvalid choice.`n" -ForegroundColor Red }
			}

			if ($newVersion) {
				Set-CurrentVersion $newVersion
				$CurrentVersion = $newVersion
				Update-CsprojVersion $newVersion
				Write-Host "`n[OK] Version updated to $newVersion`n" -ForegroundColor Green
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
			$originalBranch = git rev-parse --abbrev-ref HEAD

			Write-Host "`nCurrent branch: $originalBranch`n" -ForegroundColor Cyan
			git status --short

			$changedFiles = git status --porcelain
			if ($changedFiles) {
				Write-Host "`nShow full diff before committing? (y/n)" -ForegroundColor Yellow
				$showDiff = Read-Host
				if ($showDiff -eq "y") {
					git diff
				}

				Write-Host "`nDefault message: 'Release v$CurrentVersion'"
				$message = Read-Host "Enter commit message (or press Enter for default)"
				if (-not $message) {
					$message = "Release v$CurrentVersion"
				}

				git add .
				git commit -m $message
			} else {
				Write-Host "`nNo uncommitted changes on $originalBranch.`n" -ForegroundColor Yellow
			}

			# --- Merge into main ---
			if ($originalBranch -ne $MainBranch) {
				Write-Host "Merge '$originalBranch' into '$MainBranch'? (y/n)" -ForegroundColor Yellow
				$mergeResp = Read-Host
				if ($mergeResp -eq "y") {
					git push origin $originalBranch

					git checkout $MainBranch
					git pull origin $MainBranch
					git merge $originalBranch --no-edit

					if ($LASTEXITCODE -ne 0) {
						Write-Host "`n[FAIL] Merge conflict detected. Resolve manually, then re-run this option.`n" -ForegroundColor Red
						break
					}

					Write-Host "  [OK] Merged $originalBranch into $MainBranch`n" -ForegroundColor Green
				} else {
					Write-Host "`nSkipped merge. Staying on $originalBranch.`n" -ForegroundColor Yellow
					break
				}
			}

			# --- Tag ---
			Write-Host "Tag this commit as v$CurrentVersion? (y/n)" -ForegroundColor Yellow
			$tagResp = Read-Host
			if ($tagResp -eq "y") {
				git tag -a "v$CurrentVersion" -m "v$CurrentVersion"
				Write-Host "  [OK] Tagged v$CurrentVersion`n" -ForegroundColor Green
			}

			# --- Push main + tag ---
			Write-Host "Push '$MainBranch' (and tag, if created) to origin? (y/n)" -ForegroundColor Yellow
			$pushResp = Read-Host
			if ($pushResp -eq "y") {
				git push origin $MainBranch
				if ($tagResp -eq "y") {
					git push origin "v$CurrentVersion"
				}
				Write-Host "`nPushed to GitHub!`n" -ForegroundColor Green
			}

			# --- GitHub Release with MSI ---
			$msiPath = ".\msi-output\AdminTrayTool-$CurrentVersion.msi"

			if (-not (Test-Path $msiPath)) {
				Write-Host "`n[WARN] MSI not found at $msiPath - build it first (option 2) if you want it attached to the release.`n" -ForegroundColor Yellow
			} else {
				Write-Host "Create GitHub Release v$CurrentVersion and attach the MSI? (y/n)" -ForegroundColor Yellow
				$releaseResp = Read-Host
				if ($releaseResp -eq "y") {
					if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
						Write-Host "`n[FAIL] GitHub CLI (gh) not found. Install from https://cli.github.com and run 'gh auth login' first.`n" -ForegroundColor Red
					} else {
						Write-Host "`nRelease notes (or press Enter for default):"
						$notes = Read-Host
						if (-not $notes) {
							$notes = "Release v$CurrentVersion"
						}

						gh release create "v$CurrentVersion" $msiPath `
							--title "v$CurrentVersion" `
							--notes $notes `
							--target $MainBranch

						if ($LASTEXITCODE -eq 0) {
							Write-Host "`n[OK] GitHub Release v$CurrentVersion created with MSI attached!`n" -ForegroundColor Green
						} else {
							Write-Host "`n[FAIL] GitHub Release creation failed - see output above.`n" -ForegroundColor Red
						}
					}
				}
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