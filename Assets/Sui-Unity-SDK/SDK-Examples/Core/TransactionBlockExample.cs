using System.Collections;
using System.Numerics;
using Sui.Accounts;
using Sui.Transactions;
using Sui.Transactions.Block;
using Sui.ZKLogin;
using Sui.ZKLogin.SDK;
using UnityEngine;

public class TransactionBlockExample : MonoBehaviour
{
    [SerializeField]
    private string recipientAddress = "0xfa0f8542f256e669694624aa3ee7bfbde5af54641646a3a05924cf9e329a8a36";

    [SerializeField]
    private string zkLoginUserAddress = "0x129ed8d47e9f0ddbce4d4cd60ffc6f98976bc41d9789525ff340a0ab39a32c83"

    // JWT and ZkLogin related fields
    private string userSalt = "170837172466338254092654926024599177975";
    private string ephemeralPrivateKey = "5cHJ27eXt/0lsqhfNjXbuR7GOIj3sNHEFj8L7bhSSrM=";
    private string ephemeralPublicKey = "tsLtKW07pGVzYtJa74BU7eksnReZL5jUFxyyFJ/Wwv8=";

    [SerializeField]
    private string jwtSub = "106286931906362609286";
    
    [SerializeField]
    private string jwtAud = "573120070871-0k7ga6ns79ie0jpg1ei6ip5vje2ostt6.apps.googleusercontent.com";

    public async void TransferSuiExample()
    {
        // Create a new transaction block (equivalent to TypeScript's `const txb = new TransactionBlock();`)
        TransactionBlock txb = new TransactionBlock();

        // Define 1 SUI in MIST (1 SUI = 1_000_000_000 MIST)
        BigInteger onesuiInMist = new BigInteger(1_000_000_000);

        // Split coins from gas (equivalent to TypeScript's `txb.splitCoins(txb.gas,[MIST_PER_SUI * 1n]);`)
        TransactionArgument[] splitResult = txb.SplitCoins(
            TransactionArgumentKind.GasCoin,
            new[] { onesuiInMist }
        );

        // Transfer the split coin to the recipient (equivalent to TypeScript's `txb.transferObjects([coin], "0xfa0f...");`)
        txb.TransferObjects(
            new[] { splitResult[0] },
            new AccountAddress(recipientAddress)
        );

        // Set the sender (equivalent to TypeScript's `txb.setSender(zkLoginUserAddress)`)
        txb.SetSender(new AccountAddress(zkLoginUserAddress));

        // Create ephemeral key pair from the provided private key
        Account ephemeralKeyPair = new Account(ephemeralPrivateKey);

        // Generate address seed using userSalt, sub, and aud
        BigInteger addressSeed = Utils.GenAddressSeed(
            BigInteger.Parse(userSalt),
            "sub",
            jwtSub,
            jwtAud
        );

        Debug.Log($"Generated Address Seed: {addressSeed}");

        // TODO: Implement transaction signing with zkLogin
        // This would typically involve:
        // 1. Building the transaction
        // 2. Signing with the ephemeral key pair
        // 3. Getting the zkLogin signature
        // 4. Executing the transaction
    }
}
