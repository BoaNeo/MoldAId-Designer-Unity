using System;
using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace MeshUtil.DataStructures
{
	public class TrapezoidMap
	{
		public List<Vertex> vertices; // The vertices that splits this nodes sub space horizontally (on the X-axis in a left and a right), or null if not split by a vertex (only one of edge and vertices are non null)
		public Edge edge;							// The edge that splits this nodes sub space vertically (on the Y-axis in an upper and a lower), or null if not split by an edge (only one of edge and vertices are non null)

		public TrapezoidMap low;			// Lower value sub space
		public TrapezoidMap high;			// Higher value sub space

		public float xmax;						// Right edge of the trapezoid

		private readonly Dictionary<float,List<Vector3>> _cuts; // TODO: This is really a root-node only property. Could return it as an "out" from the constructor but feels icky.

		private TrapezoidMap() {}
		
		/// <summary>
		/// Create a 2D trapezoid map of a simple polygon. The polygon may have holes, but not self intersections.
		/// It is assumed that all vertices are in the X/Y plane with Z=0
		/// </summary>
		/// <param name="edges">Edges that describe the polygon. Counter-Clockwise edge loops defines the shape, clockwise defines holes</param>
		/// <param name="zup">Defines the Z direction to which the edge orientation relates</param>
		public TrapezoidMap(ICollection<Edge> edges)
		{
			Dictionary<float, List<Vector3>> cuts = new();
			Dictionary<int, Vertex> vertices = new Dictionary<int, Vertex>();
			xmax = Single.MaxValue;

			// Vertical splits first so we get all same-x splits bundled together
			foreach (Edge edge in edges)
			{
				Vertex v = edge.v0;
				TrapezoidMap start = FindTrapezoid(v.point, false);
				if (!vertices.ContainsKey(v.id))
				{
					vertices[v.id] = v;
					start.Split(v);
				}
			}
			// Horizontal splits
			foreach (Edge edge in edges)
			{
				if (edge.v0.id == edge.v1.id)
				{
					Debug.LogWarning("Skipping Zero Length Edge! Vertices should have been merged in source");
					continue;
				}
				
				(Vertex l, Vertex r) = (edge.v0,edge.v1);

				if (l.point.x > r.point.x || (l.point.x==r.point.x && l.point.y<r.point.y))
					(l, r) = (r, l);

				if (l.point.x < r.point.x)
				{
					Vector3 d = (r.point - l.point).normalized;
					Vector3 p = l.point + Vertex.ACCURACY * d; // Need to step in a bit to avoid hitting a corner and then proceeding outside the trapezoid
					while (p.x<r.point.x)
					{
						TrapezoidMap node = FindTrapezoid(p);
						node.Split(edge);
						float t = (node.xmax-l.point.x)/d.x ;
						p = l.point + (t + Vertex.ACCURACY) * d;
						p.x = node.xmax;
						RegisterCut(cuts,p);
					}
				}
			}

			_cuts = cuts;
		}

		private void RegisterCut(Dictionary<float, List<Vector3>> cuts, Vector3 p)
		{
			if (!cuts.TryGetValue(p.x, out List<Vector3> list))
			{
				list = new();
				cuts[p.x] = list;
			}
			list.Add(p);
		}

		/// <summary>
		/// Split this node vertically at a specific vertex
		/// </summary>
		/// <param name="vertex"></param>
		private void Split(Vertex vertex)
		{
			if (vertices == null)
			{
				vertices = new();
				low = new TrapezoidMap { xmax = vertex.point.x};
				high = new TrapezoidMap { xmax = xmax};
			}
			vertices.Add( vertex);
		}

		/// <summary>
		/// Split this node along a horizontal edge
		/// </summary>
		/// <param name="edge">Split edge</param>
		private void Split(Edge edge)
		{
			this.edge = edge;
			low = new TrapezoidMap {xmax = xmax};
			high = new TrapezoidMap {xmax = xmax};
		}

		/// <summary>
		/// Find a trapezoid that contains a specific point
		/// </summary>
		/// <param name="p">Point to search for</param>
		/// <param name="onlyLeaves">True to only retuen leaves. If false, this method will return vertical splits if the point is an exact match</param>
		/// <returns>Node that matches provided point</returns>
		public TrapezoidMap FindTrapezoid(Vector3 p, bool onlyLeaves=true)
		{
			if (vertices != null)
			{
				Vertex v = vertices[0]; // Don't care which - they all have the same X coord
				if (p.x < v.point.x)
					return low.FindTrapezoid(p, onlyLeaves);
				if (p.x > v.point.x || onlyLeaves)
					return high.FindTrapezoid(p, onlyLeaves);
				// Allow fall through to return a split node if onlyLeaves is false. Used when splitting vertically so identical splits can be grouped.
			}
			if (edge != null)
			{
				float s;
				// We care about above/below edge, not inside/outside, so always orient the edge from left to right
				if(edge.v1.point.x>edge.v0.point.x)
					s = ((Vector2)(p - edge.v0.point)).SideOf(edge.v1.point - edge.v0.point);
				else
					s = ((Vector2)(p - edge.v0.point)).SideOf(edge.v0.point - edge.v1.point);
				if (s>0)
					return low.FindTrapezoid(p, onlyLeaves);
				return high.FindTrapezoid(p, onlyLeaves);
			}
			return this;
		}

		/// <summary>
		/// Recursively collect inner edges and add to an EdgeGraph.
		/// </summary>
		/// <param name="edges">EdgeGrap where edges will be added to</param>
		/// <param name="outeredges">Edges that will not be collected</param>
		/// <param name="zup">Normal of X/Y plane for the vertices</param>
		/// <param name="min">List of vertices on the left edge of the parent trapezoid</param>
		/// <param name="max">List of vertices on the right edge of the parent trapezoid</param>
		/// <param name="upper">Upper edge of parent trapezoid</param>
		/// <param name="lower">Lower edge of parent trapezoid</param>
		private void CollectInnerEdges(EdgeGraph edges, Dictionary<long, Edge> outeredges, Dictionary<float,List<Vector3>> cuts, float zup, List<Vertex> min, List<Vertex> max, Edge upper, Edge lower)
		{
			if (vertices != null)
			{
				low?.CollectInnerEdges(edges,outeredges,cuts,zup,min, vertices, upper,lower);
				high?.CollectInnerEdges(edges,outeredges,cuts,zup,vertices, max, upper,lower);
			}
			else if(edge!=null)
			{
				low?.CollectInnerEdges(edges,outeredges,cuts,zup,min, max, edge,lower);
				high?.CollectInnerEdges(edges,outeredges,cuts,zup,min, max, upper,edge);
			}
			else
			{	
				float dx1 = zup*(lower.v1.point.x - lower.v0.point.x);
				if (dx1 < 0)
				{
					// Find inner edge between the left and the right side of the trapezoid (note that min and max *can* be the same X)
					foreach (Vertex left in min)
					{
						// only care about points inside the trapezoid
						if ( left.point.y >= lower.GetYForX( left.point.x) && left.point.y <= upper.GetYForX( left.point.x))
						{
							foreach (Vertex right in max)
							{
								if ( right.point.y >= lower.GetYForX( right.point.x) && right.point.y <= upper.GetYForX( right.point.x))
								{
									long edgeId = Edge.CalcIdAndHash(left, right).Item1;
									if (!outeredges.ContainsKey(edgeId))
									{
										if ( Mathf.Abs(left.point.x - right.point.x)<Vertex.ACCURACY && (PointsOnVerticalLine(min, left, right) || PointsOnVerticalLine(max, left, right) || CutsOnVerticalLine(cuts, left, right))) 
										{
											// Vertical edges are problematic because the outer edge test doesn't always work on them -
											// if there are multiple vertices in a vertical split, we can end up with a new edge that covers several existing edges.
											// We basically don't want an edge that goes through another vertex
											continue;
										}

										edges.Add(new Edge(left, right));
										edges.Add(new Edge(right, left));
										//Debug.DrawLine(left.point,right.point, Color.yellow,1);
										
										// We found a suitable inner edge and a trapezoid can have only one, so let's hit the road
										return;
									}
								}
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Check if any of the points in the list lie on the vertical line from l0 to l1 (I.e. assumes that all pts, l0 and l1 have the same x coordinate)
		/// </summary>
		/// <param name="pts">List of points to search</param>
		/// <param name="l0">One end of edge to search within</param>
		/// <param name="l1">Other end of edge to search within</param>
		/// <returns></returns>
		private bool PointsOnVerticalLine(ICollection<Vertex> pts, Vertex l0, Vertex l1)
		{
			foreach (Vertex pt in pts)
			{
				if (pt.id != l0.id && pt.id != l1.id)
				{
					float t = (pt.point.y - l0.point.y)/(l1.point.y - l0.point.y);
					if (t >= 0 && t <= 1)
						return true;
				}
			}
			return false;
		}

		private bool CutsOnVerticalLine(Dictionary<float, List<Vector3>> cuts, Vertex l0, Vertex l1)
		{
			if (cuts.TryGetValue(l0.point.x, out List<Vector3> list))
			{
				foreach (Vector3 pt in list)
				{
					float t = (pt.y - l0.point.y)/(l1.point.y - l0.point.y);
					if (t >= 0 && t <= 1)
						return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Create a new edge graph and prefill it with (one mutation of) all the inner edges from this trapezoid map.
		/// </summary>
		/// <param name="outeredges">The outer edges that defined the TrapezoidMap - The edges will be skipped in the Inner Edge graph</param>
		/// <param name="zup">Normal direction for the X/Y plane in which this map is defined (effectively defines front and back faces)</param>
		/// <returns></returns>
		public EdgeGraph CreateInnerEdges(Dictionary<long, Edge> outeredges, float zup)
		{
			EdgeGraph innerEdges = new();

			float size = 100000; // TODO: Use AABB and build this while building the map
			(Vertex left, Vertex right, Edge lower, Edge upper) = CreateBoundingBox(-size, size, -size, size,zup);

			List<Vertex> min = new();
			min.Add(left);
			List<Vertex> max = new();
			max.Add(right);

			CollectInnerEdges(innerEdges, outeredges, _cuts, zup,min,max,upper,lower);
			return innerEdges;
		}

		/// <summary>
		/// Helper method to generate a correct bounding box for a Trapezoid map, with a left and a right point, and an
		/// upper and a lower edge. Ensures the edges are oriented correctly for the given normal.
		/// </summary>
		/// <param name="xmin">Minimum X of bounding box</param>
		/// <param name="xmax">Maximum X of bounding box</param>
		/// <param name="ymin">Minimum Y of bounding box</param>
		/// <param name="ymax">Maximum Y of bounding box</param>
		/// <param name="zup">Normal direction in X/Y plane</param>
		/// <returns>Left, right and lower/upper edges of bounding box</returns>
		public static (Vertex, Vertex, Edge, Edge) CreateBoundingBox(float xmin, float xmax, float ymin, float ymax, float zup)
		{
			Vertex v0 = new Vertex(100000000, new Vector3(xmin, ymin, 0), 0);
			Vertex v1 = new Vertex(100000001, new Vector3(xmax, ymin, 0), 0);
			Vertex v2 = new Vertex(100000002, new Vector3(xmin, ymax, 0), 0);
			Vertex v3 = new Vertex(100000003, new Vector3(xmax, ymax, 0), 0);

			Edge lower = zup>0 ? new Edge(v0, v1) : new Edge(v1,v0);
			Edge upper = zup>0 ? new Edge(v3, v2) : new Edge(v2,v3);

			return (v0, v3, lower, upper);
		}
	}
}