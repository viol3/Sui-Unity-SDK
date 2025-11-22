using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenDive.BCS;
using Sui.Accounts;
using Sui.Cryptography.Ed25519;
using Sui.Rpc;
using Sui.Rpc.Client;
using Sui.Transactions;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class SponsoredTransactionSample : MonoBehaviour
{
    [SerializeField] private string _network;

    [SerializeField] private string _privateKey = "0xd7295b565677ad8d079fd0dc411b68af8fd43c8b65e82f6731075f4173104e0d";
    [Space]
    
    [SerializeField] private TMP_InputField _recipientAddressInput;
    private SuiClient _client;
    private Account _account;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializeSuiClient();
        _account = new Account(_privateKey);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void InitializeSuiClient()
    {
        if (_client != null)
        {
            Debug.Log("Sui client already created.");
            return;
        }
        switch (_network)
        {
            case "mainnet":
                _client = new SuiClient(Constants.MainnetConnection);
                break;
            case "testnet":
                _client = new SuiClient(Constants.TestnetConnection);
                break;
            case "devnet":
                _client = new SuiClient(Constants.DevnetConnection);
                break;
            case "localnet":
                _client = new SuiClient(Constants.LocalnetConnection);
                break;
            default:
                Debug.LogWarning($"Unknown network:{_network}, creating client with testnet...");
                _client = new SuiClient(Constants.TestnetConnection);
                break;
        }
    }

    async Task SampleTransaction()
    {
        TransactionBlock tx_block = new TransactionBlock();

        // Split coins from the gas coin to create a new coin with specified amount
        // This example splits 10,000,000 MIST (0.01 SUI) from the gas coin
        // Note: 1 SUI = 1,000,000,000 MIST
        List<TransactionArgument> splitArgs = tx_block.AddSplitCoinsTx
        (
            tx_block.gas,
            new TransactionArgument[]
            {
                tx_block.AddPure(new U64(10_000_000)) // Insert split amount here(0.01 Sui)
            }
        );
        tx_block.AddTransferObjectsTx
        (
            new TransactionArgument[]
            {
                splitArgs[0] // Insert split amount here
            },
            Sui.Accounts.AccountAddress.FromHex(_recipientAddressInput.text)
        );
        tx_block.SetSender(_account);  
        byte[] tx_bytes = await tx_block.Build(new BuildOptions(_client, null, true));
        
        // Send to sponsor API
        await SendToSponsorAPI(tx_bytes);
    }

    async Task SendToSponsorAPI(byte[] txBytes)
    {
        string url = "http://localhost:3002/sponsor";
        
        // Convert tx_bytes to base64
        string txKindBytes = Convert.ToBase64String(txBytes);
        
        // Create JSON payload
        var payload = new
        {
            transactionKindBytes = txKindBytes,
            network = _network,
            sender = _account.SuiAddress()
        };
        
        string jsonPayload = JsonConvert.SerializeObject(payload);
        Debug.Log(jsonPayload);
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            await request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Sponsor API Response: {request.downloadHandler.text}");
            }
            else
            {
                Debug.LogError($"Sponsor API Error: {request.error}");
                Debug.LogError($"{request.downloadHandler.text}");
            }
        }
    }    

    public async void OnSubmitButtonClick()
    {
        await SampleTransaction();
    }
}
