# Microsoft 365 Admin Center Deployment Guide

Audience: Digital Workspace and Microsoft 365 administrators.

For a sendable admin package, start with `docs/admin-team-handoff.md`.

## Deployment Model

Deploy through Microsoft 365 Admin Center centralized deployment. Do not distribute MSI, EXE, VSTO, COM add-ins, or endpoint installer instructions.

Recommended rollout sequence:

1. DEV pilot group.
2. UAT group.
3. Production pilot.
4. Broad production rollout.

Use separate manifests for each lifecycle stage when users need side-by-side access. Each stage must keep a unique manifest GUID and environment-specific SourceLocation.

## Hosted Manifest Preparation

Before deployment, confirm hosted placeholder URLs in the Dev, UAT, and Prod manifests have been replaced with the real Azure Static Web Apps URLs.

- Dev currently uses `https://brave-ground-0c778b50f.7.azurestaticapps.net`
- UAT currently uses `https://yellow-smoke-0aede930f.7.azurestaticapps.net`
- `https://prod.giriyoge-office-addin.example.com`

Confirm all URLs are HTTPS, externally reachable by managed clients, and use trusted certificates.

## Deploy From Integrated Apps

1. Sign in to [Microsoft 365 Admin Center](https://admin.microsoft.com) with a work or school account that can manage Integrated Apps.
2. Go to **Settings** > **Integrated apps**.
3. Select **Upload custom apps**.
4. Choose **Office Add-in**.
5. Upload the manifest for the target lifecycle stage, for example `manifests/manifest-prod.xml`.
6. Validate app metadata, permissions, icon rendering, and Word host targeting.
7. Assign specific users or Entra ID groups. Prefer groups for Dev, UAT, and Prod pilot visibility.
8. Finish deployment.
9. Allow propagation time before testing in Word.

## Deployment Governance

- Keep manifest IDs stable after deployment. Changing an ID creates a new add-in.
- Use version increments for updates.
- Require a manifest validation step in release gates.
- Use group-based targeting for controlled visibility.
- Maintain separate DEV/UAT/PROD hosting endpoints and deployment groups.

## Rollback

1. In Microsoft 365 Admin Center, open the deployed app.
2. Disable or remove the assignment for affected users/groups.
3. If a previous manifest version is approved, upload the prior package as an update.
4. Ask pilot users to restart Word after the admin change propagates.
