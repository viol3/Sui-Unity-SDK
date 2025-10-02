using Newtonsoft.Json;
using Org.BouncyCastle.Math;
using Sui.Accounts;
using Sui.ZKLogin;
using Sui.ZKLogin.SDK;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Networking;

public static class ZkLoginUtils
{
    
    public static async Task<NonceResponse> FetchNonce(string enokiPublicKey, string network, string ephemeralPublicKey, int additionalEpochs)
    {
        string url = "https://api.enoki.mystenlabs.com/v1/zklogin/nonce";
        string jsonBody = JsonConvert.SerializeObject(new NonceRequest
        {
            network = network,
            ephemeralPublicKey = ephemeralPublicKey,
            additionalEpochs = additionalEpochs
        });

        NonceResponse nonceResult = null;

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + enokiPublicKey);

            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + request.error);
            }
            else
            {
                nonceResult = JsonConvert.DeserializeObject<NonceResponse>(request.downloadHandler.text);
            }
            return nonceResult;
            //byte[] saltBytes = SaltGenerator.GenerateUserSalt("naber", "https://accounts.google.com", "186512259169-1pj5f5fj8gmeprqhat0nac0dm633lev6.apps.googleusercontent.com", "1126267443241574539940");
            //BigInteger bi = SaltGenerator.ToBigInt(saltBytes);
            //Debug.Log(bi);
            //Debug.Log(Address.JwtToAddress(_jwt, bi.ToString(), false));


        }

    }

    public static async Task<ZKPRoot> FetchZKP(string network, string ephemeralPublicKey, string jwt, string apiToken, int maxEpoch, string randomness)
    {
        string url = "https://api.enoki.mystenlabs.com/v1/zklogin/zkp";
        string jsonBody = JsonConvert.SerializeObject(new ZkpRequest
        {
            network = network,
            ephemeralPublicKey = ephemeralPublicKey,
            maxEpoch = maxEpoch,
            randomness = randomness
        });

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiToken);
            request.SetRequestHeader("zklogin-jwt", jwt);

            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + request.error);
                Debug.Log("Error: " + request.downloadHandler.text);
                return null;
            }
            else
            {
                Debug.Log(request.downloadHandler.text);
                return JsonConvert.DeserializeObject<ZKPRoot>(request.downloadHandler.text);
            }
            
            
        }
    }

    public static async Task<ZKLoginUser> FetchZKLoginData(string jwt, string apiToken)
    {
        string url = "https://api.enoki.mystenlabs.com/v1/zklogin";
        using (UnityWebRequest request = new UnityWebRequest(url, "GET"))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiToken);
            request.SetRequestHeader("zklogin-jwt", jwt);

            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + request.error);
                Debug.Log("Error: " + request.downloadHandler.text);
            }
            else
            {
                Debug.Log("Response: " + request.downloadHandler.text);
            }
            return JsonConvert.DeserializeObject<ZKLoginUser>(request.downloadHandler.text);
        }
    }


}

[System.Serializable]
public class ZkpRequest
{
    public string network;
    public string ephemeralPublicKey;
    public int maxEpoch;
    public string randomness;
}

[System.Serializable]
public class NonceRequest
{
    public string network;
    public string ephemeralPublicKey;
    public int additionalEpochs;
}

[System.Serializable]
public class NonceData
{
    public string nonce { get; set; }
    public string randomness { get; set; }
    public int epoch { get; set; }
    public int maxEpoch { get; set; }
    public long estimatedExpiration { get; set; }
}

[System.Serializable]
public class NonceResponse
{
    public NonceData data { get; set; }
}

[System.Serializable]
public class ZKLoginData
{
    public string salt { get; set; }
    public string address { get; set; }
    public string publicKey { get; set; }
}

[System.Serializable]
public class ZKLoginUser
{
    public ZKLoginData data { get; set; }
}

[System.Serializable]
public class ZKPRoot
{
    public ZKPData data { get; set; }
}

public class ZKPData
{
    public ProofPoints proofPoints { get; set; }
    public IssBase64Details issBase64Details { get; set; }
    public string headerBase64 { get; set; }
    public string addressSeed { get; set; }
}

public class IssBase64Details
{
    public string value { get; set; }
    public int indexMod4 { get; set; }
}

public class ProofPoints
{
    public List<string> a { get; set; }
    public List<List<string>> b { get; set; }
    public List<string> c { get; set; }
}
