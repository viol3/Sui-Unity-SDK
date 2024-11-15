using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using i5.Toolkit.Core.ServiceCore;
using i5.Toolkit.Core.OpenIDConnectClient;

namespace i5.Toolkit.Core.Examples.OpenIDConnectClient
{
    public class GoogleSignInOAuth : BaseServiceBootstrapper
    {
        [SerializeField] private ClientDataObject googleClientDataObject;
        [SerializeField] private ClientDataObject googleClientDataObjectEditorOnly;

        protected override void RegisterServices()
        {
            OpenIDConnectService googleOidc = new OpenIDConnectService();
            googleOidc.OidcProvider = new GoogleOidcProvider();
            // this example shows how the service can be used on an app for multiple platforms
#if !UNITY_EDITOR
            googleOidc.OidcProvider.ClientData = googleClientDataObject.clientData;
            googleOidc.RedirectURI = "com.DefaultCompany.Sui-Unity-SDK:/";
#else
            googleOidc.OidcProvider.ClientData = googleClientDataObjectEditorOnly.clientData;
            googleOidc.RedirectURI = "https://www.opendive.io";
            //googleOidc.RedirectURI = "https://www.google.com";
            googleOidc.ServerListener.ListeningUri = "http://127.0.0.1:52229/";
#endif

            ServiceManager.RegisterService(googleOidc);
        }

        protected override void UnRegisterServices()
        {
        }
    }
}