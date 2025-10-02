using NBitcoin.JsonConverters;
using Newtonsoft.Json;
using OpenDive.Utils.Jwt;
using Org.BouncyCastle.Math;
using Sui.Accounts;
using Sui.ZKLogin;
using System.Collections.Generic;
using UnityEngine;

public class ZKLoginSample : MonoBehaviour
{
    Account _ephemeralAccount;
    [SerializeField] private string _network = "testnet";
    [SerializeField] private string _enokiPublicKey;
    [SerializeField] private string _googleClientId;
    [SerializeField] private string _redirectUri = "http://localhost:3000";

    private ZKLoginUser _zkLoginUser;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Test();
    }

    async void Test()
    {
        _ephemeralAccount = new Account();
        NonceResponse nr = await ZkLoginUtils.FetchNonce(_enokiPublicKey, _network, _ephemeralAccount.PublicKey.ToSuiPublicKey(), 2);
        Debug.Log("Received Nonce => " + nr.data.nonce);
        string jwtToken = await GoogleOAuthManager.GetJWTFromGoogleLogin(_googleClientId, nr.data.nonce, _redirectUri);
        Debug.Log("Received JWT => " + jwtToken);
        JWT jwt = JWTDecoder.DecodeJWT(jwtToken);
        //BigInteger salt = SaltGenerator.GenerateUserSalt("naber", jwt.Payload.Iss, jwt.Payload.Aud, jwt.Payload.Sub);
        //Debug.Log("Salt => " + salt);
        _zkLoginUser = await ZkLoginUtils.FetchZKLoginData(jwtToken, _enokiPublicKey);
        ZKPRoot zkpRoot = await ZkLoginUtils.FetchZKP(_network, _ephemeralAccount.PublicKey.ToSuiPublicKey(), jwtToken, _enokiPublicKey, nr.data.maxEpoch, nr.data.randomness);
        string jsonData = JsonConvert.SerializeObject(zkpRoot.data);
        Debug.Log(jsonData);
        Inputs inputs = JsonConvert.DeserializeObject<Inputs>(jsonData, new ZkLoginSequenceJsonConverter());
        Debug.Log("finish");
        string zkSignature = ZkLoginSignature.GetZkLoginSignature(inputs, (ulong)nr.data.maxEpoch, null);
        Debug.Log(zkSignature);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        GoogleOAuthManager.Dispose();
    }
}
