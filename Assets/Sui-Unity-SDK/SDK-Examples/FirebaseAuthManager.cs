using System;
using System.Threading.Tasks;
using i5.Toolkit.Core.OpenIDConnectClient;
using i5.Toolkit.Core.ServiceCore;
using OpenDive.Utils.Jwt;
using UnityEngine;

public class FirebaseAuthManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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

        JwtPayload jwt = JwtDecoder.DecodeJwt(accessToken);
        Debug.Log(jwt.Aud + " ----  " + jwt.Iss
            + "  ------  " + jwt.Sub
            + "  ----- " + jwt.Exp
        );
    }
}
