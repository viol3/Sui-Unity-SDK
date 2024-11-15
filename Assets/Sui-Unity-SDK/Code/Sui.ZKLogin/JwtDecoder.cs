using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;

namespace OpenDive.Utils.Jwt
{
    [Serializable]
    public class JwtPayload
    {
        public string Iss;    // Issuer
        public string Aud;    // Audience
        public string Sub;    // Subject
        public long Exp;      // Expiration Time
        public long Iat;      // Issued At
        public Dictionary<string, string> claims;

        public JwtPayload()
        {
            claims = new Dictionary<string, string>();
        }
    }

    public static class JwtDecoder
    {
        public static JwtPayload DecodeJwt(string jwt)
        {
            try
            {
                // Split the JWT into its three parts
                string[] parts = jwt.Split('.');
                if (parts.Length != 3)
                {
                    throw new ArgumentException("Invalid JWT format");
                }

                // Decode the payload (second part)
                string payloadJson = DecodeBase64Url(parts[1]);

                // Parse using Unity's JSON utility
                var payload = new JwtPayload();
                var tempPayload = JsonUtility.FromJson<JwtPayload>(payloadJson);

                // Copy standard claims
                payload.Iss = tempPayload.Iss;
                payload.Aud = tempPayload.Aud;
                payload.Sub = tempPayload.Sub;
                payload.Exp = tempPayload.Exp;
                payload.Iat = tempPayload.Iat;

                // Parse the JSON manually to get all claims
                // (because JsonUtility doesn't support Dictionary directly)
                ParseCustomClaims(payloadJson, payload.claims);

                return payload;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to decode JWT: {ex.Message}");
                throw;
            }
        }

        private static void ParseCustomClaims(string json, Dictionary<string, string> claims)
        {
            // Simple JSON parsing for claims
            // Remove the first and last curly braces and split by commas
            json = json.Trim('{', '}');
            string[] pairs = json.Split(',');

            foreach (string pair in pairs)
            {
                try
                {
                    string[] keyValue = pair.Split(':');
                    if (keyValue.Length == 2)
                    {
                        string key = keyValue[0].Trim('"', ' ');
                        string value = keyValue[1].Trim('"', ' ');

                        // Skip standard claims
                        if (!IsStandardClaim(key))
                        {
                            claims[key] = value;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Failed to parse claim pair: {pair}. Error: {ex.Message}");
                }
            }
        }

        private static string DecodeBase64Url(string base64Url)
        {
            string padded = base64Url.PadRight(4 * ((base64Url.Length + 3) / 4), '=');
            string base64 = padded.Replace('-', '+').Replace('_', '/');
            byte[] bytes = Convert.FromBase64String(base64);
            return Encoding.UTF8.GetString(bytes);
        }

        private static bool IsStandardClaim(string claimName)
        {
            return claimName == "iss" ||
                   claimName == "sub" ||
                   claimName == "aud" ||
                   claimName == "exp" ||
                   claimName == "nbf" ||
                   claimName == "iat" ||
                   claimName == "jti";
        }

        // Helper methods
        public static bool IsTokenExpired(JwtPayload payload)
        {
            var now = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            return now >= payload.Exp;
        }

        public static DateTime GetTokenExpirationTime(JwtPayload payload)
        {
            return DateTime.UnixEpoch.AddSeconds(payload.Exp);
        }

        public static DateTime GetTokenIssuedTime(JwtPayload payload)
        {
            return DateTime.UnixEpoch.AddSeconds(payload.Iat);
        }
    }
}