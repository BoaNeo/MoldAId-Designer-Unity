using System;
using System.Collections.Generic;
using MeshUtil.DataStructures;
using UnityEngine;

namespace MeshUtil
{
	public class MutableMesh
	{
		private KdTree _vertices;
		private List<Triangle> _triangles = new ();
		private Bounds _bounds;

		public IReadOnlyList<Triangle> triangles => _triangles;

		public void ShareVerticesWith(MutableMesh shared)
		{
			// TODO: Should keep track of who we're sharing this tree with so we can make sure all triangles are included when updating!
			_vertices = shared._vertices;
		}
		
		public void Update(Triangle[] triangles, int from, int to)
		{
			if(from>_triangles?.Count)
				Debug.LogError("Trying to update MutableMesh with bad delta (first change is after end of old list)");

			bool badVerts = false;
			for (int i = from; i < to && i < _triangles.Count; i++)
			{
				Triangle t = triangles[i];
				badVerts = badVerts | t.v0.id < 0 | t.v1.id < 0 | t.v2.id < 0;
				_triangles[i] = t;
			}
			
			if(triangles.Length>_triangles.Count)
			{
				badVerts = true;
				for (int i = _triangles.Count; i < triangles.Length; i++)
					_triangles.Add(triangles[i]);
			}

			if (badVerts)
			{
				if(_vertices==null)
					BuildKdTree();
				else
					RefreshTriangles(from, to);
			}
		}

		// Can be public, but isn't because there's no need presently
		private KdTree BuildKdTree()
		{
			if (_vertices == null)
			{
				_vertices = new KdTree();
				RefreshTriangles(0,_triangles.Count);
			}
			return _vertices;
		}

		private void RefreshTriangles(int from, int to)
		{
			for(int i=from;i<to;i++)
			{
				Triangle triangle = _triangles[i];
				Vertex v0 = _vertices.AddOrGet(triangle.v0.point,triangle.v0.tag);
				Vertex v1 = _vertices.AddOrGet(triangle.v1.point,triangle.v1.tag);
				Vertex v2 = _vertices.AddOrGet(triangle.v2.point,triangle.v2.tag);
				if (v0.id != triangle.v0.id || v1.id != triangle.v1.id || v2.id != triangle.v2.id)
				{
					triangle = new Triangle(v0, v1, v2, triangle.n, triangle.tag);
					_triangles[i] = triangle;
				}
			}
		}

		public Bounds bounds
		{
			get
			{
				if (_bounds.extents.x == 0 && _triangles.Count>0)
				{
					_bounds = new Bounds(_triangles[0].v0.point, Vector3.zero);
					foreach (Triangle triangle in _triangles)
					{
						_bounds.Encapsulate(triangle.v0.point);
						_bounds.Encapsulate(triangle.v1.point);
						_bounds.Encapsulate(triangle.v2.point);
					}
				}
				return _bounds;
			}	
		}

		public Triangle AddTriangle(Vertex v0, Vertex v1, Vertex v2, Vector3 n,int tagface=0, bool reorient=false)
		{
			if (_triangles.Count == 0)
				_bounds = new Bounds(v0.point, Vector3.zero);
			else
				_bounds.Encapsulate(v0.point);
			_bounds.Encapsulate(v1.point);
			_bounds.Encapsulate(v2.point);
			Triangle t = new Triangle(v0,v1,v2,n,tagface,reorient);
			_triangles.Add(t);
			return t;
		}

		public Vertex AddVertex(Vector3 v0, int tag = 0)
		{
			if (float.IsNaN(v0.x) || float.IsNaN(v0.y) || float.IsNaN(v0.z) || float.IsInfinity(v0.x) || float.IsInfinity(v0.y) || float.IsInfinity(v0.z))
			{
				Debug.LogError($"Invalid point: {v0}!");
				return default;
			}
			return BuildKdTree().AddOrGet(v0, tag );
		}

		public void RemoveTriangles(Func<Triangle, bool> filter)
		{
			for (int i = 0; i < _triangles.Count; i++)
			{
				if (filter(_triangles[i]))
				{
					_triangles.RemoveAt(i--);
				}
			}
		}
	}
}