using Files;
using UnityEngine;
using UnityEngine.EventSystems;
using Utility;

public class CameraManager : MonoBehaviour, WorldInputManager.IWorldInputHandler
{
	[SerializeField] private Transform _pivot;
	[SerializeField] private RectTransform _uiView;
	[SerializeField] private Color _lightBackground;
	[SerializeField] private Color _darkBackground;

	private Camera _camera;
	private Vector3 _euler;
	private float _zoom;
	private Bounds _bounds;

	private float _leftMargin;
	private Ray _mouseRay;
	private Vector3 _mousePos;
	private bool _rotateModifier;
	private Vector3 _targetPosition;
	private Quaternion _targetRotation;

	public static CameraManager instance => Singleton<CameraManager>.Instance;

	private void Awake()
	{
		_camera = GetComponentInChildren<Camera>(true);
		_euler = new Vector3(45,45, 0);
		_targetRotation = Quaternion.Euler( _euler);
		_targetPosition = _pivot.position;
			
		OnAdjustZoom(0.5f);
	}

	private void Update()
	{
		float margin = _uiView.anchoredPosition.x + _uiView.rect.width;
		_leftMargin = Mathf.Lerp(_leftMargin, margin, 5* Time.deltaTime);

		Vector3 mp = Input.mousePosition;
		Vector3 md = 0.2f* (mp - _mousePos);
		float t = Mathf.Clamp(Time.deltaTime * 10, 0, 1);

		if (Input.GetMouseButton(0) || Input.GetMouseButton(2))
		{
			if (WorldInputManager.panModifier)
			{
				_targetPosition -= _camera.transform.right * md.x + _camera.transform.up * md.y;
				t *= 10;
			}
			else if( WorldInputManager.rotateModifier || _rotateModifier)
			{
				_euler.x -= md.y;
				_euler.y += md.x;
				_targetRotation = Quaternion.Euler( _euler);
				t *= 10;
			}
		}
		_mousePos = mp;

		if (PreferencesFile.current == null)
			return;

		if (WorldInputManager.instance.hasFocus)
		{
			float scale = Mathf.Clamp(_zoom ,0.1f, 0.5f);
			scale = _camera.orthographic ? 0.5f*scale : scale;
			SetZoom( _zoom + scale * Mathf.Clamp(-Input.mouseScrollDelta.y,-scale* PreferencesFile.current.mouseWheelSensitivity,scale* PreferencesFile.current.mouseWheelSensitivity), Input.mousePosition  );
		}

		_pivot.position = Vector3.Lerp(_pivot.position,_targetPosition, t) ;
		_pivot.localRotation = Quaternion.Lerp( _pivot.localRotation, _targetRotation, t);

		_camera.backgroundColor = PreferencesFile.current.skin == 0 ? _lightBackground : _darkBackground;
	}

	public void FocusOnBoundingBox(Bounds bounds)
	{
		if (bounds.size.magnitude < 100)
			bounds = new Bounds(bounds.center, 100 * Vector3.one);
		_bounds = bounds;
		_targetPosition = bounds.center;
	}

	public LayerMask GetSelectableLayerMask()
	{
		return 0x7fffffff;
	}

	public string[] GetTagPriorities()
	{
		return null;
	}

	public void SetView(float rx, float ry, float rz)
	{
		_euler = new Vector3(rx, ry, rz);
		_targetRotation = Quaternion.Euler(_euler);
		FocusOnBoundingBox(_bounds);
	}

	public void SetViewLeft() { SetView(0, 90, 0); }
	public void SetViewRight() { SetView(0,-90,0); }
	public void SetViewTop() { SetView(90,0,0); }
	public void SetViewBottom() { SetView(-90,0,0); }
	public void SetViewFront() { SetView(0,0,0); }
	public void SetViewBack() { SetView(0,180,0); }

	public bool isOrthographic => _camera.orthographic;
	public void OnSetCameraOrth()
	{
		_camera.orthographic = true;
		FocusOnBoundingBox(_bounds);
	}

	public void OnSetCameraPerspective()
	{
		_camera.orthographic = false;
		FocusOnBoundingBox(_bounds);
	}

	public void OnAdjustZoom(float value)
	{
		SetZoom(value, new Vector2(0.5f*Screen.currentResolution.width,0.5f*Screen.currentResolution.height));
	}
		
	public void OnResetFocus()
	{
		_zoom = 0.5f;
		_targetRotation = Quaternion.identity;
		FocusOnBoundingBox(_bounds);
	}

	private void SetZoom(float value, Vector2 mouse)
	{
		value = Mathf.Clamp(value, 0, 1);
		//float d = value - _zoom;
		_zoom = value;

		//Ray ray = _camera.ScreenPointToRay(mouse);
				
		Vector3 objectSizes = _bounds.max - _bounds.min;
		float objectSize = Mathf.Max(objectSizes.x, objectSizes.y, objectSizes.z);

		if (_camera.orthographic)
		{
			_camera.orthographicSize = 10 + objectSize * _zoom;
//				_camera.transform.localPosition = -objectSize*Vector3.forward;
			_camera.transform.localPosition = -3*_camera.orthographicSize*Vector3.forward;
		}
		else
		{
			float cameraDistance = 2.0f; // Constant factor
			float cameraView = 2.0f * Mathf.Tan(0.5f * Mathf.Deg2Rad * _camera.fieldOfView); // Visible height 1 unit in front
			float distance = cameraDistance * objectSize / cameraView; // Combined wanted distance from the object
			distance += (-2+5*_zoom) * objectSize; // Estimated offset from the center to the outside of the object
				
			_camera.transform.localPosition = - distance * Vector3.forward;
			//_targetPosition -= d * objectSize * ray.direction;
		}

		//_pivot.position = _targetPosition;
	}

	public bool OnSelect(RaycastHit obj, PointerEventData evt)
	{
		return true;
	}

	public bool GetNeedsHoverEvent() { return false;}
	public bool OnHover(RaycastHit hit) { return false; }

	public void OnDrag(RaycastHit obj, Vector2 deltaScreen)
	{
		_rotateModifier = true;
	}

	public bool OnRelease(RaycastHit source, RaycastHit target)
	{
		_rotateModifier = false;
		return false;
	}
}