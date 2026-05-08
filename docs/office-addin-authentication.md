# Comet Office Platform – Detailed Authentication and HTTPS Architecture

---

# 1. Overview

The new Comet Office Platform introduces:
- Microsoft Word Add-in integration
- Word Desktop/Web authoring
- SharePoint-based temporary editing
- JWT-based API authentication
- HTTPS-only communication

The platform must support:
1. Office Add-in authentication
2. Existing Comet UI integration
3. Enterprise SSO compatibility
4. Secure API communication
5. Scalable token-based architecture

This document describes the complete recommended authentication architecture in detail.

---

# 2. Existing Authentication Architecture

Current legacy architecture:

```text
Browser
    ↓
Comet UI
    ↓
Comet REST
```

Authentication works using:
- Enterprise SSO
- teaClient integration
- Browser session/cookies

Typical flow:

```text
User logs into company SSO
        ↓
Browser receives session/cookie
        ↓
Browser calls Comet REST
        ↓
teaClient validates session
        ↓
User authenticated
```

This works well for browser applications.

---

# 3. Why Office Add-ins Need Different Authentication

Office Add-ins are NOT normal browser applications.

They run inside:
- Microsoft Word Desktop
- Word Web
- Embedded Office webviews

Problems with browser session authentication:
- Cookies may not be shared
- Browser session may not exist
- Office Desktop behaves differently from browser
- Session handling becomes unreliable

Therefore:
- Browser session authentication should NOT be primary authentication for Office Add-ins.

---

# 4. Recommended Authentication Approach

Recommended architecture:

| Responsibility | Recommended Technology |
|---|---|
| User Identity | Azure AD / Microsoft Identity |
| Token Validation | Spring Security |
| Enterprise Integration | teaClient |
| Internal API Authentication | JWT |
| Authorization | CAT |
| Transport Security | HTTPS |

---

# 5. Recommended Final Architecture

```text
                    +----------------------+
                    | Enterprise SSO       |
                    | (Corporate Identity) |
                    +----------+-----------+
                               |
                               | Federation
                               v
                    +----------------------+
                    | Azure AD / M365      |
                    +----------+-----------+
                               |
          ------------------------------------------------
          |                                              |
          |                                              |
          v                                              v
+---------------------+                    +----------------------+
| Word Add-in         |                    | Comet UI (Legacy UI) |
+----------+----------+                    +----------+-----------+
           |                                            |
           | Azure AD Token                             | Session/Cookie
           |                                            |
           v                                            v
+---------------------------------------------------------------+
|                   Comet-Office-REST                           |
|---------------------------------------------------------------|
| Spring Security JWT Validation                                |
| Legacy Session Validation                                     |
| teaClient Enrichment                                          |
| Internal JWT Issuance                                         |
| Business APIs                                                 |
+---------------------------------------------------------------+
```

---

# 6. Key Authentication Concepts

---

# 6.1 Enterprise SSO

SSO means:
- Single Sign-On

Users log in once and access:
- Office
- SharePoint
- Comet
- Corporate systems

without entering password repeatedly.

---

# 6.2 Azure AD Token

Azure AD token is:
- a Microsoft-issued identity token
- digitally signed
- trusted by Microsoft ecosystem

The token proves:
- who the user is
- which tenant/company they belong to
- expiration time

---

# 6.3 JWT

JWT means:
- JSON Web Token

It is a digitally signed token used for:
- API authentication
- stateless security

JWT acts like:
- temporary digital identity card

---

# 6.4 teaClient

teaClient remains important.

Recommended usage:
- employee enrichment
- internal mapping
- legacy compatibility
- existing session validation

NOT primary Office Add-in authentication.

---

# 6.5 CAT

CAT handles:
- authorization
- roles
- permissions
- workflow access

CAT does NOT perform authentication.

---

# 7. Authentication Flows

The platform supports two authentication flows:

| Flow | Client |
|---|---|
| Azure AD + JWT | Office Add-in |
| Session/Cookie | Existing Comet UI |

---

# 8. Word Add-in Authentication Flow

---

# STEP 1 – User Opens Word

User is already authenticated to:
- Office 365
- Corporate account
- Windows/company identity

Example:

```text
User logs into company laptop
        ↓
Office automatically authenticated
```

---

# STEP 2 – Office Add-in Loads

The Comet Office Add-in initializes inside Word.

Example:

```text
Word Desktop/Web
        ↓
Comet Office Add-in loads
```

---

# STEP 3 – Add-in Requests Azure AD Token

Using Microsoft Office APIs:

```javascript
OfficeRuntime.auth.getAccessToken()
```

Microsoft returns:
- Azure AD access token

This token contains:
- user identity
- tenant information
- expiration
- Microsoft digital signature

---

# STEP 4 – Add-in Calls Login API

Example:

```http
POST /auth/login
```

Request body:

```json
{
  "azureToken": "eyJhbGc..."
}
```

---

# STEP 5 – Spring Security Validates Azure Token

Spring Security validates:
- Microsoft signature
- issuer
- tenant
- audience
- expiration

This is the primary authentication step.

At this point:
- user identity is trusted

---

# 9. Detailed Azure Token Validation

Spring Security validates:

| Validation | Purpose |
|---|---|
| Signature validation | Ensure token genuinely issued by Microsoft |
| Expiration validation | Ensure token not expired |
| Issuer validation | Ensure token came from Azure AD |
| Tenant validation | Ensure token belongs to company tenant |
| Audience validation | Ensure token intended for Comet Office app |

---

# 10. Microsoft Public Keys

Microsoft signs tokens using private keys.

Microsoft publishes public keys publicly.

Spring Security:
- downloads Microsoft public keys
- caches them
- uses them to verify signatures

No manual cryptography implementation required.

---

# 11. Example Spring Security Configuration

Dependency:

```gradle
implementation 'org.springframework.boot:spring-boot-starter-oauth2-resource-server'
```

---

# application.yml

```yaml
spring:
  security:
    oauth2:
      resourceserver:
        jwt:
          issuer-uri: https://login.microsoftonline.com/<tenant-id>/v2.0
```

Spring automatically:
- validates tokens
- verifies Microsoft signatures
- validates expiration
- validates issuer

---

# 12. User Identity Extraction

After validation:

Spring Security extracts user information.

Example:

```json
{
  "preferred_username": "ygiri@company.com",
  "name": "Yogesh Giri",
  "oid": "abc123",
  "tid": "tenant123"
}
```

---

# 13. Minimal teaClient Usage (Recommended)

Recommended architecture:
- Spring Security performs primary authentication
- teaClient used only for enterprise enrichment

Example:

```text
Spring Security validates Azure token
        ↓
teaClient enriches employee information
```

Example teaClient usage:

```java
EmployeeDetails details =
    teaClient.getEmployee("ygiri");
```

Possible enrichment:
- employee ID
- internal groups
- business mappings
- department
- legacy compatibility

---

# 14. Why Minimal teaClient Usage is Recommended

Advantages:

| Benefit | Why Important |
|---|---|
| Less coupling | Modern architecture |
| Better scalability | Local JWT validation |
| Better Office compatibility | No session dependency |
| Cleaner separation | Identity vs enrichment |
| Easier migration | Incremental modernization |

---

# 15. Internal Comet JWT Generation

After successful authentication:

Comet-Office-REST generates internal JWT.

Purpose:
- lightweight API authentication
- avoid validating Azure token on every request

Example JWT:

```json
{
  "sub": "ygiri",
  "role": "Analyst",
  "teams": ["FI"],
  "exp": 1715000000
}
```

---

# 16. Why Internal JWT is Important

Azure token:
- belongs to Microsoft ecosystem

Internal Comet JWT:
- belongs to Comet ecosystem

Advantages:
- smaller token
- custom claims
- internal control
- faster validation
- reduced Microsoft dependency

---

# 17. API Requests After Login

After login:
- Add-in uses Comet JWT for ALL future requests

Example:

```http
Authorization: Bearer comet-jwt
```

Used for:
- checkout
- check-in
- metadata
- workflow
- validation

---

# 18. Important Performance Optimization

Azure validation happens:
- ONLY during login
- token refresh

NOT every request.

Normal API requests use:
- lightweight local JWT validation

This is critical for performance.

---

# 19. Request Lifecycle

---

# Initial Authentication

```text
Add-in
    ↓
Azure AD token
    ↓
Spring Security validation
    ↓
teaClient enrichment
    ↓
Internal JWT generation
```

---

# Normal API Request

```text
Add-in
    ↓
Comet JWT
    ↓
Local JWT validation
    ↓
Business logic
```

---

# 20. Existing Comet UI Authentication Flow

Existing Comet UI continues using:
- session/cookie authentication

Flow:

```text
Browser
    ↓
Session cookie
    ↓
Comet-Office-REST
    ↓
teaClient validation
```

No immediate migration required.

---

# 21. Dual Authentication Support

Comet-Office-REST supports BOTH:

| Authentication Type | Client |
|---|---|
| JWT/Bearer Token | Word Add-in |
| Session/Cookie | Existing Comet UI |

---

# 22. Recommended Internal Authentication Pipeline

```text
Request
    ↓
JWT present?
    YES → JWT validation
    NO
    ↓
Session cookie present?
    YES → teaClient/session validation
    NO
    ↓
Reject request
```

---

# 23. Internal User Normalization

After authentication:

Normalize users into common internal object.

Example:

```json
{
  "userId": "ygiri",
  "roles": ["Analyst"],
  "authSource": "JWT"
}
```

OR

```json
{
  "userId": "ygiri",
  "roles": ["Analyst"],
  "authSource": "SESSION"
}
```

Business logic should not care about authentication source.

---

# 24. Token Expiration Strategy

Recommended token lifetimes:

| Token | Expiration |
|---|---|
| Azure AD Token | Microsoft-managed |
| Comet JWT | 1 hour |
| Refresh Token | 8–12 hours |

---

# 25. Token Refresh Flow

Users may edit documents for hours.

Recommended:
- silent token refresh

Example:

```text
JWT expires
    ↓
Add-in silently refreshes token
    ↓
New JWT issued
```

---

# 26. Recommended Token Storage

Store access token:
- in memory only

Avoid:
- localStorage
- files
- persistent storage

Reason:
- security

---

# 27. HTTPS Requirements

ALL communication must use HTTPS.

Required for:
- Office Add-ins
- authentication
- token transmission
- SharePoint integration

---

# 28. HTTPS Certificate Strategy

| Environment | Certificate Type |
|---|---|
| Development | Self-signed |
| QA/UAT | Enterprise CA |
| Production | Enterprise/Public CA |

---

# 29. Recommended Local Development Setup

Use:
- mkcert

Install:

```bash
mkcert
```

Generate certificate:

```bash
mkcert localhost 127.0.0.1
```

Generated:
- localhost.pem
- localhost-key.pem

---

# 30. Convert Certificate for Spring Boot

Convert to PKCS12:

```bash
openssl pkcs12 -export \
  -in localhost.pem \
  -inkey localhost-key.pem \
  -out keystore.p12
```

---

# 31. Spring Boot HTTPS Configuration

```yaml
server:
  port: 8443
  ssl:
    enabled: true
    key-store: classpath:keystore.p12
    key-store-password: password
    key-store-type: PKCS12
```

---

# 32. Production Certificate Storage

Store certificates in:
- Azure Key Vault
- Vault
- Kubernetes Secrets
- enterprise certificate stores

Do NOT:
- commit certificates to Git
- hardcode passwords

---

# 33. JWT Signing Strategy

Recommended:
- RSA key pair

| Key | Purpose |
|---|---|
| Private Key | Sign JWT |
| Public Key | Validate JWT |

---

# 34. RSA Key Generation

Generate private key:

```bash
openssl genrsa -out private.pem 2048
```

Generate public key:

```bash
openssl rsa -in private.pem -pubout -out public.pem
```

---

# 35. SharePoint Security Recommendation

Recommended:

```text
Add-in
    ↓
Comet-Office-REST
    ↓
SharePoint
```

Avoid:
- direct SharePoint access from Add-in

Benefits:
- centralized security
- controlled permissions
- easier auditing

---

# 36. Authentication vs Authorization

---

# Authentication

Question:
> Who are you?

Handled by:
- Azure AD
- JWT
- teaClient/session

---

# Authorization

Question:
> What are you allowed to do?

Handled by:
- CAT
- workflow rules
- team mappings

---

# 37. Final Recommended Authentication Architecture

```text
User opens Word
      ↓
Add-in loads
      ↓
Add-in gets Azure AD token
      ↓
POST /auth/login
      ↓
Spring Security validates Azure token
      ↓
Optional teaClient enrichment
      ↓
Comet JWT generated
      ↓
Add-in stores JWT
      ↓
All future APIs use Comet JWT
      ↓
Local JWT validation
      ↓
CAT authorization checks
      ↓
Business logic executes
```

---

# 38. Final Recommended Technology Stack

| Area | Recommendation |
|---|---|
| Identity Provider | Azure AD |
| Enterprise SSO | Existing Corporate SSO |
| Primary Token Validation | Spring Security |
| Enterprise Integration | teaClient |
| API Authentication | JWT |
| Authorization | CAT |
| Transport Security | HTTPS |
| JWT Signing | RSA |
| Certificate Management | Enterprise CA / Vault |
| Add-in Architecture | Thin client |
| Backend Architecture | API-first |

---

# 39. Summary

The recommended authentication architecture modernizes the Comet Office Platform while preserving compatibility with existing enterprise systems.

The architecture:
- uses Azure AD as primary identity provider
- uses Spring Security for token validation
- minimizes teaClient dependency
- supports both Office Add-ins and legacy Comet UI
- uses JWT for scalable API authentication
- uses HTTPS everywhere
- preserves CAT authorization

This approach:
- works reliably across Word Desktop and Web
- avoids browser session limitations
- supports incremental migration
- reduces coupling with legacy authentication models
- provides a scalable enterprise-grade foundation for future Office integrations