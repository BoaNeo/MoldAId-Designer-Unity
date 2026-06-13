using UnityEngine;

namespace Gizmos
{
  public class Axes : MonoBehaviour
  {
    [SerializeField] private Transform _pivot;
    [SerializeField] private LayerMask _axisGizmoLayer;
    private Camera _camera;

    private void Awake()
    {
      _camera = Camera.main;
    }

    private void Update()
    {
      if (Input.GetMouseButtonDown(0))
      {
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100000, _axisGizmoLayer))
        {
          Vector3 p = hit.collider.transform.localPosition;
          if (p.magnitude == 0)
          {
            OnToggleIsoPersp();
          }
          else
          {
            switch (hit.collider.gameObject.name)
            {
              case "BottomView": CameraManager.instance.SetViewBottom(); break;
              case "TopView": CameraManager.instance.SetViewTop(); break;
              case "FrontView": CameraManager.instance.SetViewFront(); break;
              case "BackView": CameraManager.instance.SetViewBack(); break;
              case "LeftView": CameraManager.instance.SetViewLeft(); break;
              case "RightView": CameraManager.instance.SetViewRight(); break;
            }
          }
        }
      }
    }

    public void OnToggleIsoPersp()
    {
      if(CameraManager.instance.isOrthographic)
        CameraManager.instance.OnSetCameraPerspective();
      else
        CameraManager.instance.OnSetCameraOrth();
    }

    void LateUpdate()
    {
      const float size = 1.6f;
      if (_camera.orthographic)
      {
        float s = (0.125f * _camera.orthographicSize);
        _pivot.transform.localPosition = s * new Vector3(-2.0f, 1.5f, 0.0f);
        _pivot.transform.localScale = size * s * Vector3.one;
      }
      else
      {
        _pivot.transform.localPosition = new Vector3(-2.5f, 2.00f, 0.0f);
        _pivot.transform.localScale = size * Vector3.one;
      }
      Ray ray = _camera.ScreenPointToRay(new Vector3(Screen.width, 0, 0));
      transform.position = ray.origin + 30 * ray.direction;
      _pivot.rotation = Quaternion.identity;
    }
  }
}
