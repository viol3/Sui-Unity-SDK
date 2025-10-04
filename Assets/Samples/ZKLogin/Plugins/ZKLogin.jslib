mergeInto(LibraryManager.library, 
{

  OpenURL : function(url)
  {
	url = UTF8ToString(url);
	window.open(url,'_blank');
  },
  
  GoogleLogin : function(nonce)
  {
	nonce = UTF8ToString(nonce);
	window.googleLogin(nonce);
  },


});