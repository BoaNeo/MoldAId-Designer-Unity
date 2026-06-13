using System.Collections.Generic;
using System.Text;
using MeshUtil;
using MeshUtil.DataStructures;
using UnityEngine;

namespace Test
{
	public class TrapezoidTest : MonoBehaviour
	{
		[SerializeField] private bool _run;
		[SerializeField] private bool _runAuto;
		[SerializeField] private Transform _outer;
		[SerializeField] private Transform _inner;
		[SerializeField] private Transform _verts;
		[SerializeField] private MeshFilter _meshFilter;
		[SerializeField] private int _debugTri;
		[SerializeField] private float _zup;
		[SerializeField] private bool _autoZup;
		[SerializeField] private int _importedData=-1;
		[SerializeField] private int _drawTrapezoid; 
		[SerializeField] private bool _drawHorizontals;
		[SerializeField] private bool _drawVerticals;
		[SerializeField] private bool _drawEdgeLoops;
		[SerializeField] private bool _drawEdges;
		
		private TrapezoidMap _trapezoids;
		private Dictionary<long,Edge> _edges;
		private EdgeGraph _edgeGraph;
		private List<EdgeLoop> _edgeLoops;

		private int _vid;
	/*	
		private Vector3[] a0 =
		{
			new(-1f, 0f, 0f), new(-1f, -1f, 0f),
			new(-1f, 1f, 0f), new(-1f, 0f, 0f),
			new(0f, 1f, 0f), new(-1f, 1f, 0f),
			new(1f, 1f, 0f), new(0f, 1f, 0f),
			new(1f, 0f, 0f), new(1f, 1f, 0f),
			new(1f, -1f, 0f), new(1f, 0f, 0f),
			new(0f, -1f, 0f), new(1f, -1f, 0f),
			new(-1f, -1f, 0f), new(0f, -1f, 0f),
			new(-0.4f, -0.4f, 0f), new(-0.4f, 0f, 0f),
			new(-0.4f, 0f, 0f), new(-0.4f, 0.4f, 0f),
			new(0.4f, -0.4f, 0f), new(0f, -0.4f, 0f),
			new(0f, -0.4f, 0f), new(-0.4f, -0.4f, 0f),
			new(-0.4f, 0.4f, 0f), new(0f, 0.4f, 0f),
			new(0f, 0.4f, 0f), new(0.4f, 0.4f, 0f),
			new(0.4f, 0.4f, 0f), new(0.4f, 0f, 0f),
			new(0.4f, 0f, 0f), new(0.4f, -0.4f, 0f),
		};
*/
		private Vector3[] a0 =
		{
			new(-1f, -1f, 0f),new(-1f, 0f, 0f), 
			new(-1f, 0f, 0f),new(-1f, 1f, 0f), 
			new(-1f, 1f, 0f),new(0f, 1f, 0f),
			new(0f, 1f, 0f),new(1f, 1f, 0f), 
			new(1f, 1f, 0f),new(1f, 0f, 0f), 
			new(1f, 0f, 0f),new(1f, -1f, 0f), 
			new(1f, -1f, 0f),new(0f, -1f, 0f), 
			new(0f, -1f, 0f),new(-1f, -1f, 0f), 
			new(-0.4f, 0f, 0f),new(-0.4f, -0.4f, 0f), 
			new(-0.4f, 0.4f, 0f),new(-0.4f, 0f, 0f), 
			new(0f, -0.4f, 0f),new(0.4f, -0.4f, 0f),
			new(-0.4f, -0.4f, 0f),new(0f, -0.4f, 0f), 
			new(0f, 0.4f, 0f),new(-0.4f, 0.4f, 0f), 
			new(0.4f, 0.4f, 0f),new(0f, 0.4f, 0f), 
			new(0.4f, 0f, 0f),new(0.4f, 0.4f, 0f), 
			new(0.4f, -0.4f, 0f),new(0.4f, 0f, 0f), 
		};
		
		private Vector3[] a1 =
		{
			new(-0.6433453f, -1.426818f, 0f), new(-1.129782f, -0.10396f, 0f),
			new(-1.129782f, -0.10396f, 0f), new(-1.34886f, 0.49182f, 0f),
			new(-1.34886f, 0.49182f, 0f), new(-0.4971417f, 0.8915553f, 0f),
			new(-0.4971417f, 0.8915553f, 0f), new(0.6433453f, 1.426818f, 0f),
			new(0.6433453f, 1.426818f, 0f), new(0.7907111f, 1.026058f, 0f),
			new(0.7907111f, 1.026058f, 0f), new(1.34886f, -0.49182f, 0f),
			new(1.34886f, -0.49182f, 0f), new(-0.03672894f, -1.142116f, 0f),
			new(-0.03672894f, -1.142116f, 0f), new(-0.6433453f, -1.426818f, 0f),
			new(-0.4223321f, -0.1220279f, 0f), new(-0.2573381f, -0.5707272f, 0f),
			new(-0.5395438f, 0.196728f, 0f), new(-0.4223321f, -0.1220279f, 0f),
			new(0.09981281f, -0.4031062f, 0f), new(0.5395438f, -0.196728f, 0f),
			new(-0.2573381f, -0.5707272f, 0f), new(0.09981281f, -0.4031062f, 0f),
			new(-0.1695807f, 0.3703621f, 0f), new(-0.5395438f, 0.196728f, 0f),
			new(0.2573381f, 0.5707272f, 0f), new(-0.1695807f, 0.3703621f, 0f),
			new(0.3701339f, 0.2639802f, 0f), new(0.2573381f, 0.5707272f, 0f),
			new(0.5395438f, -0.196728f, 0f), new(0.3701339f, 0.2639802f, 0f),
		};

		Vector3[] a2 = 
		{
			new (-0.2484509f,-1.566079f,0f),new (-1.42332f,-0.06567404f,0f),
			new (-0.09867626f,1.617958f,0f),new (0.04835939f,1.595983f,0f),
			new (0.04835939f,1.595983f,0f),new (0.2484509f,1.566079f,0f),
			new (0.2484509f,1.566079f,0f),new (1.041622f,0.5531341f,0f),
			new (1.041622f,0.5531341f,0f),new (1.42332f,0.06567404f,0f),
			new (0.09867626f,-1.617958f,0f),new (-0.2484509f,-1.566079f,0f),
			new (1.13302f,-0.3032995f,0f),new (0.9798415f,-0.4979903f,0f),
			new (1.42332f,0.06567404f,0f),new (1.13302f,-0.3032995f,0f),
			new (-0.9798415f,0.4979903f,0f),new (-0.09938037f,-0.6264316f,0f),
			new (-1.42332f,-0.06567404f,0f),new (-1.13302f,0.3032995f,0f),
			new (-1.13302f,0.3032995f,0f),new (-0.9798415f,0.4979903f,0f),
			new (-0.3895945f,1.248199f,0f),new (-0.09867626f,1.617958f,0f),
			new (-0.7758707f,0.7572387f,0f),new (-0.3895945f,1.248199f,0f),
			new (0.7758707f,-0.7572387f,0f),new (0.3895945f,-1.248199f,0f),
			new (0.3895945f,-1.248199f,0f),new (0.09867626f,-1.617958f,0f),
			new (-0.09938037f,-0.6264316f,0f),new (0.7758707f,-0.7572387f,0f),
			new (-0.2091211f,0.6725374f,0f),new (-0.7758707f,0.7572387f,0f),
			new (0.09938037f,0.6264316f,0f),new (-0.2091211f,0.6725374f,0f),
			new (0.4990312f,0.1160445f,0f),new (0.09938037f,0.6264316f,0f),
			new (0.9798415f,-0.4979903f,0f),new (0.4990312f,0.1160445f,0f),
		};

		private void CreateTrapezoids()
		{
			if (!_run)
				return;

			_edges = new ();
			_vid = 100;

			if (_importedData > -1)
			{
				Vector3[][] edgeVerts = {a0, a1, a2};
				Vector3[] edgeNormals ={new (0f,0f,-1f),new (0f,0f,-1f),new (0f,0f,-1f)};

				CreateVertsAndEdges(_verts, edgeVerts[_importedData], edgeNormals[_importedData]);
			}
			else
			{
				CreateEdges(_outer);
				CreateEdges(_inner);
			}

			if (_drawEdges)
			{
				foreach (Edge edge in _edges.Values)
				{
					UnityEngine.Gizmos.color = Color.blue;
					UnityEngine.Gizmos.DrawLine(edge.v0.point, edge.v1.point);
				}
			}

			if(_zup<0)
			{
				foreach (Edge edge in _edges.Values)
				{
					(edge.v0, edge.v1) = (edge.v1, edge.v0);
				}
			}
			
			_trapezoids = new TrapezoidMap(_edges.Values);

			_edgeGraph = _trapezoids.CreateInnerEdges(_edges, _zup);
			foreach (Edge edge in _edges.Values)
				_edgeGraph.Add(edge);

			_edgeLoops = _edgeGraph.ExtractLoops(_zup);

			StringBuilder sb = new StringBuilder();
			if (_drawEdgeLoops)
			{
				sb.AppendLine("Loops:");			
				foreach (EdgeLoop loop in _edgeLoops)
				{
					for (int i = 0; i < loop.count; i++)
						sb.AppendFormat("{0} ",loop[i].id);
					sb.AppendLine("");
					loop.DebugDraw(Color.white, Matrix4x4.identity);
				}
			}
			
			MeshBuilder mb = new MeshBuilder();

			EdgeLoop.DebugTriangle = _debugTri<0 ? _debugTri : _debugTri + mb.triangleCount;
			foreach (EdgeLoop loop in _edgeLoops)
				loop.EarClip(mb, _zup*Vector3.forward, 1);

			if (_meshFilter != null)
			{
				Mesh.MeshDataArray meshData = Mesh.AllocateWritableMeshData(1);
				mb.FlatShade(meshData);
				Mesh.ApplyAndDisposeWritableMeshData(meshData,_meshFilter.mesh);
			}

			float size = 10;

			(Vertex min, Vertex max, Edge lower, Edge upper) = TrapezoidMap.CreateBoundingBox(-size, size, -size, size, _zup);
			
			sb.AppendLine("");
			DrawNode(0, 0, _trapezoids, min, max, lower, upper, sb);

			if (sb.Length > 0)
				Debug.Log(sb.ToString());
			_run = _runAuto;
		}

		private void CreateVertsAndEdges(Transform verts, Vector3[] edgeVerts, Vector3 edgeNormal)
		{
			KdTree vertices = new KdTree();
			for (int i = 0; i < edgeVerts.Length;)
			{
				Vertex v0 = vertices.AddOrGet(edgeVerts[i++],0);
				Vertex v1 = vertices.AddOrGet(edgeVerts[i++],0);

				Edge e = new Edge(v0, v1);

				_edges[e.id] = e;

				while (verts.childCount<=v0.id || verts.childCount<=v1.id)
				{
					GameObject go = new GameObject();
					go.transform.SetParent(verts);
				}

				SetVert(verts,v0);
				SetVert(verts,v1);
			}
			if(_autoZup)
				_zup = edgeNormal.z;
		}

		private void SetVert(Transform verts, Vertex v0)
		{
			Transform ob = verts.GetChild(v0.id);
			ob.name = $"Vertex {v0.id}";
			ob.position = v0.point;
		}

		private void CreateEdges(Transform container)
		{
			Vertex v1 = default;
			Vertex first = default;
			for(int i=0;i<=container.childCount;i++)
			{
				Vertex v0 = v1;
				if (i < container.childCount)
				{
					Transform child = container.GetChild(i);
					v1 = new Vertex(_vid++, child.position, 0);
					child.name = $"Vertex {v1.id}";
					VertexGO go = child.GetComponent<VertexGO>();
					if (go != null)
						go.vertexId = v1.id;
				}
				else v1 = first;
				
				if (i > 0 && !v0.AlmostEqual(v1))
				{
					Edge edge = new Edge(v0, v1);
					_edges[edge.id] = edge;
				}
				else first = v1;
			}
		}

		private void OnDrawGizmos()
		{
			CreateTrapezoids();
		}
		
		private int DrawNode(int i, int n, TrapezoidMap node, Vertex min, Vertex max, Edge lower, Edge upper, StringBuilder sb)
		{
			if (node == null)
				return n;

			//Debug.Log($"Drawing {node.debugName}");
			if (node.vertices != null)
			{
				Vertex v = node.vertices[0];
				if (_drawVerticals)
				{
					float x = v.point.x;
					UnityEngine.Gizmos.color = Color.cyan;
					UnityEngine.Gizmos.DrawLine(new Vector3(x,-100),new Vector3(x,100));
				}
				PrintWithIndent(i, sb, $"Split V({v.id})");
				n = DrawNode(i+1,n,node.low,min, v,lower, upper, sb);
				n = DrawNode(i+1,n,node.high,v, max,lower, upper, sb);
			}
			else if( node.edge!=null)
			{
				if (_drawHorizontals)
				{
					UnityEngine.Gizmos.color = Color.cyan;
					UnityEngine.Gizmos.DrawLine(node.edge.v0.point, node.edge.v1.point);
				}
				PrintWithIndent(i, sb, $"Split H({node.edge.v0.id}->{node.edge.v1.id})");
				n = DrawNode(i+1,n, node.low,min, max,lower, node.edge, sb);
				n = DrawNode(i+1,n, node.high,min, max,node.edge, upper, sb);
			}
			else
			{
				/*
					if (min.id != 0 && max.id != 0)
					{
						if (min.point.y >= lower.GetYForX(min.point.x) && min.point.y <= upper.GetYForX(min.point.x) &&
						    max.point.y >= lower.GetYForX(max.point.x) && max.point.y <= upper.GetYForX(max.point.x))
						{
							Gizmos.color = Color.magenta;
							Gizmos.DrawLine(min.point,max.point);
						}
					}*/
				n++;
				PrintWithIndent(i, sb, $"Node {n} ({min.id} -> {max.id}) Edge:{lower.v0.id},{lower.v1.id} -> {upper.v0.id},{upper.v1.id}");
				if (_drawTrapezoid==-1 || _drawTrapezoid == n)
				{
					float xmin = min.point.x;
					float xmax = max.point.x;
					float y0 = upper.GetYForX(xmin);
					float y1 = upper.GetYForX(xmax);
					float y2 = lower.GetYForX(xmax);
					float y3 = lower.GetYForX(xmin);

					Vector3 v0 = new Vector3(xmin, y0, 0);
					Vector3 v1 = new Vector3(xmax, y1, 0);
					Vector3 v2 = new Vector3(xmax, y2, 0);
					Vector3 v3 = new Vector3(xmin, y3, 0);

					
					float margin = 0.0f;
					Vector3 p0 = v0 + margin * new Vector3((v1 - v0).normalized.x , (v3 - v0).normalized.y);
					Vector3 p1 = v1 + margin * new Vector3((v0 - v1).normalized.x , (v2 - v1).normalized.y);
					Vector3 p2 = v2+ margin * new Vector3((v3 - v2).normalized.x , (v1 - v2).normalized.y);
					Vector3 p3 = v3 + margin * new Vector3((v2 - v3).normalized.x , (v0 - v3).normalized.y);

					//					if(node.side == TrapezoidMap.Side.Inside)
					//				Gizmos.color = Color.green;
					//				else
					UnityEngine.Gizmos.color = Color.red;

					UnityEngine.Gizmos.DrawLine( p0, p1);
					UnityEngine.Gizmos.DrawLine( p1, p2);
					UnityEngine.Gizmos.DrawLine( p2, p3);
					UnityEngine.Gizmos.DrawLine( p3, p0);
				}
			}
			return n;
		}

		private void PrintWithIndent(int i, StringBuilder sb, string p2)
		{
			while (i > 0)
			{
				sb.Append(" ");
				i--;
			}
			sb.AppendLine(p2);
		}
	}
}