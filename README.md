# my-idp

## Issue a certificate

```powershell
# Define certificate parameters
$certName = "MyOIDCRSACertificate"
$certPassword = ConvertTo-SecureString -String "1" -Force -AsPlainText

# Generate a self-signed RSA certificate
$cert = New-SelfSignedCertificate -DnsName "my-idp.com" -FriendlyName $certName -CertStoreLocation "Cert:\CurrentUser\My" -KeyAlgorithm RSA -KeyLength 2048
```