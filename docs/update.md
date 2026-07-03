# Comet Word Integration – SharePoint Online vs SharePoint On-Prem Assessment

## 1. SharePoint Online

### Proposed Usage
Use SharePoint Online as a temporary collaborative workspace while a report is being authored. Once the report is published, the final DOCX will be stored back on NFS and removed from SharePoint.

### Benefits
- Real-time co-authoring and live editing with changes immediately visible to other users.
- Support for modern Microsoft Graph APIs, enabling:
  - Backend PDF generation without opening the document.
  - Uploading/downloading documents programmatically.
  - Managing document permissions programmatically.
  - Better integration with Office.js and future Microsoft capabilities.
- Native autosave and collaboration features.

### Considerations / Risks
- Standard SharePoint Online site storage is 50 GB (can be increased with approvals).
- Due to the firm's 10-year retention policy, deleting a document from SharePoint may not immediately release storage because retained content continues to consume capacity.
- An exemption from the retention policy may be required if SharePoint is used only as a temporary workspace.
- No Nomura-managed backup/DR solution currently exists. We are verifying whether Microsoft's native resiliency, replication, and recovery capabilities satisfy internal backup and regulatory requirements.

---

## 2. SharePoint On-Prem

### Proposed Usage
Use SharePoint On-Prem as a temporary document workspace with synchronization occurring when users explicitly save the document (Save or Ctrl+S).

### Benefits
- No dependency on Microsoft Graph APIs or app registration.
- Retention policies are less restrictive. Once content is deleted and removed from the recycle bin, storage is reclaimed.
- Existing backup and retention mechanisms may align better with current internal processes.

### Limitations
- No real-time co-authoring experience comparable to SharePoint Online.
- Autosave is disabled by default. We need to investigate whether programmatic autosave can be implemented.
- Modern Microsoft Graph APIs are unavailable.
- Backend PDF generation is not supported through SharePoint REST APIs; PDF generation would need to be performed through:
  - Office.js/UI workflow, or
  - A separate document conversion service (e.g., Aspose).
- Some future Microsoft investments and cloud capabilities are primarily targeted toward SharePoint Online.

---

# Office.js Add-in Deployment

## Option 1 – Microsoft 365 Centralized Deployment (Preferred)

### Benefits
- Centralized deployment of the Office.js add-in through the Microsoft 365 Admin Center.
- No MSI/EXE installation required (unlike VSTO add-ins).
- Easy rollout to specific user groups.
- Simplified version management and upgrades.

### Current Challenge
- Nomura currently does not have this capability enabled.
- M365 team is confirming timelines and feasibility.

---

## Option 2 – SharePoint App Catalog Deployment

### Benefits
- Deploy the same Office.js add-in through a SharePoint Online or SharePoint On-Prem App Catalog.
- Provides an alternative if centralized deployment is unavailable.

### Current Status
- We are validating with the SharePoint team whether an App Catalog is available and supported within the firm.

---

# Alternative Approach – .NET Add-in / WebView

### Benefits
- Full Word COM API access.
- Greater control over document-level operations.

### Drawbacks
- Requires .NET installation on client machines.
- More complex deployment and maintenance.
- Loses many of the advantages of the lightweight Office.js deployment model.

---

# Recommendation

| Capability | SharePoint Online | SharePoint On-Prem |
|------------|-------------------|--------------------|
| Real-time collaboration | ✅ | ❌ |
| Autosave | ✅ | ⚠️ Investigation required |
| Microsoft Graph APIs | ✅ | ❌ |
| Backend PDF generation | ✅ | ❌ |
| Programmatic permission management | ✅ | Limited |
| Storage retention concerns | ⚠️ | ✅ |
| Simpler compliance with existing retention processes | ❌ | ✅ |
| Future Microsoft investment | ✅ | ⚠️ Limited |

**Current Recommendation:** SharePoint Online provides the best user experience and technical capabilities for collaborative authoring and backend document operations. However, retention policy and storage management need to be resolved before finalizing the approach. SharePoint On-Prem remains a viable fallback option if compliance or retention requirements prevent the use of SharePoint Online.
