using System.Collections;
using System.Numerics;
using Sui.Accounts;
using Sui.Transactions;
using Sui.Transactions.Block;
using UnityEngine;

public class TransactionBlockExample : MonoBehaviour
{
    [SerializeField]
    private string recipientAddress = "0xfa0f8542f256e669694624aa3ee7bfbde5af54641646a3a05924cf9e329a8a36";

    [SerializeField]
    private string zkLoginUserAddress;

    public void TransferSuiExample()
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

        // At this point, you would typically:
        // 1. Build the transaction
        // 2. Sign it with the sender's keypair
        // 3. Execute it using the SuiClient
        // These steps would be handled by your transaction execution logic
    }
}
