using System;
using System.Collections.Generic;
using FeatureGraph;
using Gizmos;
using MeshUtil;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using Utility;

namespace Visuals
{
	public class Visual : PooledObject
	{
		public enum SelectionPriority { Primary, Secondary, Ignore }
		public enum Mode { Opaque, Transparent, Overhang }

		[SerializeField] private VisualChild _childPrefab;
		[SerializeField] private Material _solid;
		[SerializeField] private Material _transparent;
		[SerializeField] private Material _overhang;
		[SerializeField] private Material _crossSection;

		private DataRef<MeshBuilder> _source = new ( );
		private DataRef<XForm> _xform = new ( );

		private List<VisualChild> _children = new ();
		private Action _onSelect;
		private Color _color;
		private bool _selected;
		private Action _onDebugDraw;
		private Mode _mode;
		private Material _materialOverride;

		private int _crossSectionPosId;
		private int _crossSectionNormalId;
		public bool isCrossSection => _materialOverride == _crossSection;

		private void Awake()
		{
			_crossSectionPosId = Shader.PropertyToID("_PlanePosition");
			_crossSectionNormalId =  Shader.PropertyToID("_PlaneNormal");
		}

		public override void OnRecycled()
		{
			for (int i = 0; i < _children.Count; i++)
			{
				// TODO: I'm not sure why these are not pooled??
				Destroy(_children[i].gameObject);
			}
			_children.Clear();
		}

		public void SetVisible(bool visible, DataRef<MeshBuilder> mesh, DataRef<XForm> xform, Mode mode, Color color, bool selected = false, Action onSelect = null, Action onDebugDraw=null)
		{
			if (!visible)
			{
				gameObject.SetActive(false);
				return;
			}
			_source.UseDataFrom(mesh);
			if(xform!=null)
				_xform.UseDataFrom(xform);
			_selected = selected;
			_onSelect = onSelect;
			_onDebugDraw = onDebugDraw;
			_mode = mode;
			_color = color;
			try
			{
				gameObject.SetActive(true);
			}
			catch (Exception e)
			{
				Debug.LogError($"Failed to activate object {mesh.blockname}: {e.Message}");
			}
			SetRenderMode(mode,color);
		}
		
		public void SetCrossSection(bool enabled, Vector3 position, Vector3 normal)
		{
			Material m = null;
			if (enabled)
			{
				_crossSection.SetVector( _crossSectionPosId, position);
				_crossSection.SetVector( _crossSectionNormalId, normal);
				m = _crossSection;
			}

			SetMaterialOverride(m);
		}

		private void Update()
		{
			if (_source.changed)
			{
				_source.changed = false;
				SetMesh(_source.value);
			}

			if (_xform.hasData && _xform.changed)
			{
				_xform.changed = false;
				SetMesh(_source.value);
			}

			if(	_children.Count > 0 && _children[0].selected!=_selected)
			{
				foreach (VisualChild c in _children)
					c.selected = _selected;
			}
		}

		private void OnDrawGizmos()
		{
			if (_onDebugDraw != null)
				_onDebugDraw();
		}

		public void SetMesh(MeshBuilder mb)
		{
			if (mb == null)
				return;

			Debug.Log($"Setting visual transform for {_source.blockname} with position {_xform.value.position}");

			Matrix4x4 m = Matrix4x4.identity; //mb.transform;
			if(_xform.hasData)
				m *= _xform.value.localToWorldMatrix;
			Transform t = transform;
			t.position = m.MultiplyPoint(Vector3.zero);
			t.rotation = m.rotation;
			t.localScale = m.lossyScale;

			if(!mb.changed)
				return;

			Debug.Log($"Rebuilding visual mesh for {_source.blockname}");
			
			Mesh.MeshDataArray meshData = Mesh.AllocateWritableMeshData(1);
			// TODO: Shading should/could be done in the background - only creation of the MeshDataArray must be on main thread!
			mb.FlatShade(meshData, true);

			// TODO: This prep for multiple meshes probably doesn't make any sense - should use multiple "Visual" instead.
			Mesh.MeshDataArray[] meshes = { meshData };

			for (int i = transform.childCount; i < meshes.Length; i++)
			{
				VisualChild v = Instantiate(_childPrefab, Vector3.zero, Quaternion.identity, transform);
				v.Init(this);
				_children.Add( v );
			}

			for (int i=0;i<_children.Count;i++)
			{
				VisualChild child = _children[i];
				child.transform.localPosition = Vector3.zero;
				child.transform.localRotation = Quaternion.identity;
				if (i < meshes.Length)
				{
					child.gameObject.SetActive(true);
					Mesh.ApplyAndDisposeWritableMeshData( meshes[i], child.mesh, MeshUpdateFlags.Default );
					child.mesh.bounds = mb.GetBounds();
					child.Refresh();
					child.name = name;
				}
				else
				{
					child.gameObject.SetActive(false);
				}
			}
			
			UpdateChildMaterial();
		}

		private void SetRenderMode(Mode rendermode, Color color)
		{
			_color = color;
			_mode = rendermode;
			UpdateChildMaterial();
		}

		private void UpdateChildMaterial()
		{
			Material m = _materialOverride;
			if (m == null)
			{
				switch (_mode)
				{
					case Mode.Opaque: 
						m = _solid;
						break;
					case Mode.Transparent:
						m = _transparent;
						break;
					case Mode.Overhang:
						m = _overhang;
						break;
				}
			}
			foreach (VisualChild child in _children)
			{
				child.SetMaterial(m, _color);
			}
		}

		public void SetMaterialOverride(Material materialOverride)
		{
			_materialOverride = materialOverride;
			UpdateChildMaterial();
		}

		public bool OnSelect(PointerEventData evt)
		{
			if (evt.button==PointerEventData.InputButton.Left && _onSelect != null)
			{
				_onSelect();
				return true;
			}

			if (evt.button == PointerEventData.InputButton.Right)
			{
				ActionMenu.ActionMenu.ShowMenu(evt,
					("Solid", () => { SetRenderMode(Mode.Opaque, _color); }),
					("Transparent", () => { SetRenderMode(Mode.Transparent, _color); }),
					("Overhang", () => { SetRenderMode(Mode.Overhang, _color); })
					);
			}
			
			return true;
		}
	}
}