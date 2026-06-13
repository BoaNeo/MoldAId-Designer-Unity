using System.Collections.Generic;
using UnityEngine;

namespace MeshUtil.DataStructures
{
	public class TrianglesById : MeshBuilder.ITriangleSet
	{
		private Dictionary<long, Triangle> _triangles = new ();
		private MeshBuilder _meshBuilder;

		public void Initialize(MeshBuilder mb)
		{
			_meshBuilder = mb;

			foreach (Triangle t in mb.GetTriangles())
				AddTriangle(t);
		}

		private void AddTriangle(Triangle triangle)
		{
			_triangles[CalculateId(triangle)] = triangle;
		}
/*
		public void OnTriangleRemoved(Triangle triangle)
		{
			_triangles.Remove(CalculateId(triangle));
		}

*/
		public void DebugDraw()
		{
		}

		public long CalculateId(Triangle t)
		{
			return CalculateId(t.v0, t.v1, t.v2);
		}

		public long CalculateId(Vertex v0, Vertex v1, Vertex v2)
		{
			if (v0.id > v1.id)
			{
				if (v1.id > v2.id)
					return BuildId(v0, v1, v2);
				if( v0.id>v2.id)
					return BuildId(v0, v2, v1);
				return BuildId(v2, v0, v1);
			}
			{
				if (v0.id > v2.id)
					return BuildId(v1, v0, v2);
				if( v1.id>v2.id)
					return BuildId(v1, v2, v0);
				return BuildId(v2, v1, v0);
			}
		}

		private long BuildId(Vertex v0, Vertex v1, Vertex v2)
		{
			unchecked
			{
				return (long)v0.id << 42 | (long)v1.id<<21 | (long)v2.id;
			}
		}

		public void AddUniqueTriangle(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 n, int tagface=0, bool reorient=false, int tag0=0, int tag1=0, int tag2=0)
		{
			Vertex v0 = _meshBuilder.AddVertex(p0, tag0);
			Vertex v1 = _meshBuilder.AddVertex(p1, tag1);
			Vertex v2 = _meshBuilder.AddVertex(p2, tag2);
			if(!_triangles.ContainsKey(CalculateId(v0,v1,v2)))
			{
				AddTriangle(_meshBuilder.AddTriangle(v0, v1, v2, n, tagface, reorient));
			}
		}
	}
}