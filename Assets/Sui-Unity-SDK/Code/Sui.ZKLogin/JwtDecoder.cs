using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;

namespace OpenDive.Utils.Jwt
{
    using System;
    using System.Text;
    using UnityEngine;
    using Newtonsoft.Json;

    public class JWTDecoder
    {
        /// <summary>
        /// Decodes a JWT token and extracts the header and payload as strongly-typed classes.
        /// </summary>
        /// <param name="token">The JWT token string</param>
        /// <returns>A JWT object containing the header and payload</returns>
        public static JWT DecodeJWT(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                Debug.LogError("JWT token is null or empty!");
                return null;
            }

            string[] parts = token.Split('.');
            if (parts.Length != 3)
            {
                Debug.LogError("Invalid JWT token format!");
                return null;
            }

            try
            {
                // Decode header and payload
                string headerJson = Base64UrlDecode(parts[0]);
                string payloadJson = Base64UrlDecode(parts[1]);

                // Deserialize JSON into classes
                JWTHeader header = JsonConvert.DeserializeObject<JWTHeader>(headerJson);
                JWTPayload payload = JsonConvert.DeserializeObject<JWTPayload>(payloadJson);

                // Return a JWT object
                return new JWT
                {
                    Header = header,
                    Payload = payload,
                    Signature = parts[2]
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error decoding JWT: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Decodes a Base64 URL-encoded string.
        /// </summary>
        /// <param name="base64Url">The Base64 URL-encoded string</param>
        /// <returns>The decoded string</returns>
        private static string Base64UrlDecode(string base64Url)
        {
            string padded = base64Url.Replace('-', '+').Replace('_', '/');
            switch (padded.Length % 4)
            {
                case 2: padded += "=="; break;
                case 3: padded += "="; break;
            }

            byte[] data = Convert.FromBase64String(padded);
            return Encoding.UTF8.GetString(data);
        }
    }

    /// <summary>
    /// Represents a decoded JWT with header, payload, and signature.
    /// </summary>
    public class JWT
    {
        public JWTHeader Header { get; set; }
        public JWTPayload Payload { get; set; }
        public string Signature { get; set; }
    }

    /// <summary>
    /// Represents the JWT header.
    /// </summary>
    public class JWTHeader
    {
        public string alg { get; set; } // Algorithm
        public string typ { get; set; } // Token type
    }

    /// <summary>
    /// Represents the JWT payload with common claims.
    /// </summary>
    public class JWTPayload
    {
        public string Iss { get; set; } // Issuer
        public string Sub { get; set; } // Subject
        public string Aud { get; set; } // Audience
        public long? Auth_time { get; set; } // Authentication time (Unix timestamp)
        public long? Iat { get; set; } // Issued at (Unix timestamp)
        public long? Exp { get; set; } // Expiration time (Unix timestamp)
        public string Email { get; set; } // Email address
        public string Name { get; set; } // Name
        public string Jti { get; set; } // JWT ID (unique identifier for the token)

        // Add additional properties as needed based on your JWT structure.
    }

    //[Serializable]
    //public class JwtPayload
    //{
    //    public string Iss;    // Issuer
    //    public string Aud;    // Audience
    //    public string Sub;    // Subject
    //    public long Exp;      // Expiration Time
    //    public long Iat;      // Issued At
    //    public Dictionary<string, string> claims;

    //    public JwtPayload()
    //    {
    //        claims = new Dictionary<string, string>();
    //    }
    //}

    //public static class JwtDecoder
    //{
    //    public static JwtPayload DecodeJwt(string jwt)
    //    {
    //        try
    //        {
    //            // Split the JWT into its three parts
    //            string[] parts = jwt.Split('.');
    //            if (parts.Length != 3)
    //            {
    //                throw new ArgumentException("Invalid JWT format");
    //            }

    //            // Decode the payload (second part)
    //            string payloadJson = DecodeBase64Url(parts[1]);

    //            // Parse using Unity's JSON utility
    //            var payload = new JwtPayload();
    //            var tempPayload = JsonUtility.FromJson<JwtPayload>(payloadJson);

    //            // Copy standard claims
    //            payload.Iss = tempPayload.Iss;
    //            payload.Aud = tempPayload.Aud;
    //            payload.Sub = tempPayload.Sub;
    //            payload.Exp = tempPayload.Exp;
    //            payload.Iat = tempPayload.Iat;

    //            // Parse the JSON manually to get all claims
    //            // (because JsonUtility doesn't support Dictionary directly)
    //            ParseCustomClaims(payloadJson, payload.claims);

    //            return payload;
    //        }
    //        catch (Exception ex)
    //        {
    //            Debug.LogError($"Failed to decode JWT: {ex.Message}");
    //            throw;
    //        }
    //    }

    //    private static void ParseCustomClaims(string json, Dictionary<string, string> claims)
    //    {
    //        // Simple JSON parsing for claims
    //        // Remove the first and last curly braces and split by commas
    //        json = json.Trim('{', '}');
    //        string[] pairs = json.Split(',');

    //        foreach (string pair in pairs)
    //        {
    //            try
    //            {
    //                string[] keyValue = pair.Split(':');
    //                if (keyValue.Length == 2)
    //                {
    //                    string key = keyValue[0].Trim('"', ' ');
    //                    string value = keyValue[1].Trim('"', ' ');

    //                    // Skip standard claims
    //                    if (!IsStandardClaim(key))
    //                    {
    //                        claims[key] = value;
    //                    }
    //                }
    //            }
    //            catch (Exception ex)
    //            {
    //                Debug.LogWarning($"Failed to parse claim pair: {pair}. Error: {ex.Message}");
    //            }
    //        }
    //    }

    //    private static string DecodeBase64Url(string base64Url)
    //    {
    //        string padded = base64Url.PadRight(4 * ((base64Url.Length + 3) / 4), '=');
    //        string base64 = padded.Replace('-', '+').Replace('_', '/');
    //        byte[] bytes = Convert.FromBase64String(base64);
    //        return Encoding.UTF8.GetString(bytes);
    //    }

    //    private static bool IsStandardClaim(string claimName)
    //    {
    //        return claimName == "iss" ||
    //               claimName == "sub" ||
    //               claimName == "aud" ||
    //               claimName == "exp" ||
    //               claimName == "nbf" ||
    //               claimName == "iat" ||
    //               claimName == "jti";
    //    }

    //    // Helper methods
    //    public static bool IsTokenExpired(JwtPayload payload)
    //    {
    //        var now = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
    //        return now >= payload.Exp;
    //    }

    //    public static DateTime GetTokenExpirationTime(JwtPayload payload)
    //    {
    //        return DateTime.UnixEpoch.AddSeconds(payload.Exp);
    //    }

    //    public static DateTime GetTokenIssuedTime(JwtPayload payload)
    //    {
    //        return DateTime.UnixEpoch.AddSeconds(payload.Iat);
    //    }
    //}
}