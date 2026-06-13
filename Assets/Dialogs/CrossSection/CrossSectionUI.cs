using FeatureGraph;
using Gizmos;
using UIComponents;
using UnityEngine;
using Visuals;

namespace Dialogs.CrossSection
{
	public class CrossSectionUI : Dialog
	{
		[SerializeField] private Gizmo _gizmoPrefab;
		[SerializeField] private InputLineUI _lineUI;
		[SerializeField] private Transition _buttonUI;

		[SerializeField] private GameObject _gizmoPanel;
		[SerializeField] private GameObject _gizmoPlaneOn;
		[SerializeField] private GameObject _gizmoPartOn;

		private DataRef<XForm> _plane = new();
		private Visual[] _visuals;
		private Gizmo _planeGizmo;
		private Gizmo[] _partGizmos;
		private bool _showingCrossSection;

		public bool showCrossSection
		{
			get => _showingCrossSection;
			set { _showingCrossSection = value; RefreshCrossSection(); }
		}

		public bool showCrossSectionGizmo
		{
			get => _planeGizmo.isVisible;
			set
			{
				_planeGizmo.isVisible = value;
				foreach(Gizmo g in _partGizmos)
					g.isVisible = !value;
				_gizmoPartOn.SetActive( !value );
				_gizmoPlaneOn.SetActive( value );
			}
		}

		private void RefreshCrossSection()
		{
			foreach (Visual visual in _visuals)
			{
				visual.SetCrossSection(_showingCrossSection, _plane.value.position, _plane.value.forward);
			}

			if (!_showingCrossSection)
				showCrossSectionGizmo = false;
		}

		private void Awake()
		{
			_lineUI.gameObject.SetActive(false);
			_planeGizmo = Instantiate(_gizmoPrefab);
			_plane.NewDataBlock("CrossSection");
		}

		private void Update()
		{
			if (_plane.changed && showCrossSection)
			{
				RefreshCrossSection();
				_plane.changed = false;
			}
		}

		public void WithVisuals(Visual[] v, Gizmo[] g)
		{
			if (v == null || v.Length == 0)
			{
				OnClose();
				return;
			}

			_visuals = v;
			_partGizmos = g;
			_gizmoPanel.SetActive(g.Length>0);
			OnDefinePlane();
		}

		public void OnClose()
		{
			showCrossSection = false;
			Hide();
		}
		public void OnFlipPlane()
		{
			XForm current = _plane.value;
			_plane.Set(current.WithRotation( Quaternion.LookRotation(-current.forward)));
			RefreshCrossSection();
		}

		public void OnAlignX()
		{
			XForm current = _plane.value;
			_plane.Set(current.WithRotation( Quaternion.LookRotation(Vector3.left)));
			RefreshCrossSection();
		}

		public void OnAlignY()
		{
			XForm current = _plane.value;
			_plane.Set(current.WithRotation( Quaternion.LookRotation(Vector3.up)));
			RefreshCrossSection();
		}

		public void OnAlignZ()
		{
			XForm current = _plane.value;
			_plane.Set(current.WithRotation( Quaternion.LookRotation(Vector3.forward)));
			RefreshCrossSection();
		}

		public void OnActivatePlaneGizmo(bool active)
		{
			showCrossSectionGizmo = active;
		}

		public void OnDefinePlane()
		{
			showCrossSection = false;
			_buttonUI.Hide();
			_lineUI.GetLine("Use mouse to draw a line where the cross section will be placed", (Vector2 from, Vector3 to) =>
			{
				Ray r1 = Camera.main.ScreenPointToRay(@from);
				Ray r2 = Camera.main.ScreenPointToRay(@to);

				Vector3 n = Vector3.Cross(r1.origin+r1.direction - (r2.origin+r2.direction), r1.direction).normalized;

				Vector3 c = Vector3.zero;
				foreach (Visual visual in _visuals)
				{
					c += visual.transform.position;
				}
				c /= _visuals.Length;
				Vector3 a = c - r1.origin;
				Vector3 ap = r1.origin + a - Vector3.Dot(a, n) * n;
				
				_plane.Set(new XForm( ap, Quaternion.LookRotation(n)));
				_planeGizmo.Setup( _plane, GizmoHandleFlags.MoveZ, GizmoSpace.Local);
				showCrossSection = true;
				showCrossSectionGizmo = true;
				_buttonUI.Show();
			});
		}
	}
}