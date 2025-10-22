using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using OpenDive.BCS;
using Sui.Cryptography;
using Sui.ZKLogin;
using UnityEngine;

namespace Sui.Tests.ZkLogin
{
    [TestFixture]
    public class ZKLoginTest : MonoBehaviour
    {
        [Test]
        public void JwtToAddress_1()
        {
            string jwt = "eyJhbGciOiJSUzI1NiIsImtpZCI6Ijg5Y2UzNTk4YzQ3M2FmMWJkYTRiZmY5NWU2Yzg3MzY0NTAyMDZmYmEiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJodHRwczovL2FjY291bnRzLmdvb2dsZS5jb20iLCJhenAiOiI1NzMxMjAwNzA4NzEtMGs3Z2E2bnM3OWllMGpwZzFlaTZpcDV2amUyb3N0dDYuYXBwcy5nb29nbGV1c2VyY29udGVudC5jb20iLCJhdWQiOiI1NzMxMjAwNzA4NzEtMGs3Z2E2bnM3OWllMGpwZzFlaTZpcDV2amUyb3N0dDYuYXBwcy5nb29nbGV1c2VyY29udGVudC5jb20iLCJzdWIiOiIxMDYyODY5MzE5MDYzNjI2MDkyODYiLCJub25jZSI6InhzYnA0XzRZY3RPdkYxSzNuRVFORlhaYXlIayIsIm5iZiI6MTczNjU1NjE2MCwiaWF0IjoxNzM2NTU2NDYwLCJleHAiOjE3MzY1NjAwNjAsImp0aSI6ImU1NDliMWYxYjAzOWJhMWVmZmNlMjQxNTJiM2E0ODBmM2U0ZWY4OTAifQ.Lsou_uz3puLZxVs4mLRxn5RNpGfSVHMi7u5EX1jcEcVVq0v1SD8y03gnhkgNqBKozgU8H9WoGB9pYtWIRB7yxtWhipPJbbMlKmYAborsk_2GTASqy_hDJpYPOfLGHmFgZp4wXzcoi87PFWMBCJu1uzAN4zlZWUAv3-H6YV16TSkx2t-pjtzvfXtK0SJIfGhN5UgmgYHVbOvwTjpPI1ah7bm1aRZzo4DAWtBk9U6fcpHoLm6vSqjSabnvpmktdRzQK4Hk2uzCsfCSsAR0mf2q4WO0vmy_xlZpkjFVv3M3pBA63MuciMxCclXJXxx0RMx9D0TMWvO_-F-XHbKGlI_FXA";
            string userSalt = "254046730921541395157831213406089663029";
            string address = ZKLogin.SDK.Address.JwtToAddress(jwt, userSalt);

            string expected = "0x7a9545d3633d05df805b5b2d7821a2e28ba21765a7b5b8255d22d9857190ca89";
            Assert.AreEqual(expected, address);
        }

        [Test]
        public void ZkLoginSignatureSerialization_Test()
        {
            //string anEphemeralSignature = "AEp+O5GEAF/5tKNDdWBObNf/1uIrbOJmE+xpnlBD2Vikqhbd0zLrQ2NJyquYXp4KrvWUOl7Hso+OK0eiV97ffwucM8VdtG2hjf/RUGNO5JNUH+D/gHtE9sHe6ZEnxwZL7g==";
            //string aSignature =
            //    "BQNNMTE3MDE4NjY4MTI3MDQ1MTcyMTM5MTQ2MTI3OTg2NzQ3NDg2NTc3NTU1NjY1ODY1OTc0MzQ4MTA5NDEyNDA0ODMzNDY3NjkzNjkyNjdNMTQxMjA0Mzg5OTgwNjM2OTIyOTczODYyNDk3NTQyMzA5NzI3MTUxNTM4NzY1Mzc1MzAxNjg4ODM5ODE1MTM1ODQ1ODYxNzIxOTU4NDEBMQMCTDE4Njc0NTQ1MDE2MDI1ODM4NDg4NTI3ODc3ODI3NjE5OTY1NjAxNzAxMTgyNDkyOTk1MDcwMTQ5OTkyMzA4ODY4NTI1NTY5OTgyNzNNMTQ0NjY0MTk2OTg2NzkxMTYzMTM0NzUyMTA2NTQ1NjI5NDkxMjgzNDk1OTcxMDE3NjkyNTY5NTkwMTAwMDMxODg4ODYwOTEwODAzMTACTTExMDcyOTU0NTYyOTI0NTg4NDk2MTQ4NjMyNDc0MDc4NDMyNDA2NjMzMjg4OTQ4MjU2NzE4ODA5NzE0ODYxOTg2MTE5MzAzNTI5NzYwTTE5NzkwNTE2MDEwNzg0OTM1MTAwMTUwNjE0OTg5MDk3OTA4MjMzODk5NzE4NjQ1NTM2MTMwNzI3NzczNzEzNDA3NjExMTYxMzY4MDQ2AgExATADTTEwNDIzMjg5MDUxODUzMDMzOTE1MzgwODEwNTE2MTMwMjA1NzQ3MTgyODY3NTk2NDU3MTM5OTk5MTc2NzE0NDc2NDE1MTQ4Mzc2MzUwTTIxNzg1NzE5Njk1ODQ4MDEzOTA4MDYxNDkyOTg5NzY1Nzc3Nzg4MTQyMjU1ODk3OTg2MzAwMjQxNTYxMjgwMTk2NzQ1MTc0OTM0NDU3ATExeUpwYzNNaU9pSm9kSFJ3Y3pvdkwyRmpZMjkxYm5SekxtZHZiMmRzWlM1amIyMGlMQwFmZXlKaGJHY2lPaUpTVXpJMU5pSXNJbXRwWkNJNkltSTVZV00yTURGa01UTXhabVEwWm1aa05UVTJabVl3TXpKaFlXSXhPRGc0T0RCalpHVXpZamtpTENKMGVYQWlPaUpLVjFRaWZRTTEzMzIyODk3OTMwMTYzMjE4NTMyMjY2NDMwNDA5NTEwMzk0MzE2OTg1Mjc0NzY5MTI1NjY3MjkwNjAwMzIxNTY0MjU5NDY2NTExNzExrgAAAAAAAABhAEp+O5GEAF/5tKNDdWBObNf/1uIrbOJmE+xpnlBD2Vikqhbd0zLrQ2NJyquYXp4KrvWUOl7Hso+OK0eiV97ffwucM8VdtG2hjf/RUGNO5JNUH+D/gHtE9sHe6ZEnxwZL7g==";

            //Inputs inputs = new Inputs();
            //inputs.AddressSeed = "13322897930163218532266430409510394316985274769125667290600321564259466511711";
            //inputs.HeaderBase64 = "eyJhbGciOiJSUzI1NiIsImtpZCI6ImI5YWM2MDFkMTMxZmQ0ZmZkNTU2ZmYwMzJhYWIxODg4ODBjZGUzYjkiLCJ0eXAiOiJKV1QifQ";

            //ZkLoginSignatureInputsClaim isBase64Details = new ZkLoginSignatureInputsClaim();
            //isBase64Details.IndexMod4 = 1;
            //isBase64Details.Value = "yJpc3MiOiJodHRwczovL2FjY291bnRzLmdvb2dsZS5jb20iLC";

            //ProofPoints proofPoints = new ProofPoints();
            //proofPoints.a = new Sequence(new string[] {
            //    "11701866812704517213914612798674748657755566586597434810941240483346769369267",
            //    "14120438998063692297386249754230972715153876537530168883981513584586172195841",
            //    "1" }.ToList().Select(str => new BString(str)).ToArray());
            //proofPoints.B = new Sequence(new Sequence[] {
            //    new Sequence(new string[] {
            //        "1867454501602583848852787782761996560170118249299507014999230886852556998273",
            //        "14466419698679116313475210654562949128349597101769256959010003188886091080310",
            //    }.ToList().Select(str => new BString(str)).ToArray()),
            //    new Sequence(new string[] {
            //        "11072954562924588496148632474078432406633288948256718809714861986119303529760",
            //        "19790516010784935100150614989097908233899718645536130727773713407611161368046",
            //    }.ToList().Select(str => new BString(str)).ToArray()),
            //    new Sequence(new string[] {
            //        "1",
            //        "0",
            //    }.ToList().Select(str => new BString(str)).ToArray())
            //});
            //proofPoints.C = new Sequence(new string[] {
            //    "10423289051853033915380810516130205747182867596457139999176714476415148376350",
            //    "21785719695848013908061492989765777788142255897986300241561280196745174934457",
            //    "1" }.ToList().Select(str => new BString(str)).ToArray());

            //inputs.IssBase64Details = isBase64Details;
            //inputs.ProofPoints = proofPoints;


            //ZkLoginSignature sig = new ZkLoginSignature();
            //sig.Inputs = inputs;
            //sig.MaxEpoch = 174;
            //sig.UserSignature = SignatureBase.FromBase64(anEphemeralSignature);

            //string zkLoginSig = ZkLoginSignature.GetZkLoginSignature(
            //    inputs,
            //    174,
            //    SignatureBase.FromBase64(anEphemeralSignature)
            //);

            //Assert.AreEqual(aSignature, zkLoginSig, zkLoginSig);
        }

        [Test]
        public void DeserializeZKProof()
        {
            //const string _json = @"
            //{
            //  ""proofPoints"": {
            //    ""a"": [
            //      ""4454772426092695461025526140941405399454899312669366419304644685541372991"",
            //      ""8984360732286567349467607604973692590498659599949043946802050756343996655257"",
            //      ""1""
            //    ],
            //    ""b"": [
            //      [
            //        ""5939285442433252238643447962718695316515939493003067067660140419052394067374"",
            //        ""10968346574805666769486235627786455340980429660905317386693376120772062842737""
            //      ],
            //      [
            //        ""7437397859167857539066360925390204696863004950945870991330674766958779783813"",
            //        ""12539115227231208276932613867571779044478503094001957273918169691699545769778""
            //      ],
            //      [
            //        ""1"",
            //        ""0""
            //      ]
            //    ],
            //    ""c"": [
            //      ""1673409057821404283328916517110901257722439392303041984587615329246021137745"",
            //      ""18444563522308523770311478643556861027404946545454450865686689352122468253916"",
            //      ""1""
            //    ]
            //  },
            //  ""issBase64Details"": {
            //    ""value"": ""yJpc3MiOiJodHRwczovL2FjY291bnRzLmdvb2dsZS5jb20iLC"",
            //    ""indexMod4"": 1
            //  },
            //  ""headerBase64"": ""eyJhbGciOiJSUzI1NiIsImtpZCI6IjY2MGVmM2I5Nzg0YmRmNTZlYmU4NTlmNTc3ZjdmYjJlOGMxY2VmZmIiLCJ0eXAiOiJKV1QifQ""
            //}";

            //ZKProof proof = JsonConvert.DeserializeObject<ZKProof>(_json);

            //Assert.IsNotNull(proof, "Deserialization failed, root is null");

            //Assert.AreEqual(
            //    "eyJhbGciOiJSUzI1NiIsImtpZCI6IjY2MGVmM2I5Nzg0YmRmNTZlYmU4NTlmNTc3ZjdmYjJlOGMxY2VmZmIiLCJ0eXAiOiJKV1QifQ",
            //    proof.HeaderBase64);

            //Assert.IsNotNull(proof.IssBase64Details);

            //Assert.AreEqual("yJpc3MiOiJodHRwczovL2FjY291bnRzLmdvb2dsZS5jb20iLC", proof.IssBase64Details.Value);

            //Assert.AreEqual(1, proof.IssBase64Details.IndexMod4);

            //Sequence proofPointsA = proof.ProofPoints.A;
            //Assert.AreEqual(3, proofPointsA.Length);
            //Assert.AreEqual("4454772426092695461025526140941405399454899318912669366419304644685541372991", proofPointsA[0]);
            //Assert.AreEqual("8984360732286567349467607604973692590498659599949043946802050756343996655257", proofPointsA[1]);
            //Assert.AreEqual("1", proofPointsA[2]);
        }
    }
}