using System;
using System.Threading.Tasks;
using i5.Toolkit.Core.OpenIDConnectClient;
using i5.Toolkit.Core.ServiceCore;
using OpenDive.Utils.Jwt;
using UnityEngine;
using Firebase.Extensions;
using i5.Toolkit.Core.RocketChatClient;
using System.Net.Mail;
using Newtonsoft.Json.Linq;

public class FirebaseAuthManager : MonoBehaviour
{
    private Firebase.Auth.FirebaseAuth auth;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //auth = Firebase.Auth.FirebaseAuth.DefaultInstance;

        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                //app = Firebase.FirebaseApp.DefaultInstance;

                auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
                //auth.StateChanged += AuthStateChanged;
                //AuthStateChanged(this, null);

                // Set a flag here to indicate whether Firebase is ready to use by your app.
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SignIn()
    {
        SignInWithGoogle();
    }

    private async Task SignInWithGoogle()
    {
        ServiceManager.GetService<OpenIDConnectService>().LoginCompleted += OnLoginCompleted;

        Debug.Log(" +++ " + ServiceManager.GetService<OpenIDConnectService>().RedirectURI);
        await ServiceManager.GetService<OpenIDConnectService>().OpenLoginPageAsync();
    }

    private void OnLoginCompleted(object sender, EventArgs e)
    {
        string accessToken = ServiceManager.GetService<OpenIDConnectService>().AccessToken;
        Debug.Log("Sign In Done");
        Debug.Log("TOKEN: " + accessToken);

        //JwtPayload jwt = JwtDecoder.DecodeJwt(accessToken);
        //Debug.Log(jwt.Aud + " ----  " + jwt.Iss
        //    + "  ------  " + jwt.Sub
        //    + "  ----- " + jwt.Exp
        //);

        Firebase.Auth.Credential credential =
            Firebase.Auth.GoogleAuthProvider.GetCredential(null, accessToken);

        auth.SignInAndRetrieveDataWithCredentialAsync(credential).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInAndRetrieveDataWithCredentialAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInAndRetrieveDataWithCredentialAsync encountered an error: " + task.Exception);
                return;
            }

            Firebase.Auth.AuthResult result = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);

            result.User.TokenAsync(true).ContinueWith(task => {
                if (task.IsCanceled)
                {
                    Debug.LogError("TokenAsync was canceled.");
                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.LogError("TokenAsync encountered an error: " + task.Exception);
                    return;
                }

                string idToken = task.Result;

                Debug.Log("ID TOKEN:  " + idToken);

                //JwtPayload payload = JwtDecoder.DecodeJwt(idToken);
                //Debug.Log(payload.Aud + "  --  "
                //    + payload.claims + " --- "
                //    + payload.Exp + " --- "
                //    + payload.Iat + " --- "
                //    + payload.Iss + " ... ");

                JWT decodedJWT = JWTDecoder.DecodeJWT(idToken);

                if (decodedJWT != null)
                {
                    Debug.Log("Header:");
                    Debug.Log($"Algorithm: {decodedJWT.Header.alg}");
                    Debug.Log($"Type: {decodedJWT.Header.typ}");

                    Debug.Log("Payload:");
                    Debug.Log($"Issuer: {decodedJWT.Payload.Iss}");
                    Debug.Log($"Subject: {decodedJWT.Payload.Sub}");
                    Debug.Log($"Audience: {decodedJWT.Payload.Aud}");
                    Debug.Log($"Email: {decodedJWT.Payload.Email}");
                    Debug.Log($"Name: {decodedJWT.Payload.Name}");
                    //Debug.Log($"Issued At: {UnixTimestampToDateTime(decodedJWT.Payload.iat)}");
                    //Debug.Log($"Expires At: {UnixTimestampToDateTime(decodedJWT.Payload.exp)}");
                    Debug.Log($"Issued At: {decodedJWT.Payload.Iat}");
                    Debug.Log($"Expires At: {decodedJWT.Payload.Exp}");

                    Debug.Log("Signature:");
                    Debug.Log(decodedJWT.Signature);
                }
                else
                {
                    Debug.LogError("Failed to decode JWT.");
                }

                // Send token to your backend via HTTPS
                // ...
            });
        });
    }

    //void AuthStateChanged(object sender, System.EventArgs eventArgs)
    //{
    //    if (auth.CurrentUser != user)
    //    {
    //        bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null
    //            && auth.CurrentUser.IsValid();
    //        if (!signedIn && user != null)
    //        {
    //            DebugLog("Signed out " + user.UserId);
    //        }
    //        user = auth.CurrentUser;
    //        if (signedIn)
    //        {
    //            DebugLog("Signed in " + user.UserId);
    //            displayName = user.DisplayName ?? "";
    //            emailAddress = user.Email ?? "";
    //            photoUrl = user.PhotoUrl ?? "";
    //        }
    //    }
    //}
}
