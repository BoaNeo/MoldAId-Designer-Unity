using UnityEngine;

namespace Gizmos
{
	public class GizmoHandle : MonoBehaviour
	{
		[SerializeField] private MeshRenderer _renderer;
		[SerializeField] private Material _normal;
		[SerializeField] private Material _selected;

		public enum DragMode { ForwardVector, Screen, Rotation}
		public DragMode dragMode { get; set; }

		private Gizmo gizmo
		{
			get
			{
				if (_gizmo == null)
					_gizmo = GetComponentInParent<Gizmo>();
				return _gizmo;
			}
		}
		private Gizmo _gizmo;
		private bool _hasHover;
		private bool _isActive;
		private Vector2 _rotationDirection;

		public void Drag(Vector2 deltaScreen)
		{
			Camera camera = Camera.main;
			if (camera == null)
				return;

			Transform t = transform;
			Vector3 from = t.position;
			Vector3 fwd = t.forward;

			switch (dragMode)
			{
				case DragMode.ForwardVector:
				{
					Vector2 hp0 = camera.WorldToScreenPoint(from);
					Vector2 hp1 = camera.WorldToScreenPoint(from + fwd);

					Vector2 hd = (hp1 - hp0);

					if (hd.sqrMagnitude > 0)
					{
						hd /= hd.sqrMagnitude;

						float signedDistance = hd.x * deltaScreen.x + hd.y * deltaScreen.y;
						gizmo.Drag(fwd * signedDistance, true);
					}

					break;
				}
				case DragMode.Screen:
				{
					Vector2 hp0 = camera.WorldToScreenPoint(from);
					Vector2 hp1 = hp0 + deltaScreen;

					Vector3 vp0 = camera.ScreenToViewportPoint(hp0);
					Vector3 vp1 = camera.ScreenToViewportPoint(hp1);

					Vector3 delta = camera.ViewportToWorldPoint(vp1) - camera.ViewportToWorldPoint(vp0);
				
					gizmo.Drag( delta, true);
					break;
				}
				case DragMode.Rotation:
				{
					float amountX = 720.0f * Mathf.Abs(deltaScreen.x) / Screen.currentResolution.width;
					float amountY = 720.0f * Mathf.Abs(deltaScreen.y) / Screen.currentResolution.height;

					float dirX = deltaScreen.x > 0 ? -_rotationDirection.x : _rotationDirection.x;
					float dirY = deltaScreen.y < 0 ? -_rotationDirection.y : _rotationDirection.y;

					Quaternion q = Quaternion.AngleAxis(amountX*dirX+amountY*dirY, _renderer.transform.forward);
					gizmo.Rotate(q, true);

					break;
				}
			}
		}

		public void Activate()
		{
			_isActive = true;
			_hasHover = true;
			EstablishRotationDirection();
		}

		private void EstablishRotationDirection()
		{
			Transform t = transform;
			Camera camera = Camera.main;
			Vector2 mouse = Input.mousePosition;
			Vector2 center = camera.WorldToScreenPoint(t.position);

			float farNear = Vector3.Dot(_renderer.transform.forward, camera.transform.forward);
			farNear = farNear < 0 ? -1 : 1;

			_rotationDirection.y = farNear * (mouse.x<center.x ? -1 : 1);
			_rotationDirection.x = farNear * (mouse.y<center.y ? -1 : 1);

			Debug.Log($"RotationDirection={_rotationDirection}");
		}

		public void Deactivate()
		{
			gizmo.Drag(Vector3.zero, false);
			_isActive = false;
			_hasHover = false;
		}

		public void OnHover()
		{
			_hasHover = true;
			gizmo.OnHover();
		}
		
		public void SetVisibility(float hover)
		{
			if ( _isActive || _hasHover )
			{
				_renderer.material = _selected;
				hover = 1.0f;
			}
			else
				_renderer.material = _normal;
			Material m = _renderer.material;
			Color c = m.color;
			c.a = hover;
			m.color = c;
			_hasHover = _isActive;
		}
	}
}