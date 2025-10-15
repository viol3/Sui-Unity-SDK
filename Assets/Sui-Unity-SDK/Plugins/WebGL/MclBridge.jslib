

var g_mclModule = null;

var MclJsBridge = {
    // C#'tan gelen 'MclJs_mclBn_init' çağrısını,
    // doğrudan mcl_c.js içindeki '_mclBn_init' fonksiyonuna yönlendiriyoruz.
    // Bu işlem anında (senkron) gerçekleşir.
    Unity_mclBn_init: async function(curve, compiledTimeVar) 
	{
		g_mclModule = await window.Module();
		g_mclModule._mclBn_init(curve, compiledTimeVar);
		console.log("inited mcl success")
        //return Module._mclBn_init(curve, compiledTimeVar);
    },
	
	Unity_mclBnFr_clear: function(frPtr) 
	{
		g_mclModule._mclBnFr_clear(frPtr);
		console.log("mclBnFr_clear success")
        //return Module._mclBn_init(curve, compiledTimeVar);
    },
	
	Unity_mclBnFr_setInt32: function(frPtr, newValue) 
	{
        g_mclModule._mclBnFr_setInt32(frPtr, newValue);
		console.log("mclBnFr_setInt32 success => " + newValue)
    },

    // Diğer fonksiyonlar da aynı basitlikle eklenecek:
    // MclJs_mclBnFr_clear: function(p) { _mclBnFr_clear(p); },
};


mergeInto(LibraryManager.library, MclJsBridge);