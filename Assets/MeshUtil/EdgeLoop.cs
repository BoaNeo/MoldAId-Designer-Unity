using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace MeshUtil
{
	public class EdgeLoop
	{
		private List<Vertex> _loop = new List<Vertex>();

		public int count => _loop.Count;
		public Vertex this[int i2] => _loop[(i2+count)%count];

		public void Add(Vertex v)
		{
			Insert(count,v);
		}
/*
		public (Vertex, Vertex, Vertex) Get(int from)
		{
			return (_loop[from % _loop.Count], _loop[(from + 1) % _loop.Count], _loop[(from + 2) % _loop.Count]);
		}

		public void Remove(Vertex v)
		{
			Remove(_loop.IndexOf(v));
		}


		public void Remove(int i)
		{
			_loop.RemoveAt(i%count);
		}
		
		public void Clear()
		{
			_loop.Clear();
		}
*/
		public bool Insert(int i, Vertex v)
		{
			if (count > 0)
			{
				Vertex prevprev = _loop[ (i+count+count-2)%count];
				Vertex nextnext = _loop[(i + 2) % count];
				Vertex next = _loop[(i + 1) % count];
				Vertex prev = _loop[ (i+count-1)%count ];
				if (prev.id == v.id || prevprev.id==v.id || next.id==v.id || nextnext.id==v.id)
				{
//					Debug.LogWarning("Attempt to add the same vertex as a neighbour in a loop - skipping");
//					return false;
				}
			}
			_loop.Insert(i, v);
			return true;
		}

		/// <summary>
		/// Merge a second loop into this one at the specified index
		/// </summary>
		/// <param name="i"></param>
		/// <param name="other"></param>
		/// <param name="startAt"></param>
		/*
		public void Merge(int i, EdgeLoop other, Vertex startAt)
		{
			i %= count;

			bool start = false;
			int inserts = 0;
			int j = 0;
			while (inserts<other.count)
			{
				j %= other.count;
				Vertex next = other._loop[j++];
				if (next.id == startAt.id)
					start = true;
				if (start)
				{
					if (Insert(i, next))
						i++;
					inserts++;
				}
			}
			i %= count;
			Insert(i,startAt);
		}
*/
		/// <summary>
		/// Cut the edge loop between the provided index and the terminating vertex. The Vertex ends up in both lists
		/// </summary>
		/// <param name="i"></param>
		/// <param name="to"></param>
		/// <returns></returns>
/*
		public EdgeLoop Split(int iLast, int iFirst, Vertex to)
		{
			iFirst %= count;
			iLast %= count;
			Vertex first = _loop[iFirst++];
			Vertex last = _loop[iLast];
			bool cut = false;

			EdgeLoop other = new EdgeLoop();

			int i = iFirst;
			Vertex v;
			do
			{
				i %= count;
				v = _loop[i];

				if (v.id == to.id)
					cut = true;

				if (cut)
				{
					other.Add(v);
					if(v.id!=to.id)
						_loop.RemoveAt(i--);
				}
				i++;
			} while (v.id != last.id);
			return other;
		}
*/
/*		public bool ClosestVertexInTriangle(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 n, ref float maxdist, out Vertex vertex, out int foundAtIndex)
		{
			vertex = default;
			foundAtIndex = -1;
			for(int i=0;i<_loop.Count;i++)
			{
				Vertex v = _loop[i];
				if (!v.point.AlmostEqual(p0) && !v.point.AlmostEqual(p1) && !v.point.AlmostEqual(p2))
				{
					double d0 = v.point.DistanceToPlane( p0, Vector3.Cross( -n, p1-p0));
					double d1 = v.point.DistanceToPlane( p1, Vector3.Cross( -n, p2-p1));
					double d2 = v.point.DistanceToPlane( p2, Vector3.Cross( -n, p0-p2));
					// To be inside the triangle the point has to be in front of (positive distance to) all planes
					if (d0 >= 0 && d1 >= 0 && d2 >= 0)
					{
						if (d2 < maxdist)
						{
							maxdist = (float)d2;
							vertex = v;
							foundAtIndex = i;
						}
					}
				}
			}
			return foundAtIndex>=0;
		}
*//*
		public float DistanceToClosestEdgeFrom(Vector3 midpoint, Vector3 direction, out Vertex edgev0, out Vertex edgev1)
		{
			float d = float.MaxValue;
			edgev0 = default;
			edgev1 = default;
			for (int i = 0; i < _loop.Count; i++)
			{
				Vertex v0 = _loop[i];
				Vertex v1 = _loop[(i + 1) % count];
				
				// v0 + t * (v1 - v0) = midpoint + s * direction
				// (v0 + t * (v1 - v0) - midpoint)/direction = s
				// (v0.x+t*(v1.x-v0.x)-midpoint.x)/direction.x = s
				// (v0.y+t*(v1.y-v0.y)-midpoint.y)/direction.y = s
				// (v0.x+t*(v1.x-v0.x)-midpoint.x)/direction.x - (v0.y+t*(v1.y-v0.y)-midpoint.y)/direction.y = 0
				// ((v0.x+t*(v1.x-v0.x)-midpoint.x)*direction.y- (v0.y+t*(v1.y-v0.y)-midpoint.y)*direction.x)/direction.x*direction.y = 0
				// (v0.x+t*(v1.x-v0.x)-midpoint.x)*direction.y- (v0.y+t*(v1.y-v0.y)-midpoint.y)*direction.x = 0
				// v0.x*direction.y+t*(v1.x-v0.x)*direction.y-midpoint.x*direction.y - v0.y*direction.x-t*(v1.y-v0.y)*direction.x+midpoint.y*direction.x = 0
				// t*((v1.x-v0.x)*direction.y - (v1.y-v0.y)*direction.x) = -midpoint.y*direction.x + v0.y*direction.x - v0.x*direction.y + midpoint.x*direction.y
				float t = (-midpoint.y * direction.x + v0.point.y * direction.x - v0.point.x * direction.y + midpoint.x * direction.y) /
				          ((v1.point.x - v0.point.x) * direction.y - (v1.point.y - v0.point.y) * direction.x);

				if (t >= 0 && t <= 1)
				{
					// v0 + t * (v1 - v0) = midpoint + s * direction
					// (v0 + t * (v1 - v0) - midpoint)/direction = s 
					float s;
					if( Mathf.Abs(direction.x)>Mathf.Abs(direction.y) )
						s = (v0.point.x + t * (v1.point.x - v0.point.x) - midpoint.x) / direction.x;
					else
						s = (v0.point.y + t * (v1.point.y - v0.point.y) - midpoint.y) / direction.y;
					if (s > Vertex.ACCURACY && s < d)
					{
						edgev0 = v0;
						edgev1 = v1;
						d = s;
					}
				}
			}
			return d;
		}
*/
		public void DebugDraw(Color c, Matrix4x4 m)
		{
			for (int i = 0; i < _loop.Count; i++)
			{
				Debug.DrawLine(m.MultiplyPoint(_loop[i].point),m.MultiplyPoint(_loop[(i+1)%count].point),c,1.0f);
			}
		}

		public void EarClip(MeshBuilder mb, Vector3 n, int faceTag)
		{
			int i = 1;
			int failed = 0;
			bool allowSlivers = false;
			bool ignoreInnerVertices = false;
			bool pickAny = false;
			while (count>2 && failed<_loop.Count)
			{
				if (_loop.Count == 3)
				{
					mb.AddTriangle(_loop[2], _loop[1], _loop[0], n, faceTag);
					return;
				}
				
				Vertex p0 = _loop[(i-1+_loop.Count)%_loop.Count];
				Vertex p1 = _loop[i%_loop.Count];
				Vertex p2 = _loop[(i + 1)%_loop.Count];

				Vector3 v1 = p2.point - p1.point;
				Vector3 v0 = p0.point - p1.point;

				failed++; // Assume failure, will reset to zero if it succeeds
				float side = ((Vector2) v0).SideOf(v1, n.z);
				if( side>Vertex.ACCURACY || (side>=0 && allowSlivers) || pickAny)
				{
					bool isEar = true;
					if (!ignoreInnerVertices)
					{
						for (int x = 0; isEar && x < _loop.Count; x++)
						{
							Vertex v = _loop[x];
							if (v.id!=p0.id && v.id!=p1.id && v.id!=p2.id && ((Vector2)v.point).IsInTriangle(p2.point,p1.point, p0.point, n.z))
							{
								isEar = false;
							}
						}
					}

					if (isEar)
					{
						if (_debugTri < 0 || _triCount == _debugTri)
						{
							mb.AddTriangle(p2, p1, p0, n, faceTag);
						}
						_loop.RemoveAt( i%_loop.Count );
						i--;
						_triCount++;
						ignoreInnerVertices = false;
						allowSlivers = false;
						pickAny = false;
						failed = 0;
					}
				}

				if (failed >= _loop.Count)
				{
					if (ignoreInnerVertices)
					{
						Debug.LogWarning("No good triangles, picking the next one whatever it is");
						pickAny = true;
					}
					if (allowSlivers)
					{
						Debug.LogWarning("No good triangles, ignoring inner vertices");
						ignoreInnerVertices = true;
					}
					if(!allowSlivers)
					{
						Debug.LogWarning("No good triangles, allowing slivers");
						allowSlivers = true;
					}
					failed = 0;
				}
				i++;
			}
			if(failed>0) 
				Debug.LogWarning($"Unable to build {failed} triangles for edge loop");
 		}

		public static int DebugTriangle { set { _triCount = 0; _debugTri = value; } }

		private static int _debugTri = -1;
		private static int _triCount;
	}
}