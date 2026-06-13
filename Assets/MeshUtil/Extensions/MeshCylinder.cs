using UnityEngine;

namespace MeshUtil.Extensions
{
	public static class MeshCylinder
	{
		public static MeshBuilder GenerateCylinder(this MeshBuilder mb, float radius, float height, int sides)
		{
			float da = 2 * Mathf.PI / sides;
			float a = 0;
			(Vertex v1, Vertex v2) = Points(a);
			for (int i = 0; i < sides; i++)
			{
				if (i == sides - 1)
					a = 0;
				else
					a += da;
				(Vertex v0, Vertex v3) = (v1, v2);
				(v1, v2) = Points(a);

				mb.AddTriangle(v0, v1, v2, default);
				mb.AddTriangle(v0, v2, v3, default);
			}

			Vertex ch = mb.AddVertex(new Vector3(0, 0,height));
			Vertex cl = mb.AddVertex(new Vector3(0,  0,0));
			foreach (Edge edge in Edge.All(mb, (Edge e) => (e.v0.point.z>=height && e.v1.point.z>=height) || (e.v0.point.z<=0 && e.v1.point.z<=0) ).Values)
			{
				if (edge.v0.point.z > 0)
					mb.AddTriangle(edge.v0, edge.v1, ch, Vector3.forward, 0, true);
				else
					mb.AddTriangle(edge.v0, edge.v1, cl, Vector3.back, 0, true);
			}
			
			return mb;

			(Vertex, Vertex) Points(float a)
			{
				float x0 = radius * Mathf.Cos(a);
				float y0 = radius * Mathf.Sin(a);
				Vector3 v1 = new Vector3(x0,y0,0);
				Vector3 v2 = new Vector3(x0,y0,height);
				return (mb.AddVertex(v1), mb.AddVertex(v2));
			}
		}
	}
}