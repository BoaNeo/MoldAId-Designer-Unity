using UnityEngine;

namespace MeshUtil
{
	public readonly struct Triangle
	{
		public readonly Vertex v0;
		public readonly Vertex v1;
		public readonly Vertex v2;

		public readonly Vector3 n;

		public readonly int tag;

		public Vector3 mid => (v0.point + v1.point + v2.point) / 3.0f;

		/// <summary>
		/// The Triangle Id is derived from the vertex ids in their defined order to make sure it's unique and that back and front faces gets different Ids.
		/// This allow 21 bits per vertex so 2^21 = 2.097.152 vertices
		/// </summary>

		public long id => (((long) v0.id) << 42) | (((long) v1.id) << 21) | (uint) v2.id;

		internal Triangle(Vertex v0, Vertex v1, Vertex v2, Vector3 n, int tag=0, bool reorient=false)
		{
			bool autonormal = false;
			if (n == default || reorient)
			{
				
				Vector3 triangle_n = Vector3.Cross(v1.point - v0.point, v2.point - v0.point);
				
				if(n==default)
				{
					n = triangle_n.normalized; // No normal provided, assume vertex order defines the normal
					autonormal = true;
				}
				else if (Vector3.Dot(triangle_n, n) < 0)
					(v1, v2) = (v2, v1); // Swap vertex order if normal is not pointing in the desired direction
			}

			#if DEBUG
			if(n.magnitude<0.99f)
				Debug.LogWarning($"Malformed normal: {n}! Autonormal={autonormal} Reorient={reorient}");
			#endif

			this.tag = tag;
			this.n = n;
			this.v0 = v0;
			this.v1 = v1;
			this.v2 = v2;
		}

		public Bounds GetAABB()
		{
			Vector3 min = new Vector3( 
				Mathf.Min(v0.point.x, v1.point.x, v2.point.x),
				Mathf.Min(v0.point.y, v1.point.y, v2.point.y),
				Mathf.Min(v0.point.z, v1.point.z, v2.point.z));
			Vector3 max = new Vector3( 
				Mathf.Max(v0.point.x, v1.point.x, v2.point.x),
				Mathf.Max(v0.point.y, v1.point.y, v2.point.y),
				Mathf.Max(v0.point.z, v1.point.z, v2.point.z));
			Vector3 size = max - min;
			return new Bounds(min+size/2, size);
		}
	}
}