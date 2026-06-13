using System;
using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace MeshUtil
{
	public class Edge
	{
		public long id;
		public Vertex v0;
		public Vertex v1;
//		public ICollection<Triangle> triangles => _triangles;

		private List<Triangle> _triangles;

		private int _hash;

		public Edge(Vertex v0, Vertex v1)
		{
			this.v0 = v0;
			this.v1 = v1;
			(id,_hash) = CalcIdAndHash(v0, v1);
		}
/*
		public Edge(Vertex v0, Vertex v1, Triangle t) : this(v0,v1)
		{
			AddTriangle(t);
		}
*/
		public static (long,int) CalcIdAndHash(Vertex v0, Vertex v1)
		{
			unchecked
			{
				(int id0, int id1) =v0.id>v1.id ? (v0.id, v1.id) : (v1.id, v0.id);
				int hash = 17;
				hash = hash * 31 + id0;
				hash = hash * 31 + id1;
				return ((long)id0 << 32 | (long)id1, hash);
			}
		}

		public override int GetHashCode()
		{
			return _hash;
		}

		public override bool Equals(object o)
		{
			return this == o || o is Edge e2 && (id==e2.id);
		}
/*
		public static void TriangleSpannedBy(Edge e1, Edge e2, out Vertex v0, out Vertex v1, out Vertex v2)
		{
			if (e1.v1.id == e2.v0.id)
			{
				v0 = e1.v0;
				v1 = e1.v1;
				v2 = e2.v1;
			}
			else if (e1.v1.id == e2.v1.id)
			{
				v0 = e1.v0;
				v1 = e1.v1;
				v2 = e2.v0;
			}
			else if (e1.v0.id == e2.v0.id)
			{
				v0 = e1.v1;
				v1 = e1.v0;
				v2 = e2.v1;
			}
			else if (e1.v0.id == e2.v1.id)
			{
				v0 = e1.v1;
				v1 = e1.v0;
				v2 = e2.v0;
			}
			else
			{
				Debug.LogWarning("Edges have no shared vertex ??");
				v0 = default;
				v1 = default;
				v2 = default;
			}
		}
	*/	
		public void DebugDraw(Color color, float scale, Matrix4x4 m)
		{
			Vector3 p0 = scale*m.MultiplyPoint(v0.point);
			Vector3 p1 = scale*m.MultiplyPoint(v1.point);
    	Debug.DrawLine(p0,p1,color);
      DebugExtension.DrawBox(p0,.02f*scale, Color.white);
      DebugExtension.DrawBox(p1,.02f*scale, Color.white);

      Vector3 c = (p0 + p1) / 2;
      Vector3 n = Vector3.Cross(v1.point - v0.point, Vector3.back);
      
      Debug.DrawLine( c, c+.1f*scale*m.MultiplyVector(n), Color.magenta);
		}
		
		public static Dictionary<long,Edge> All(MeshBuilder mb, Func<Edge, bool>filter=null, Dictionary<long, Edge> edges=null)
		{
			if(edges==null)
				edges = new Dictionary<long, Edge>();

			foreach(Triangle t in mb.GetTriangles())
			{
				Add(new Edge(t.v0, t.v1), t);
				Add(new Edge(t.v1, t.v2), t);
				Add(new Edge(t.v2, t.v0), t);
			}
			return edges;

			void Add(Edge e,Triangle t)
			{
				if (filter!=null && !filter(e))
					return;
				if (edges.TryGetValue(e.id, out Edge olde))
					e = olde;
				else
					edges[e.id] = e;
			//	e.AddTriangle(t);
			}
		}
/*
		private void AddTriangle(Triangle triangle)
		{
			if (_triangles == null)
				_triangles = new ();
			_triangles.Add(triangle);
		}*/
/*
		public bool Intersects(Edge o, double epsilon)
		{
			// TODO: Add quick AABB test 

			// Generally, let ends of edges extend a small amount outside the edge to catch ends that touch other edges
			double s0 = 0-epsilon;
			double s1 = 1+epsilon;
			double t0 = 0-epsilon;
			double t1 = 1+epsilon;

			// However, don't do this if they share vertices
			if (o.v0.id == v0.id)
			{
				t0 = epsilon;
				s0 = epsilon;
			}
			if (o.v1.id == v1.id)
			{
				t1 = 1-epsilon;
				s1 = 1-epsilon;
			}
			if (o.v0.id == v1.id)
			{
				t0 = epsilon;
				s1 = 1-epsilon;
			}
			if (o.v1.id == v0.id)
			{
				t1 = 1-epsilon;
				s0 = epsilon;
			}

			// Finding and intersection is just a question of solving the lines equality equation:
			// o.v0 + t*(o.v1 - o.v0) = v0 + s*(v1-v0)
			// t = (v0.x + s*(v1.x-v0.x) - o.v0.x) / (o.v1.x-o.v0.x)
			// t = (v0.y + s*(v1.y-v0.y) - o.v0.y) / (o.v1.y-o.v0.y)
			// (v0.x + s*(v1.x-v0.x) - o.v0.x) / (o.v1.x-o.v0.x) = (v0.y + s*(v1.y-v0.y) - o.v0.y) / (o.v1.y-o.v0.y)
			// (v0.x + s*(v1.x-v0.x) - o.v0.x) * (o.v1.y-o.v0.y) = (v0.y + s*(v1.y-v0.y) - o.v0.y) * (o.v1.x-o.v0.x)
			// dox = (o.v1.x-o.v0.x)
			// doy = (o.v1.y-o.v0.y)
			// dx = (v1.x-v0.x)
			// dy = (v1.y-v0.y)
			// v0.x*doy + s*dx*doy - o.v0.x*doy  = v0.y*dox + s*dy*dox - o.v0.y*dox
			// s*dx*doy - s*dy*dox = v0.y*dox - o.v0.y*dox - v0.x*doy + o.v0.x*doy
			// s*(dx*doy - dy*dox) = (v0.y - o.v0.y)*dox + (o.v0.x - v0.x)*doy
			// s = ((v0.y - o.v0.y)*dox + (o.v0.x - v0.x)*doy) / (dx*doy - dy*dox)
			// t = (v0.x + s*dx - o.v0.x) / dox

			double dox = o.v1.point.x - o.v0.point.x;
			double doy = o.v1.point.y - o.v0.point.y;
			double dx = v1.point.x - v0.point.x;
			double dy = v1.point.y - v0.point.y;
			double s = ((v0.point.y - o.v0.point.y) * dox + (o.v0.point.x - v0.point.x) * doy) / (dx * doy - dy * dox);
			
			if (double.IsNaN(s) || double.IsInfinity(s))
			{
				return false; // Parallel lines
			}
			
			if (s >= s0 && s <= s1)
			{
				double t;
				if(Math.Abs(doy)>Math.Abs(dox))
					t = (v0.point.y + s * dy - o.v0.point.y) / doy;
				else
				  t = (v0.point.x + s * dx - o.v0.point.x) / dox;

				if (double.IsNaN(t) || double.IsInfinity(t))
					return false; // Should not happen!
				
				if ( t>=t0 && t<=t1) //(t>=0 && t<=1) || (s>=0 && s<=1 && t >= -epsilon && t <= 1+epsilon))
					return true;
			}
			return false;
		}

		public Triangle GetFirstTriangle()
		{
			if (_triangles == null || _triangles.Count == 0)
				return default;
			return _triangles[0];
		}

		public int GetTriangleCount()
		{
			return _triangles?.Count??0;
		}
*/
		public float GetYForX(float x)
		{
			float t = (x - v0.point.x)/(v1.point.x - v0.point.x);
			return v0.point.y + t * (v1.point.y - v0.point.y);
		}
	}
}