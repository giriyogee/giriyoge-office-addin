# Validation And Troubleshooting Checklist

## Deployment Success

- Manifest validates with `npm run validate:manifests`.
- Manifest has a unique GUID for the target environment.
- SourceLocation is HTTPS and reachable from the target client.
- Word host is configured through `<Host Name="Document"/>`.
- Office.js loads from Microsoft CDN.
- Task pane opens from Word Home tab.
- Task pane displays `Hello World - <environment>`.
- Clicking **Insert paragraph** inserts `Hello from <environment>` into the Word document.
- Icons and support URL resolve successfully.
- Hosted Dev, UAT, and Prod manifests use real Azure Static Web Apps URLs before Word testing.

## Group Visibility

- Target users are members of the assigned Entra ID group.
- Out-of-scope users are not direct members or nested members of the group.
- Users have eligible Microsoft 365 licensing.
- Users are signed into Word with their organizational account.
- Admin Center assignment shows the expected group.
- Word has been restarted after propagation.

## Troubleshooting

| Symptom | Checks |
| --- | --- |
| Add-in does not appear | Confirm centralized deployment assignment, group membership, license, and propagation time. |
| Task pane is blank | Open browser dev tools for Office Add-ins, confirm HTTPS certificate trust and app server availability. |
| Certificate warning | Re-run `npm run certs:install`, then restart Word and the browser webview runtime. |
| Wrong environment text | Confirm the correct build command, hosted URL stamping, and matching manifest were used. |
| Multiple local add-ins collide | Confirm manifest GUIDs and DisplayName values are unique. |
| Manifest upload fails | Run `npm run validate:manifest:prod` and verify all production URLs are HTTPS. |

## Phase 1 Exit Criteria

- Dev, UAT, and Production manifests are validated after real Azure Static Web Apps URLs replace placeholders.
- Local opens in Word and displays `Hello World - Local`.
- Dev opens in Word and displays `Hello World - Dev`.
- UAT opens in Word and displays `Hello World - UAT`.
- Each local environment can insert its matching `Hello from <environment>` paragraph into the document.
- Admin team can deploy to a pilot Entra ID group through Microsoft 365 Admin Center.
