# Phase 1 Hosting: Azure Static Web Apps

## Recommendation

Use three separate Azure Static Web Apps on the Free plan:

| Environment | Azure resource | Angular build | Manifest |
| --- | --- | --- | --- |
| Dev | `giriyoge-office-addin-dev` | `npm run build:dev` | `manifests/manifest-dev.xml` |
| UAT | `giriyoge-office-addin-uat` | `npm run build:uat` | `manifests/manifest-uat.xml` |
| Prod | `giriyoge-office-addin-prod` | `npm run build:prod` | `manifests/manifest-prod.xml` |

This is the clearest model for a personal Office Add-in because each environment gets an independent stable URL, deployment token, manifest, Microsoft 365 Admin Center app entry, and rollback surface.

Avoid using one Static Web App with preview environments for this phase. Static Web Apps preview environments are useful for pull request or branch review, but Dev, UAT, and Prod Office manifests need stable lifecycle URLs and clean admin deployment boundaries. The Free plan supports personal projects, free SSL certificates, and static hosting without requiring APIs.

Local remains localhost-only:

- Local app: `https://localhost:4200`
- Local manifest: `manifests/manifest-local.xml`
- Local command: `npm run start:local`

## Current Hosted Placeholders

The hosted files currently use syntactically valid HTTPS placeholders:

| Environment | Placeholder |
| --- | --- |
| Dev | `https://brave-ground-0c778b50f.7.azurestaticapps.net` |
| UAT | `https://yellow-smoke-0aede930f.7.azurestaticapps.net` |
| Prod | `https://prod.giriyoge-office-addin.example.com` |

After Azure creates each Static Web App, replace these placeholders with the real `https://...azurestaticapps.net` URL.

## Create Azure Resources

Do this only when you are ready to create Azure resources. Stay on the Free plan for Phase 1.

1. Sign in to the Azure portal.
2. Create one resource group, for example `rg-giriyoge-office-addin`.
3. Create three Static Web Apps:
   - `giriyoge-office-addin-dev`
   - `giriyoge-office-addin-uat`
   - `giriyoge-office-addin-prod`
4. For each Static Web App:
   - Plan: Free
   - Region: choose the nearest available region
   - Deployment source: Manual/Other if you are deploying from this local project with the SWA CLI
   - APIs: none
5. Open each Static Web App overview page and copy its default URL.

## Update URLs After Azure Gives You Hostnames

Run these from the project root, replacing the sample URLs with your real Azure Static Web Apps URLs.

```powershell
.\scripts\update-hosted-urls.ps1 -Environment dev -Url "https://YOUR-DEV-HOST.azurestaticapps.net"
.\scripts\update-hosted-urls.ps1 -Environment uat -Url "https://YOUR-UAT-HOST.azurestaticapps.net"
.\scripts\update-hosted-urls.ps1 -Environment prod -Url "https://YOUR-PROD-HOST.azurestaticapps.net"
```

This updates both the Angular environment file and the matching hosted manifest.

## Build Each Environment

```powershell
npm run build:dev
npm run build:uat
npm run build:prod
```

The deployable folder is:

```text
dist/giriyoge-office-addin/browser
```

## Deploy Each Environment

Install or invoke the Azure Static Web Apps CLI when you are ready to deploy. This may require network access and Azure sign-in.

```powershell
npx @azure/static-web-apps-cli@latest deploy ".\dist\giriyoge-office-addin\browser" --app-name "giriyoge-office-addin-dev" --resource-group "rg-giriyoge-office-addin" --env production

npm run build:uat
npx @azure/static-web-apps-cli@latest deploy ".\dist\giriyoge-office-addin\browser" --app-name "giriyoge-office-addin-uat" --resource-group "rg-giriyoge-office-addin" --env production

npm run build:prod
npx @azure/static-web-apps-cli@latest deploy ".\dist\giriyoge-office-addin\browser" --app-name "giriyoge-office-addin-prod" --resource-group "rg-giriyoge-office-addin" --env production
```

For Dev, run `npm run build:dev` immediately before the first deploy command. Each app uses `--env production` because each Azure resource is its production slot for that lifecycle stage.

## Validate Manifests And Builds

After real URLs are stamped in:

```powershell
npm run validate:manifest:dev
npm run validate:manifest:uat
npm run validate:manifest:prod

npm run build:dev
npm run build:uat
npm run build:prod
```

## Test In Word

For personal hosted testing, sideload each manifest one at a time:

```powershell
npm run sideload:word:dev
npm run sideload:word:uat
```

For Prod, use Microsoft 365 Admin Center upload or add a temporary sideload script if needed.

Expected results:

| Environment | Task pane text | Inserted text |
| --- | --- | --- |
| Dev | `Hello World - Dev` | `Hello from Dev` |
| UAT | `Hello World - UAT` | `Hello from UAT` |
| Prod | `Hello World - Production` | `Hello from Production` |

## Later Microsoft 365 Admin Center Mapping

In a later deployment phase, each hosted manifest maps to a separate custom app in Microsoft 365 Admin Center:

| Manifest | Suggested assignment |
| --- | --- |
| `manifest-dev.xml` | `M365-Addin-Giriyoge-DEV-Users` |
| `manifest-uat.xml` | `M365-Addin-Giriyoge-UAT-Users` |
| `manifest-prod.xml` | `M365-Addin-Giriyoge-PROD-Pilot`, then broader production group |

Microsoft recommends centralized deployment through Integrated Apps for most organizations. Use Entra ID security groups for visibility control, keep manifest IDs stable after deployment, and upload a manifest update when metadata such as URLs, icons, command labels, or permissions changes.
