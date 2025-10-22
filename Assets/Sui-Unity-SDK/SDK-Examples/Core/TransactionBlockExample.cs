
using UnityEngine;

public class TransactionBlockExample : MonoBehaviour
{
    [SerializeField]
    private string recipientAddress = "0xfa0f8542f256e669694624aa3ee7bfbde5af54641646a3a05924cf9e329a8a36";

    [SerializeField]
    private string zkLoginUserAddress = "0x129ed8d47e9f0ddbce4d4cd60ffc6f98976bc41d9789525ff340a0ab39a32c83";

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
        
    }
}
