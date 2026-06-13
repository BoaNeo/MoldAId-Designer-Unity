using IO;
using UnityEngine;

namespace Gizmos
{
	public struct XForm : IStreamable
	{
		public static XForm identity = new XForm( Vector3.zero, Quaternion.identity );


		public Vector3 position
		{
			get => _position;
			set => _position = value;
		}

		public Vector3 GetPosition(XForm parent)
		{
			return parent.localToWorldMatrix.MultiplyPoint(_position);
		}

		public void SetPosition(XForm parent, Vector3 position)
		{
			_position = parent.worldToLocalMatrix.MultiplyPoint(position);
		}

		public Quaternion rotation
		{
			get => FixQuaternion(_rotation);
			set => _rotation = value;
		}

		public Quaternion GetRotation(XForm parent)
		{
			return parent.rotation * FixQuaternion(_rotation);
		}

		public void SetRotation(XForm parent, Quaternion quaternion)
		{
			_rotation = Quaternion.Inverse(parent.rotation) * quaternion;
		}

		public Vector3 forward => rotation * Vector3.forward;
		public Matrix4x4 worldToLocalMatrix => Matrix4x4.Rotate(rotation).inverse*Matrix4x4.Translate(-position);
		public Matrix4x4 localToWorldMatrix => Matrix4x4.Translate(position)*Matrix4x4.Rotate(rotation);

		private Vector3 _position;
		private Quaternion _rotation;

		public XForm(XForm origin, Vector3 position = default, Quaternion rotation = default)
		{
			_position = default;
			_rotation = default;
			SetRotation(origin,rotation);
			SetPosition(origin,position);
		}

		public XForm(Vector3 position, Quaternion rotation)
		{
			_position = position;
			_rotation = rotation;
		}

		private Quaternion FixQuaternion(Quaternion q)
		{
			return q.normalized;//==default ? Quaternion.identity : q;
		}

		public XForm WithPosition(Vector3 value)
		{
			return new XForm ( value, rotation );
		}

		public XForm WithRotation(Quaternion value)
		{
			return new XForm ( position , value );
		}

		public void Serialize(DataStream data)
		{
			data.Serialize("position", ref _position);
			data.Serialize("rotation", ref _rotation);
		}
	}
}