# FAQ

**Q: Does AdminTrayTool require every technician to have their own Google Workspace admin credentials?**
A: Yes — each machine authenticates its own GAM7 OAuth session tied to whoever signs in during setup. AdminTrayTool doesn't share or proxy credentials between machines.

**Q: Will upgrading wipe my configuration?**
A: No. `config.json` and `groupTemplates.json` are stored outside the application's install folder and are preserved across upgrades.

**Q: Can I use AdminTrayTool without GAM7 / without Google Workspace?**
A: The Web Portals, RDP, and Admin Tools quick-launch features work independently. Chromebook Management and Group Management require GAM7 and a Google Workspace admin account.

**Q: Where can I request a new feature or report a bug?**
A: Open an issue on the [GitHub repository](https://github.com/nitheraz/AdminTrayTool/issues).

**Q: How do I know what version I'm running?**
A: Tray icon → About AdminTrayTool.