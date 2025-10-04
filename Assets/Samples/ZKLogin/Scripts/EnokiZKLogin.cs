using Chaos.NaCl;
using Newtonsoft.Json;
using OpenDive.Utils.Jwt;
using Sui.Accounts;
using Sui.Cryptography;
using Sui.Rpc;
using Sui.Rpc.Client;
using Sui.Rpc.Models;
using Sui.Transactions;
using Sui.Utilities;
using Sui.ZKLogin.Utils;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using UnityEngine;

namespace Sui.ZKLogin
{
    /// <summary>
    /// Manages ZK Login authentication and integration with the Sui blockchain network.
    /// Handles client initialization, ephemeral account management, Google OAuth authentication, Signing and Executing Transactions with ZKLogin Account.
    /// </summary>
    public static class EnokiZKLogin
    {
        private static string _network;
        private static string _enokiPublicKey;

        private static SuiClient _client;
        private static Account _ephemeralAccount;
        private static EnokiZKLoginUser _zkLoginUser;
        private static EnokiZKPResponse _zkpResponse;
        private static int _maxEpoch;

        private static IJwtFetcher _jwtFetcher;

        private static bool _inited = false;

        /// <summary>
        /// Initializes the ZKLoginManager with the specified configuration parameters.
        /// </summary>
        /// <param name="network">The Sui network to connect to (mainnet, testnet, devnet, or localnet).</param>
        /// <param name="enokiPublicKey">The Enoki public key for ZK Login authentication => https://portal.enoki.mystenlabs.com/</param>

        public static void Init(string network, string enokiPublicKey)
        {
            if (_inited)
            {
                Debug.Log("ZKLoginManager already inited");
                return;
            }
            _network = network;
            _enokiPublicKey = enokiPublicKey;
            _ephemeralAccount = new Account();
            CreateSuiClient();
            _inited = true;
            Debug.Log("ZKLoginManager inited succesfully.");
        }

        /// <summary>
        /// Creates and configures a SuiClient instance based on the specified network.
        /// If a client already exists, logs a message and returns without creating a new one.
        /// Defaults to testnet if the specified network is unknown.
        /// </summary>
        static void CreateSuiClient()
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
                    Debug.LogWarning($"Unknown network:{_network}, creating client with testnet");
                    _client = new SuiClient(Constants.TestnetConnection);
                    break;
            }
        }

        public static void SetClient(SuiClient client)
        {
            _client = client;
        }

        public static SuiClient GetClient() 
        { 
            return _client; 
        }

        public static string GetSuiAddress()
        {
            if (_zkLoginUser == null)
            {
                Debug.LogError("You need to login or load ZKLoginUser data to get sui address.");
                return null;
            }
            return _zkLoginUser.data.address;
        }

        public static bool IsInited()
        {
            return _inited;
        }

        public static bool IsLogged()
        {
            return _zkpResponse != null;
        } 

        public static EnokiZKLoginUser GetZKLoginUser()
        {
            return _zkLoginUser;
        }

        public static EnokiZKPResponse GetZKP()
        {
            return _zkpResponse;
        }

        public static Account GetEphemeralAccount()
        { 
            return _ephemeralAccount; 
        }

        public static int GetMaxEpoch()
        { 
            return _maxEpoch; 
        }

        public static void LoadZKPResponse(EnokiZKPResponse zkpResponse)
        {
            _zkpResponse = zkpResponse;
        }

        public static void LoadZKLoginUser(EnokiZKLoginUser zkLoginUser)
        {
            _zkLoginUser = zkLoginUser;
        }

        public static void LoadEphemeralKey(Account ephemeralAccount)
        {
            _ephemeralAccount = ephemeralAccount;
        }

        public static void LoadMaxEpoch(int maxEpoch)
        {
            _maxEpoch = maxEpoch;
        }

        public static void LoadJwtFetcher(IJwtFetcher jwtFetcher)
        {
            _jwtFetcher = jwtFetcher;
        }

        public static async Task<EnokiZKPResponse> Login()
        {
            if (!_inited)
            {
                Debug.LogWarning("ZKLoginManager is not inited. Use Init() first.");
                return null;
            }
            if (_zkpResponse == null)
            {
                EnokiNonceResponse nr = await EnokiZkLoginUtils.FetchNonce(_enokiPublicKey, _network, _ephemeralAccount.PublicKey.ToSuiPublicKey(), 2);
                if (nr == null)
                {
                    return null;
                }
                _maxEpoch = nr.data.maxEpoch;
                if(_jwtFetcher == null)
                {
                    Debug.LogWarning("JwtFetcher is not assigned. You need to create a JwtFetcher and assign it via LoadJwtFetcher().");
                    return null;
                }
                string jwtToken = await _jwtFetcher.FetchJwt(nr.data.nonce);
                JWT jwt = JWTDecoder.DecodeJWT(jwtToken);
                if (jwt == null)
                {
                    return null;
                }
                _zkLoginUser = await EnokiZkLoginUtils.FetchZKLoginData(jwtToken, _enokiPublicKey);
                if (_zkLoginUser == null)
                {
                    return null;
                }
                _zkpResponse = await EnokiZkLoginUtils.FetchZKP(_network, _ephemeralAccount.PublicKey.ToSuiPublicKey(), jwtToken, _enokiPublicKey, nr.data.maxEpoch, nr.data.randomness);
            }
            return _zkpResponse;
        }

        public static void Logout()
        {
            if (!_inited)
            {
                Debug.LogWarning("ZKLoginManager is not inited. Use Init() first.");
                return;
            }
            _zkLoginUser = null;
            _zkpResponse = null;
            _maxEpoch = 0;
        }

        public static async Task ValidateMaxEpoch()
        {
            RpcResult<SuiSystemSummary> summary = await _client.GetLatestSuiSystemStateAsync();
            if (summary.Result.Epoch > BigInteger.Parse(_maxEpoch.ToString()))
            {
                Debug.LogWarning($"Max Epoch is not valid anymore({_maxEpoch}), logging out...");
                Logout();
            }
        }

        public static async Task<RpcResult<TransactionBlockResponse>> SignAndExecuteTransactionBlock(TransactionBlock transactionBlock)
        {
            string jsonData = JsonConvert.SerializeObject(_zkpResponse.data);
            Inputs inputs = JsonConvert.DeserializeObject<Inputs>(jsonData);

            transactionBlock.SetSenderIfNotSet(Sui.Accounts.AccountAddress.FromHex(_zkLoginUser.data.address));
            byte[] userTxBytes = await transactionBlock.Build(new BuildOptions(_client));
            if (transactionBlock.Error != null)
            {
                Debug.LogError(transactionBlock.Error.Message);
                return null;
            }

            SignatureBase signature = _ephemeralAccount.SignTransactionBlock(userTxBytes);
            SuiResult<string> signature_result = _ephemeralAccount.ToSerializedSignature(signature);
            if (signature_result.Error != null)
            {
                Debug.LogError(signature_result.Error.Message);
                return null;
            }

            string zkSignature = ZkLoginSignature.GetZkLoginSignature(inputs, (ulong)_maxEpoch, CryptoBytes.FromBase64String(signature_result.Result));
            Debug.Log("Zk Signature => " + zkSignature);
            TransactionBlockResponseOptions opts = new TransactionBlockResponseOptions
            {
                ShowInput = false,
                ShowEffects = true,
                ShowEvents = true,
                ShowObjectChanges = true,
                ShowBalanceChanges = true
            };
            RpcResult<TransactionBlockResponse> response = await _client.ExecuteTransactionBlockAsync(userTxBytes, new List<string>() { zkSignature }, opts);

            return response;
        }

        public static void Dispose()
        {

        }
    }

}


