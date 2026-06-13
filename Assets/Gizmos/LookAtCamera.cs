using UnityEngine;

namespace Gizmos
{
	public class LookAtCamera : MonoBehaviour
	{
		private Camera _camera;

		private void Update()
		{
			if (_camera == null)
				_camera = Camera.main;
			if (_camera != null)
			{
				Quaternion q = Quaternion.LookRotation(-_camera.transform.forward, Vector3.forward);
				transform.rotation = q;
			}
		}
	}
}