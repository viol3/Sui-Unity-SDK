using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenDive.BCS;
using Org.BouncyCastle.Crypto;
using Sui.Accounts;
using Sui.Rpc;
using Sui.Rpc.Client;
using Sui.Rpc.Models;
using Sui.Transactions;
using Sui.Types;
using System;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class SealBridge : MonoBehaviour
{
    private Account _account;
    private SuiClient _suiClient;

    private string _data = "myspecialmessage";
    private string _objectId = "0xadeaf09c14beeff621c3a46da73f74346a1a8caf8eb26a0efc816c7d9a1c18a9";
    private string _packageId = "0xf3dfe70b4916fecaecf7928bb8221031c28d5130c66e8fa7e645ce8785846f91";
    private string _moduleName = "private_data";
    private string _funcName = "store_entry";
    private string _suiClientUrl = "testnet";
    
    byte[] _nonceBytes = { 179, 187, 103, 40, 166, 131, 240, 66, 249, 74, 252, 248, 94, 86, 237, 156, 126, 166, 204, 121, 87, 83, 242, 54, 142, 192, 68, 94, 192, 49, 245, 27 };
    private string[] _serverObjectIds = { "0x73d05d62c18d9374e3ea529e8e0ed6161da1a141a94d3f76ae3fe4e99356db75", "0xf5d14a81a982144ae441cd7d64b09027f116a468bd36e7eca494f750591623c8" };



    [DllImport("__Internal")] private static extern void StartEncrypt(string data, string packageId, string suiAddress, string nonceB64, string suiClientUrl, string serverObjectIdsJson);
    [DllImport("__Internal")] private static extern void StartDecrypt(string encryptedBytesBase64, string txBytesBase64, string privateKeyB64, string packageId, string suiClientUrl, string serverObjectIdsJson);
    

    private IEnumerator Start()
    {
        _suiClient = new SuiClient(Constants.TestnetConnection);
        _account = new Account("0x8358b8f5a0850969194d0cd0e6e70dad2ec27b981669a8caf9fc566a17c9c115");
        yield return new WaitForSeconds(3f);

        //string serverObjectIdsJson = JsonConvert.SerializeObject(new SealServerObjectWrapper { items = _serverObjectIds });
        //StartEncrypt(_data, _packageId, _account.SuiAddress().ToHex(), Convert.ToBase64String(_nonceBytes), _suiClientUrl, serverObjectIdsJson);
        DecryptTest();
    }

    async void DecryptTest()
    {
        
        var response = await _suiClient.GetObjectAsync(new AccountAddress(_objectId), new ObjectDataOptions() { ShowContent = true });
        var moveObject = (ParsedMoveObject)response.Result.Data.Content.ParsedData;

        var fields = moveObject.Fields;

        var encryptedBytes = (moveObject.Fields["data"] as JArray).Select(jv => (byte)jv).ToArray();
        

        var tx_block = new TransactionBlock();

        tx_block.AddMoveCallTx
        (
            SuiMoveNormalizedStructType.FromStr($"{_packageId}::{_moduleName}::seal_approve"),
            new SerializableTypeTag[] { },
            new TransactionArgument[]
            {
                tx_block.AddPure(new OpenDive.BCS.Bytes(Sui.Seal.Utils.CreatePolicyId(_account.SuiAddress().ToHex(), _nonceBytes))),
                tx_block.AddObjectInput(_objectId)
            }
        );
        //tx_block.SetSender(_account);
        var txBytes = await tx_block.Build(new BuildOptions(_suiClient, null, true, null));

        string encryptedBytesBase64 = Convert.ToBase64String(encryptedBytes);
        string privateKeyBase64 = _account.PrivateKey.ToBase64();
        string txBytesBase64 = Convert.ToBase64String(txBytes);
        string serverObjectIdsJson = JsonConvert.SerializeObject(new SealServerObjectWrapper { items = _serverObjectIds });
        Debug.Log("calling external decrypt...");
        StartDecrypt(encryptedBytesBase64, txBytesBase64, privateKeyBase64, _packageId, _suiClientUrl, serverObjectIdsJson);
    }

    void OnEncryptionCompleted(string encryptedObjectBase64)
    {
        Debug.Log("encryptedObjectBase64 => " + encryptedObjectBase64);
        byte[] encryptedBytes = Convert.FromBase64String(encryptedObjectBase64);
        SendEncryptedBytesToSui(encryptedBytes);
    }

    async void SendEncryptedBytesToSui(byte[] encryptedBytes)
    {
        TransactionBlock tx_block = new TransactionBlock();
        tx_block.AddMoveCallTx
        (
            SuiMoveNormalizedStructType.FromStr($"{_packageId}::{_moduleName}::{_funcName}"),
            new SerializableTypeTag[] { },
            new TransactionArgument[]
            {
                tx_block.AddPure(new OpenDive.BCS.Bytes(_nonceBytes)),
                tx_block.AddPure(new OpenDive.BCS.Bytes(encryptedBytes))
            }
        );
        var result = await _suiClient.SignAndExecuteTransactionBlockAsync(tx_block, _account);
        Debug.Log(result.Result.Digest);
    }
}

[System.Serializable]
public class SealServerObjectWrapper
{
    public string[] items;
    public SealServerObjectWrapper()
    {

    }
}
