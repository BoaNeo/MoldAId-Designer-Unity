using System;
using System.Collections.Generic;
using FeatureGraph;
using UnityEngine;
using UnityEngine.EventSystems;
using Utility;

namespace Gizmos
{
	public class GizmoManager : MonoBehaviour, WorldInputManager.IWorldInputHandler
	{
		[SerializeField] private LayerMask _gizmoLayer;
		[SerializeField] private Gizmo _gizmoPrefab;

		private Dictionary<string,Gizmo> _gizmos = new Dictionary<string, Gizmo>();
		private GizmoHandle _currentHandle;
		private Camera _camera;

		public bool hasActiveHandle => _currentHandle != null;

		private void Awake()
		{
			_camera = Camera.main;
		}
		
		public void HideAllGizmos()
		{
			foreach (Gizmo gv in _gizmos.Values)
				gv.Teardown();
		}

		public Gizmo SetGizmo(bool visible, DataRef<XForm> xformref, GizmoHandleFlags handles, GizmoSpace space, Action<XForm, bool> feedback=null)
		{
			if (xformref == null)
				return null;

			if (!_gizmos.TryGetValue(xformref.blockname, out Gizmo gv))
			{
				gv = ObjectPool.Instantiate(_gizmoPrefab, xformref.value.position, xformref.value.rotation);
				_gizmos[xformref.blockname] = gv;
			}

			gv.isVisible = visible;
			gv.Setup( xformref, handles, space, feedback);

			return gv;
		}

		public LayerMask GetSelectableLayerMask()
		{
			return _gizmoLayer;
		}

		public string[] GetTagPriorities()
		{
			return null;
		}

		public bool GetNeedsHoverEvent()
		{
			return true;
		}

		public bool OnHover(RaycastHit hit)
		{
			GameObject obj = hit.collider?.gameObject;
			if (obj != null)
			{
				GizmoHandle handle = obj.GetComponent<GizmoHandle>();
				if (handle != null)
				{
					handle.OnHover();
					return true;
				}
			}
			return false;
		}

		public bool OnSelect(RaycastHit hit, PointerEventData evt)
		{
			GameObject obj = hit.collider?.gameObject;
			if (obj == null)
				_currentHandle = null;
			else
				_currentHandle = obj.GetComponent<GizmoHandle>();
			if(_currentHandle)
				_currentHandle.Activate();
			return _currentHandle != null;
		}

		public void OnDrag(RaycastHit hit, Vector2 deltaScreen)
		{
			if (_currentHandle)
			{
				_currentHandle.Drag(deltaScreen);
			}
		}

		public bool OnRelease(RaycastHit source, RaycastHit target)
		{
			if (_currentHandle)
			{
				_currentHandle.Deactivate();
				_currentHandle = null;
				return true;
			}
			return false;
		}
	}
}