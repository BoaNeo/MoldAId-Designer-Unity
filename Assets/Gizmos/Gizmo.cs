using System;
using FeatureGraph;
using Undo;
using UnityEngine;
using Utility;

namespace Gizmos
{
	public class Gizmo : PooledObject
	{
		[SerializeField] private GizmoHandle _moveX;
		[SerializeField] private GizmoHandle _moveY;
		[SerializeField] private GizmoHandle _moveZ;
		[SerializeField] private GizmoHandle _moveView;
		[SerializeField] private GizmoHandle _rotateX;
		[SerializeField] private GizmoHandle _rotateY;
		[SerializeField] private GizmoHandle _rotateZ;
		[SerializeField] private Transform _handles;

		private Camera _camera;
		private DataRef<XForm> _transform;
		private GizmoSpace _space;
		private float _hover;
		private bool _firstChange = true;
		private XForm _oldValue;
		private Action<XForm,bool> _feedback;

		public bool isVisible
		{
			get => _handles.gameObject.activeSelf;
			set => _handles.gameObject.SetActive(value);
		}

		private void Awake()
		{
			_moveView.dragMode = GizmoHandle.DragMode.Screen;
			_rotateX.dragMode = GizmoHandle.DragMode.Rotation;
			_rotateY.dragMode = GizmoHandle.DragMode.Rotation;
			_rotateZ.dragMode = GizmoHandle.DragMode.Rotation;
			_camera = Camera.main;
		}

		public void Setup(DataRef<XForm> xformref, GizmoHandleFlags handles, GizmoSpace space, Action<XForm, bool> feedback=null)
		{
			_transform = xformref;
			_feedback = feedback;

			_space = space;

			_moveX.gameObject.SetActive( (handles & GizmoHandleFlags.MoveX) != 0 );
			_moveY.gameObject.SetActive( (handles & GizmoHandleFlags.MoveY) != 0 );
			_moveZ.gameObject.SetActive( (handles & GizmoHandleFlags.MoveZ) != 0 );
			_moveView.dragMode = (handles & GizmoHandleFlags.MoveView) != 0 ? GizmoHandle.DragMode.Screen : 0; // Center gizmo ball is always visible, but not always active
			_rotateX.gameObject.SetActive( (handles & GizmoHandleFlags.RotateX) != 0 );
			_rotateY.gameObject.SetActive( (handles & GizmoHandleFlags.RotateY) != 0 );
			_rotateZ.gameObject.SetActive( (handles & GizmoHandleFlags.RotateZ) != 0 );

			Scale(); // Avoid initial jitter since we only do this on LateUpdate()
			
			gameObject.SetActive(true);
		}

		public void Teardown()
		{
			_transform = null;
			_feedback = null;
			gameObject.SetActive(false);
		}

		public void Drag(Vector3 delta, bool changing)
		{
			delta = transform.worldToLocalMatrix * delta;
			if (!_moveX.gameObject.activeSelf)
				delta.x = 0;
			if (!_moveY.gameObject.activeSelf)
				delta.y = 0;
			if (!_moveZ.gameObject.activeSelf)
				delta.z = 0;
			delta = transform.localToWorldMatrix * delta;

			SetXForm(_transform.value.WithPosition(_transform.value.position + delta), changing);
		}

		public void Rotate(Quaternion q, bool changing)
		{
			SetXForm( _transform.value.WithRotation(q*_transform.value.rotation), changing);
		}

		private void SetXForm(XForm newvalue, bool changing)
		{
			if (_firstChange)
			{
				_oldValue = _transform.value;
				_firstChange = false;
			}

			XForm oldvalue = _oldValue;
			
			FeatureManager.transientChange = changing;

			if (changing)
			{
				_transform.Set(newvalue);
			}
			else
			{
				UndoManager.Append(() =>
				{
					Debug.Log($"Gizmo commiting xform to undo stack: {newvalue.position} - old value was {oldvalue.position} - drag value was {_transform.value.position}");
					_transform.Set(newvalue);
				}, () =>
				{
					Debug.Log($"Gizmo restoring xform from undo stack: {oldvalue.position}");
					_transform.Set(oldvalue);
				});
				_firstChange = true;
			}
			_feedback?.Invoke(newvalue, changing);
		}

		public void OnHover()
		{
			_hover = 1.0f;
		}

		private void LateUpdate()
		{
			if (_transform == null)
				return;
			
			Scale();
			transform.position = _transform.value.position;
			if (_space == GizmoSpace.World)
			{
				transform.rotation = Quaternion.identity;
				Quaternion q = _transform.value.rotation;
				Vector3 e = q.eulerAngles;
				_rotateX.transform.localRotation = Quaternion.AngleAxis(e.x, Vector3.right);
				_rotateY.transform.localRotation = Quaternion.AngleAxis(e.y, Vector3.up);
				_rotateZ.transform.localRotation = Quaternion.AngleAxis(e.z, Vector3.forward);
			}
			else
			{
				transform.rotation = _transform.value.rotation;
				_rotateX.transform.localRotation = Quaternion.identity;
				_rotateY.transform.localRotation = Quaternion.identity;
				_rotateZ.transform.localRotation = Quaternion.identity;
			}
			if (_hover > 0)
				_hover -= Time.deltaTime;
			float v = 1-Mathf.Sqrt(1-Mathf.Clamp(_hover, 0, 1));
			_moveX.SetVisibility(v);
			_moveY.SetVisibility(v);
			_moveZ.SetVisibility(v);
			_rotateX.SetVisibility(v);
			_rotateY.SetVisibility(v);
			_rotateZ.SetVisibility(v);
		}

		private void Scale()
		{
			float distanceToCamera;
			if (_camera.orthographic)
				distanceToCamera = 1.25f*_camera.orthographicSize;
			else
				distanceToCamera = (_camera.transform.position - transform.position).magnitude;
			_handles.localScale = Vector3.one * distanceToCamera * 0.1f;
		}
	}
}