using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using my_idp.Models;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using Microsoft.ApplicationInsights;
using System.Text.Json;

namespace my_idp
{
    public class Commons
    {

        public static Lazy<X509SigningCredentials> LoadCertificate(string signingCertThumbprint)
        {
            return new Lazy<X509SigningCredentials>(() =>
            {

                X509Store certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                certStore.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection certCollection = certStore.Certificates.Find(
                                            X509FindType.FindByThumbprint,
                                            signingCertThumbprint,
                                            false);
                // Get the first cert with the thumb-print
                if (certCollection.Count > 0)
                {
                    return new X509SigningCredentials(certCollection[0]);
                }

                throw new Exception("Certificate not found");
            });
        }
        public static string BuildJwtToken(X509SigningCredentials SigningCredentials, string issuer, HttpRequest request, HomeViewModel model)
        {
            // Token issuance date and time
            DateTime time = DateTime.Now;

            // Prepare the sub claim base64 encoded
            string sub = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(model.email!));       

            // All parameters send to Azure AD B2C needs to be sent as claims
            IList<System.Security.Claims.Claim> claims = new List<System.Security.Claims.Claim>();
            claims.Add(new System.Security.Claims.Claim("sub", sub, System.Security.Claims.ClaimValueTypes.String, issuer));
            claims.Add(new System.Security.Claims.Claim("iat", ((DateTimeOffset)time).ToUnixTimeSeconds().ToString(), System.Security.Claims.ClaimValueTypes.Integer, issuer));
            claims.Add(new System.Security.Claims.Claim("email", model.email!, System.Security.Claims.ClaimValueTypes.String, issuer));
            claims.Add(new System.Security.Claims.Claim("email_verified", true.ToString(), System.Security.Claims.ClaimValueTypes.Boolean, issuer));
            claims.Add(new System.Security.Claims.Claim("version", true.ToString(), System.Reflection.Assembly.GetExecutingAssembly()!.GetName()!.Version!.ToString()!, issuer));

            if (model.name != null)
            {
                claims.Add(new System.Security.Claims.Claim("name", model.name, System.Security.Claims.ClaimValueTypes.String, issuer));
                claims.Add(new System.Security.Claims.Claim("given_name", model.name.Split(' ')[0], System.Security.Claims.ClaimValueTypes.String, issuer));
                claims.Add(new System.Security.Claims.Claim("family_name", model.name.Split(' ')[1], System.Security.Claims.ClaimValueTypes.String, issuer));
            }

            if (model.phone_number != null)
            {
                claims.Add(new System.Security.Claims.Claim("phone_number", model.phone_number, System.Security.Claims.ClaimValueTypes.String, issuer));
            }

            if (model.locality != null)
            {
                claims.Add(new System.Security.Claims.Claim("locality", model.locality, System.Security.Claims.ClaimValueTypes.String, issuer));
            }

            if (model.country != null)
            {
                claims.Add(new System.Security.Claims.Claim("country", model.country, System.Security.Claims.ClaimValueTypes.String, issuer));
            }

            // Create the token
            JwtSecurityToken token = new JwtSecurityToken(
                    issuer,
                    model.client_id,
                    claims,
                    time,
                    time.AddHours(24),
                    SigningCredentials);

            // Get the representation of the signed token
            JwtSecurityTokenHandler jwtHandler = new JwtSecurityTokenHandler();

            return jwtHandler.WriteToken(token);
        }
    }
}