using System;
using System.Threading;
using MeshUtil;
using MeshUtil.DataStructures;
using MeshUtil.Extensions;
using TMPro;
using UnityEngine;
using Utility;

namespace Test
{
	public class RaycastTest : MonoBehaviour
	{
		[SerializeField] private Transform _from;
		[SerializeField] private Transform _to;
		[SerializeField] private MeshFilter _source;
		[SerializeField] private bool _auto;
		[SerializeField] private bool _cast;
		[SerializeField] private bool _singleStep;
		[SerializeField] private bool _showSpheres;

		private MeshBuilder _loaded;
		private WorkerThread _worker;
		private bool _busy;
		private int _break;

		private void Start()
		{
		
//		_loaded = new MeshBuilder().Import("/Users/mrmac/Documents/stl samples/442L_OutputModel_PRECAST.stl",Matrix4x4.identity);
		_loaded = new MeshBuilder().Import("/Users/mrmac/Documents/stl samples/420.1 - Hynds smart cover G2 housing opt 4 C Class.STL");
//		_loaded = new MeshBuilder().Import("/Users/mrmac/Documents/stl samples/Oshape.stl",Matrix4x4.identity);
//			_loaded = new MeshBuilder().Import("/Users/mrmac/Documents/stl samples/Cavity.stl",Matrix4x4.identity);
//		_loaded = new MeshBuilder().CloneFrom(_source.mesh);

			Apply(_loaded,Matrix4x4.identity,_source.mesh);
			_worker = gameObject.AddComponent<WorkerThread>();
		}

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
				if (_cast)
				{
					_cast = _auto;
					_busy = true;

					float t0 = Time.realtimeSinceStartup;
					Debug.Log($"Starting new background job at {t0}");
					Vector3 from = _from.position;
					Vector3 to = _to.position;
					_worker.RunInBackground(() =>
					{
						try
						{
							BoundingSphereGrid raycaster = _loaded.GetTrianglesAs<BoundingSphereGrid>();
							raycaster.RayCast( new Ray(from, (to - from).normalized), triangle =>
							{
								_checking = triangle;
								if (_singleStep)
								{
									_pause = true;
									_break = 1;
								}
								while (_pause) Thread.Sleep(1);
								return true;
							}, out BoundingSphereGrid.TriangleHit r);
						}
						catch (Exception e)
						{
							Debug.LogError($"Raycast failed with {e}");
							_auto = false;
							_cast = false;
						}
						_worker.RunOnMain(() =>
						{
							Debug.Log($"Updating mesh after {1000 * (Time.realtimeSinceStartup-t0)} milliseconds");
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
		private BoundingSphereGrid.TriangleHit _checking;
		private void OnDrawGizmos()
		{
#if DEBUG_MODE
			Gizmos.color = Color.cyan;
			Gizmos.DrawLine(_from.position,_to.position);

			if(_showSpheres)
				_loaded.DebugDraw();

			if (_busy)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawSphere(BoundingSphereGrid.debugSearchDelta, 0.5f);

				BoundingSphereGrid.debugCurrentSphere.DebugDraw(BoundingSphereGrid.debugRadius);

				if (_busy)
				{
					Gizmos.color = Color.white;
					DrawTriangle(_checking);
				}

				Gizmos.color = Color.yellow;
				Gizmos.DrawSphere(BoundingSphereGrid.debugHitPoint, 1.0f);
				DrawTriangle(BoundingSphereGrid.debugLastHit);
			}

			Gizmos.color = Color.green;
			DrawTriangle(BoundingSphereGrid.debugBestHit);
#endif
		}

		private void DrawTriangle(Triangle t)
		{
			UnityEngine.Gizmos.DrawLine(t.v0.point, t.v1.point);
			UnityEngine.Gizmos.DrawLine(t.v1.point, t.v2.point);
			UnityEngine.Gizmos.DrawLine(t.v2.point, t.v0.point);
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
