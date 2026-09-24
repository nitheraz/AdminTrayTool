# Build Quick Start

This is the quickest way to build and release AdminTrayTool.

## Local MSI Build

From the AdminTrayTool repository root:

```powershell
.\Build-MSI.ps1 -ProductVersion "<version>"
```

The MSI will be created in:

```text
msi-output\AdminTrayTool-<version>.msi
```

The version can be changed to the version you are actually building.

---

## Interactive Build Helper

For the guided workflow, run:

```powershell
.\Build-Helper.ps1
```

The menu provides:

```text
[1] Check Prerequisites
[2] Build MSI
[3] Update Version Number
[4] Open MSI Output Folder
[5] Commit, Merge to Main, Push & Trigger GitHub Release
[6] Exit
```

For a normal release:

1. Check prerequisites.
2. Update the version.
3. Build the MSI.
4. Test the MSI.
5. Commit the changes.
6. Merge into `main`.
7. Create the version tag.
8. Push `main` and the tag.

---

## Official GitHub Release

The GitHub Actions workflow creates the official release when a version tag is pushed.

For example:

```powershell
git tag v<version>
git push origin v<version>
```

GitHub Actions will then build:

```text
AdminTrayTool-<version>.msi
AdminTrayTool-portable.zip
```

and attach them to the GitHub Release.

---

## Versioning

The project uses:

```text
VERSION.txt
```

The interactive build helper can update the version automatically.

Supported version format:

```text
MAJOR.MINOR.PATCH
```

Example:

```text
<version>
```

---

## Important

The local MSI build and GitHub Actions build are separate environments.

The local build uses the WiX installation on your development PC.

The GitHub Actions workflow locates WiX on the GitHub-hosted Windows runner.

For an official release, always verify that the GitHub Actions workflow completes successfully and that the generated MSI and portable ZIP are present in the GitHub Release.
