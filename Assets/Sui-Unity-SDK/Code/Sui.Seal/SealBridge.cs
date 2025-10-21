using Newtonsoft.Json;
using OpenDive.BCS;
using Sui.Cryptography;
using Sui.Rpc.Client;
using Sui.Transactions;
using Sui.Types;
using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEngine;

namespace Sui.Seal
{
    public class SealBridge : MonoBehaviour
    {
        private SuiClient _suiClient;

        private string _packageId = "0xf3dfe70b4916fecaecf7928bb8221031c28d5130c66e8fa7e645ce8785846f91";
        private string _moduleName = "private_data";
        private string _funcName = "store_entry";
        private int _threshold = 2;
        private string[] _serverObjectIds = { "0x73d05d62c18d9374e3ea529e8e0ed6161da1a141a94d3f76ae3fe4e99356db75", "0xf5d14a81a982144ae441cd7d64b09027f116a468bd36e7eca494f750591623c8" };

        private byte[] _encryptedBytes = null;
        private byte[] _decryptedBytes = null;
        private bool _canceled = false;

        [DllImport("__Internal")] private static extern void StartEncrypt(string data, string packageId, string suiAddress, string nonceB64, string suiClientUrl, string serverObjectIdsJson, int threshold);
        [DllImport("__Internal")] private static extern void StartDecrypt(string encryptedBytesBase64, string txBytesBase64, string privateKeyB64, string suiAddress, string packageId, string suiClientUrl, string serverObjectIdsJson);
        [DllImport("__Internal")] private static extern void StartDecryptWithZKLogin(string encryptedBytesBase64, string txBytesBase64, string ephemeralPrivateKeyB64, string inputBytesB64, uint maxEpoch, string suiAddress, string packageId, string suiClientUrl, string serverObjectIdsJson);

        public static SealBridge Instance;
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void SetSuiClient(SuiClient suiClient)
        {
            _suiClient = suiClient;
        }

        public void SetThreshold(int threshold)
        {
            _threshold = threshold;
        }

        public void SetServerObjectIds(params string[] serverObjectIds)
        {
            _serverObjectIds = serverObjectIds;
        }

        public void SetPackageInformation(string packageId, string moduleName, string funcName)
        {
            _packageId = packageId;
            _moduleName = moduleName;
            _funcName = funcName;
        }

        public async Task<TransactionBlock> Encrypt(string data, string suiAddressHex)
        {
            _canceled = false;
            _encryptedBytes = null;
            byte[] nonceBytes = new byte[32];
            RandomNumberGenerator.Fill(nonceBytes);
            string serverObjectIdsJson = JsonConvert.SerializeObject(new SealServerObjectWrapper { items = _serverObjectIds });
            StartEncrypt(data, _packageId, suiAddressHex, Convert.ToBase64String(nonceBytes), _suiClient.Connection.FULL_NODE, serverObjectIdsJson, _threshold);
            while (_encryptedBytes == null && !_canceled)
            {
                await Task.Yield();
            }
            if (_canceled)
            {
                return null;
            }
            return PrepareTransactionBlock(_encryptedBytes, nonceBytes);
        }

        public async Task<byte[]> DecryptWithZKLogin(byte[] encryptedBytes, byte[] proofInputBytes, uint maxEpoch, byte[] nonceBytes, string objectId, string ephemeralPrivateKeyBase64, string suiAddressHex)
        {
            _canceled = false;
            _decryptedBytes = null;
            var tx_block = new TransactionBlock();
            tx_block.AddMoveCallTx
            (
                SuiMoveNormalizedStructType.FromStr($"{_packageId}::{_moduleName}::seal_approve"),
                new SerializableTypeTag[] { },
                new TransactionArgument[]
                {
                    tx_block.AddPure(new OpenDive.BCS.Bytes(Sui.Seal.Utils.CreatePolicyId(suiAddressHex, nonceBytes))),
                    tx_block.AddObjectInput(objectId)
                }
            );
            var txBytes = await tx_block.Build(new BuildOptions(_suiClient, null, true, null));
            string txBytesBase64 = Convert.ToBase64String(txBytes);
            string encryptedBytesBase64 = Convert.ToBase64String(encryptedBytes);
            string serverObjectIdsJson = JsonConvert.SerializeObject(new SealServerObjectWrapper { items = _serverObjectIds });
            string inputBytesB64 = Convert.ToBase64String(proofInputBytes);
            StartDecryptWithZKLogin(encryptedBytesBase64, txBytesBase64, ephemeralPrivateKeyBase64, inputBytesB64, maxEpoch, suiAddressHex, _packageId, _suiClient.Connection.FULL_NODE, serverObjectIdsJson);
            while (_decryptedBytes == null && !_canceled)
            {
                await Task.Yield();
            }
            if (_canceled)
            {
                return null;
            }
            return _decryptedBytes;
        }

        public async Task<byte[]> Decrypt(byte[] encryptedBytes, byte[] nonceBytes, string objectId, string privateKeyBase64, string suiAddressHex)
        {
            _canceled = false;
            _decryptedBytes = null;
            var tx_block = new TransactionBlock();
            tx_block.AddMoveCallTx
            (
                SuiMoveNormalizedStructType.FromStr($"{_packageId}::{_moduleName}::seal_approve"),
                new SerializableTypeTag[] { },
                new TransactionArgument[]
                {
                    tx_block.AddPure(new OpenDive.BCS.Bytes(Sui.Seal.Utils.CreatePolicyId(suiAddressHex, nonceBytes))),
                    tx_block.AddObjectInput(objectId)
                }
            );
            var txBytes = await tx_block.Build(new BuildOptions(_suiClient, null, true, null));
            string txBytesBase64 = Convert.ToBase64String(txBytes);
            string encryptedBytesBase64 = Convert.ToBase64String(encryptedBytes);
            string serverObjectIdsJson = JsonConvert.SerializeObject(new SealServerObjectWrapper { items = _serverObjectIds });
            StartDecrypt(encryptedBytesBase64, txBytesBase64, privateKeyBase64, suiAddressHex, _packageId, _suiClient.Connection.FULL_NODE, serverObjectIdsJson);
            while (_decryptedBytes == null && !_canceled)
            {
                await Task.Yield();
            }
            if (_canceled)
            {
                return null;
            }
            return _decryptedBytes;
        }

        void OnEncryptionCompleted(string encryptedObjectBase64)
        {
            if (encryptedObjectBase64.StartsWith("SEAL_ERROR|"))
            {
                _canceled = true;
                throw new Exception(encryptedObjectBase64.Split('|')[1]);
            }
            _encryptedBytes = Convert.FromBase64String(encryptedObjectBase64);
        }

        void OnDecryptionCompleted(string decryptedObjectBase64)
        {
            if (decryptedObjectBase64.StartsWith("SEAL_ERROR|"))
            {
                _canceled = true;
                throw new Exception(decryptedObjectBase64.Split('|')[1]);
            }
            _decryptedBytes = Convert.FromBase64String(decryptedObjectBase64);
        }

        TransactionBlock PrepareTransactionBlock(byte[] encryptedBytes, byte[] nonceBytes)
        {
            TransactionBlock tx_block = new TransactionBlock();
            tx_block.AddMoveCallTx
            (
                SuiMoveNormalizedStructType.FromStr($"{_packageId}::{_moduleName}::{_funcName}"),
                new SerializableTypeTag[] { },
                new TransactionArgument[]
                {
                tx_block.AddPure(new OpenDive.BCS.Bytes(nonceBytes)),
                tx_block.AddPure(new OpenDive.BCS.Bytes(encryptedBytes))
                }
            );
            return tx_block;
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
}

