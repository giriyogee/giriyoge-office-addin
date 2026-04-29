# giriyoge-office-addin

Enterprise Microsoft 365 Word task pane add-in built with Angular and Office.js.

Phase 1 intentionally validates the deployment path first: opening the add-in in Word renders `Hello World - <environment>`.

## Architecture

- Add-in type: manifest-based Microsoft Word task pane add-in.
- Runtime: Office.js only, loaded from `https://appsforoffice.microsoft.com/lib/1/hosted/office.js`.
- Framework: Angular 21 active stable line.
- Deployment: Azure Static Web Apps for Phase 1 hosting, then Microsoft 365 Admin Center centralized deployment for users or Entra ID groups.
- Environment isolation: each hosted environment has its own manifest GUID, display name, source location, and Static Web App.

## Environment Matrix

| Environment | Manifest | Add-in ID | SourceLocation |
| --- | --- | --- | --- |
| Production | `manifests/manifest-prod.xml` | `4f65e113-2216-41d8-8ba2-49e6fd7fd004` | `https://prod.giriyoge-office-addin.example.com` |
| Local | `manifests/manifest-local.xml` | `0b6d995f-1226-4f2c-a10d-e65eb8c7df01` | `https://localhost:4200` |
| Dev | `manifests/manifest-dev.xml` | `1389d7c4-7cc2-492d-9950-b07e5dd62d02` | `https://brave-ground-0c778b50f.7.azurestaticapps.net` |
| UAT | `manifests/manifest-uat.xml` | `72504f9c-9bfa-49f0-9dec-094e81febe03` | `https://yellow-smoke-0aede930f.7.azurestaticapps.net` |

## Quick Start

1. Install Node.js 20.19 or later.
2. Run `npm install`.
3. Run `npm run certs:install`.
4. Run `npm run start:local`.
5. In another terminal, run `npm run sideload:word:local`.
6. Confirm Word opens the task pane and displays `Hello World - Local`.

## Commands

- `npm run build:prod`
- `npm run start:local`
- `npm run start:dev`
- `npm run start:uat`
- `npm run build:local`
- `npm run build:dev`
- `npm run build:uat`
- `npm run validate:manifests`

## Phase 1 Hosting

Use three separate Azure Static Web Apps on the Free plan for Dev, UAT, and Prod. After Azure provides the real URLs, run:

```powershell
.\scripts\update-hosted-urls.ps1 -Environment dev -Url "https://YOUR-DEV-HOST.azurestaticapps.net"
.\scripts\update-hosted-urls.ps1 -Environment uat -Url "https://YOUR-UAT-HOST.azurestaticapps.net"
.\scripts\update-hosted-urls.ps1 -Environment prod -Url "https://YOUR-PROD-HOST.azurestaticapps.net"
```

## Documentation

- [Local testing guide](docs/local-testing.md)
- [Phase 1 Azure Static Web Apps hosting guide](docs/phase1-hosting-azure-static-web-apps.md)
- [Admin team handoff guide](docs/admin-team-handoff.md)
- [Microsoft 365 Admin Center deployment guide](docs/m365-admin-center-deployment.md)
- [Entra ID group visibility guide](docs/entra-group-visibility.md)
- [Validation and troubleshooting checklist](docs/validation-checklist.md)
