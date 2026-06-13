using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Utility;

namespace UIComponents
{
	public class ResourceLoader : MonoBehaviour
	{
		public static ResourceLoader instance => Singleton<ResourceLoader>.Instance;

		public void LoadTexture(string url, Action<Texture2D> when_loaded)
		{
			if (!string.IsNullOrEmpty(url))
			{
				if (url.Contains("https:"))
					StartCoroutine(GetTextureForURL(url, when_loaded));
				else
					StartCoroutine(GetTextureForFile(url, when_loaded));
			}
			else
				when_loaded(null);
		}
		
		private IEnumerator GetTextureForURL(string url, Action<Texture2D> when_loaded) {
			using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
			{
				yield return www.SendWebRequest();

				try
				{
					if (www.responseCode==200 && www.result!=UnityWebRequest.Result.ConnectionError)
					{
						var tex = DownloadHandlerTexture.GetContent(www);
						if (tex != null)
						{
							Debug.Log($"Downloaded texture from {url}");
							when_loaded(tex);
							yield break;
						}
					}
					else
						Debug.Log("GetTextureFromURL "+url+" failed with " + www.responseCode+":"+www.error);
				}
				catch (Exception e)
				{
					Debug.Log("GetTextureFromURL "+url+" failed with Exception: "+e);
				}
				when_loaded(null);
			}			
		}		
		
		private IEnumerator GetTextureForFile(string path, Action<Texture2D> when_loaded)
		{
			var tex = new Texture2D(16,16,TextureFormat.RGB24,false,true);
			try
			{
				byte[] file = File.ReadAllBytes(path);
				tex.LoadImage(file);
			}
			catch (Exception e)
			{
				Debug.Log("Failed to load image from path "+path+" : "+e);
			}
			when_loaded(tex);
			yield break;
		}
	}
}