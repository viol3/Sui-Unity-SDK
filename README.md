<p align="center">
	<img src="./Resources/SuiLogo.png" alt="SuiUnitySDKLogo" width="515" height="214" />
</p>


# Sui-Unity-SDK #

## 		Sui Made Easy in Unity

[![Made with Unity](https://img.shields.io/badge/Made%20with-Unity-57b9d3.svg?style=flat&logo=unity)](https://unity3d.com) [![](https://dcbadge.vercel.app/api/server/sui)](https://discord.gg/sui)

The OpenDive Sui Unity SDK is the first fully-featured Unity SDK with offline transaction building. 

This means that games built with our SDK can directly craft custom Move calls without relying Sui's "unsafe" RPC calls under the [**Transaction Builder API**](https://docs.sui.io/sui-api-ref#transaction-builder-api) -- which in turn reduces the number of RPC / Network requests.

Our SDK fully supports mobile, desktop and WebGL application built with the Unity game engine.

- [About the Project](#about-the-project)
  - [Motivation](#motivation)
  - [Our Vision](#our-vision)


- [Features](#features)
- [Requirements](#requirements)
- [Dependencies](#dependencies)
- [Installation](#installation)
  - [Unity Package Importer](#unity-package-importer)
- [Test Suite](#test-suite)
- [Using Sui-Unity-SDK](#using-sui-unity-sdk)
  - [SuiClient](#suiclient)
  - [FaucetClient](#faucetclient)
  - [TransactionBlock](#transactionblock)
  - [Account](#account)
- [License](#license)

## About the Project

**Sui-Unity-SDK** is a C# Unity SDK designed to provide Unity developers with a seamless and efficient way to interact with the Sui Blockchain. Our aim is to lower the barrier for Unity developers, enabling them to leverage and integrate Sui's vast and versatile ecosystem into their games without having to delve deep into the complexities of the ecosystem.

### Motivation

With the rise of Web3 technologies being leveraged for games of all genres, it is very vital that developers have access to the necessary tools to simplify the process of leveraging such technologies. While Sui has an incredible suite of capabilities, there was a clear need for a C# Unity SDK that aligns with the idiomatic practices of the engine and language, along with the expectations of such development tools.

### Out Vision

We envision a tool that not only provides the flexibility and customization for experienced Sui developers, but also provides an easy entry point for novice and beginner developers who aren't as familiar with the various complex concepts that come with Web3 development. Our ultimate goal is to foster innovation by providing Unity developers with the right tools to integrate Sui's capabilities into their applications effortlessly.

### Features ###

- [x] Create new accounts using the ED25519 Key Standard.
- [x] Simulate and submit transaction.
- [x] Transfer Sui.
- [x] Air drop Sui tokens.
- [x] Merge and Split coins.
- [x] Publish modules.
- [x] Transfer objects.
- [x] Execute move calls.
- [x] Retrieving objects, transactions, checkpoints, coins, and events.
- [x] Local Transaction Building.
- [x] Local, custom, dev, test, and main net compatibility.
- [x] Comprehensive Unit and Integration Test coverage.
- [x] [ZK Login](https://docs.sui.io/concepts/cryptography/zklogin)

### Running End-to-End Test
The SDK contains a test suite that leverages a local instance of the the Sui client. Prior to running the tests please install the Sui client and run the following command:

```
RUST_LOG="off,sui_node=info" sui start --with-faucet --force-regenesis --with-indexer
```

### On ZK Login

>  zkLogin is a two-factor authentication scheme; sending a transaction requires both a credential from a recent OAuth login and a salt not managed by the OAuth provider. An attacker who compromises an OAuth account cannot transact from the user's corresponding Sui address unless they separately compromise the salt.

> zkLogin is one of several native Sui signature schemes thanks to Sui's cryptography agility. It integrates with other Sui primitives, like sponsored transactions and multisig.

#### How ZK Login Works
How zkLogin works

In rough sketches, the zkLogin protocol relies on the following:

1. A JWT is a signed payload from OAuth providers, including a user-defined field named nonce. zkLogin leverages the OpenID Connect OAuth flow by defining the nonce as a public key and an expiry epoch.
2. The wallet stores an ephemeral KeyPair, where the ephemeral public key is defined in the nonce. The ephemeral private key signs transactions for a brief session, eliminating the need for user memorization. The Groth16 zero-knowledge proof is generated based on the JWT, concealing privacy-sensitive fields.
3. A transaction is submitted on-chain with the ephemeral signature and the ZK proof. Sui authorities execute the transaction after verifying the ephemeral signature and the proof.
4. Instead of deriving the Sui address based on a public key, the zkLogin address is derived from sub (that uniquely identifies the user per provider), iss (identifies the provider), aud (identifies the application) and user_salt (a value that unlinks the OAuth identifier with the on-chain address).


### Make Account Transfer Quickly and Easily! ###

With how easy the library is to use, Sui-Unity-SDK gives you the power to quickly setup a Sui account and be able to transfer coins in only a few lines of code:
```c#
// Initialize Accounts.
Account alice = new Account();
Account bob = new Account();

// Initialize Client and Transaction Block
SuiClient client = new SuiClient(Constants.LocalnetConnection);
TransactionBlock tx_block = new TransactionBlock();

// Call Transfer Objects
tx_block.AddTransferObjectsTx(new SuiTransactionArgument[] { tx_block.gas }, bob.SuiAddress());

// Call SignAndExecuteTransactionBlockAsync
RpcResult<TransactionBlockResponse> published_tx_block = await client.SignAndExecuteTransactionBlockAsync
(
    tx_block,
    alice
);

// Return the TransactionBlockResponse
return published_tx_block.Result;
```

### Requirements ###

| Platforms                              | Unity Version | Installation           | Status       |
| -------------------------------------- | ------------- | ---------------------- | ------------ |
| Windows /  Mac / iOS / Android / WebGL | 2021.3.x      | Unity Package Importer | Fully Tested |
| Windows /  Mac / iOS / Android / WebGL | 2022.2.x      | Unity Package Importer | Fully Tested |

### Dependencies
- [Chaos.NaCl.Standard](https://www.nuget.org/packages/Chaos.NaCl.Standard/)
- Microsoft.Extensions.Logging.Abstractions.1.0.0 — required by NBitcoin.7.0.22
- Newtonsoft.Json
- NBitcoin.7.0.22
- [Portable.BouncyCastle](https://www.nuget.org/packages/Portable.BouncyCastle)

### Installation ###

#### Unity Package Importer ####

- Download the latest `sui-unity-sdk-xx.unitypackage` file from [Release](https://github.com/OpenDive/Sui-Unity-SDK/releases)
- Inside Unity, Click on `Assets` → `Import Packages` → `Custom Package.` and select the downloaded file.

​	**NOTE:**  As of Unity 2021.x.x, Newtonsoft Json is a common dependency. Prior versions of Unity require installing Newtonsoft.

### Test Suite
The SDK's test suite can be found in the following directory: `Assets/Sui-Unity-SDK/Tests`.   

The test suite covers:
- Account - private / public keys, signatures and verification
- Transactions - creation, signing, serialization, and deserialization
	- Coin Querying
	- Extended API
	- Governance API
	- Move Utilities
	- Read API
	- Write API
- BCS serialization and deserialization

### Using Sui-Unity-SDK ###

Sui-Unity-SDK is designed to be very easy to integrate into your own Unity projects. The main functionality comes from several key classes: `SuiClient`, `FacetClient`, `TransactionBlock`, and `Account`.   

There are four core classes:
- **SuiClient** - used to query the Sui blockchain
- **FaucetClient** - used to request for airdrops
- **TransactionBlock** - used to represent a transaction block object
- **Account** - used for representing sui accounts

#### SuiClient ####

The RPC Client provides you with the fundamental transaction endpoints needed for interacting with the Sui Blockchain. The following showcases how to initialize the `SuiClient`.

```c#
// Initialize Client with a local network connection
SuiClient client = new SuiClient(Constants.LocalnetConnection);
```

As shown before, it only take a few lines of code to initialize a transfer for Sui coins. This is the main class developers will be leveraging to interact directly with the Sui Blockchain via RPC Client calls. Here's another example showing how to publish a package:

```c#
// Initialize Account
Account alice = new Account();

// Initialize Client and Transaction Block
SuiClient client = new SuiClient(Constants.LocalnetConnection);
TransactionBlock tx_block = new TransactionBlock();

// Initialize Modules and Dependencies
//
// Note: Please utilize an outside tool for
// importing compiled packages as JSON files.
string[] modules = new string[] { "MODULE-BASE-64" };
string[] dependencies = new string[] { "DEPENDENCIES-HEXADECIMAL" };

// Call Publish and Transfer Object
List<SuiTransactionArgument> cap = tx_block.AddPublishTx
(
    modules,
    dependencies
);
tx_block.AddTransferObjectsTx(cap.ToArray(), alice.SuiAddress());

// Call SignAndExecuteTransactionBlockAsync
RpcResult<TransactionBlockResponse> published_tx_block = await this.Client.SignAndExecuteTransactionBlockAsync(tx_block, alice);

// Return the TransactionBlockResponse
return published_tx_block.Result;
```

Here's also how to do various other calls:

```csharp
// Initialize Account
Account alice = new Account();

// Initialize Client and coins
SuiClient client = new SuiClient(Constants.LocalnetConnection);
SuiStructTag sui_coin = new SuiStructTag("0x2::sui::SUI");
SuiStructTag coin = new SuiStructTag("0x2::coin::Coin");

// Get coin metadata and print the name
RpcResult<CoinMetadata> coin_metadata = await client.GetCoinMetadataAsync(sui_coin);
Debug.Log($"Sui coin name: {coin_metadata.Name}");

// Get account balance
RpcResult<Balance> alice_balance = await client.GetBalanceAsync(alice, sui_coin);
Debug.Log($"Alice's current Sui coin balance: {alice_balance.TotalBalance}'");

// Get coin
RpcResult<CoinPage> alice_sui_coins = await client.GetCoinsAsync(alice, sui_coin);
Debug.Log($"Alice's Sui coin object ID - {alice_sui_coins.Result.Data[0].CoinObjectID.ToString()}");

// Get Move structs
RpcResult<SuiMoveNormalizedStruct> coin_move_struct = await client.GetNormalizedMoveStructAsync
(
    coin_struct
);
Debug.Log($"Coin struct fields: {Utils.ToReadableString(coin_move_struct.Fields)}");

// Get Objects
CoinDetails gas_coin = alice_sui_coins.Result.Data[0];
ObjectData details = gas_coin.ToSuiObjectData();
RpcResult<ObjectDataResponse> coin_object = await client.GetObjectAsync
(
    details.ObjectID
);
Debug.Log($"Sui coin object type: {coin_object.Result.Data.Type.ToString()}");
```

#### FaucetClient ####

The Faucet Client allows the developer to leverage the ability to fund wallets on any of the non-main networks within the Sui Blockchain. This can easily speed up development times through automating the process of funding accounts. Here's an example on how to use the Faucet Client:

```csharp
// Initialize Account
Account alice = Account.Generate();

// Initialize Funding Request
FaucetClient faucet = new FaucetClient(Constants.LocalnetConnection);
bool result = await faucet.AirdropGasAsync(alice.SuiAddress());
```

#### TransactionBlock ####

The Transaction Block is a local object that helps developers build out the serialized representation of transaction data. This is the core of utilizing offline transaction building, along with building out interactions with on-chain objects. Here is an example to build out a simple Move call:

```c#
// Initialize Account
Account alice = Account.Generate();

// Initialize a transaction block
TransactionBlock tx_block = new TransactionBlock();

// Call to create a Move call
tx_block.AddMoveCallTx
(
    SuiMoveNormalizedStructType.FromStr("0x2::pay::split"),
    new SerializableTypeTag[] { new SerializableTypeTag("0x2::sui::SUI") },
    new SuiTransactionArgument[]
    {
            tx_block.AddObjectInput("COIN-OBJECT-ID"),  // Insert coin object ID here
            tx_block.AddPure(new U64(100))  // Insert split amount here
    }
);

// Sign and execute transaction block
Task<RpcResult<TransactionBlockResponse>> result_task = client.SignAndExecuteTransactionBlockAsync
(
    tx_block,
    alice
);
```

As well, developers can utilize any arbitrary move calls published to the network. Here is an example using a serializer to set a value:

```c#
// Initialize Account
Account alice = Account.Generate();

// Initialize a transaction block
TransactionBlock tx_block = new TransactionBlock();

string package_id = "PACKAGE-ID";  // Insert Package ID here

// Call to create a Move call, specifically setting a value
tx_block.AddMoveCallTx
(
    SuiMoveNormalizedStructType.FromStr($"{package_id}::serializer_tests::value"),
    new SerializableTypeTag[] { },
    new SuiTransactionArgument[]
    {
            tx_block.AddObjectInput("SHARED-OBJECT-ID")  // Insert shared object ID here
    }
);
tx_block.AddMoveCallTx
(
    SuiMoveNormalizedStructType.FromStr($"{package_id}::serializer_tests::set_value"),
    new SerializableTypeTag[] { },
    new SuiTransactionArgument[]
    {
            tx_block.AddObjectInput("SHARED-OBJECT-ID")  // Insert shared object ID here
    }
);

// Sign and execute transaction block
Task<RpcResult<TransactionBlockResponse>> result_task = client.SignAndExecuteTransactionBlockAsync
(
    tx_block,
    alice
);
```

#### Account ####

Accounts within the SDK represent a Sui user's account, that give ease of access to the needed information you'd need for communicating with the Sui Blockchain. Here are some example initializations of accounts:

```c#
// Generate Random Account
alice = Account.Generate();

// Initialize Account Using Hexadecimal Private Key
const string PrivateKeyHex = "0x64f57603b58af16907c18a866123286e1cbce89790613558dc1775abb3fc5c8c";
bob = Account.LoadKey(PrivateKeyHex);

// Initialize Account Using Private and Public Key Bytes
private static readonly byte[] PrivateKeyBytes = {
	100, 245, 118, 3, 181, 138, 241, 105,
	7, 193, 138, 134, 97, 35, 40, 110,
	28, 188, 232, 151, 144, 97, 53, 88,
	220, 23, 117, 171, 179, 252, 92, 140
};
private static readonly byte[] PublicKeyBytes = {
	88, 110, 60, 141, 68, 125, 118, 121,
	34, 46, 19, 144, 51, 227, 130, 2,
	53, 227, 61, 165, 9, 30, 155, 11,
	184, 241, 161, 18, 207, 12, 143, 245
};

// Initialize the account
Account chad = new Account(PrivateKeyBytes, PublicKeyBytes);
```

From there, you're able to retrieve the byte array of the private and public keys, along with signing and verifying messages and transactions, as the core function of an Account object, as shown here:

```csharp
// Retrieve Private Key
Ed25519.PrivateKey privateKey = chad.PrivateKey;

// Retrieve Public Key
Ed25519.PublicKey publicKey = chad.PublicKey;
```

The developer can now use the Account to sign various messages and transactions for interacting with the Sui Blockchain, as shown here:

```c#
// Create a Signature Object Storing the Signature of the Signed Message
private static readonly byte[] MessageUt8Bytes = {
	87, 69, 76, 67, 79, 77, 69, 32,
	84, 79, 32, 65, 80, 84, 79, 83, 33 
};
SignatureBase signature = privateKey.Sign(MessageUt8Bytes);
```

Developers can also verify the integrity of the message using the public key, as shown here:

```c#
// Initiailize Verified Bool Object
bool verified = chad.Verify(MessageUt8Bytes, signature);
```

### Sui-Enoki-ZKLogin Unity SDK
A complete Unity implementation for authenticating users and signing transactions on the Sui blockchain using Enoki-ZKLogin (Zero-Knowledge Login) with Google OAuth.

#### Enoki ZK-Login Features

1. Google OAuth Integration: OAuth 2.0 with Google accounts for WebGL/Desktop/Mobile
2. Multi-Platform Support: Works on Desktop (Windows, Mac, Linux), WebGL and Mobile.
3. Session Persistence: Optional local storage of authentication data & ZK-Proof data.
4. Transaction Signing: Sign and execute Sui blockchain transactions with ZKLogin.
5. Epoch Validation: Automatic session expiration checking.

#### Enoki ZK-Login Prerequisites

1. Google OAuth Web Client ID from Google Cloud Console
2. Enoki Public Key from Enoki Portal

#### Google Cloud Console Setup

1. Go to Google Cloud Console
2. Create a new project or select an existing one
3. Navigate to APIs & Services > Credentials
4. Create OAuth 2.0 Client ID:

Application type: Web application

Authorized JavaScript origins:
- http://localhost:3000 (for desktop testing & mobile devices)
- [Your WebGL deployment URL]

Authorized redirect URIs:
- http://localhost:3000 (for desktop & mobile devices)
- [Your WebGL deployment URL]

Save your clientId and clientSecret for later use.

#### Enoki Setup

- Visit Enoki Portal
- Create an account & application with ZKLogin feature and obtain your Public Key
- Note your network (testnet, mainnet, devnet)

#### Enoki ZK-Login Installation
1. Import the Sui Unity SDK into your project
2. Copy all ZKLogin scripts into your project:

- EnokiZKLogin.cs
- EnokiZkLoginUtils.cs
- IJwtFetcher.cs (interface)
- And Samples folder.

#### Platform Support
1. Desktop [Windows(tested), Mac(not tested), Linux(not tested)]
2. Mobile [Android(tested), iOS(not tested)]
3. WebGL(tested)

#### Desktop
Uses GoogleOAuthDesktopJwtFetcher which:

- Opens browser for Google authentication
- Starts local HTTP server on specified port (default: 3000)
- Captures OAuth redirect automatically
- Uses returned JWT token to generate ZKUserData.

#### WebGL
Uses GoogleOAuthWebGLJwtFetcher which:

- Requires JavaScript plugin (ZKLogin.jslib)
- Handles OAuth 2.0 authentication in browser context
- Must be served from proper web server with CORS headers

### Enoki-ZK Login Security Considerations
⚠️ CRITICAL: PlayerPrefs Storage Warning

The _saveZKPOnDevice option uses Unity's PlayerPrefs which:

- Stores data in plain text
- Persists indefinitely
- Is NOT encrypted
- Can be accessed if device is compromised

Stored sensitive data includes:

- Ephemeral private key (can sign transactions)
- Zero-Knowledge Proof (authentication data)
Together, these allow full transaction signing capability

#### Production Recommendations
DO NOT use PlayerPrefs rawly in production! Instead:

1. Mobile Apps:

- iOS: Use Keychain Services
- Android: Use Android Keystore

2. Session-Only Storage:

- Store credentials only in memory
- Clear on application exit
- Require re-authentication on restart

3. Desktop Apps:

- Use OS-specific secure storage
- Windows: Credential Manager
- macOS: Keychain
- Linux: Secret Service API

WebGL:

- We don't recommend store credentials locally with rawData.
- Use some encryption solutions.
- Use session-only authentication
- Implement proper server-side session management

### Enoki-ZKLogin API Reference

Initialize the ZKLogin system with network and Enoki key:
```c#
EnokiZKLogin.Init(network, enokiPublicKey);
```
Authentication:
```c#
await EnokiZKLogin.Login();
```

Logout:
```c#
await EnokiZKLogin.Logout();
```

Check if zk-login user currently logged:
```c#
EnokiZKLogin.IsLogged();
```

Validate if current session has expired:
```c#
await EnokiZKLogin.ValidateMaxEpoch();
```

Get User's ZKLogin Sui Address:
```c#
EnokiZKLogin.GetSuiAddress();
```

Get User's ZKLogin Ephemeral Account:
```c#
EnokiZKLogin.GetEphemeralAccount();
```

Get User's ZKLogin Datas:
```c#
EnokiZKLogin.GetZKLoginUser();
EnokiZKLogin.GetZKP();
EnokiZKLogin.GetMaxEpoch();
```
Load previous session:
```c#
EnokiZKLogin.LoadZKPResponse(zkpResponse);
EnokiZKLogin.LoadZKLoginUser(zkLoginUser);
EnokiZKLogin.LoadEphemeralKey(ephemeralAccount);
EnokiZKLogin.LoadMaxEpoch(maxEpoch);
```

Sign and execute transactions:
```c#
EnokiZKLogin.SignAndExecuteTransactionBlock(TransactionBlock tx);
```

Get the sui client of ZKLogin system or set external sui client:
```c#
EnokiZKLogin.GetClient()
EnokiZKLogin.SetClient(SuiClient client)
```

### License ###

Sui-Unity-SDK is released under the MIT license. [See LICENSE](https://github.com/OpenDive/Sui-Unity-SDK/LICENSE) for details.
