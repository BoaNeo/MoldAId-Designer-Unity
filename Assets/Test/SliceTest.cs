using System;
using System.Threading;
using MeshUtil;
using MeshUtil.Extensions;
using TMPro;
using UnityEngine;
using Utility;

namespace Test
{
	public class SliceTest : MonoBehaviour
	{
		[SerializeField] private TMP_Text _text;
		[SerializeField] private Transform _cutplane;
		[SerializeField] private MeshFilter _source;
		[SerializeField] private MeshFilter _high;
		[SerializeField] private MeshFilter _lo;
		[SerializeField] private bool _rotate;
		[SerializeField] private bool _auto;
		[SerializeField] private bool _slice;
		[SerializeField] private bool _singleStep;
		private MeshBuilder _loaded;
		private WorkerThread _worker;
		private bool _busy;

		private void Start()
		{
		
//		_loaded = new MeshBuilder().Import("/Users/mrmac/Documents/MoldGenerator/stl samples/442L_OutputModel_PRECAST.stl",Matrix4x4.identity);
//		_loaded = new MeshBuilder().Import("/Users/mrmac/Documents/MoldGenerator/stl samples/420.1 - Hynds smart cover G2 housing opt 4 C Class.STL",Matrix4x4.identity);
//		_loaded = new MeshBuilder().Import("/Users/mrmac/Documents/MoldGenerator/stl samples/Oshape.stl",Matrix4x4.identity);
//		_loaded = new MeshBuilder().Import("/Users/mrmac/Documents/MoldGenerator/stl samples/Cavity.stl",Matrix4x4.identity);
		_loaded = new MeshBuilder().Import("/Users/mrmac/Documents/MoldGenerator/stl samples/460 - Brose Gearwheel part.STL");
//		_loaded = new MeshBuilder().CloneFrom(_source.mesh);

			//Matrix4x4 xform = _cutplane.worldToLocalMatrix;
			//float t0 = Time.realtimeSinceStartup;
			//_loaded.Transform(xform);
			//t0 = Time.realtimeSinceStartup - t0;
			//Debug.Log($"Transform time={t0*1000}ms");

			//_mb.BackupVertices();
			Apply(_loaded,Matrix4x4.identity,_source.mesh);

			_worker = gameObject.AddComponent<WorkerThread>();
		}

//		private int _rotationIndex = 0;
		private bool _pause;
		private void Update()
		{
//		if (_auto && _rotate)
			Vector3[] euler =
			{
				new Vector3(55.22834f, 71.54482f, 211.0928f),
			};
			if (!_busy)
			{
				if (_slice)
				{
					_slice = _auto;
					_busy = true;
					Matrix4x4 xform = _cutplane.worldToLocalMatrix;
					Matrix4x4 xformInv = _cutplane.localToWorldMatrix;

					if (_rotate)
					{
//					transform.rotation = Quaternion.Euler(euler[ (_rotationIndex++)%euler.Length ]);
						transform.RotateAround( transform.position, Vector3.left, 1);
					}
					Debug.Log($"Starting new background with cutplane {VectorExtension.ToAccurateString(_cutplane.transform.position)} : {VectorExtension.ToAccurateString(_cutplane.transform.rotation.eulerAngles)}");

					MeshBuilder mb = _loaded.TransformGPU(xform);
					long t = Log.ElapsedTime();
//					mb.Slice( 1, out MeshBuilder m1, out MeshBuilder m2);
//					ComputeShaderExtension ce = mb.SliceGPU( Matrix4x4.identity, 1);
					t = Log.ElapsedTime(t, "Sliced");

					_worker.RunInBackground(() =>
					{
	//					mb.SliceGPUFinish( ce, out MeshBuilder m1, out MeshBuilder m2);
						/*
						try
						{
							//_mb.MergeVertices();
							StepWiseFill(_singleStep,m2, _lo, Vector3.back, xformInv);
							t = Log.ElapsedTime(t, "Fill1");
						
						// Works:
							StepWiseFill(_singleStep,m1, _high, Vector3.forward, xformInv);
							t = Log.ElapsedTime(t, "Fill2");
						}
						catch (Exception e)
						{
							Debug.LogError($"Hole FIlling failed with {e}");
							_auto = false;
							_slice = false;
							_rotate = false;
						}*/
						_worker.RunOnMain(() =>
						{
		//					Apply(m1, xformInv, _high.mesh);
			//				Apply(m2, xformInv, _lo.mesh);
							_busy = false;
						});
					});
				}
			}
		}

		private void Apply(MeshBuilder m1, Matrix4x4 xform, Mesh mesh)
		{
			if (m1.changed)
			{
				Mesh.MeshDataArray meshData = Mesh.AllocateWritableMeshData(1);
				if(xform!=default && !xform.isIdentity)
					m1 = m1.TransformGPU(xform);
				m1.FlatShade(meshData);
				Mesh.ApplyAndDisposeWritableMeshData(meshData,mesh);
			}
		}

		private Color[] boundarycolors = { Color.red, Color.blue, Color.green, Color.yellow, Color.magenta, Color.cyan, Color.white, Color.black };
		private int _break;
		[SerializeField] private bool _trapezoid;

		private void StepWiseFill(bool singlestep, MeshBuilder m1, MeshFilter target, Vector3 forward, Matrix4x4 xformInv)
		{
			if(_trapezoid)
				m1.FillTrapezoid(1, forward);
			/*
			else
				m1.FillXY(1,forward, singlestep?(Vertex v0,Vertex v1,Vertex v2) =>
			{
				if(_singleStep)
					_pause = true;
				_worker.RunOnMain(() =>
				{
					Apply(m1,xformInv,target.mesh);
					float scale = 100.0f;
					int c = 0;
					foreach (Edge edge in MeshXYFillNoEdges.DebugCurrentLoop)
					{
						Color col = Color.black;
						if(Equals(edge, MeshXYFillNoEdges.DebugCurrentEdge))
							col = Color.white;
						edge.DebugDraw(col, scale, xformInv);
					}
					if (MeshXYFillNoEdges.DebugCurrentEdge != null)// && MeshXYFillNoEdges.DebugCurrentV2!=null)
					{
						Debug.DrawLine( scale*xformInv.MultiplyPoint(MeshXYFillNoEdges.DebugCurrentEdge.v0.point), scale*xformInv.MultiplyPoint(MeshXYFillNoEdges.DebugCurrentV2.point),Color.yellow);
						Debug.DrawLine( scale*xformInv.MultiplyPoint(MeshXYFillNoEdges.DebugCurrentEdge.v1.point), scale*xformInv.MultiplyPoint(MeshXYFillNoEdges.DebugCurrentV2.point),Color.yellow);
					}
					MeshXYFillNoEdges.DebugRemovedEdge0?.DebugDraw(Color.red, scale, xformInv);
					MeshXYFillNoEdges.DebugRemovedEdge1?.DebugDraw(Color.red, scale, xformInv);
					MeshXYFillNoEdges.DebugRemovedEdge2?.DebugDraw(Color.red, scale, xformInv);
					MeshXYFillNoEdges.DebugAddedEdge0?.DebugDraw(Color.green, scale, xformInv);
					MeshXYFillNoEdges.DebugAddedEdge1?.DebugDraw(Color.green, scale, xformInv);
					MeshXYFillNoEdges.DebugAddedEdge2?.DebugDraw(Color.green, scale, xformInv);
//					if (MeshXYFillNoEdges.DebugCheckVertex != null)
					{
						DebugExtension.DrawBox(scale*xformInv.MultiplyPoint(MeshXYFillNoEdges.DebugCheckVertex.point),scale*0.2f, Color.red);
					}
//					if (v0 != null)
					{
						Debug.Log($"Added Triangle {v0.id}:{v0.point.ToAccurateString()} , {v1.id}:{v1.point.ToAccurateString()} , {v2.id}:{v2.point.ToAccurateString()}");
						_text.text = $"Triangle {v0.id}:{v0.point.ToAccurateString()} , {v1.id}:{v1.point.ToAccurateString()} , {v2.id}:{v2.point.ToAccurateString()}";
					}
					_break = 1;
				});
				while(_pause)
					Thread.Sleep(1);
			}:(Action<Vertex,Vertex,Vertex>)null);
			 */
		}

		void LateUpdate()
		{
			if (_break>0)
			{
				_break--;
				if (_break == 0)
				{
					Debug.Break();
					_pause = false;
				}
			}
		}
	}
}
