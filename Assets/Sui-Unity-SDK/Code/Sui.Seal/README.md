# SealBridge for Unity WebGL (Sui Seal SDK Integration)

**Author:** viol3  
**Version:** 1.0.5  
**License:** MIT  

SealBridge provides a C# / Unity-side integration with the **Sui Seal encryption system**, enabling developers to securely store and retrieve encrypted data on-chain. It uses the compiled **seal.bundle.js** (from `@mysten/seal` TypeScript SDK) as a native WebGL plugin, allowing hybrid encryption and key server coordination directly within Unity.

---

## ✨ Overview

SealBridge offers Unity developers a fully managed bridge to the **Sui Seal SDK**, built only for WebGL platform for now.  
It manages encryption and decryption flows and constructs properly authenticated Sui Move transactions that reference access policies and threshold key servers.

This SDK is ideal for:
- Storing encrypted user data or assets on the Sui blockchain  
- Gating access using Sui Move-based policy objects  
- Supporting **Enoki zkLogin sessions** and ephemeral authentication  
- Enabling **verifiable, privacy-preserving game state sharing**

---

## 🏗 Architecture

SealBridge communicates with a WebAssembly/JS runtime (`seal.bundle.js`) that performs:
- Data encryption using Seal’s decentralized key management  
- Policy validation and access delegation through Sui Move objects  
- Decryption flow authorization via private key or zkLogin proof  

The Unity C# layer wraps this process with asynchronous task-based APIs and transaction builder utilities from the **OpenDive Sui-Unity-SDK**.

---

## ⚙️ Requirements

| Component | Version |
|------------|----------|
| Unity | 6000.60f1 LTS or newer |
| Platform | WebGL |
| Sui Network | Testnet / Mainnet |
| Seal JS Runtime | `seal.bundle.js` built from `@mysten/seal` v0.9.0+ |
| Sui-Unity-SDK | OpenDive.Sui SDK

---

## 📦 Installation

1. Clone or download this repository's Sui-Unity-SDK folder into your Unity project's `Assets/` folder:

2. Move **WebGLTemplates** folder to Assets folder from Sui-Unity-SDK folder

3. Open "Seal" scene in **Sui-Unity-SDK/Samples/Seal/Scenes** folder and put a testnet private key with testnet coins.

4. In Build Settings, put the Seal scene as main scene.

5. In Player Settings, find Resolution and Presentation tab and under the WebGL Template, choose the Seal template.

6. Start to build as WebGL, wait for web build, after built successful, it will open a localhost website, you can try Encryption/Decryption there.

Note => Ensure you have a valid Sui account and node endpoint configured via:
var client = new SuiClient(Constants.TestnetConnection);
var account = new Account(privateKeyHex);

---

## 🧠 Usage Example

### 1. Initialize SealBridge

<pre><code class="language-csharp">
void Start()
{
	var client = new SuiClient(Constants.TestnetConnection);
	SealBridge.Instance.SetSuiClient(client);
	SealBridge.Instance.SetPackageInformation(
	"YOUR PACKAGE ID",
	"MODULE NAME",
	"FUNCTION NAME");
}
</code></pre>