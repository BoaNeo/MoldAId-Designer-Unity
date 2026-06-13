using System;
using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace MeshUtil.DataStructures
{
	public class BoundingSphereGrid : MeshBuilder.ITriangleSet
	{
		public class TriangleHit
		{
			public Vertex v0;
			public Vertex v1;
			public Vertex v2;

			public Vector3 n;
			public long id;
			public int tag;
			internal int trace;

			public Vector3 mid => (v0.point + v1.point + v2.point) / 3.0f;

			public TriangleHit(Triangle triangle)
			{
				v0 = triangle.v0;
				v1 = triangle.v1;
				v2 = triangle.v2;
				n = triangle.n;
				id = triangle.id;
				tag = triangle.tag;
			}
		}

		public struct Sphere
		{
			public List<TriangleHit> triangles;

			#if DEBUG_MODE
			public Vector3 center;
			public void DebugDraw(float radius)
			{
				UnityEngine.Gizmos.DrawWireSphere( center, radius);
			}
			#endif
		}

		private float _radius;
		private Sphere[] _subs;
		private Bounds _bb;
		private int _xdivs;
		private int _ydivs;
		private int _zdivs;
		private float _step;
		private Dictionary<long, TriangleHit> _triangles = new ();

		const float SQRT2 = 1.4142135624f;
		private static Vector3 HALF = new(0.5f, 0.5f, 0.5f);
		private int _trace;
		public IEnumerable<TriangleHit> triangles => _triangles.Values;

		public BoundingSphereGrid()
		{}
		public void Initialize(MeshBuilder mb)
		{
			Bounds outerBB = mb.GetBounds();

			// TODO: Heuristics to decide steps size: More total triangles should generate more steps -
			// Time seems to generally half with double the number of steps, but keep in mind that memory increase in 3 dimensions
			_step = Mathf.Max(outerBB.size.x, outerBB.size.y, outerBB.size.z) / 20;
			_radius = SQRT2 * _step/2.0f;
			_bb = outerBB;

			#if DEBUG_MODE
			debugRadius = _radius;
			#endif

			_xdivs = Mathf.CeilToInt(outerBB.size.x / _step);
			_ydivs = Mathf.CeilToInt(outerBB.size.y / _step);
			_zdivs = Mathf.CeilToInt(outerBB.size.z / _step);
			_subs = new Sphere[_xdivs*_ydivs*_zdivs];

			for (int x = 0; x < _xdivs; x++)
			{
				for (int y = 0; y < _ydivs; y++)
				{
					for (int z = 0; z < _zdivs; z++)
					{
						int i = GridToIndex(x, y, z);
						Vector3 c = GridToPoint(x,y,z);
						Sphere s = new Sphere();
						#if DEBUG_MODE
						s.center = c;
						#endif
						_subs[i] = s;
					}
				}
			}

			foreach (Triangle t in mb.GetTriangles())
				AddTriangle(t);
		}

		private int GridToIndex(int x, int y, int z)
		{
			return x + y * _xdivs + z * _xdivs*_ydivs;
		}

		private Vector3 GridToPoint(int x, int y, int z)
		{
			return _bb.min + _step * new Vector3(x +0.5f, y+0.5f , z+0.5f );
		}

		private (int,int,int) PointToGrid(Vector3 pt)
		{
			pt = (pt - _bb.min) / _step - HALF;
			return (Mathf.RoundToInt(pt.x), Mathf.RoundToInt(pt.y), Mathf.RoundToInt(pt.z));
		}
		
		private Sphere Get(Vector3 pt)
		{
			(int x, int y, int z) = PointToGrid(pt);

			if (x < 0 || y < 0 || z < 0 || x >= _xdivs || y >= _ydivs || z >= _zdivs)
				return default;

			return _subs[ GridToIndex(x,y,z)];
		}

		public bool RayCast(Ray ray, Func<TriangleHit,bool> filter, out TriangleHit hit)
		{
			hit = null;

			_trace++;

			ray.direction.Normalize();

			float delta = 0;
			float min = float.MaxValue;

			if (!_bb.IntersectRay(ray, out delta))
				return false;

			float max = delta + Mathf.Max(_bb.size.x, _bb.size.y, _bb.size.z);

			delta -= 0.01f; // Back off a bit so we don't miss a face that is exactly on the boundary

			while (delta<max)
			{
				Sphere sphere = Get( ray.origin + delta * ray.direction);

				#if DEBUG_MODE
				debugSearchDelta = ray.origin + delta * ray.direction;
				debugCurrentSphere = sphere;
				#endif
				
				if (sphere.triangles != null)
				{
					foreach (TriangleHit th in sphere.triangles)
					{
						if( th.trace!=_trace)
						{
							th.trace = _trace;
							
							if(filter == null || filter(th))
							{
								if (ray.IntersectsTriangle(th.v0.point, th.v1.point, th.v2.point, th.n, out float d, out Vector3 ip))
								{
									#if DEBUG_MODE
									debugLastHit = triangle;
									debugHitPoint = ip;
									#endif
									if (d < min)
									{
										min = d;
										hit = th;
										#if DEBUG_MODE
										debugBestHit = triangle;
										#endif
									}
								}
							}
						}
					}

					if (min<float.MaxValue)
						return true;
				}

				delta += _step * .99f; //_radius; // Keeping it safe with less than _step so we don't risk skipping a sphere. Taking the same sphere twice is not very expensive because we already tagged all the faces in it
			}

			return false;
		}

		#if DEBUG_MODE
		public static Sphere debugCurrentSphere { get; set; }
		public static Vector3 debugHitPoint { get; set; }
		public static Triangle debugLastHit { get; set; }
		public static Triangle debugBestHit { get; set; }
		public static float debugRadius { get; set; }
		public static Vector3 debugSearchDelta { get; set; }
    #endif

		private void AddTriangle(Triangle triangle)
		{
			Vector3 margin = (2*_radius-_step)*HALF;
			
			Bounds aabb = triangle.GetAABB();
			(int xmin,int ymin,int zmin) = PointToGrid(aabb.min - margin);
			(int xmax,int ymax,int zmax) = PointToGrid(aabb.max + margin);

			xmin = Mathf.Clamp(xmin, 0, _xdivs-1);
			ymin = Mathf.Clamp(ymin, 0, _ydivs-1);
			zmin = Mathf.Clamp(zmin, 0, _zdivs-1);
			xmax = Mathf.Clamp(xmax, 0, _xdivs-1);
			ymax = Mathf.Clamp(ymax, 0, _ydivs-1);
			zmax = Mathf.Clamp(zmax, 0, _zdivs-1);

			if (!_triangles.TryGetValue(triangle.id, out TriangleHit th))
			{
				th = new TriangleHit(triangle);
				_triangles[triangle.id] = th;
			}

			for (int x = xmin; x <= xmax; x++)
			{
				for (int y = ymin; y <= ymax; y++)
				{
					for (int z = zmin; z <= zmax; z++)
					{
						int i = GridToIndex(x, y, z);
						Sphere sphere = _subs[i];
						if (sphere.triangles == null)
						{
							sphere.triangles = new ();
							_subs[i] = sphere;
						}
						sphere.triangles.Add(th);
					}
				}
			}
		}

		public void DebugDraw()
		{
			#if DEBUG_MODE
			UnityEngine.Gizmos.color = Color.grey;
			
			foreach (Sphere bsh in _subs)
			{
				bsh.DebugDraw(_radius);
			}

			if (debugCurrentSphere.triangles != null)
			{
				UnityEngine.Gizmos.color = Color.white;
				foreach (Triangle t in debugCurrentSphere.triangles)
				{
					UnityEngine.Gizmos.DrawLine(t.v0.point,t.v1.point);
					UnityEngine.Gizmos.DrawLine(t.v1.point,t.v2.point);
					UnityEngine.Gizmos.DrawLine(t.v2.point,t.v0.point);
				}				
			}
			#endif
		}

		public TriangleHit GetTriangle(long triangle)
		{
			return _triangles[triangle];
		}
	}
}