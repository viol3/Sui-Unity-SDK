using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenDive.BCS;
using Sui.Accounts;
using Sui.Rpc;
using Sui.Rpc.Client;
using Sui.Rpc.Models;
using Sui.Transactions;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Sui.Seal
{
    public class SealSample : MonoBehaviour
    {
        [SerializeField] private string _packageId = "0xf3dfe70b4916fecaecf7928bb8221031c28d5130c66e8fa7e645ce8785846f91";
        [SerializeField] private string _moduleName = "private_data";
        [SerializeField] private string _funcName = "store_entry";
        [SerializeField] private int _threshold = 2;
        [SerializeField] private string[] _serverObjectIds = { "0x73d05d62c18d9374e3ea529e8e0ed6161da1a141a94d3f76ae3fe4e99356db75", "0xf5d14a81a982144ae441cd7d64b09027f116a468bd36e7eca494f750591623c8" };
        [Space]
        [SerializeField] private string _privateKeyHex;
        [Space]
        [Header("UI")]
        [SerializeField] private Button _encryptButton;
        [SerializeField] private Button _decryptButton;
        [SerializeField] private TMP_Text _encryptedText;
        [SerializeField] private TMP_Text _decryptText;
        [SerializeField] private TMP_InputField _encryptTextField;
        [SerializeField] private TMP_InputField _decryptTextField;

        private Account _account;
        private SuiClient _client;
        void Start()
        {
            _client = new SuiClient(Constants.TestnetConnection);
            _account = new Account(_privateKeyHex);
            SealBridge.Instance.SetSuiClient(_client);
            SealBridge.Instance.SetThreshold(_threshold);
            SealBridge.Instance.SetPackageInformation(_packageId, _moduleName, _funcName);
            SealBridge.Instance.SetServerObjectIds(_serverObjectIds);
        }

        public async void OnEncryptButtonClick()
        {
            string dataToEncrypt = _encryptTextField.text;
            TransactionBlock txBlock = await SealBridge.Instance.Encrypt(dataToEncrypt, _account.SuiAddress().ToHex());
            await _client.SignAndExecuteTransactionBlockAsync(txBlock, _account);
            _encryptedText.gameObject.SetActive(true);
        }

        public async void OnDecryptButtonClick()
        {
            string objectId = _decryptTextField.text;
            var response = await _client.GetObjectAsync(new Accounts.AccountAddress(objectId), new ObjectDataOptions() { ShowContent = true });
            var moveObject = (ParsedMoveObject)response.Result.Data.Content.ParsedData;

            var fields = moveObject.Fields;
            byte[] encryptedBytes = (moveObject.Fields["data"] as JArray).Select(jv => (byte)jv).ToArray();
            byte[] nonceBytes = (moveObject.Fields["nonce"] as JArray).Select(jv => (byte)jv).ToArray();
            byte[] decryptedBytes = await SealBridge.Instance.Decrypt(encryptedBytes, nonceBytes, objectId, _account.PrivateKey.ToBase64(), _account.SuiAddress().ToHex());
            _decryptText.text = Encoding.UTF8.GetString(decryptedBytes);
        }

    }

}
