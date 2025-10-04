using Newtonsoft.Json;
using OpenDive.BCS;
using Sui.Accounts;
using Sui.Cryptography.Ed25519;
using Sui.Rpc;
using Sui.Rpc.Client;
using Sui.Rpc.Models;
using Sui.Transactions;
using Sui.ZKLogin;
using Sui.ZKLogin.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class EnokiZKLoginSample : MonoBehaviour
{
    [SerializeField] private string _network;
    [SerializeField] private string _enokiPublicKey;
    [SerializeField] private string _googleClientId;
    [SerializeField] private string _redirectUri = "http://localhost:3000";
    [Space]
    [SerializeField] private bool _saveZKPOnDevice = true;
    [Space]
    [Header("UI")]
    [SerializeField] private Button _loginButton;
    [SerializeField] private Button _sampleTransactionButton;
    [SerializeField] private TMP_Text _addressText;
    [SerializeField] private TMP_Text _transactionLogText;

    private Account _ephemeralAccount;
    private SuiClient _client;

    private const string ZKP_PREF = "ZKP";
    private const string ZKLOGINUSER_PREF = "ZKLOGINUSER";
    private const string EPHEMERAL_PRIVATEKEY = "EPHEMERAL_PRIVATEKEY";
    private const string MAX_EPOCH = "MAX_EPOCH";
    async void Start()
    {
        EnokiZKLogin.Init(_network, _enokiPublicKey);
        _client = EnokiZKLogin.GetClient();
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        EnokiZKLogin.LoadJwtFetcher(new GoogleOAuthDesktopJwtFetcher(_googleClientId, _redirectUri));
#endif
        if (PlayerPrefs.HasKey(ZKP_PREF))
        {
            EnokiZKPResponse zkpResponse = JsonConvert.DeserializeObject<EnokiZKPResponse>(PlayerPrefs.GetString(ZKP_PREF));
            EnokiZKLoginUser zkLoginUser = JsonConvert.DeserializeObject<EnokiZKLoginUser>(PlayerPrefs.GetString(ZKLOGINUSER_PREF));
            _ephemeralAccount = new Account(PlayerPrefs.GetString(EPHEMERAL_PRIVATEKEY));

            EnokiZKLogin.LoadZKPResponse(zkpResponse);
            EnokiZKLogin.LoadZKLoginUser(zkLoginUser);
            EnokiZKLogin.LoadEphemeralKey(_ephemeralAccount);
            EnokiZKLogin.LoadMaxEpoch(PlayerPrefs.GetInt(MAX_EPOCH));
            await EnokiZKLogin.ValidateMaxEpoch();
        }
        UpdateUI();
    }

    public async void OnLoginButtonClick()
    {
        EnokiZKPResponse zkpResponse = await EnokiZKLogin.Login();
        if (_saveZKPOnDevice)
        {
            PlayerPrefs.SetString(ZKP_PREF, JsonConvert.SerializeObject(zkpResponse));
            PlayerPrefs.SetString(ZKLOGINUSER_PREF, JsonConvert.SerializeObject(EnokiZKLogin.GetZKLoginUser()));
            PlayerPrefs.SetString(EPHEMERAL_PRIVATEKEY, EnokiZKLogin.GetEphemeralAccount().PrivateKey.ToHex());
            PlayerPrefs.SetInt(MAX_EPOCH, EnokiZKLogin.GetMaxEpoch());
        }
        UpdateUI();
    }

    public void OnSampleTransactionButtonClick()
    {
        SampleTransaction();
    }

    async void SampleTransaction()
    {
        TransactionBlock tx_block = new TransactionBlock();
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
            Sui.Accounts.AccountAddress.FromHex("0x0d9b5ca4ebae5f4a7bd3f17e4e36cd6f868d8f0c5a7f977f94f836631fe0288d")
        );
        RpcResult<TransactionBlockResponse> response = await EnokiZKLogin.SignAndExecuteTransactionBlock(tx_block);
        if(response.Error != null)
        {
            _transactionLogText.text = "<color=red>Tx Error => " + response.Error.Message + "</color>";
        }
        else
        {
            _transactionLogText.text = "<color=green>Tx Success => " + response.Result.Digest + "</color>";
        }
    }

    void UpdateUI()
    {
        _loginButton.interactable = !EnokiZKLogin.IsLogged();
        _addressText.text = EnokiZKLogin.IsLogged() ? EnokiZKLogin.GetSuiAddress() : "Login to see your zkLogin Sui Address";
        _sampleTransactionButton.interactable = EnokiZKLogin.IsLogged();
        _transactionLogText.text = "";
    }

    
}
