# GAM7 Setup & Troubleshooting

AdminTrayTool uses [GAM7](https://github.com/GAM-team/GAM) for Google Workspace operations such as Chromebook and Group Management.

GAM7 is bundled with the AdminTrayTool installer.

## Authentication Flow

When a GAM7-powered feature is opened, AdminTrayTool checks the GAM7 configuration in the following order.

### 1. GAM7 Executable

AdminTrayTool first checks that the bundled GAM7 executable is available.

If it cannot be found, reinstall AdminTrayTool.

### 2. GAM7 Project

AdminTrayTool checks for the GAM7 project configuration, including:

```text
%USERPROFILE%\.gam\client_secrets.json
```

If the file does not exist, you will need to either:

* use your organization's existing GAM7 project; or
* create a new project if this is the first GAM7 setup for your organization.

### 3. OAuth Authentication

AdminTrayTool checks whether the machine has a valid GAM7 OAuth configuration.

If authentication is required, GAM7 will start the OAuth process and open a browser for Google Workspace sign-in.

## Using an Existing GAM7 Project

If your organization already has GAM7 configured on another trusted machine, **do not create another Google Cloud project unless there is a specific reason to do so**.

Instead, obtain the organization's existing `client_secrets.json` from a trusted, working GAM7 installation and copy it to:

```text
%USERPROFILE%\.gam\
```

The expected result is:

```text
%USERPROFILE%\.gam\client_secrets.json
```

You can then retry the OAuth setup.

### Security Considerations

`client_secrets.json` contains the OAuth client configuration used by GAM7. It should still be handled as organizational configuration and should only be copied between trusted, organization-managed computers.

**Do not:**

* commit `client_secrets.json` to GitHub;
* upload it to a public file-sharing service;
* include your organization's OAuth credentials in a public software release;
* copy another technician's OAuth tokens to your machine.

The OAuth tokens generated during authentication are separate from the client secrets file and should normally remain associated with the individual user's GAM7 session.

## Creating a New GAM7 Project

Only create a new project when your organization does not already have an appropriate GAM7 project.

Run:

```text
gam create project
```

and then:

```text
gam oauth create
```

Follow the prompts provided by GAM7.

Creating multiple projects for the same organization without a clear reason can make it difficult to determine which Google Cloud project and OAuth client should be maintained.

# Common Errors

## "No such file or directory: client_secrets.json"

Your machine does not currently have the GAM7 project credentials required for authentication.

If your organization already has a working GAM7 setup:

1. Do not create another Google Cloud project.

2. Copy the existing `client_secrets.json` from a trusted, working GAM7 machine.

3. Place it in:

   ```text
   %USERPROFILE%\.gam\
   ```

4. Retry the GAM7 OAuth setup.

If this is the first GAM7 machine for your organization, follow the [new project setup](#creating-a-new-gam7-project) process instead.

## "invalid_client: The provided client secret is invalid"

The OAuth client information does not match the client registered in Google Cloud.

Possible causes include:

* the wrong `client_secrets.json` was copied;
* the OAuth client was deleted;
* the OAuth client credentials were regenerated;
* the file belongs to a different Google Cloud project.

Download a fresh `client_secrets.json` from the correct OAuth client in your organization's Google Cloud project and then run:

```text
gam oauth create
```

## "Client Secrets File... Does not exist"

GAM7 cannot locate the OAuth client credentials.

If your organization already has a GAM7 project, copy the existing `client_secrets.json` into:

```text
%USERPROFILE%\.gam\
```

If this is a completely new GAM7 setup, run:

```text
gam create project
gam oauth create
```

## GAM7 Commands Work but AdminTrayTool Shows "Not Authenticated"

If GAM7 works successfully from a terminal but AdminTrayTool still reports that the machine is not authenticated, check the GAM7 configuration.

In particular, look for an `oauth2service_json` entry in:

```text
%USERPROFILE%\.gam\gam.cfg
```

If the entry references a service-account file that does not exist on the machine, it may generate a warning.

If that service-account configuration is not required by your organization's GAM7 setup, the obsolete line can be removed from `gam.cfg`.

> **Important:** Do not remove GAM7 configuration entries unless you understand what they are used for. If your organization relies on service-account authentication for a specific workflow, consult your Google Workspace/GAM7 administrator before changing the configuration.
