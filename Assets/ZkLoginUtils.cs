using Newtonsoft.Json;
using Org.BouncyCastle.Math;
using Sui.Accounts;
using Sui.ZKLogin.SDK;
using System;
using System.Collections;
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
    //IEnumerator Init1()
    //{
    //    Account account = new Account(Sui.Cryptography.SignatureUtils.SignatureScheme.ED25519);

    //    string url = "https://api.enoki.mystenlabs.com/v1/zklogin/nonce";
    //    string url2 = "https://api.enoki.mystenlabs.com/v1/zklogin/zkp";
    //    string apiToken = "enoki_public_3ed3c88ff07ca65cf46192bbaf8e0787";

    //    // JSON body
    //    string jsonBody = JsonConvert.SerializeObject(new NonceRequest
    //    {
    //        network = "testnet",
    //        ephemeralPublicKey = account.PublicKey.ToSuiPublicKey(),
    //        additionalEpochs = 2
    //    });



    //    NonceResponse nr = null;

    //    using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
    //    {
    //        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
    //        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
    //        request.downloadHandler = new DownloadHandlerBuffer();
    //        request.SetRequestHeader("Content-Type", "application/json");
    //        request.SetRequestHeader("Authorization", "Bearer " + apiToken);

    //        yield return request.SendWebRequest();

    //        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
    //        {
    //            Debug.LogError("Error: " + request.error);
    //        }
    //        else
    //        {
    //            Debug.Log("Response: " + request.downloadHandler.text);
    //            nr = JsonConvert.DeserializeObject<NonceResponse>(request.downloadHandler.text);
    //        }
    //        byte[] saltBytes = SaltGenerator.GenerateUserSalt("naber", "https://accounts.google.com", "186512259169-1pj5f5fj8gmeprqhat0nac0dm633lev6.apps.googleusercontent.com", "1126267443241574539940");
    //        BigInteger bi = SaltGenerator.ToBigInt(saltBytes);
    //        Debug.Log(bi);
    //        Debug.Log(Address.JwtToAddress(_jwt, bi.ToString(), false));


    //    }
    //    yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
    //    string jsonBody2 = JsonConvert.SerializeObject(new ZkpRequest
    //    {
    //        network = "testnet",
    //        ephemeralPublicKey = account.PublicKey.ToSuiPublicKey(),
    //        maxEpoch = nr.data.maxEpoch,
    //        randomness = nr.data.randomness
    //    });

    //    using (UnityWebRequest request = new UnityWebRequest(url2, "POST"))
    //    {
    //        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody2);
    //        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
    //        request.downloadHandler = new DownloadHandlerBuffer();
    //        request.SetRequestHeader("Content-Type", "application/json");
    //        request.SetRequestHeader("Authorization", "Bearer " + apiToken);
    //        request.SetRequestHeader("zklogin-jwt", _jwt);

    //        yield return request.SendWebRequest();

    //        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
    //        {
    //            Debug.LogError("Error: " + request.error);
    //            Debug.Log("Error: " + request.downloadHandler.text);
    //        }
    //        else
    //        {
    //            Debug.Log("Response: " + request.downloadHandler.text);

    //        }
    //    }
    //}

    
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