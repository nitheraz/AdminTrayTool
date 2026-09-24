#!/usr/bin/env powershell
# AdminTrayTool Build Helper - Interactive menu

$VersionFile = ".\VERSION.txt"
$CsprojPath = "AdminTrayTool.csproj"
$MainBranch = "main"

function Get-CurrentVersion {
    if (-not (Test-Path $VersionFile)) {
        throw "VERSION.txt was not found. Create VERSION.txt with a version such as <version>."
    }

    $version = (Get-Content $VersionFile -Raw).Trim()

    if (-not (Test-VersionFormat $version)) {
        throw "Invalid version in VERSION.txt: '$version'. Expected format: MAJOR.MINOR.PATCH"
    }

    return $version
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
        "major" {
            $parts[0]++
            $parts[1] = 0
            $parts[2] = 0
        }

        "minor" {
            $parts[1]++
            $parts[2] = 0
        }

        "patch" {
            $parts[2]++
        }
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
    Write-Host "[5] Commit, Merge to Main, Push & Trigger GitHub Actions Release"
    Write-Host "[6] Exit`n"

    $choice = Read-Host "Enter choice (1-6)"

    switch ($choice) {
        "1" {
            Write-Host "`nChecking prerequisites...`n" -ForegroundColor Cyan

            $WiXPath = "C:\Program Files (x86)\WiX Toolset v3.11\bin"

            if (
                (Test-Path "$WiXPath\heat.exe") -and
                (Test-Path "$WiXPath\candle.exe") -and
                (Test-Path "$WiXPath\light.exe")
            ) {
                Write-Host "  [OK] WiX Toolset v3.11`n" -ForegroundColor Green
            }
            else {
                Write-Host "  [FAIL] WiX Toolset v3.11 not found`n" -ForegroundColor Red
            }

            if (Get-Command dotnet -ErrorAction SilentlyContinue) {
                $dotnetVer = & dotnet --version 2>$null
                Write-Host "  [OK] .NET SDK ($dotnetVer)`n" -ForegroundColor Green
            }
            else {
                Write-Host "  [FAIL] .NET SDK not found`n" -ForegroundColor Red
            }

            if (Test-Path "installer\Product.wxs") {
                Write-Host "  [OK] installer\Product.wxs`n" -ForegroundColor Green
            }
            else {
                Write-Host "  [FAIL] installer\Product.wxs not found`n" -ForegroundColor Red
            }

            if (Get-Command gh -ErrorAction SilentlyContinue) {
                $ghVer = & gh --version 2>$null | Select-Object -First 1
                Write-Host "  [OK] GitHub CLI ($ghVer)`n" -ForegroundColor Green

                $ghAuthStatus = & gh auth status 2>&1

                if ($LASTEXITCODE -eq 0) {
                    Write-Host "  [OK] GitHub CLI authenticated`n" -ForegroundColor Green
                }
                else {
                    Write-Host "  [WARN] GitHub CLI not authenticated - run 'gh auth login'`n" -ForegroundColor Yellow
                }
            }
            else {
                Write-Host "  [FAIL] GitHub CLI (gh) not found - install from https://cli.github.com`n" -ForegroundColor Red
            }
        }

        "2" {
            Write-Host "`nBuilding MSI version $CurrentVersion...`n" -ForegroundColor Cyan

            & .\Build-MSI.ps1 `
                -ProductVersion $CurrentVersion `
                -Configuration "Release"

            if ($LASTEXITCODE -eq 0) {
                Write-Host "`nMSI build complete at .\msi-output\AdminTrayTool-$CurrentVersion.msi`n" -ForegroundColor Green
            }
            else {
                Write-Host "`n[FAIL] MSI build failed. See the output above for details.`n" -ForegroundColor Red
            }
        }

        "3" {
            Write-Host "`nCurrent version: $CurrentVersion`n" -ForegroundColor Cyan

            Write-Host "[1] Bump patch (bug fixes)       -> $(Bump-Version $CurrentVersion 'patch')"
            Write-Host "[2] Bump minor (new feature)     -> $(Bump-Version $CurrentVersion 'minor')"
            Write-Host "[3] Bump major (breaking change) -> $(Bump-Version $CurrentVersion 'major')"
            Write-Host "[4] Enter custom version"
            Write-Host "[5] Cancel`n"

            $vChoice = Read-Host "Enter choice (1-5)"

            $newVersion = $null

            switch ($vChoice) {
                "1" {
                    $newVersion = Bump-Version $CurrentVersion "patch"
                }

                "2" {
                    $newVersion = Bump-Version $CurrentVersion "minor"
                }

                "3" {
                    $newVersion = Bump-Version $CurrentVersion "major"
                }

                "4" {
                    $input = Read-Host "Enter custom version (e.g., 1.2.0)"

                    if (Test-VersionFormat $input) {
                        $newVersion = $input
                    }
                    else {
                        Write-Host "`n[FAIL] Invalid format. Use MAJOR.MINOR.PATCH (e.g., 1.2.0)`n" -ForegroundColor Red
                    }
                }

                "5" {
                    Write-Host "`nCancelled.`n" -ForegroundColor Yellow
                }

                default {
                    Write-Host "`nInvalid choice.`n" -ForegroundColor Red
                }
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
            }
            else {
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
                Write-Host "(For multi-line messages, press Enter here and use 'git commit' manually instead - this prompt only accepts one line.)"

                $message = Read-Host "Enter commit message (or press Enter for default)"

                if (-not $message) {
                    $message = "Release v$CurrentVersion"
                }

                git add .
                git commit -m $message

                if ($LASTEXITCODE -ne 0) {
                    Write-Host "`n[FAIL] Commit failed. Resolve the issue before continuing.`n" -ForegroundColor Red
                    break
                }
            }
            else {
                Write-Host "`nNo uncommitted changes on $originalBranch.`n" -ForegroundColor Yellow
            }

            # --- Merge into main ---
            if ($originalBranch -ne $MainBranch) {
                Write-Host "Merge '$originalBranch' into '$MainBranch'? (y/n)" -ForegroundColor Yellow

                $mergeResp = Read-Host

                if ($mergeResp -eq "y") {
                    git push origin $originalBranch

                    if ($LASTEXITCODE -ne 0) {
                        Write-Host "`n[FAIL] Failed to push '$originalBranch'.`n" -ForegroundColor Red
                        break
                    }

                    git checkout $MainBranch

                    if ($LASTEXITCODE -ne 0) {
                        Write-Host "`n[FAIL] Failed to checkout '$MainBranch'.`n" -ForegroundColor Red
                        break
                    }

                    git pull origin $MainBranch

                    if ($LASTEXITCODE -ne 0) {
                        Write-Host "`n[FAIL] 'git pull' failed or hit a conflict. Resolve manually (git status will show details), then re-run this option.`n" -ForegroundColor Red
                        break
                    }

                    git merge $originalBranch --no-edit

                    if ($LASTEXITCODE -ne 0) {
                        Write-Host "`n[FAIL] Merge conflict detected. Resolve manually, then re-run this option.`n" -ForegroundColor Red
                        break
                    }

                    Write-Host "  [OK] Merged $originalBranch into $MainBranch`n" -ForegroundColor Green
                }
                else {
                    Write-Host "`nSkipped merge. Staying on $originalBranch.`n" -ForegroundColor Yellow
                    break
                }
            }

            # --- Re-read version ---
            $CurrentVersion = Get-CurrentVersion

            # --- Tag ---
            Write-Host "Tag this commit as v$CurrentVersion? (y/n)" -ForegroundColor Yellow

            $tagResp = Read-Host

            if ($tagResp -eq "y") {
                git tag -a "v$CurrentVersion" -m "v$CurrentVersion"

                if ($LASTEXITCODE -eq 0) {
                    Write-Host "  [OK] Tagged v$CurrentVersion`n" -ForegroundColor Green
                }
                else {
                    Write-Host "  [FAIL] Tagging failed - see output above.`n" -ForegroundColor Red
                    $tagResp = "n"
                }
            }

            # --- Push main and tag ---
            Write-Host "Push '$MainBranch' (and tag, if created) to origin? (y/n)" -ForegroundColor Yellow

            $pushResp = Read-Host

            if ($pushResp -eq "y") {
                git push origin $MainBranch

                if ($LASTEXITCODE -eq 0) {
                    Write-Host "  [OK] Pushed $MainBranch to origin`n" -ForegroundColor Green

                    if ($tagResp -eq "y") {
                        git push origin "v$CurrentVersion"

                        if ($LASTEXITCODE -eq 0) {
                            Write-Host "  [OK] Pushed tag v$CurrentVersion to origin`n" -ForegroundColor Green
                        }
                        else {
                            Write-Host "  [FAIL] Pushing tag v$CurrentVersion failed - see output above.`n" -ForegroundColor Red
                        }
                    }
                }
                else {
                    Write-Host "`n[FAIL] Push to $MainBranch was rejected or failed. Your local branch may be behind origin - run 'git pull origin $MainBranch' and resolve any conflicts, then re-run this option.`n" -ForegroundColor Red
                    break
                }
            }
            else {
                Write-Host "`nPush cancelled. No GitHub Actions release will be created.`n" -ForegroundColor Yellow
                break
            }

            # --- GitHub Actions release ---
            $CurrentVersion = Get-CurrentVersion

            if ($tagResp -eq "y" -and $pushResp -eq "y") {
                Write-Host ""
                Write-Host "GitHub Actions will now build and publish v$CurrentVersion." -ForegroundColor Cyan
                Write-Host ""
                Write-Host "The release will contain:" -ForegroundColor Gray
                Write-Host "  AdminTrayTool-$CurrentVersion.msi" -ForegroundColor Gray
                Write-Host "  AdminTrayTool-portable.zip" -ForegroundColor Gray
                Write-Host ""
                Write-Host "No local MSI will be uploaded by Build-Helper.ps1." -ForegroundColor Green
                Write-Host ""
                Write-Host "Monitor the GitHub Actions workflow for the build and release." -ForegroundColor Cyan
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