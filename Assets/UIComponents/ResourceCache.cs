using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using UnityEngine;
using Utility;

namespace UIComponents
{
  public class CachedTexture
  {
    internal string _uri;
    internal float _releasedTime;
    public Texture2D texture { get; set; }
    public int refCount {      get { return _refCount;  }    }

    private int _refCount;
    private event Action<CachedTexture> _waiters;
    
    public CachedTexture(string uri, Action<CachedTexture> whenLoaded)
    {
      _uri = uri;
      Allocate(whenLoaded);
    }

    public void Release()
    {
      _refCount--;
      _releasedTime = Time.time;
    }

    public void Allocate(Action<CachedTexture> whenDone)
    {
      _refCount++;
      if (texture != null)
        whenDone(this);
      else
        _waiters += whenDone;
    }

    public void WhenLoaded(Texture2D texture2D)
    {
      texture = texture2D;
      texture.Compress(false);
      if(_waiters!=null)
        _waiters.Invoke(this);
      _waiters = null;
    }
  }
  
  public class ResourceCache : MonoBehaviour
  {
    public float _pruneRate = 5.0f;

    private Dictionary<string, CachedTexture> _cache = new Dictionary<string, CachedTexture>();
    private float _time;
    private bool _flush;

    public static ResourceCache instance { get { return Singleton<ResourceCache>.Instance;  }}

    public void LoadAsync(string uri, Action<CachedTexture> whenDone)
    {
      uri = uri.Trim();
      CachedTexture tex;
      lock (_cache)
      {
        _cache.TryGetValue(uri, out tex);
      }
      if(tex!=null)
      {
        tex.Allocate(whenDone);
      }
      else
      {
        string cached_file = null;
        tex = new CachedTexture(uri,whenDone);
        lock (_cache)
        {
          _cache[uri] = tex;
        }
        if (uri.StartsWith("http"))
        {
          string rootpath = Application.persistentDataPath + "/ImageCache";
          if (!Directory.Exists(rootpath))
            Directory.CreateDirectory(rootpath);
          cached_file = rootpath + "/" + GetSha256Hash(uri);
          if (File.Exists(cached_file))
          {
            Debug.Log("Replacing remote resource "+uri+" with previously persisted "+cached_file);
            uri = cached_file;
            cached_file = null;
          }
        }
        ResourceLoader.instance.LoadTexture( uri, d =>
        {
          if (d != null)
          {
            if (cached_file!=null)
            {
              Debug.Log("Persisting remote resource "+uri+" to "+cached_file);
              File.WriteAllBytes(cached_file, d.EncodeToJPG());
            }
            tex.WhenLoaded(d);
          }
        });
      }
    }

    static SHA256 _sha256 = SHA256.Create();
    static string GetSha256Hash(string input)
    {
	    // Convert the input string to a byte array and compute the hash.
	    byte[] data = _sha256.ComputeHash(Encoding.UTF8.GetBytes(input));

	    // Create a new Stringbuilder to collect the bytes
	    // and create a string.
	    StringBuilder sBuilder = new StringBuilder();

	    // Loop through each byte of the hashed data 
	    // and format each one as a hexadecimal string.
	    for (int i = 0; i < data.Length; i++)
	    {
		    sBuilder.Append(data[i].ToString("x2"));
	    }

	    // Return the hexadecimal string.
	    return sBuilder.ToString();
    }

    public void Unload(CachedTexture cachedTexture)
    {
      Debug.Log("Unloading cached texture: "+cachedTexture._uri);
      lock (_cache)
      {
        _cache.Remove(cachedTexture._uri);
      }
    }

    private void Update()
    {
      _time = Time.time;
      if (_flush)
      {
        Resources.UnloadUnusedAssets();
        GC.Collect();
        _flush = false;
      }
    }

    private void Awake()
    {
      new Thread( Prune ).Start();
    }

    private void Prune()
    {
      while (true)
      {
        List<CachedTexture> currentlist;
        lock (_cache)
        {
          currentlist = new List<CachedTexture>(_cache.Values);
        }
        foreach (CachedTexture cachedTexture in currentlist)
        {
          if (cachedTexture.refCount <= 0 && (_time - cachedTexture._releasedTime) > _pruneRate) // Unload cached textures 5 seconds after they're last referenced
          {
            _flush = true;
            Unload(cachedTexture);
          }
        }
        Thread.Sleep((int) (1000*_pruneRate));
      }
    }
  }
}