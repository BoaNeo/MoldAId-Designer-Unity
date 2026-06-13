using UnityEngine;
using Utility;

namespace Gizmos
{
	public class Marker : PooledObject
	{
		public enum Flags { KeepCurrent=0x01, Disconnected=0x02 }

		public Flags flags { get; set; }

		private Camera _camera;

		private void Awake()
		{
			_camera = Camera.main;
		}

		public bool UpdateHit(RaycastHit hit)
		{
			if (hit.collider != null)
			{
				transform.position = hit.point;
				transform.forward = hit.normal;
				return true;
			}
			return false;
		}

		private void LateUpdate()
		{
			Scale();
		}

		private void Scale()
		{
			float distanceToCamera;
			if (_camera.orthographic)
				distanceToCamera = 1.25f*_camera.orthographicSize;
			else
				distanceToCamera = (_camera.transform.position - transform.position).magnitude;
			transform.localScale = Vector3.one * distanceToCamera * 0.075f;
		}
	}
}