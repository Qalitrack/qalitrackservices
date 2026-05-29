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




