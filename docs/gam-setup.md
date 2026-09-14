# GAM7 Setup & Troubleshooting

AdminTrayTool uses [GAM7](https://github.com/GAM-team/GAM) under the hood for all Google Workspace operations. GAM7 is bundled with the installer.

## Authentication Flow

AdminTrayTool checks, in order:

1. **GAM7 executable found?** If not, reinstall AdminTrayTool.
2. **GAM Project exists?** (`client_secrets.json`) — if missing, you'll be prompted to create a new project or point to an existing one.
3. **OAuth token exists?** (`oauth2.txt`) — if missing or invalid, you'll be prompted to run `gam oauth create`, which opens a browser for sign-in.

## Common Errors

### "No such file or directory: client_secrets.json"

Your machine has no GAM project configured. If your organization already uses GAM elsewhere, copy the existing `client_secrets.json` from a working machine into `%USERPROFILE%\.gam\` on this one, then retry. Only create a brand-new project if this is the very first machine being set up for your organization — creating a second project when one already exists leads to confusion about which is authoritative.

### "invalid_client: The provided client secret is invalid"

The OAuth client credentials no longer match what Google has on record — usually because the client secret was rotated or regenerated in Google Cloud Console. Re-download `client_secrets.json` from the correct OAuth client and re-run `gam oauth create`.

### "Client Secrets File... Does not exist"

Run:

gam create project
gam oauth create

in order, in a terminal, to set up a new project and authorize it.

### GAM commands succeed but AdminTrayTool still shows "not authenticated"

Check for a stray `oauth2service_json` warning in the GAM output — if `gam.cfg` references a service-account file that doesn't exist on this machine, it's usually harmless, but can be cleaned up by removing that line from `%USERPROFILE%\.gam\gam.cfg`.