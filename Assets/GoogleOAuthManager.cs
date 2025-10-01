using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Networking;

public static class GoogleOAuthManager
{
    private static string redirectUri = "http://localhost:3000/";

    private static HttpListener httpListener;

    // Start OAuth flow
    public static async Task<string> GetJWTFromGoogleLogin(string clientId, string nonce)
    {
        // Build OAuth URL
        string authUrl = $"https://accounts.google.com/o/oauth2/v2/auth?" +
            $"client_id={clientId}" +
            $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
            $"&response_type=id_token" +
            $"&scope=openid" +
            $"&nonce=" + nonce;

        Debug.Log("Opening Google OAuth URL...");

        // Open browser
        Application.OpenURL(authUrl);

        // Start local server and wait for callback
        return await FetchJWT();
    }

    private async static Task<string> FetchJWT()
    {
        httpListener = new HttpListener();
        httpListener.Prefixes.Add(redirectUri);

        try
        {
            httpListener.Start();
            Debug.Log($"Local server started: {redirectUri}");

            // Wait for callback
            HttpListenerContext context = await httpListener.GetContextAsync();
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;


            string html = @"
<!DOCTYPE html>
<html>
<head>
<title>Google Login</title>
</head>
<body>
<h1>Processing login...</h1>
<script>
    // Extract token from fragment (after #)
    var fragment = window.location.hash.substring(1);
    var params = new URLSearchParams(fragment);
    var idToken = params.get('id_token');
        
    if (idToken) {
        // Redirect to same URL but with query string (?) instead of fragment (#)
        window.location.href = '/?id_token=' + idToken;
    } else if (error) {
        window.location.href = '/?error=' + error;
    } else {
        document.body.innerHTML = '<h1>Error!</h1><p>No token received.</p>';
    }
</script>
</body>
</html>";
            SendResponse(response, html);

            context = await httpListener.GetContextAsync();
            request = context.Request;
            response = context.Response;

            // Parse URL parameters
            string token = request.QueryString.Get("id_token");
            
            //await StartLocalServer();
            httpListener.Stop();
            return token;


        }
        catch (Exception ex)
        {
            Debug.LogError($"Server error: {ex.Message}");
            if (httpListener != null && httpListener.IsListening)
            {
                httpListener.Stop();
            }
            return "";
        }
    }

    private static void SendResponse(HttpListenerResponse response, string html)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(html);
        response.ContentLength64 = buffer.Length;
        response.ContentType = "text/html";
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }

    // JSON response models
    [Serializable]
    private class TokenResponse
    {
        public string access_token;
        public string refresh_token;
        public int expires_in;
        public string token_type;
        public string id_token;
    }

    [Serializable]
    public class UserInfo
    {
        public string id;
        public string email;
        public bool verified_email;
        public string name;
        public string given_name;
        public string family_name;
        public string picture;
    }
}