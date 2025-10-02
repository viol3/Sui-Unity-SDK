using Sui.Accounts;
using UnityEngine;

public class ZKLoginSample : MonoBehaviour
{
    Account _ephemeralAccount;
    [SerializeField] private string _enokiPublicKey;
    [SerializeField] private string _googleClientId;
    [SerializeField] private string _redirectUri = "http://localhost:3000";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Test();
    }

    async void Test()
    {
        _ephemeralAccount = new Account();
        NonceResponse nr = await ZkLoginUtils.FetchNonce(_enokiPublicKey, "testnet", _ephemeralAccount.PublicKey.ToSuiPublicKey(), 2);
        Debug.Log("Received Nonce => " + nr.data.nonce);
        string jwt = await GoogleOAuthManager.GetJWTFromGoogleLogin(_googleClientId, nr.data.nonce, _redirectUri);
        Debug.Log(jwt);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
