using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace MCL.BLS12_381.Net
{
    public static unsafe partial class MCL_Imports
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // WEBGL İÇİN: MclBridge.jslib'deki fonksiyonu çağır.
        [DllImport("__Internal")] public static extern void Unity_mclBn_init(int curve, int compiledTimeVar);
        [DllImport("__Internal")] public static extern void Unity_mclBnFr_clear(Fr* ptr);
        [DllImport("__Internal")] public static extern void Unity_mclBnFr_setInt32(Fr* x, int n);
#else

        // DİĞER PLATFORMLAR İÇİN: MclBls12381 sınıfı üzerinden dinamik yüklemeyi çağır.
        public static int mclBn_init(int curve, int compiledTimeVar) =>
            MclBls12381.Imports.MclBnInit.Value(curve, compiledTimeVar);
#endif
    }
}