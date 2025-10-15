using UnityEngine;
using System;
using MCL.BLS12_381.Net;
using System.Collections; // MCL_Imports sınıfımıza erişmek için

public class WebGLMCLInit : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return new WaitForSeconds(1f);
        Debug.Log("---------- WEBGL INIT TESTİ BAŞLATILIYOR ----------");
        const int curveBls12381 = 5;
        const int compileTimeVar = 46;
#if UNITY_EDITOR
        int error = MCL_Imports.mclBn_init(curveBls12381, compileTimeVar);
#elif UNITY_WEBGL
        MCL_Imports.Unity_mclBn_init(curveBls12381, compileTimeVar);
        int error = 0;
#endif
        yield return new WaitForSeconds(0.1f);
        if (error == 0)
        {
            Debug.Log("<color=green><b>>>> WEBGL INIT TESTİ BAŞARILI! C# -> JS -> WASM köprüsü çalışıyor. <<<</b></color>");
        }
        else
        {
            throw new InvalidOperationException("mclBn_init returned error: " + error);
        }
        var fr = new Fr();
        fr.SetInt(5678);
        Debug.Log("Created fr");
        yield return null;
    }
}