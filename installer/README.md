This installer folder contains a WiX Product.wxs template for building an MSI for AdminTrayTool.

Build notes:
- The GitHub Actions workflow expects Product.wxs at installer/Product.wxs.
- The workflow sets $(var.SourceDir) to the repository root 'publish' directory when invoking candle.exe via -dSourceDir.
- You can run WiX locally:
  - Install WiX Toolset 3.11
  - cd installer
  - candle.exe -dSourceDir="..\publish" Product.wxs
  - light.exe -ext WixUtilExtension Product.wixobj -o AdminTrayTool.msi

Heat.exe is optionally useful to harvest the publish folder into a ComponentGroup. Example:
  heat dir ..\publish -cg AppFiles -dr INSTALLFOLDER -gg -sfrag -srd -var var.SourceDir -out components.wxs

If using heat, include components.wxs in Product.wxs and reference the ComponentGroupRef AppFiles (already present in Product.wxs).

Permissions:
- The Product.wxs grants 'Users' modify rights to the ProgramData\AdminTrayTool folder via util:PermissionEx. Adjust per security requirements.

Signing:
- Sign the MSI after build using signtool with your code-signing certificate.

CI:
- The supplied GitHub Actions workflow installs WiX via Chocolatey and runs candle & light. Ensure WiX path matches runner installation.
