using UnityEngine;
using Utility;

namespace MeshUtil
{
	public readonly struct Vertex
	{
		public const float ACCURACY = 10*float.Epsilon; // 0.000001f; // 1.0f = 1 mm

		public readonly Vector3 point;
		public readonly int tag;
		public readonly int id;

		public Vertex( int id, Vector3 point, int tag)
		{
			this.id = id;
			this.point = point;
			this.tag = tag;
		}
		
		public override int GetHashCode() { return id; }
		public override bool Equals(object o)
		{
			return o is Vertex v2 && v2.id==id;
		}
		public bool AlmostEqual(Vertex v0)
		{
			return v0.id == id || v0.point.AlmostEqual(point);
		}
	}
}