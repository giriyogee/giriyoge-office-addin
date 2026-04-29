# Entra ID Group-Based Visibility Setup

## Recommended Groups

Create dedicated security groups in Microsoft Entra ID:

- `M365-Addin-Giriyoge-DEV-Users`
- `M365-Addin-Giriyoge-UAT-Users`
- `M365-Addin-Giriyoge-PROD-Pilot`
- `M365-Addin-Giriyoge-PROD-All`

Use security groups for explicit deployment targeting. Keep ownership with Digital Workspace or the platform operations team.

## Create Or Validate Groups

1. Open [Microsoft Entra admin center](https://entra.microsoft.com).
2. Go to **Identity** > **Groups** > **All groups**.
3. Create or select the target security group.
4. Add pilot users.
5. Confirm users have Microsoft 365 Apps for enterprise and Exchange Online mailboxes, which are required for centralized Office Add-in deployment scenarios.

## Assign Group In Microsoft 365 Admin Center

1. Open [Microsoft 365 Admin Center](https://admin.microsoft.com).
2. Go to **Settings** > **Integrated apps**.
3. Select the Giriyoge add-in.
4. Open **Users** or **Assignments**.
5. Select the intended Entra ID security group.
6. Save changes.
7. Wait for deployment propagation.

## Visibility Validation

1. Test with one user inside the group.
2. Test with one user outside the group.
3. Confirm only the in-scope user sees the add-in in Word.
4. Confirm group membership changes take effect after propagation and Word restart.

## Operating Standard

- Never assign early lifecycle manifests to broad production groups.
- Avoid direct individual assignment except for emergency validation.
- Record group object IDs in release documentation.
- Use change management for production group expansion.
