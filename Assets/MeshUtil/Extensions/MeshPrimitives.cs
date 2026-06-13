using UnityEngine;

namespace MeshUtil.Extensions
{
	public static class MeshPrimitives
	{
		public static Triangle AddTriangle(this MeshBuilder mb, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 n = default, int tagface=0, bool reorient=false, int tag0 = 0, int tag1 = 0, int tag2 = 0)
		{
			Vertex v0 = mb.AddVertex(p0, tag0);
			Vertex v1 = mb.AddVertex(p1, tag1);
			Vertex v2 = mb.AddVertex(p2, tag2);

			if (v0.id == v1.id || v0.id == v2.id || v1.id == v2.id)
			{
				//Debug.LogWarning("Detected sliver triangle (Maybe due to snapping?)");
				return default;
			}
			
			return mb.AddTriangle(v0,v1,v2, n, tagface, reorient);
		}

		public static void AddQuad(this MeshBuilder mb, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 n=default, int tagface=0, bool reorient=false, int tag0=0,int tag1=0,int tag2=0, int tag3=0)
		{
			mb.AddTriangle(v0, v1, v2, n, tagface, reorient, tag0,tag1,tag2);
			mb.AddTriangle(v0, v2, v3, n, tagface, reorient, tag0, tag2, tag3);
		}
	}
}