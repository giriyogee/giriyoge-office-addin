Yes. I checked the current Microsoft documentation, and I would slightly refine the approach we discussed earlier.

For COMET's Java Spring Boot backend authenticating to Microsoft Entra ID with a certificate, the clean current approach is:

Generate private key + CSR → corporate CA signs CSR → receive certificate → give public certificate to IAM → keep private key with COMET → use the private key + certificate from your backend to authenticate.

Microsoft explicitly documents this model for MSAL Java: the Entra app gets the public certificate, while the Java client uses the corresponding private key + X509 certificate. Microsoft also supports PKCS#12 as an alternative input format. 

Here is the complete process for your setup.


---

1. Decide what you're actually creating

Your COMET backend is a confidential client/application.

You want:

COMET Spring Boot
       │
       │ Certificate-based client authentication
       ▼
Microsoft Entra ID
       │
       │ Access token
       ▼
Microsoft Graph
       │
       ▼
SharePoint

Microsoft recommends certificates rather than client secrets for production confidential applications. 


---

2. Generate the private key

I recommend generating the private key yourself, rather than asking the certificate portal to generate it.

For example, using OpenSSL:

openssl genrsa -out comet-entra.key 2048

This creates:

comet-entra.key

This is the most sensitive file.

Do NOT give this to IAM.

Do not put it in the CSR ZIP.

Do not commit it to Git.

Do not email it.

Ideally, eventually it should live in your organization's approved secret/certificate store.


---

3. Generate the CSR

Generate the CSR using that private key:

openssl req -new \
  -key comet-entra.key \
  -out comet-entra.csr

You will be asked for certificate subject information.

The result:

comet-entra.csr

The CSR contains the public key and certificate-request information, but not your private key.

Microsoft's own Java certificate sample demonstrates using OpenSSL to generate the private key and CSR before registering the resulting certificate with Entra. 


---

4. ZIP the CSR

Your Nomura portal specifically asks for a CSR in ZIP format.

So:

zip comet-entra-csr.zip comet-entra.csr

Your ZIP should contain:

comet-entra-csr.zip
└── comet-entra.csr

Do NOT do this:

comet-entra-csr.zip
├── comet-entra.csr
└── comet-entra.key    ❌

The private key must remain with COMET.


---

5. Submit the CSR to the Nomura certificate portal

Now you're using the portal for what it is intended to do:

Your CSR
   │
   ▼
Nomura Certificate Authority / Certificate Service
   │
   │ validates / approves
   │ signs CSR
   ▼
Corporate X.509 certificate

You are not asking the portal to generate your application's identity from scratch.

You're asking it to sign the public key contained in your CSR.


---

6. What should you select for the certificate format?

This is where I would be careful with the choices you showed me earlier.

You have things such as:

DER .cer

PKCS#7

PKCS#8

JKS

etc.


For the certificate that IAM needs

You ultimately need:

comet-entra.cer

Microsoft Entra's current documentation says that when uploading a certificate to an App Registration, the accepted file types include:

.cer
.pem
.crt



So DER .cer is a perfectly good choice for the certificate that IAM needs.

I would NOT choose PKCS#7 for this.

PKCS#7 is primarily a certificate/chain container and isn't what your Spring Boot application needs for the private-key authentication credential.

And I wouldn't choose JKS just because COMET is Java.

This is the important correction from our earlier discussion.

JKS is a Java keystore format. It is not required by Microsoft Entra or MSAL Java.

MSAL Java can work with:

PrivateKey
X509Certificate

directly, or with a PKCS#12 input. 

So if the portal is simply issuing you the certificate, DER .cer is the cleanest output.


---

7. Receive the signed certificate

After the certificate request is approved, you should receive something like:

comet-entra.cer

Now your files are:

comet-entra.key     🔐 PRIVATE KEY
comet-entra.csr        CSR
comet-entra.cer     📜 SIGNED CERTIFICATE

The .key and .cer correspond to each other because the CSR was generated from that private key.


---

8. Give the .cer to IAM

This is what your IAM colleague was asking for.

Give them:

comet-entra.cer

They upload it to:

Microsoft Entra ID → App registrations → COMET → Certificates & secrets → Certificates → Upload certificate

Microsoft explicitly says only the public certificate should be uploaded; the private key stays with the application. 

So:

File	COMET	IAM

comet-entra.key	🔐 Keep	❌ Never
comet-entra.csr	Keep/archive	❌
comet-entra.cer	Keep	✅ Give to IAM
Private key password	🔐 Keep	❌



---

9. IAM registers the certificate

After IAM uploads the .cer, Entra knows:

> "This public key belongs to the COMET application."



Microsoft recommends recording the certificate thumbprint after registration. 

Your COMET configuration should have things like:

TENANT_ID
CLIENT_ID
CERTIFICATE
PRIVATE_KEY

The private key never goes into Entra.


---

10. What does COMET actually use?

This is important.

Your Spring Boot application ultimately needs:

PrivateKey
+
X509Certificate

MSAL Java explicitly documents this approach:

PrivateKey privateKey;
X509Certificate publicKey;

IClientCredential credential =
    ClientCredentialFactory.createFromCertificate(
        privateKey,
        publicKey);



Then:

ConfidentialClientApplication app =
    ConfidentialClientApplication
        .builder(CLIENT_ID, credential)
        .authority(AUTHORITY)
        .build();

The application uses the private key to sign the authentication assertion, and Entra validates it against the public certificate registered on the app.


---

11. Where should you store the private key?

For development, you could technically have:

comet-entra.key

on the server.

For production, don't put it in your Git repository or application JAR.

Microsoft recommends secure certificate management and specifically points to Azure Key Vault for production certificate access/management. 

Given your Nomura environment, I'd follow whatever your organization's approved secret/certificate store is.

For example:

Certificate / Secret Store
                         │
                         │ private key
                         ▼
                   COMET Spring Boot
                         │
                         │ signs assertion
                         ▼
                    Microsoft Entra


---

12. Do you need PKCS#12?

Not necessarily.

This is where our earlier discussion got unnecessarily complicated.

Microsoft's MSAL Java API supports two relevant approaches:

Approach A — private key + certificate

PrivateKey
X509Certificate

This is explicitly documented by Microsoft. 

Approach B — PKCS#12

MSAL Java also supports a PKCS#12 input stream. 

So you don't need the certificate portal to issue PKCS#12.

If your infrastructure prefers PKCS#12, you can create it after receiving the corporate certificate:

openssl pkcs12 -export \
  -inkey comet-entra.key \
  -in comet-entra.cer \
  -out comet-entra.p12 \
  -name comet-entra

Then your Java application can use the .p12.

Microsoft's Azure Identity Java library also explicitly supports PFX/PKCS#12 for ClientCertificateCredential. 


---

13. What about JKS?

You don't need JKS for Entra authentication.

That's the main conclusion I want you to take away.

JKS can work because Java can load it and retrieve the PrivateKey and certificate. But it is just one possible storage/container format.

Your architecture doesn't require:

Certificate Portal → JKS → Entra

It can simply be:

OpenSSL
   │
   ├── Private key
   └── CSR
         │
         ▼
Nomura Certificate Portal
         │
         ▼
      .cer
         │
         ├────────► IAM → Entra
         │
         ▼
COMET securely stores private key
         │
         ▼
Spring Boot / MSAL Java
         │
         ▼
      Entra ID

And if you want a convenient container:

private.key + certificate.cer
              │
              ▼
          PKCS#12
              │
              ▼
       Spring Boot / MSAL


---

14. What I would do for COMET

Given everything you've shown me, this is the approach I'd recommend:

Certificate creation

1. OpenSSL
      ↓
2. Generate RSA private key
      ↓
3. Generate CSR
      ↓
4. ZIP CSR
      ↓
5. Submit to Nomura certificate portal
      ↓
6. Receive corporate-signed .cer

Entra registration

.cer
 ↓
IAM
 ↓
Entra App Registration
 ↓
Certificate credential

COMET

Private key 🔐
      +
Certificate 📜
      ↓
Secure certificate/secret store
      ↓
Spring Boot
      ↓
MSAL Java
      ↓
Entra ID
      ↓
Access token
      ↓
Microsoft Graph
      ↓
SharePoint

This is consistent with Microsoft's current documentation: public certificate registered in Entra, private key retained by the application, with MSAL Java supporting either the key/certificate objects or PKCS#12. 

One thing I would verify before generating the CSR

Don't run the OpenSSL commands yet until you confirm the certificate portal's CSR requirements, especially:

RSA vs EC

required key size

required Subject/CN

whether SAN is required

certificate validity period

whether the corporate CA requires a specific CSR signature/hash

whether the certificate must include a particular EKU such as Client Authentication


If you send me the CSR requirements section from that Nomura portal, I can give you the exact OpenSSL commands and values to use, including how to generate the CSR ZIP correctly.