using System.Collections.Generic;
using MeshUtil.DataStructures;
using UnityEngine;
using Utility;

namespace MeshUtil.Extensions
{
	public static class MeshTrapezoidFill
	{
		public static void FillTrapezoid(this MeshBuilder mb, int boundaryTag, Vector3 n, int faceTag=0)
		{
			n.x = 0;
			n.y = 0;
			n.Normalize();
			long t = Log.ElapsedTime();
			Dictionary<long,Edge> edges = Edge.All( mb, e => e.v0.tag==boundaryTag && e.v1.tag == boundaryTag  );
			t = Log.ElapsedTime(t, $"Collected {edges.Count} Edges");
/*
			StringBuilder sb = new StringBuilder("Vector3[] edgeVerts = {");
			foreach (Edge edge in edges.Values)
			{
				sb.Append($"new ({edge.v0.point.x}f,{edge.v0.point.y}f,{edge.v0.point.z}f),");
				sb.AppendLine($"new ({edge.v1.point.x}f,{edge.v1.point.y}f,{edge.v1.point.z}f),");
			}
			sb.AppendLine($"}};\n Vector3 edgeNormal = new ({n.x}f,{n.y}f,{n.z}f);");
			Debug.Log(sb.ToString());
*/

			TrapezoidMap trapezoids = new TrapezoidMap(edges.Values);
			t = Log.ElapsedTime(t, $"Generated Trapezoids");

			EdgeGraph edgeGraph = trapezoids.CreateInnerEdges(edges, n.z);
			foreach (Edge edge in edges.Values)
				edgeGraph.Add(edge);
			t = Log.ElapsedTime(t, $"Generated {edgeGraph.count-edges.Count} Inner Edges");

			List<EdgeLoop> edgeLoops = edgeGraph.ExtractLoops(n.z);
			t = Log.ElapsedTime(t, $"Extracted {edgeLoops.Count} Loops");

			foreach (EdgeLoop loop in edgeLoops)
				loop.EarClip(mb, n, faceTag);
			t = Log.ElapsedTime(t, "EarClipped Simple Polygons");
		}		
	}
}