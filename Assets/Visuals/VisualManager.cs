using System;
using System.Collections.Generic;
using FeatureGraph;
using Gizmos;
using MeshUtil;
using UnityEngine;
using Utility;

namespace Visuals
{
	public class VisualManager : MonoBehaviour
	{
		[SerializeField] private Visual _visualPrefab;

		private Dictionary<string, Visual> _visuals = new Dictionary<string, Visual>();

		public void Clear()
		{
			foreach(Visual v in _visuals.Values)
				ObjectPool.Recycle(v);
			_visuals.Clear();
		}
		
		public void HideAll()
		{
			foreach(Visual v in _visuals.Values)
				v.SetVisible(false, null, null, Visual.Mode.Opaque,default);
		}
		
		public Visual SetVisible(bool visible, DataRef<MeshBuilder> mesh, DataRef<XForm> xform, Visual.Mode mode, Color color, bool selected, Action onSelect, Action onDebugDraw)
		{
			if (!_visuals.TryGetValue(mesh.blockname, out Visual v))
			{
				v = ObjectPool.Instantiate(_visualPrefab);
				_visuals[mesh.blockname] = v;
				v.name = mesh.blockname;
			}
			v.SetVisible(visible, mesh, xform, mode, color, selected, onSelect, onDebugDraw);
			return v;
		}
	}
}