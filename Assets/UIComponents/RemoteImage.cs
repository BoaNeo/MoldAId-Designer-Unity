using UnityEngine;
using UnityEngine.UI;

namespace UIComponents
{
  public class RemoteImage : MonoBehaviour
  {
    public enum ScaleMode
    {
      None, // Do not change the size of the RawImage
      AspectFit, // Resize RawImage to fit inside this rect, but keep aspect ratio
      AspectFill, // Resize RawImage to fill this rect, but keep aspect ratio
	    AspectMatchWidth, // Resize RawImage to match the width of this rect, don't care about the height but maintain aspect ratio
	    AspectMatchHeight, // Resize RawImage to match the hright of this rect, don't are about the width but maintain aspect ratio
    }

    [SerializeField] private ScaleMode _scaleMode = ScaleMode.AspectFill;
    [SerializeField] private Vector2 _resolution = new Vector2(128, 128);
    private string _url;
    private CachedTexture _cachedTexture;
    private RawImage _img;

    private void Awake()
    {
      _img = GetComponentInChildren<RawImage>();
      if (_img == null) {
	      GameObject go = new GameObject("RawImage");
	      go.transform.parent = transform;
	      go.transform.localPosition = Vector3.zero;
	      _img = go.AddComponent<RawImage>();
      }
    }

    public string url
    {
      get => _url;
      set
      {
	      string fixedurl = value==null?null:value.Replace("#format#", $"{_resolution.x}x{_resolution.y}");
	      if (_url==null || fixedurl==null || !fixedurl.Equals(_url))
        {
          _url = fixedurl;
          _img.texture = null;
          _img.gameObject.SetActive(false);
          
          if (_url != null) {
            ResourceCache.instance.LoadAsync(fixedurl, texture =>
            {
              if (_url == fixedurl) // URL May have been changed since the original request was started - in that case, this request is void and we wait for the next one before doing anything at all
              {
                if (_cachedTexture != null)
                  _cachedTexture.Release();
                _cachedTexture = texture;
                _img.texture = texture.texture;
                _img.gameObject.SetActive(true);
                if (texture.texture!=null && _scaleMode != ScaleMode.None)
                {
                  Rect outer = ((RectTransform) transform).rect;
                  RectTransform inner = _img.rectTransform;
                  float aspect = texture.texture.height / (float)texture.texture.width;
                  float rw = outer.width / texture.texture.width;
                  float rh = outer.height / texture.texture.height;
                  float w = outer.width, h = outer.height;
                  switch (_scaleMode) {
	                  case ScaleMode.AspectMatchHeight:
		                  w = h / aspect;
		                  break;
	                  case ScaleMode.AspectMatchWidth:
		                  h = w * aspect;
		                  break;
	                  case ScaleMode.AspectFill:
		                  if (rh > rw) {
			                  h = outer.height;
			                  w = outer.height / aspect;
		                  } else {
			                  w = outer.width;
			                  h = outer.width * aspect;
		                  }
		                  break;
	                  case ScaleMode.AspectFit:
		                  if (rh > rw) {
			                  w = outer.width;
			                  h = outer.width * aspect;
		                  } else {
			                  h = outer.height;
			                  w = outer.height / aspect;
		                  }
		                  break;
                  }
                  inner.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
                  inner.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
                }
              }
              else
              {
                texture.Release();
              }
            });
          }
        }
      }
    }
  }
}