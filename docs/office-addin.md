# Comet Word Integration – Checkout, Authoring, Workflow, and Platform Migration Design

---

# 1. Overview

The existing Comet system uses **CKEditor4** for report authoring, where:

- Each report is stored as sections in the database
- Each section contains HTML content
- Metadata and tag data are stored alongside section content
- No DOCX file exists during authoring

The new design replaces CKEditor4 with Microsoft Word Desktop (preferred) and Word Web (fallback) and transitions to a document-based model (DOCX).

In the new system:

- The DOCX document becomes the single source of truth for report content
- NFS stores the authoritative persisted DOCX version
- SharePoint copies are transient editing artifacts only
- SharePoint is used temporarily for editing
- The database stores only:
  - Metadata
  - Tag data
  - Workflow state
  - File references
  - Versioning information
  - Checkout session information

The migration is performed report type by report type, starting with:

- FI Periodical
- Company Note
- Sector Note

Legacy report types continue using the CKEditor platform until migrated.

---

# 2. Migration Strategy

The migration follows a parallel platform architecture.

Both systems coexist:

| Platform | Report Types |
|---|---|
| Legacy CKEditor Platform | Existing report types |
| Word Authoring Platform | Migrated report types |

The migration is performed incrementally to:

- Minimize production risk
- Avoid impacting existing Comet REST APIs
- Allow gradual operational stabilization
- Validate Word-based workflows progressively

---

# 3. Key Design Principles

| Principle | Description |
|---|---|
| Single Source of Truth | DOCX stored in NFS contains all report content |
| No Redundancy | Section content is NOT stored in DB |
| Temporary Editing | SharePoint used only during active editing |
| Platform Isolation | Word platform isolated from legacy CKEditor platform |
| Separation of Concerns | Content APIs separate from Office orchestration |
| Word Fidelity | Microsoft Word is the only editor |
| Backward Compatibility | Existing workflows and CAT authorization preserved |
| Incremental Migration | Report types migrated gradually |
| API-First Design | Office platform exposed through dedicated REST APIs |
| Thin Add-in | Business logic resides in backend services |

The Office Add-in should remain orchestration-focused and avoid embedding business logic. All workflow, validation, authorization, and persistence decisions must remain server-side.

---

# 4. System Architecture

## Legacy Platform

```text
Comet UI
    ↓
Comet REST
    ↓
CKEditor
    ↓
Database Section Storage
```

## New Office Platform

```text
Word Add-in / Comet UI
        ↓
Comet-Office-REST
        ↓
Comet Content REST
        ↓
NFS / SharePoint / CAT
```

---

# 5. System Components

| Component | Responsibility |
|---|---|
| Comet UI | Launch/edit entry point |
| Comet-Office-REST | Office orchestration and business APIs |
| Comet Content REST | NFS DOCX read/write operations |
| SharePoint | Temporary editing workspace |
| Microsoft Word Desktop/Web | Authoring interface |
| Comet Office Add-in | Word-integrated Comet functionality |
| CAT Application | Authorization and role mapping |
| NFS | Authoritative DOCX storage |

---

# 6. Platform Routing

Reports are routed based on report type and authoring platform.

Example:

| Report Type | Platform |
|---|---|
| FI Periodical | Word Platform |
| Company Note | Word Platform |
| Sector Note | Word Platform |
| Legacy Reports | CKEditor Platform |

Recommended field:

| Field | Example |
|---|---|
| authoringPlatform | CKEDITOR |
| authoringPlatform | WORD |

---

# 7. Authorization Model (Unchanged)

Authorization continues to be governed by the CAT application.

Users:
- Have business roles
- Belong to teams

Roles include:
- Analyst
- SAReviewer
- JPReviewer
- Distribution
- Translator

## Access Rules

### Authored State

Only Analysts associated with the report team:
- Can view
- Can checkout/edit

### SAReview State

Only SAReviewer:
- Can edit

Analysts:
- View only

### Other States

Continue following existing CAT authorization rules.

---

# 8. Authentication Model

## Existing Platform

Current Comet UI authentication continues unchanged:
- Browser-based SSO
- teaClient integration
- Session/cookie-based authentication

## Office Platform Authentication

The Word Add-in uses:
- HTTPS
- Token-based authentication
- JWT access tokens

Recommended flow:

```text
Word Add-in
      ↓
Office Identity / Enterprise SSO
      ↓
Comet-Office-REST
      ↓
JWT Issued
      ↓
Authenticated API Calls
```

## Authentication Responsibilities

| Component | Responsibility |
|---|---|
| Enterprise SSO | User authentication |
| teaClient | Existing enterprise auth integration |
| Comet-Office-REST | Token issuance and validation |
| CAT | Authorization only |

---

# 9. Data Model Evolution

## Legacy Model (CKEditor)

Database stores:
- sectionHtml
- metadata
- tagdata

## New Word Model

Content stored exclusively in DOCX.

Sections represented using:
- Word content controls

Database stores:
- Metadata
- Tag data
- Workflow state
- File references
- Version metadata
- Checkout session metadata

---

# 10. Recommended Core Entities

## Report

| Field | Purpose |
|---|---|
| reportId | Unique report identifier |
| reportType | FI Periodical, Company Note |
| authoringPlatform | WORD / CKEDITOR |
| workflowState | Current workflow |
| currentVersion | Current document version |
| nfsPath | DOCX storage location |
| createdBy | Creator |
| createdDate | Creation timestamp |

## CheckoutSession

| Field | Purpose |
|---|---|
| sessionId | Checkout session |
| reportId | Associated report |
| userId | Checked out by |
| sessionOwner | Active editing owner |
| sharepointFileId | Temporary SharePoint file |
| sharepointPath | Temporary SharePoint location |
| documentLeaseId | Lease/lock identifier |
| checkoutTime | Checkout timestamp |
| leaseExpiry | Session expiry |
| status | ACTIVE / CHECKED_IN / EXPIRED |

## ReportVersion

| Field | Purpose |
|---|---|
| versionId | Version identifier |
| reportId | Associated report |
| versionNumber | Sequential version |
| nfsPath | Version DOCX location |
| checksum | Integrity validation |
| createdBy | Version creator |
| createdDate | Version timestamp |

---

# 11. Checkout (Edit) Flow

## Legacy Flow

```text
Comet UI
    ↓
Comet REST
    ↓
Comet Content REST
    ↓
Database
    ↓
CKEditor UI
```

## New Word Flow

```text
Comet UI / Add-in
        ↓
Comet-Office-REST
        ↓
Comet Content REST
        ↓
NFS
        ↓
Comet-Office-REST
        ↓
SharePoint Upload
        ↓
Generate Edit URL
        ↓
Microsoft Word Desktop/Web
        ↓
Office Add-in Loads
```

## Checkout Steps

1. User clicks Edit (Checkout)
2. Request reaches Comet-Office-REST
3. Content REST retrieves DOCX from NFS
4. Comet-Office-REST:
   - Uploads DOCX to SharePoint
   - Grants access
   - Creates checkout session
   - Generates edit link
5. User redirected to Word Desktop (preferred) or Word Web
6. Office Add-in initializes
7. JWT authentication established
8. Document becomes editable

---

# 12. Authoring Phase

Editing occurs entirely inside Microsoft Word.

Word handles:
- Layout and formatting
- Track changes
- Auto-save
- Rich document fidelity

The Office Add-in provides:
- Section awareness via content controls
- Metadata updates
- Workflow actions
- Check-in
- Validation integration

---

# 13. Content Controls Strategy

Sections inside DOCX are represented using Word content controls.

Recommended usage:

| Content Control Type | Purpose |
|---|---|
| Section Controls | Report sections |
| Metadata Controls | Report metadata |
| Exhibit Controls | Charts/tables |
| Tag Controls | Semantic tagging |

Important:
- Structural elements should not rely on paragraph parsing
- Important regions should remain content-control driven
- Document structure should be governed through content controls rather than visual formatting alone
- Mandatory structural regions should be validated during check-in and workflow transitions

---

# 14. Check-in Flow

```text
Office Add-in
      ↓
Comet-Office-REST
      ↓
Download DOCX from SharePoint
      ↓
Comet Content REST
      ↓
Save to NFS
      ↓
Create Version
      ↓
Delete SharePoint File
      ↓
Update Workflow State
```

## Check-in Steps

1. User clicks Check-in
2. Add-in calls Comet-Office-REST
3. DOCX downloaded from SharePoint
4. Content REST saves DOCX to NFS
5. Version entry created
6. SharePoint file deleted
7. Checkout session closed
8. Report marked CHECKED_IN

---

# 15. Workflow (Unchanged)

| State | Editable By | Viewable By |
|---|---|---|
| Authored | Analyst (team-based) | Analyst |
| SAReview | SAReviewer | Analyst (view-only) |
| JPReview | JPReviewer | Others |
| Distribution | Distribution | Others |
| Translator | Translator | Others |

---

# 16. SharePoint Usage

| Aspect | Behavior |
|---|---|
| Purpose | Temporary editing workspace |
| Lifecycle | Created at checkout |
| Cleanup | Deleted at check-in |
| Persistence | Not authoritative |
| Ownership | Managed by Comet-Office-REST |

---

# 17. Versioning Strategy

DOCX files should use immutable versioning.

Example:

```text
/report123/
    v1.docx
    v2.docx
    v3.docx
```

Benefits:
- Rollback
- Auditability
- Recovery
- Historical traceability

---

# 18. Publishing Pipeline

Publishing artifacts are generated from authoritative DOCX files stored in NFS.

Example:

```text
NFS DOCX
    ↓
Publishing Service
    ↓
Generate:
    - HTML5
    - PDF
    - Email HTML
    - Distribution artifacts
```

Publishing outputs are derived directly from DOCX rather than database content.

---

# 19. Edge Case Handling

## User Does Not Check-In

Background recovery job:
- Detects stale checkout
- Downloads latest DOCX
- Saves to NFS
- Deletes SharePoint file
- Marks session expired

## Multiple Checkout Attempts

Blocked if:
- Report already checked out
- Active checkout session exists

## Token Expiration

Add-in should support:
- Silent token refresh
- Session renewal

## Document Validation Failure

Check-in blocked if:
- Required content controls missing
- Invalid template structure
- Metadata inconsistency detected

---

# 20. Document Validation

Recommended validation checks:

- Required content controls exist
- Metadata integrity
- Template version compatibility
- Template compatibility validation
- Unsupported template detection
- Missing mandatory content controls
- Structural integrity
- Restricted sections preserved

Validation should occur:
- During check-in
- During workflow submission

---

# 21. Template Versioning

Templates should be versioned.

Recommended metadata:

| Field | Purpose |
|---|---|
| templateVersion | Template compatibility |
| reportType | Associated report type |

This supports:
- Future migrations
- Backward compatibility
- Template evolution

---

# 22. Security Recommendations

## Recommended

- HTTPS only
- JWT-based API authentication
- Backend-controlled SharePoint access
- Short-lived access tokens
- Silent token refresh

## Avoid

- Direct SharePoint access from Add-in
- Long-lived tokens
- Browser cookie dependency in Add-in
- Storing passwords in Add-in

---

# 23. Benefits

- Eliminates DB/DOCX duplication
- DOCX becomes authoritative source
- Preserves existing workflow model
- Preserves CAT authorization
- Enables Word-native fidelity
- Simplifies long-term content architecture
- Supports gradual migration
- Reduces risk to existing platform
- Enables future Office/AI integrations

---

# 24. Summary

The system transitions from a database-driven HTML authoring model to a document-driven DOCX platform.

Microsoft Word becomes the primary authoring interface, while:
- NFS stores authoritative content
- SharePoint acts as temporary editing workspace
- Comet-Office-REST manages Office orchestration
- CAT continues governing authorization

The migration occurs incrementally by report type, allowing:
- Controlled rollout
- Operational stabilization
- Reduced production risk
- Gradual retirement of the CKEditor platform

This architecture establishes a scalable enterprise document authoring platform centered around DOCX as the single source of truth.