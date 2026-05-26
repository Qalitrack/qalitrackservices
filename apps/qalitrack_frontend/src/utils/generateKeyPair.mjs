/**
 * generateKeyPair.mjs — One-time ECDSA P-256 key pair generator
 *
 * Run ONCE with:  node src/utils/generateKeyPair.mjs
 *
 * WHAT TO DO WITH THE OUTPUT:
 *   PUBLIC_KEY_JWK  → paste into src/utils/licenseUtils.js  (safe to commit)
 *   PRIVATE_KEY_JWK → store in your ERP environment / secrets vault ONLY
 *                     NEVER commit it, NEVER put it in the app
 *
 * The private key signs licenses. The public key verifies them.
 * Without the private key, no one can forge a valid license.
 */

import { webcrypto } from "crypto";
const { subtle } = webcrypto;

const keyPair = await subtle.generateKey(
  { name: "ECDSA", namedCurve: "P-256" },
  true,
  ["sign", "verify"]
);

const privateJwk = await subtle.exportKey("jwk", keyPair.privateKey);
const publicJwk  = await subtle.exportKey("jwk", keyPair.publicKey);

// Strip private key fields from public JWK just to be explicit
const { d: _d, ...safePublicJwk } = privateJwk; // privateJwk has 'd'; publicJwk doesn't — just being safe
void _d;

console.log("\n========================================================");
console.log("  PUBLIC KEY JWK  →  paste into licenseUtils.js");
console.log("========================================================");
console.log(JSON.stringify(publicJwk, null, 2));

console.log("\n========================================================");
console.log("  PRIVATE KEY JWK  →  store in ERP ONLY, never in app");
console.log("========================================================");
console.log(JSON.stringify(privateJwk, null, 2));

console.log("\n========================================================");
console.log("  C# / ASP.NET — how to use the private key in your ERP");
console.log("========================================================");
console.log(`
// NuGet packages needed:
//   Microsoft.IdentityModel.Tokens
//   System.IdentityModel.Tokens.Jwt

using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

public class LicenseService
{
    // Store the private key JWK in appsettings / Azure Key Vault / environment variable
    // NEVER hardcode it here
    private readonly ECDsa _privateKey;

    public LicenseService(string privateKeyJwkJson)
    {
        var jwk = new JsonWebKey(privateKeyJwkJson);
        _privateKey = ECDsa.Create();
        var ecParams = new ECParameters
        {
            Curve = ECCurve.NamedCurves.nistP256,
            Q = new ECPoint
            {
                X = Base64UrlEncoder.DecodeBytes(jwk.X),
                Y = Base64UrlEncoder.DecodeBytes(jwk.Y),
            },
            D = Base64UrlEncoder.DecodeBytes(jwk.D), // private key component
        };
        _privateKey.ImportParameters(ecParams);
    }

    public string IssueLicense(
        string customerId,
        string appId,         // e.g. "qalitrack-frontend"
        string[] features,    // e.g. ["kiosk"]
        DateTime expiresAt,
        string? machineId = null)   // optional hardware binding
    {
        var key = new ECDsaSecurityKey(_privateKey);
        var creds = new SigningCredentials(key, SecurityAlgorithms.EcdsaSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, customerId),
            new("app",      appId),
            new("features", System.Text.Json.JsonSerializer.Serialize(features),
                JsonClaimValueTypes.JsonArray),
        };

        if (machineId is not null)
            claims.Add(new Claim("mid", machineId));

        var token = new JwtSecurityToken(
            issuer:             "qalibrated.co.ke",
            claims:             claims,
            notBefore:          DateTime.UtcNow,
            expires:            expiresAt,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

// USAGE EXAMPLE:
// var svc = new LicenseService(Environment.GetEnvironmentVariable("LICENSE_PRIVATE_KEY_JWK"));
// string licenseToken = svc.IssueLicense(
//     customerId: "KTDA-001",
//     appId:      "qalitrack-frontend",
//     features:   new[] { "kiosk" },
//     expiresAt:  DateTime.UtcNow.AddYears(1)
// );
// // Send licenseToken to customer via email / ERP portal

// ERP VALIDATION ENDPOINT (for app check-ins):
// [HttpPost("api/licenses/validate")]
// public IActionResult Validate([FromBody] ValidateRequest req)
// {
//     var license = _db.Licenses.FirstOrDefault(l => l.Token == req.Token);
//     if (license == null)          return Unauthorized(new { reason = "unknown_key" });
//     if (license.Revoked)          return Unauthorized(new { reason = "revoked" });
//     if (license.AppId != req.AppId) return Unauthorized(new { reason = "wrong_app" });
//     if (license.ExpiresAt < DateTime.UtcNow) return Unauthorized(new { reason = "expired" });
//     if (license.MachineId != null && license.MachineId != req.MachineId)
//                                   return Unauthorized(new { reason = "machine_mismatch" });
//     license.LastSeen = DateTime.UtcNow; // audit trail
//     _db.SaveChanges();
//     return Ok(new { valid = true, customerId = license.CustomerId, expiresAt = license.ExpiresAt });
// }
`);

console.log("========================================================\n");
