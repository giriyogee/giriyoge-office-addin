# Local Testing Guide

## Prerequisites

1. Microsoft Word for Microsoft 365 desktop signed in with a work or school account.
2. Node.js 20.19 or later.
3. Local administrator rights to trust development certificates.
4. Network access to `https://appsforoffice.microsoft.com` for Office.js.

## Install Dependencies

```powershell
cd giriyoge-office-addin
npm install
```

## Configure HTTPS

Install and trust the Office Add-in development certificate.

```powershell
npm run certs:install
npm run certs:verify
```

The local development server must be HTTPS because Office Add-ins are hosted web apps and production Office clients require secure origins.
The local, dev, and UAT start scripts explicitly use the trusted certificate generated in `%USERPROFILE%\.office-addin-dev-certs`.

## Run Local

```powershell
npm run start:local
```

Open a second terminal.

```powershell
npm run sideload:word:local
```

Expected result: Word opens with a Giriyoge command on the Home tab. Opening the task pane displays:

```text
Hello World - Local
```

Click **Insert paragraph** in the task pane. Expected document content:

```text
Hello from Local
```

The sideload scripts use `--no-debug` because Phase 1 validates deployment and rendering only. If Word reports that content is blocked because it is not signed by a valid security certificate, stop `ng serve`, run `npm run certs:install`, restart Word, and start the local server again.

## Run Multiple Local Environments Together

Use separate terminals.

```powershell
npm run start:local
npm run start:dev
npm run start:uat
```

Then sideload each manifest:

```powershell
npm run sideload:word:local
npm run sideload:word:dev
npm run sideload:word:uat
```

Each manifest uses a unique GUID and display name, so Word can install them side by side.

## Validate Manifests

```powershell
npm run validate:manifests
```

Fix validation errors before any admin deployment handoff.

## Local Cleanup

Use the Office Add-in debugging tools to stop sideloading.

```powershell
npm run stop:sideload
```

If Word caches an old manifest, close all Office applications, clear the Office add-in cache, and sideload again.
