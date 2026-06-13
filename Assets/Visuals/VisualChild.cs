using Plugins.QuickOutline.Scripts;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Visuals
{
	public class VisualChild : MonoBehaviour
	{
		[SerializeField] private Outline _outline;

		private MeshCollider _collider;
		private MeshFilter _filter;
		private MeshRenderer _renderer;
		private Visual _parent;
		private MeshFilter _outlineFilter;
		private bool _refreshMaterial;
		private Color _color;
		private Material _material;

		private void Awake()
		{
			_outlineFilter = _outline.GetComponent<MeshFilter>();
			_collider = GetComponent<MeshCollider>();
			_filter = GetComponent<MeshFilter>();
			_renderer = GetComponent<MeshRenderer>();
		}

		public void Init(Visual parent)
		{
			_parent = parent;
			_filter.sharedMesh = new Mesh();
			_outlineFilter.sharedMesh = _filter.mesh;
		}
		
		public bool OnSelect(PointerEventData evt)
		{
			return _parent.OnSelect(evt);
		}

		public Mesh mesh => _filter.mesh;

		public void Refresh()
		{
			_collider.sharedMesh = _filter.sharedMesh;
			_outlineFilter.sharedMesh = _filter.sharedMesh;
		}

		private void Update()
		{
			if (_refreshMaterial)
			{
				_renderer.sharedMaterial = _material;
				_renderer.material.color = _color;
				_refreshMaterial = false;
			}
		}

		public void SetMaterial(Material mat, Color color)
		{
			_material = mat;
			_color = color;
			_refreshMaterial = true;
		}

		public bool selected
		{
			get => _outline.enabled;
			set
			{
//				_outline.gameObject.SetActive(true);
				_outline.enabled = value;
			}
		}
/*
		private void SetRenderMode (Material material,RenderMode blendMode)
    {
      switch (blendMode)
      {
        case RenderMode.Opaque:
	        _renderer.shadowCastingMode = ShadowCastingMode.On;
          material.SetOverrideTag("RenderType", "Opaque");
          material.SetFloat("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
          material.SetFloat("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
          material.SetFloat("_ZWrite", 1f);
          material.DisableKeyword("_ALPHATEST_ON");
          material.DisableKeyword("_ALPHABLEND_ON");
          material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
          material.renderQueue = 2000;
          break;
        case RenderMode.Cutout:
	        _renderer.shadowCastingMode = ShadowCastingMode.On;
          material.SetOverrideTag("RenderType", "TransparentCutout");
          material.SetFloat("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
          material.SetFloat("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
          material.SetFloat("_ZWrite", 1f);
          material.EnableKeyword("_ALPHATEST_ON");
          material.DisableKeyword("_ALPHABLEND_ON");
          material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
          material.renderQueue = 2450;
          break;
        case RenderMode.Fade:
	        _renderer.shadowCastingMode = ShadowCastingMode.Off;
          material.SetOverrideTag("RenderType", "Transparent");
          material.SetFloat("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
          material.SetFloat("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
          material.SetFloat("_ZWrite", 0.0f);
          material.DisableKeyword("_ALPHATEST_ON");
          material.EnableKeyword("_ALPHABLEND_ON");
          material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
          material.renderQueue = 2499;
          break;
        case RenderMode.Transparent:
	        _renderer.shadowCastingMode = ShadowCastingMode.Off;
          material.SetOverrideTag("RenderType", "Transparent");
          material.SetFloat("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
          material.SetFloat("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
          material.SetFloat("_ZWrite", 0.0f);
          material.DisableKeyword("_ALPHATEST_ON");
          material.DisableKeyword("_ALPHABLEND_ON");
          material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
          material.renderQueue = 3000;
          break;
      }
    }
      */
	}
}