using System.Collections.Generic;
using UnityEngine;

namespace MeshUtil.DataStructures
{
	public class EdgeGraph
	{
		private readonly Dictionary<int, List<Edge>> _edgesByVertex = new ();
		public bool isEmpty => _edgesByVertex.Count == 0;
		public int count => _edgesByVertex.Count;
		
		public void Add(Edge edge)
		{
			if (edge.v0.id == edge.v1.id)
			{
				Debug.LogWarning("Ignoring Zero Length Edge!");
				return;
			}
			if (!_edgesByVertex.TryGetValue(edge.v0.id, out List<Edge> list))
			{
				list = new();
				_edgesByVertex[edge.v0.id] = list;
			}
			list.Add(edge);
		}

		private bool ExtractNext(ref Edge prev, float zup)
		{
			List<Edge> next = null;
			if (prev == null)
			{
				foreach (List<Edge> e in _edgesByVertex.Values)
				{
					prev = e[0];
					e.RemoveAt(0);
					if (e.Count == 0)
						_edgesByVertex.Remove(prev.v0.id);
					return true;
				}
			}
			else
				_edgesByVertex.TryGetValue(prev.v1.id, out next);

			if (next == null || next.Count == 0)
				return false;

			if (next.Count == 1)
			{
				_edgesByVertex.Remove(prev.v1.id);
				//Debug.Log($"ExtractNext(1) {prev.v1.id} : {next[0].v0.id}->{next[0].v1.id}");
				prev = next[0];
				return true;
			}

			float bestangle = 360;
			int besti = -1;
			Vector3 d = prev.v0.point - prev.v1.point;
			for (int i = 0; i < next.Count; i++)
			{
				Edge ne = next[i];

				// Because edge IDs are non directional, the edge going back to where we came from will have the same ID as the one that got us here
				// Such an edge is most likely an internal edge separating two parts of the polygon, we don't want to follow that
				if (ne.id != prev.id) 
				{
					// TODO: Use 2D SideOf
					float angle = Vector3.SignedAngle( d, ne.v1.point - ne.v0.point, zup * Vector3.forward);
					if (angle < 0)
						angle = 360 + angle;
					if (angle < bestangle)
					{
						bestangle = angle;
						besti = i;
					}
				}
			}

			if (besti < 0)
			{
				Debug.LogWarning("No vertex found!");
				return false;
			}
			//Debug.Log($"ExtractNext(2) {prev.v1.id} : {next[besti].v0.id} -> {next[besti].v1.id}");
			prev = next[besti];
			next.RemoveAt(besti);
			return true;
		}

		public List<EdgeLoop> ExtractLoops(float zup)
		{
			List<EdgeLoop> loops = new ();
			while (!isEmpty)
			{
				EdgeLoop loop = new();
				Edge e = null;
				ExtractNext(ref e, zup);
				int start = e.v0.id;
				//Debug.Log($"New VertexLoop from {start}");
				do
				{
					//Debug.Log($"VertexLoop {e.v0.id}->{e.v1.id}");
					loop.Add(e.v0);
					if (!ExtractNext(ref e, zup))
						break;
				} while (e.v1.id != start);
				loop.Add(e.v0);

				//Debug.Log($"VertexLoop Complete");
				loops.Add(loop);
			}
			return loops;
		}
	}
}