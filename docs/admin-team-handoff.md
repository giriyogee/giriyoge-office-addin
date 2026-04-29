# Admin Team Handoff: Giriyoge Word Add-in

Audience: Microsoft 365, Digital Workplace, Endpoint, and Collaboration administrators.

## Request Summary

Please deploy the Giriyoge Microsoft Word task pane add-in through Microsoft 365 Admin Center centralized deployment.

This is a manifest-based Office.js add-in:

- Host app: Microsoft Word
- Add-in type: task pane add-in
- Runtime: Office.js web add-in
- Installation package: XML manifest only
- No MSI, EXE, VSTO, COM add-in, desktop agent, browser extension, or endpoint install
- No Microsoft Graph, authentication, backend API, or tenant-wide consent in Phase 1

## Official Microsoft References

- Deploy Office Add-ins in Microsoft 365 Admin Center: https://learn.microsoft.com/en-us/microsoft-365/admin/manage/manage-deployment-of-add-ins
- Centralized deployment FAQ: https://learn.microsoft.com/en-us/microsoft-365/admin/manage/centralized-deployment-faq
- Office Add-ins manifest overview: https://learn.microsoft.com/en-us/office/dev/add-ins/develop/add-in-manifests
- AppDomains manifest element: https://learn.microsoft.com/en-us/javascript/api/manifest/appdomains

## Environment Inventory

| Environment | Manifest file | Display name | Manifest ID | Hosted app URL | Recommended assignment |
| --- | --- | --- | --- | --- | --- |
| Dev | `manifests/manifest-dev.xml` | Giriyoge Office Add-in Dev | `1389d7c4-7cc2-492d-9950-b07e5dd62d02` | `https://brave-ground-0c778b50f.7.azurestaticapps.net` | `M365-Addin-Giriyoge-DEV-Users` |
| UAT | `manifests/manifest-uat.xml` | Giriyoge Office Add-in UAT | `72504f9c-9bfa-49f0-9dec-094e81febe03` | `https://yellow-smoke-0aede930f.7.azurestaticapps.net` | `M365-Addin-Giriyoge-UAT-Users` |
| Prod | `manifests/manifest-prod.xml` | Giriyoge Office Add-in | `4f65e113-2216-41d8-8ba2-49e6fd7fd004` | Pending final production URL | `M365-Addin-Giriyoge-PROD-Pilot` first |

Dev and UAT are expected to appear side-by-side because they have unique manifest IDs, display names, source locations, and ribbon command IDs.

## Prerequisites For Admin Team

- Admin signs in with a Microsoft 365 work or school account. Personal Microsoft accounts cannot access Microsoft 365 Admin Center deployment.
- Admin has a role that can manage Integrated Apps or Office Add-ins, such as Global Administrator or Exchange Administrator.
- Target users use Microsoft 365 Apps and sign in to Word with organizational credentials.
- Target users have Exchange Online mailboxes. Centralized deployment is for online mailboxes, not on-premises Exchange mailboxes.
- Target assignment groups already exist or can be created in Microsoft Entra ID.
- The hosted app URLs and icon URLs are reachable over HTTPS from corporate devices and networks.

## Recommended Entra ID Groups

Create or confirm these security groups:

- `M365-Addin-Giriyoge-DEV-Users`
- `M365-Addin-Giriyoge-UAT-Users`
- `M365-Addin-Giriyoge-PROD-Pilot`
- `M365-Addin-Giriyoge-PROD-All`

Use direct group membership for pilot testing. Avoid relying on nested groups for the first validation pass.

## Deployment Steps

Repeat these steps once per manifest.

1. Sign in to https://admin.microsoft.com with an organizational admin account.
2. Go to **Settings** > **Integrated apps**.
3. Select **Upload custom apps**.
4. Select **Office Add-in**.
5. Choose the option to upload a manifest file.
6. Upload the target manifest:
   - Dev: `manifests/manifest-dev.xml`
   - UAT: `manifests/manifest-uat.xml`
   - Prod: `manifests/manifest-prod.xml`
7. Review the app metadata:
   - Display name
   - Provider
   - Word host
   - Requested permission: `ReadWriteDocument`
   - Icon URLs
   - Task pane/source URL
8. Assign access:
   - Dev: specific users or `M365-Addin-Giriyoge-DEV-Users`
   - UAT: specific users or `M365-Addin-Giriyoge-UAT-Users`
   - Prod: start with `M365-Addin-Giriyoge-PROD-Pilot`
9. Finish deployment.
10. Ask assigned users to fully close and reopen Word.
11. Allow propagation time. Microsoft notes that add-ins can take up to 24 hours to appear and updates can take up to 72 hours.

## Validation Steps In Word

Use a user account that is assigned to the target group.

1. Sign in to Microsoft Word with the organizational account.
2. Restart Word after the deployment is complete.
3. Open a blank document.
4. Check the Home ribbon and Add-ins area for the deployed add-in.
5. Open the task pane.
6. Confirm the expected text:

| Environment | Expected task pane text | Expected inserted text |
| --- | --- | --- |
| Dev | `Hello World - Dev` | `Hello from Dev` |
| UAT | `Hello World - UAT` | `Hello from UAT` |
| Prod | `Hello World - Production` | `Hello from Production` |

7. Click **Insert paragraph**.
8. Confirm the expected text is inserted into the document.

## Side-By-Side Dev And UAT Test

Assign the same pilot user to both:

- `M365-Addin-Giriyoge-DEV-Users`
- `M365-Addin-Giriyoge-UAT-Users`

After propagation, the user should see both:

- Giriyoge Office Add-in Dev
- Giriyoge Office Add-in UAT

If only one appears, verify both custom apps are active, both assignments include the pilot user, and the user has restarted Word.

## Update Process

The Angular web app can be updated independently by redeploying the hosted Static Web App.

Upload a new manifest only when manifest metadata changes, such as:

- SourceLocation
- AppDomains
- DisplayName
- Icons
- Ribbon commands
- Permissions
- Version

Keep each manifest ID stable after deployment. Changing the ID creates a separate add-in rather than updating the existing one.

## Rollback

For an urgent rollback:

1. In Microsoft 365 Admin Center, go to **Settings** > **Integrated apps**.
2. Open the affected Giriyoge add-in.
3. Remove the assignment or turn off the app.
4. If a previous manifest is approved, upload it as an update.
5. Ask pilot users to restart Word after propagation.

## Troubleshooting

| Symptom | Checks |
| --- | --- |
| Add-in does not appear | Confirm assignment, group membership, license, Exchange Online mailbox, and Word restart. |
| Only Dev or only UAT appears | Confirm Dev and UAT are deployed as separate custom apps and assigned to the same pilot user. |
| Task pane is blank | Confirm the Azure Static Web App URL opens in a browser and corporate network policy allows it. |
| Icon does not appear | Open the icon URL from the manifest in a browser. |
| Manifest upload fails | Validate the XML manifest and confirm all URLs are HTTPS. |
| Wrong environment opens | Confirm the add-in DisplayName and SourceLocation match the intended environment. |

## Handoff Email Template

Subject: Request to deploy Giriyoge Word Office.js add-in manifests to pilot groups

Hello Admin Team,

Please deploy the attached Giriyoge Microsoft Word Office.js task pane add-in manifests through Microsoft 365 Admin Center > Settings > Integrated apps > Upload custom apps.

This is a web-based Office Add-in using XML manifests only. It does not require MSI, EXE, VSTO, COM add-ins, endpoint software, Microsoft Graph permissions, or tenant-wide app consent in Phase 1.

Please deploy:

- `manifest-dev.xml` to `M365-Addin-Giriyoge-DEV-Users`
- `manifest-uat.xml` to `M365-Addin-Giriyoge-UAT-Users`

For a side-by-side test, please assign my organizational user account to both groups. After propagation and Word restart, I should see both "Giriyoge Office Add-in Dev" and "Giriyoge Office Add-in UAT" in Word.

Expected validation:

- Dev task pane shows `Hello World - Dev` and inserts `Hello from Dev`
- UAT task pane shows `Hello World - UAT` and inserts `Hello from UAT`

Thank you.
