namespace MeshUtil.Extensions
{/*
	public static class MeshXYFillNoEdges
	{
		private const int TAG_DELETEDEDGE = int.MaxValue;
		private const float epsilon = 0.5f*Vertex.ACCURACY;
		
		private enum Error { None, 
			TriangleAlreadyExist,
			TriangleIntersectsEdge,
			No3rdVertex
		}

		public static void FillXY(this MeshBuilder mb, int tag, Vector3 n, Action<Vertex,Vertex,Vertex> progressCallback=null)
		{
			// First we collect all edges that were tagged by the slicer as being part of the cut boundary
			// Then, for each edge, we search for a third vertex on the boundary that allow us to create a valid triangle. A triangle is valid if
			// 1. The vertex is on the "open" side of the edge
			// 2. There are no other vertices inside the created triangle
			// 3. There are No edges crossing the triangle
			// Whenever we create a new edge, we add it to the set of remaining edges
			// Whenever we use an existing edge in a triangle, we remove it from the set of remaining edges

			long t = Log.ElapsedTime();
			Dictionary<long,Edge> edges = Edge.All( mb, e => e.v0.tag==tag && e.v1.tag == tag  );
			t = Log.ElapsedTime(t, "Collected Edges");
			
			DebugCurrentLoop = edges.Values;
			progressCallback?.Invoke(default,default,default);

			for(int pass=1;pass<=2;pass++)
			{
				int edgesClosed = 0;
				int edgesTotal = edges.Count;
				bool hasMoreEdges = true;
				while ( hasMoreEdges )
				{
					hasMoreEdges = false;
					foreach (Edge edge in edges.Values)
					{
						if (edge.tag < pass)
						{
							hasMoreEdges = true;
							DebugCurrentEdge = edge;
							progressCallback?.Invoke(default,default,default);
							Error error = CloseEdge(edge, pass==1);
							if (error==Error.None)
							{
								edgesClosed++;
								break; // We modified the edge collection, so need to bail and re-iterate
							}
							edge.tag = pass; // Don't look at this edge again in this pass
							if(pass==2) // Only log errors on last pass
								Debug.LogWarning($"Failed to close edge {edge.id} ({error})");
						}
					}
				}
				t = Log.ElapsedTime(t, $"Pass {pass} closed {edgesClosed} of {edgesTotal} edges.");
			}

			Error CloseEdge(Edge edge, bool firstPass)
			{
				if(edge.GetTriangleCount()!=1)
					Debug.LogError($"Edge has {edge.GetTriangleCount()} triangles (??)");

				// The edge came off of an existing triangle, so from the holes perspective, we need the opposite orientation
				(Vertex v1, Vertex v0) = (edge.v0, edge.v1);

				Vector3 inside = Vector3.Cross( v1.point - v0.point, n).normalized;
				// Center for the vertex collection is the middle of the line (so this defines the "closest" vertex
				Vector3 zero = (v0.point + v1.point) / 2;

				// Build a vertex list sorted by distance to our zero point
				VertexCollection vertices = new VertexCollection( zero );

				void MaybeAdd(Vertex v)
				{
					throw new Exception("I broke this by removing the tag. To fix it, create a VertexTags instance for this instead");
//					v.tag++;
					if (v.id == edge.v0.id || v.id == edge.v1.id)
						return;
					if (v.point.DistanceToLineXY(edge.v0.point, inside) >= 0) // Positive distance along the inside normal means point is on the backside
						return;
					vertices.AddReference(v);
				}

				foreach (Edge e in edges.Values)
				{
					if (e.tag != TAG_DELETEDEDGE)
					{
						MaybeAdd(e.v0);
						MaybeAdd(e.v1);
					}
				}

				Error error = Error.No3rdVertex;
				int cnt = 0;
				foreach (Vertex v2 in vertices.vertices)
				{
					if (firstPass)
					{
						float a0 = Vector3.Dot( (v1.point - v0.point).normalized, (v2.point - v0.point).normalized);
						if (a0 <= -.75f)
							continue;
						float a1 = Vector3.Dot((v0.point - v1.point).normalized, (v2.point - v1.point).normalized);
						if ( a1 <= -.75f )
							continue;
						if (cnt++ > 100)
							return error;
						// First pass only tests the first "nice triangle" vertex we find and bails regardless of whether it works or not.
						return CloseTriangle(vertices, v0, v1, v2);
					}
					// Second pass returns when we succeed ...
					error = CloseTriangle(vertices, v0, v1, v2);
					if(error==Error.None)
						return Error.None;
				}
				// ... or when there are no more vertices
				return error;
			}

			Error CloseTriangle(VertexCollection vertices, Vertex v0, Vertex v1, Vertex v2)
			{
				DebugCurrentV2 = v2;
				progressCallback?.Invoke(default,default,default);

				if ( vertices.GetVertexInsideTriangleXY(v0, v1, v2, n, out Vertex inner))
				{
					v2 = inner;
					DebugCurrentV2 = v2;
					progressCallback?.Invoke(default,default,default);
				}

				Triangle t = new Triangle(-1,v0, v1, v2,n);

				Edge e0 = new Edge(t.v0, t.v1, t);
				Edge e1 = new Edge(t.v1, t.v2, t);
				Edge e2 = new Edge(t.v2, t.v0, t);

				foreach (Edge otheredge in edges.Values)
				{
					if (otheredge.tag!=TAG_DELETEDEDGE && !otheredge.Equals(e0) && !otheredge.Equals(e1) && !otheredge.Equals(e2))
					{
						if (e0.Intersects(otheredge, epsilon))
							return Error.TriangleIntersectsEdge;

						if (e1.Intersects(otheredge, epsilon))
							return Error.TriangleIntersectsEdge;

						if (e2.Intersects(otheredge, epsilon))
							return Error.TriangleIntersectsEdge;
					}
				}
			
				mb.AddTriangle(v0,v1,v2,n);

				if (edges.ContainsKey(e0.id))
				{
					DebugRemovedEdge0 = e0;
					edges[e0.id].tag = TAG_DELETEDEDGE;
				}
				else
				{
					DebugAddedEdge0 = e0;
					edges[e0.id] = e0;
				}

				if (edges.ContainsKey(e1.id))
				{
					DebugRemovedEdge1 = e1;
					edges[e1.id].tag = TAG_DELETEDEDGE;
				}
				else
				{
					DebugAddedEdge1 = e1;
					edges[e1.id] = e1;
				}

				if (edges.ContainsKey(e2.id))
				{
					DebugRemovedEdge2 = e2;
					edges[e2.id].tag = TAG_DELETEDEDGE;
				}
				else
				{
					DebugAddedEdge2 = e2;
					edges[e2.id] = e2;
				}
				DebugCurrentV2 = default;
				DebugCurrentEdge = null;
				progressCallback?.Invoke(default,default,default);
				DebugAddedEdge0 = null;
				DebugAddedEdge1 = null;
				DebugAddedEdge2 = null;
				DebugRemovedEdge0 = null;
				DebugRemovedEdge1 = null;
				DebugRemovedEdge2 = null;
				return Error.None;
			}
		}

		public static Vertex DebugCurrentV2 { get; set; }
		public static Vertex DebugCheckVertex { get; set; }
		public static Edge DebugCurrentEdge { get; set; }
		public static Edge DebugRemovedEdge0 { get; set; }
		public static Edge DebugRemovedEdge1 { get; set; }
		public static Edge DebugRemovedEdge2 { get; set; }
		public static Edge DebugAddedEdge0 { get; set; }
		public static Edge DebugAddedEdge1 { get; set; }
		public static Edge DebugAddedEdge2 { get; set; }
		public static ICollection<Edge> DebugCurrentLoop { get; set; }
	}*/
}