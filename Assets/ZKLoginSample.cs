using Chaos.NaCl;
using NBitcoin;
using NBitcoin.JsonConverters;
using Newtonsoft.Json;
using OpenDive.BCS;
using OpenDive.Utils.Jwt;
using Org.BouncyCastle.Asn1.Ocsp;
using Sui.Accounts;
using Sui.Cryptography;
using Sui.Rpc;
using Sui.Rpc.Client;
using Sui.Rpc.Models;
using Sui.Transactions;
using Sui.Transactions.Builder;
using Sui.Types;
using Sui.Utilities;
using Sui.ZKLogin;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Numerics;
using System.Text;
using Unity.Android.Gradle;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UIElements;

public class ZKLoginSample : MonoBehaviour
{
    Account _ephemeralAccount;
    [SerializeField] private string _network = "testnet";
    [SerializeField] private string _enokiPublicKey;
    [SerializeField] private string _googleClientId;
    [SerializeField] private string _redirectUri = "http://localhost:3000";
    
    SuiClient _client;
    private ZKLoginUser _zkLoginUser;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Test();
        //LogBase64BytesInline("BQNMMTE0MzIyNzg0MTA4MTMwMDQ4NjQ4MDE1OTE5MTA4MjgxOTYxMDc2NjI5MTQ4MjQ5NDU3NTE3MjA2MjkxMDc0NzM5MzA1MDQzNDMyNkw1MTMzMDY4OTY0MzAxOTAzOTg3NzIwMTUzNjY1OTg1MTAxMTg3MDU5MTQxMjkyMzY4MTM5NzcxMTE4MTQ0ODE3NjMxNDE4MTAxOTk5ATEDAkw3OTI0ODI3ODUyNDcyODg1Njk2MDgyNDc2OTIwMjcyNjkyNzE2ODk0MDU4OTE3NDk2NDQzMjk0MTc4MTE4NDM3MDA3MTk2ODQxNzExTTE1NzI3ODA2MTMyNjU1NzQ4MzE1Mjg1MTQ1MTM2NzA4MTY2NDkzOTk4MjYzMjk4MDc2MDA1NzUxODkwOTE1NTM5MjI0MjYyMjg4NzExAkw0NTQ1NzY0NDU0NTI4Mjk2ODU3MjMwOTMxMTg0ODk5MzA1MzI2ODc3Mjg2Mjc5Mjg0NTM4OTY4OTkwOTU1NzY4ODUzMjY1NjkxOTE5TDcyODA2ODMwMDgzNDQ5MTI0ODQ1MDc0OTUxMDUwNTAzODQ5MzUyNDY3OTMwMTYyMzYxMjc4MTczMDc1Mzk5NzgxMDE1Mzc0ODM5NjUCATEBMANNMTI0MTA1OTgyOTMwOTk1MTI1NDk2NTcxNjQ1MjE3MTgzMTUwOTE2MTU4Nzc3MTY5MTE1MTQxNTIyNjcyNjk4Mjc3NDg5Nzk3Nzg3NTdNMTUyMzU4MzY1OTc2MzMwNDU5Mzg5MTcxMjQxNjkxODEzNzAzNzQ2NDk1NDExODUzNzg1ODE4ODc3NTE0MjMzMzM2NzI3ODA5OTAzNjcBMTF5SnBjM01pT2lKb2RIUndjem92TDJGalkyOTFiblJ6TG1kdmIyZHNaUzVqYjIwaUxDAWZleUpoYkdjaU9pSlNVekkxTmlJc0ltdHBaQ0k2SWpFM1pqQm1NR1l4TkdVNVkyRm1ZVGxoWWpVeE9EQXhOVEJoWlRjeE5HTTVabVF4WWpWak1qWWlMQ0owZVhBaU9pSktWMVFpZlFNMTIzNzEyMjE0NjUyMDg5NDg1NzY3MTY1NjEyNzc4MzA1MDIwOTEwMDg3ODQ4NDA5OTc1NDgxODM5OTI0MzEzOTAyMjc3NjU3NTc5NzVuAwAAAAAAAGIC1BMsRoFm3vcXCyzctSu/D5GEojw3NDYDazzA21eLP89XbPADXQj1RWy7RCBsEn+SIOrg8FbSI14gdXfBGgvI2QMhYlB49Q8JFyy0IHaBO9vFp18AdIjKjVCy2l99wpv4kg==");
        //LogBase64BytesInline("BQNMNDY5NjE4NzgxMjk0OTI0NzA4OTUwNDY0OTMzOTc2NTg4ODQ2NDQwNjI3NjA0NjU2OTcwNDMyODUwOTY4OTU2NTY5NDgxNDQwNTAyMUw2NjA0NzU1OTcwMDk0NTcwODI0MzEyMzAwNDU3NjY2OTMzMDE3MDc0MDM4MjgwNjQwODExMDU1MDU1NjQzNDIwNjIyNzYzNDk5MzcyATEDAk0xMjUxMDE2NTIxMzM5MzEyNjQxOTc0MjE5NDA0Njc0NzY1MDYyNzYwMDY3NTA2MjU4MTg0NDc4NzIwNDIyNTA5MDc1NzY1NTg4NzY4MEw5MDA2NzMxNTcyMzM3OTIwMjk3NDcwNjY5MjgwNDUyNTIwODgxMTIxNTYxMDI1MDc3NjMxNjY4NzAzOTE1ODE0NzY4MDEyNDEzODA3Aks2MDQzODk1MTI4Nzk2NDYyMDI3NTc0OTUxMTE2Mzg2NTI2MzczMDYwNTczMzUzODc0MDM0NjU3NjUzOTA1MTI5Mjc4NDcyMzA5MzhNMTUwMTcyOTcwODM5OTUxNDk1NzcwNDU4NjUyOTg3OTA0Mzc2Nzg3MzA1MjY4MjM1NTEwOTIyNjY0NDk4NjU0NTEwMzUwMDI2MzE4NzICATEBMANMNTkyMTQ5NTQ0NTIwMTMyMTcxNzQ3NjQ1MjA1ODcwOTcxNzAzNTEzMDY4NDE1NTQyNTI4MTUyMDQwMzc2ODUwMDE1Njc3OTIxODM3Nk0xMDY0MjAyNzgzOTUzNDU2MjIyMjQ1OTAxNjYwMzI3NDgwNjA2MzUyMTc4MjMzNjMwODcyMzIxODY5ODgyNTUzMDEwOTE5MjY2NzgzNwExMXlKcGMzTWlPaUpvZEhSd2N6b3ZMMkZqWTI5MWJuUnpMbWR2YjJkc1pTNWpiMjBpTEMBZmV5SmhiR2NpT2lKU1V6STFOaUlzSW10cFpDSTZJamt5TjJJNFptSTJOMkppWVdRM056UTBOV1UxWm1WaE5HTTNNV0ZoT1RnME5tUTNaR1JrTURFaUxDSjBlWEFpT2lKS1YxUWlmUUwzNjUxODM1NDkxMjMyNTMwODM5MDI3NjI3ODYwNDE0MzQ1MjQ3NzM3MTA3NDM1NDYzMTQzNDg5NTkyNjkxNDA1Nzc5MDcwNDk4MDcybAMAAAAAAABiAr/mLg09VtLIyYp3VVbrcvcyslqj5rcMlbx1Mqj1ZfzbNs1+1i2CuuKIODtrNXD29neIydCk10snPsJ1nLEcPscDhpjOOLV4WR7wTqNvT3MGJI11ZIBjUJbW20sZgBYR5+s=");
        //LogBase64BytesInline("BQNMNDY5NjE4NzgxMjk0OTI0NzA4OTUwNDY0OTMzOTc2NTg4ODQ2NDQwNjI3NjA0NjU2OTcwNDMyODUwOTY4OTU2NTY5NDgxNDQwNTAyMUw2NjA0NzU1OTcwMDk0NTcwODI0MzEyMzAwNDU3NjY2OTMzMDE3MDc0MDM4MjgwNjQwODExMDU1MDU1NjQzNDIwNjIyNzYzNDk5MzcyATEDAk0xMjUxMDE2NTIxMzM5MzEyNjQxOTc0MjE5NDA0Njc0NzY1MDYyNzYwMDY3NTA2MjU4MTg0NDc4NzIwNDIyNTA5MDc1NzY1NTg4NzY4MEw5MDA2NzMxNTcyMzM3OTIwMjk3NDcwNjY5MjgwNDUyNTIwODgxMTIxNTYxMDI1MDc3NjMxNjY4NzAzOTE1ODE0NzY4MDEyNDEzODA3Aks2MDQzODk1MTI4Nzk2NDYyMDI3NTc0OTUxMTE2Mzg2NTI2MzczMDYwNTczMzUzODc0MDM0NjU3NjUzOTA1MTI5Mjc4NDcyMzA5MzhNMTUwMTcyOTcwODM5OTUxNDk1NzcwNDU4NjUyOTg3OTA0Mzc2Nzg3MzA1MjY4MjM1NTEwOTIyNjY0NDk4NjU0NTEwMzUwMDI2MzE4NzICATEBMANMNTkyMTQ5NTQ0NTIwMTMyMTcxNzQ3NjQ1MjA1ODcwOTcxNzAzNTEzMDY4NDE1NTQyNTI4MTUyMDQwMzc2ODUwMDE1Njc3OTIxODM3Nk0xMDY0MjAyNzgzOTUzNDU2MjIyMjQ1OTAxNjYwMzI3NDgwNjA2MzUyMTc4MjMzNjMwODcyMzIxODY5ODgyNTUzMDEwOTE5MjY2NzgzNwExMXlKcGMzTWlPaUpvZEhSd2N6b3ZMMkZqWTI5MWJuUnpMbWR2YjJkc1pTNWpiMjBpTEMBZmV5SmhiR2NpT2lKU1V6STFOaUlzSW10cFpDSTZJamt5TjJJNFptSTJOMkppWVdRM056UTBOV1UxWm1WaE5HTTNNV0ZoT1RnME5tUTNaR1JrTURFaUxDSjBlWEFpT2lKS1YxUWlmUUwzNjUxODM1NDkxMjMyNTMwODM5MDI3NjI3ODYwNDE0MzQ1MjQ3NzM3MTA3NDM1NDYzMTQzNDg5NTkyNjkxNDA1Nzc5MDcwNDk4MDcybAMAAAAAAABiArrh5dkJyvudd/g0xj3Hc1LoHWJDGYT5USspP/JTlSGDODoUEvlWl2bhYENfrqKonGWMa+yaONIJrulKluDPo74DhpjOOLV4WR7wTqNvT3MGJI11ZIBjUJbW20sZgBYR5+s=");
    }

    async void Test5()
    {
        _client = new SuiClient(Constants.TestnetConnection);
        _ephemeralAccount = new Account("0x40bdbe2b4076f5c6560d22194be9b7142b540edd2459061e4ff0a6ddd88e700c");
        TransactionBlock tx_block = new TransactionBlock();

        tx_block.AddTransferObjectsTx(new TransactionArgument[] { tx_block.gas }, Sui.Accounts.AccountAddress.FromHex("0x0d9b5ca4ebae5f4a7bd3f17e4e36cd6f868d8f0c5a7f977f94f836631fe0288d"));

        tx_block.SetSender(_ephemeralAccount.SuiAddress());
        
        byte[] userTxBytes = await tx_block.Build(new BuildOptions(_client));
        Debug.Log("built");
        if (tx_block.Error != null)
        {
            Debug.LogError(tx_block.Error.Message);
            return;
        }
            
        SignatureBase signature = _ephemeralAccount.SignTransactionBlock(userTxBytes);
        SuiResult<string> signature_result = _ephemeralAccount.ToSerializedSignature(signature);
        if (signature_result.Error != null)
        {
            Debug.LogError(signature_result.Error.Message);
            return;
        }
        TransactionBlockResponseOptions opts = new TransactionBlockResponseOptions
        {
            ShowInput = false,
            ShowEffects = true,
            ShowEvents = true,
            ShowObjectChanges = true,
            ShowBalanceChanges = true
        };
        RpcResult<TransactionBlockResponse> response = await _client.ExecuteTransactionBlockAsync(userTxBytes, new List<string> { signature_result.Result }, opts);

    }

    async void Test4()
    {
        _client = new SuiClient(Constants.TestnetConnection);
        _ephemeralAccount = new Account();
        string json = @"{
            ""proofPoints"": {
                ""a"": [
                    ""4696187812949247089504649339765888464406276046569704328509689565694814405021"",
                    ""6604755970094570824312300457666933017074038280640811055055643420622763499372"",
                    ""1""
                ],
                ""b"": [
                    [
                        ""12510165213393126419742194046747650627600675062581844787204225090757655887680"",
                        ""9006731572337920297470669280452520881121561025077631668703915814768012413807""
                    ],
                    [
                        ""604389512879646202757495111638652637306057335387403465765390512927847230938"",
                        ""15017297083995149577045865298790437678730526823551092266449865451035002631872""
                    ],
                    [
                        ""1"",
                        ""0""
                    ]
                ],
                ""c"": [
                    ""5921495445201321717476452058709717035130684155425281520403768500156779218376"",
                    ""10642027839534562222459016603274806063521782336308723218698825530109192667837"",
                    ""1""
                ]
            },
            ""issBase64Details"": {
                ""value"": ""yJpc3MiOiJodHRwczovL2FjY291bnRzLmdvb2dsZS5jb20iLC"",
                ""indexMod4"": 1
            },
            ""headerBase64"": ""eyJhbGciOiJSUzI1NiIsImtpZCI6IjkyN2I4ZmI2N2JiYWQ3NzQ0NWU1ZmVhNGM3MWFhOTg0NmQ3ZGRkMDEiLCJ0eXAiOiJKV1QifQ"",
            ""addressSeed"": ""3651835491232530839027627860414345247737107435463143489592691405779070498072""
        }";
        Inputs inputs = JsonConvert.DeserializeObject<Inputs>(json);
        OpenDive.BCS.Serialization ser = new OpenDive.BCS.Serialization();
        inputs.Serialize(ser);
        ser.Serialize((ulong)876);
        List<byte> buff = new List<byte>();
        buff.Add(0x05);
        buff.AddRange(ser.GetBytes());

        TransactionBlock tx_block = new TransactionBlock();
        List<TransactionArgument> splitArgs = tx_block.AddSplitCoinsTx
        (
            tx_block.AddObjectInput("0x84cb6fa3b1dbbe35e0c346811740923536263448406dee35a130e748b36fa823"),
            new TransactionArgument[]
            {
                tx_block.AddPure(new U64(10000000)) // Insert split amount here
            }
        );
        tx_block.AddTransferObjectsTx
        (
            new TransactionArgument[]
            {
                splitArgs[0] // Insert split amount here
            },
            Sui.Accounts.AccountAddress.FromHex("0x0d9b5ca4ebae5f4a7bd3f17e4e36cd6f868d8f0c5a7f977f94f836631fe0288d")
        );
        tx_block.SetSender(Sui.Accounts.AccountAddress.FromHex("0x0b79e524c83d97674e743d5b05b1c67a67e570f81274f00295eb9cbdda855943"));
        byte[] userTxBytes = await tx_block.Build(new BuildOptions(_client, null, true));
        SignatureBase signature = _ephemeralAccount.Sign(userTxBytes);
        byte[] userSignatureBytes = signature.SignatureBytes;
        Debug.Log(userSignatureBytes.Length);
        Debug.Log(_ephemeralAccount.PublicKey.ToSuiBytes().Length);
        buff.Add(0x00); // Ed25519 flag
        buff.AddRange(userSignatureBytes);      // 64 bytes
        buff.AddRange(_ephemeralAccount.PublicKey.ToSuiBytes());
        LogBytes(buff.ToArray());
        string reserializedBase64 = Convert.ToBase64String(buff.ToArray());
        Debug.Log(reserializedBase64);
    }

    void Test3()
    {
        string a = "4696187812949247089504649339765888464406276046569704328509689565694814405021";
        string b = "6604755970094570824312300457666933017074038280640811055055643420622763499372";

        string workingSignature = "BQNMNDY5NjE4NzgxMjk0OTI0NzA4OTUwNDY0OTMzOTc2NTg4ODQ2NDQwNjI3NjA0NjU2OTcwNDMyODUwOTY4OTU2NTY5NDgxNDQwNTAyMUw2NjA0NzU1OTcwMDk0NTcwODI0MzEyMzAwNDU3NjY2OTMzMDE3MDc0MDM4MjgwNjQwODExMDU1MDU1NjQzNDIwNjIyNzYzNDk5MzcyATEDAk0xMjUxMDE2NTIxMzM5MzEyNjQxOTc0MjE5NDA0Njc0NzY1MDYyNzYwMDY3NTA2MjU4MTg0NDc4NzIwNDIyNTA5MDc1NzY1NTg4NzY4MEw5MDA2NzMxNTcyMzM3OTIwMjk3NDcwNjY5MjgwNDUyNTIwODgxMTIxNTYxMDI1MDc3NjMxNjY4NzAzOTE1ODE0NzY4MDEyNDEzODA3Aks2MDQzODk1MTI4Nzk2NDYyMDI3NTc0OTUxMTE2Mzg2NTI2MzczMDYwNTczMzUzODc0MDM0NjU3NjUzOTA1MTI5Mjc4NDcyMzA5MzhNMTUwMTcyOTcwODM5OTUxNDk1NzcwNDU4NjUyOTg3OTA0Mzc2Nzg3MzA1MjY4MjM1NTEwOTIyNjY0NDk4NjU0NTEwMzUwMDI2MzE4NzICATEBMANMNTkyMTQ5NTQ0NTIwMTMyMTcxNzQ3NjQ1MjA1ODcwOTcxNzAzNTEzMDY4NDE1NTQyNTI4MTUyMDQwMzc2ODUwMDE1Njc3OTIxODM3Nk0xMDY0MjAyNzgzOTUzNDU2MjIyMjQ1OTAxNjYwMzI3NDgwNjA2MzUyMTc4MjMzNjMwODcyMzIxODY5ODgyNTUzMDEwOTE5MjY2NzgzNwExMXlKcGMzTWlPaUpvZEhSd2N6b3ZMMkZqWTI5MWJuUnpMbWR2YjJkc1pTNWpiMjBpTEMBZmV5SmhiR2NpT2lKU1V6STFOaUlzSW10cFpDSTZJamt5TjJJNFptSTJOMkppWVdRM056UTBOV1UxWm1WaE5HTTNNV0ZoT1RnME5tUTNaR1JrTURFaUxDSjBlWEFpT2lKS1YxUWlmUUwzNjUxODM1NDkxMjMyNTMwODM5MDI3NjI3ODYwNDE0MzQ1MjQ3NzM3MTA3NDM1NDYzMTQzNDg5NTkyNjkxNDA1Nzc5MDcwNDk4MDcybAMAAAAAAABiArrh5dkJyvudd/g0xj3Hc1LoHWJDGYT5USspP/JTlSGDODoUEvlWl2bhYENfrqKonGWMa+yaONIJrulKluDPo74DhpjOOLV4WR7wTqNvT3MGJI11ZIBjUJbW20sZgBYR5+s=";
        LogBase64BytesInline(workingSignature);
        string[] separators = { ",", ":", " ", "|", ";", "-", "=", "*", "#", "+", "@", "$", "!", "&", "u", "U" };
        foreach (var sep in separators)
        {
            string combined = a + sep + b;
            string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(combined));
            Debug.Log($"{sep} => {base64[102]}");
            Debug.Log(base64);
        }
        
    }

    void LogBytes(byte[] bytes)
    {
        string hexString = BitConverter.ToString(bytes); // 0x yerine sadece hex
        Debug.Log($"Bytes ({bytes.Length}): {hexString}");
    }

    void LogBase64BytesInline(string base64String)
    {
        try
        {
            byte[] bytes = Convert.FromBase64String(base64String);
            string hexString = BitConverter.ToString(bytes); // 0x yerine sadece hex
            Debug.Log($"Bytes ({bytes.Length}): {hexString}");
        }
        catch (FormatException ex)
        {
            Debug.LogError($"Base64 string format hatasi: {ex.Message}");
        }
    }

    string StringToBase64(string text)
    {
        byte[] stringBytes = Encoding.UTF8.GetBytes(text);

        string base64 = Convert.ToBase64String(stringBytes);

        return base64;
    }

    string BigIntegerToBase64(string numStr)
    {
        BigInteger big = BigInteger.Parse(numStr);

        byte[] bytesLE = big.ToByteArray();
        if (bytesLE[bytesLE.Length - 1] == 0) 
            Array.Resize(ref bytesLE, bytesLE.Length - 1);

        Array.Reverse(bytesLE);

        string base64 = Convert.ToBase64String(bytesLE);
        string noPad = base64.TrimEnd('=');

        return noPad;
    }

    void Test2(string workingSignature, string failSignature)
    {
        
        byte[] workingSigBytes = Convert.FromBase64String(workingSignature);
        byte[] faultSigBytes = Convert.FromBase64String(failSignature);
        CompareSignatures(workingSigBytes, faultSigBytes, 32);

    }

    async void Test()
    {
        _client = new SuiClient(Constants.TestnetConnection);
        _ephemeralAccount = new Account("0x40bdbe2b4076f5c6560d22194be9b7142b540edd2459061e4ff0a6ddd88e700c");
        Debug.Log(_ephemeralAccount.PrivateKey.ToHex());
        Debug.Log(_ephemeralAccount.SuiAddress());
        Debug.Log(_ephemeralAccount.PrivateKey.ToHex());
        Debug.Log(_ephemeralAccount.PublicKey.ToSuiPublicKey());
        Debug.Log(_ephemeralAccount.PublicKey.ToBase64());
        NonceResponse nr = await ZkLoginUtils.FetchNonce(_enokiPublicKey, _network, _ephemeralAccount.PublicKey.ToSuiPublicKey(), 2);
        string jwtToken = await GoogleOAuthManager.GetJWTFromGoogleLogin(_googleClientId, nr.data.nonce, _redirectUri);
        JWT jwt = JWTDecoder.DecodeJWT(jwtToken);
        _zkLoginUser = await ZkLoginUtils.FetchZKLoginData(jwtToken, _enokiPublicKey);
        Debug.Log(_zkLoginUser.data.address);
        Debug.Log(_zkLoginUser.data.publicKey);
        Debug.Log(_zkLoginUser.data.salt);
        ZKPRoot zkpRoot = await ZkLoginUtils.FetchZKP(_network, _ephemeralAccount.PublicKey.ToSuiPublicKey(), jwtToken, _enokiPublicKey, nr.data.maxEpoch, nr.data.randomness);
        string jsonData = JsonConvert.SerializeObject(zkpRoot.data);
        //Debug.Log(jsonData);
        Inputs inputs = JsonConvert.DeserializeObject<Inputs>(jsonData);
        TransactionBlock tx_block = new TransactionBlock();
        List<TransactionArgument> splitArgs = tx_block.AddSplitCoinsTx
        (
            tx_block.gas,
            new TransactionArgument[]
            {
                tx_block.AddPure(new U64(10000000)) // Insert split amount here
            }
        );
        tx_block.AddTransferObjectsTx
        (
            new TransactionArgument[]
            {
                splitArgs[0] // Insert split amount here
            },
            Sui.Accounts.AccountAddress.FromHex("0x0d9b5ca4ebae5f4a7bd3f17e4e36cd6f868d8f0c5a7f977f94f836631fe0288d")
        );
        tx_block.SetSender(Sui.Accounts.AccountAddress.FromHex(_zkLoginUser.data.address));
        byte[] userTxBytes = await tx_block.Build(new BuildOptions(_client));
        Debug.Log("built");
        if (tx_block.Error != null)
        {
            Debug.LogError(tx_block.Error.Message);
            return;
        }

        SignatureBase signature = _ephemeralAccount.SignTransactionBlock(userTxBytes);
        SuiResult<string> signature_result = _ephemeralAccount.ToSerializedSignature(signature);
        if (signature_result.Error != null)
        {
            Debug.LogError(signature_result.Error.Message);
            return;
        }

        string zkSignature = ZkLoginSignature.GetZkLoginSignature(inputs, (ulong)nr.data.maxEpoch, CryptoBytes.FromBase64String(signature_result.Result));
        Debug.Log("Zk Signature => " + zkSignature);
        byte[] zkSigBytes = Convert.FromBase64String(zkSignature);
        Debug.Log(zkSigBytes.Length);
        LogBase64BytesInline(zkSignature);
        List<string> signList = new List<string>();
        signList.Add(zkSignature);
        TransactionBlockResponseOptions opts = new TransactionBlockResponseOptions
        {
            ShowInput = false,
            ShowEffects = true,
            ShowEvents = true,
            ShowObjectChanges = true,
            ShowBalanceChanges = true
        };
        RpcResult<TransactionBlockResponse> response = await _client.ExecuteTransactionBlockAsync(userTxBytes, signList, opts, RequestType.WaitForEffectsCert);

        Debug.Log(response.Result.Digest);

    }

    void CompareSignatures(byte[] workingSig, byte[] faultySig, int pubKeyLength)
    {
        if (workingSig.Length < 1 + pubKeyLength || faultySig.Length < 1 + pubKeyLength)
        {
            Debug.Log("Signature length too short for the given pubKeyLength!");
            return;
        }
        CompareProofPointsFromSignature(workingSig, faultySig);
        // Flag byte
        //byte workingFlag = workingSig[0];
        //byte faultyFlag = faultySig[0];
        //Debug.Log($"Flag: working=0x{workingFlag:X2}, faulty=0x{faultyFlag:X2}");

        //int workingUserSigLength = workingSig.Length - 1 - pubKeyLength;
        //int faultyUserSigLength = faultySig.Length - 1 - pubKeyLength;

        //byte[] workingUserSig = new byte[workingUserSigLength];
        //byte[] faultyUserSig = new byte[faultyUserSigLength];

        //Array.Copy(workingSig, 1, workingUserSig, 0, workingUserSigLength);
        //Array.Copy(faultySig, 1, faultyUserSig, 0, faultyUserSigLength);

        //Debug.Log($"UserSignature length: working={workingUserSig.Length}, faulty={faultyUserSig.Length}");
        //Debug.Log($"UserSignature hex diff:");

        //int minLength = Math.Min(workingUserSig.Length, faultyUserSig.Length);
        //for (int i = 0; i < minLength; i++)
        //{
        //    if (workingUserSig[i] != faultyUserSig[i])
        //        Debug.Log($"Byte {i}: working=0x{workingUserSig[i]:X2}, faulty=0x{faultyUserSig[i]:X2}");
        //}

        //if (workingUserSig.Length != faultyUserSig.Length)
        //    Debug.Log("UserSignature lengths differ!");

        //// Public Key (son pubKeyLength byte)
        //byte[] workingPubKey = new byte[pubKeyLength];
        //byte[] faultyPubKey = new byte[pubKeyLength];

        //Array.Copy(workingSig, workingSig.Length - pubKeyLength, workingPubKey, 0, pubKeyLength);
        //Array.Copy(faultySig, faultySig.Length - pubKeyLength, faultyPubKey, 0, pubKeyLength);

        //Debug.Log("PublicKey diff:");
        //for (int i = 0; i < pubKeyLength; i++)
        //{
        //    if (workingPubKey[i] != faultyPubKey[i])
        //        Debug.Log($"Byte {i}: working=0x{workingPubKey[i]:X2}, faulty=0x{faultyPubKey[i]:X2}");
        //}
    }

    void CompareProofPointsFromSignature(byte[] workingSigBytes, byte[] faultySigBytes)
    {
        int offset = 1; // Flag bayt

        // Deserialization helper: uleb128
        int ReadUleb128(byte[] data, ref int idx)
        {
            int result = 0;
            int shift = 0;
            while (true)
            {
                byte b = data[idx++];
                result |= (b & 0x7F) << shift;
                if ((b & 0x80) == 0) break;
                shift += 7;
            }
            return result;
        }

        int workingIdx = offset;
        int faultyIdx = offset;

        // --- A vector ---
        int workingACount = ReadUleb128(workingSigBytes, ref workingIdx);
        int faultyACount = ReadUleb128(faultySigBytes, ref faultyIdx);
        Debug.Log($"A count: working={workingACount}, faulty={faultyACount}");

        int minACount = Math.Min(workingACount, faultyACount);
        for (int i = 0; i < minACount; i++)
        {
            int workingLen = ReadUleb128(workingSigBytes, ref workingIdx);
            int faultyLen = ReadUleb128(faultySigBytes, ref faultyIdx);

            Debug.Log($"A[{i}] length: working={workingLen}, faulty={faultyLen}");

            workingIdx += workingLen;
            faultyIdx += faultyLen;
        }
        if (workingACount != faultyACount) Debug.Log("A element counts differ!");

        // --- B vector<vector> ---
        int workingBCount = ReadUleb128(workingSigBytes, ref workingIdx);
        int faultyBCount = ReadUleb128(faultySigBytes, ref faultyIdx);
        Debug.Log($"B outer count: working={workingBCount}, faulty={faultyBCount}");

        int minBCount = Math.Min(workingBCount, faultyBCount);
        for (int i = 0; i < minBCount; i++)
        {
            int workingInnerCount = ReadUleb128(workingSigBytes, ref workingIdx);
            int faultyInnerCount = ReadUleb128(faultySigBytes, ref faultyIdx);
            Debug.Log($"B[{i}] inner count: working={workingInnerCount}, faulty={faultyInnerCount}");

            int minInner = Math.Min(workingInnerCount, faultyInnerCount);
            for (int j = 0; j < minInner; j++)
            {
                int workingLen = ReadUleb128(workingSigBytes, ref workingIdx);
                int faultyLen = ReadUleb128(faultySigBytes, ref faultyIdx);
                Debug.Log($"B[{i}][{j}] length: working={workingLen}, faulty={faultyLen}");

                workingIdx += workingLen;
                faultyIdx += faultyLen;
            }
        }

        // --- C vector ---
        int workingCCount = ReadUleb128(workingSigBytes, ref workingIdx);
        int faultyCCount = ReadUleb128(faultySigBytes, ref faultyIdx);
        Debug.Log($"C count: working={workingCCount}, faulty={faultyCCount}");

        int minCCount = Math.Min(workingCCount, faultyCCount);
        for (int i = 0; i < minCCount; i++)
        {
            int workingLen = ReadUleb128(workingSigBytes, ref workingIdx);
            int faultyLen = ReadUleb128(faultySigBytes, ref faultyIdx);
            Debug.Log($"C[{i}] length: working={workingLen}, faulty={faultyLen}");

            workingIdx += workingLen;
            faultyIdx += faultyLen;
        }

        if (workingCCount != faultyCCount) Debug.Log("C element counts differ!");
    }

    private void OnDestroy()
    {
        GoogleOAuthManager.Dispose();
    }
}
