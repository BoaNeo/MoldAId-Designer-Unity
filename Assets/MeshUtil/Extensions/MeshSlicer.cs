using System;
using UnityEngine;
using Utility;

namespace MeshUtil.Extensions
{
	public static class MeshSlicer
	{
		private const int LOW_TRIANGLE_COUNT=0;
		private const int HIGH_TRIANGLE_COUNT=1;
		private const int COUNTERS=2;

		public static Action<Action> SliceGPU(this MeshBuilder mb, Matrix4x4 transform, int tag, MeshBuilder r1, MeshBuilder r2)
		{
			return (Action whenDone) =>
			{
				long t0 = Log.ElapsedTime();

				ComputeShaderExtension cs = ComputeShaderExtension.Get("ComputeShaders/SlicerShader");

				int maxtri = 2 * mb.triangleCount;
				unsafe
				{
					cs.SetMatrix("transform", transform);
					cs.SetInt("count", mb.triangleCount);
					cs.SetBuffer("triangles", mb.GetComputeBuffer());
					cs.SetBuffer("low", new ComputeBuffer(maxtri, sizeof(Triangle)));
					cs.SetBuffer("high", new ComputeBuffer(maxtri, sizeof(Triangle)));
					cs.SetBuffer("counters", new ComputeBuffer(COUNTERS, sizeof(int)));
				}

				t0 = Log.ElapsedTime(t0, "Prepared Slicer Compute Shader");

				cs.Dispatch(mb.triangleCount);

				cs.WaitForBuffers(new[] {"counters", "low", "high"}, (ComputeBuffer[] buffers) =>
				{
					int[] counters = new int[COUNTERS];
					buffers[0].GetData(counters);
					int lowcount = counters[LOW_TRIANGLE_COUNT];
					int hicount = counters[HIGH_TRIANGLE_COUNT];
					r1.SetComputeBuffer(buffers[1], lowcount);
					r2.SetComputeBuffer(buffers[2], hicount);
					cs.Dispose();
					t0 = Log.ElapsedTime(t0, $"Retrieved data from Slicer Compute Shader (lowcount={lowcount}, hicount={hicount}, maxcount={maxtri})");
					if(lowcount>maxtri)
						Debug.LogError("Pre-allocated compute buffer (lo) too small!");
					if(hicount>maxtri)
						Debug.LogError("Pre-allocated compute buffer (hi) too small!");
					whenDone();
				});
			};
		}
		
		public static void Slice(this MeshBuilder mb, Matrix4x4 transform, int tag, out MeshBuilder r1, out MeshBuilder r2)
		{
			MeshBuilder low = new MeshBuilder();
			MeshBuilder hi = new MeshBuilder();

			float zlo = - Vertex.ACCURACY/2;
			float zhi = + Vertex.ACCURACY/2;

			Vector3 cut(Vector3 p0, Vector3 p1, bool flip)
			{
				if (flip)
					(p0, p1) = (p1, p0);

				double dx = p1.x - p0.x;
				double dy = p1.y - p0.y;
				double dz = p1.z - p0.z;

				if( Math.Abs(dz)<Vertex.ACCURACY)
					Debug.LogError("Should not be in here!");
				double b = -p0.z / dz;

				Vector3 p = new Vector3((float) (p0.x + b * dx), (float) (p0.y + b * dy), 0);

				return p;
			}
			void tri(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 tn, int t0,int t1, int t2)
			{
				if (p0.z > zlo && p0.z < zhi)
					p0.z = 0;
				if (p1.z > zlo && p1.z < zhi)
					p1.z = 0;
				if (p2.z > zlo && p2.z < zhi)
					p2.z = 0;
				if (p0.z < zhi && p1.z < zhi && p2.z < zhi)
					low.AddTriangle(p0, p1, p2, tn, 0, false, t0,t1,t2);
				else if (p0.z > zlo && p1.z > zlo && p2.z > zlo)
					hi.AddTriangle(p0, p1, p2, tn, 0, false,t0,t1,t2);
				else
				{
					Debug.LogWarning("Triangle is entirely contained in the cutplane");
					// This triangle is exactly in the cutplane - either remove it and leave a hole, or create two opposite triangles and don't tag it's edges (since it's not part of the boundary)
				//	low.AddTriangle(p0, p1, p2, tn,t0,t1,t2);
				//	hi.AddTriangle(p0, p1, p2, -tn,t0,t1,t2);
				}
			}
			bool crossing(Vector3 v0, Vector3 v1)
			{
				return v0.z <= zlo && v1.z >= zhi || v0.z >= zlo && v1.z <= zhi;
			}

			Matrix4x4 m = transform;
			// Loop all triangles
			foreach (Triangle t in mb.GetTriangles())
			{
				Vector3 p0 = m.MultiplyPoint(t.v0.point);
				Vector3 p1 = m.MultiplyPoint(t.v1.point);
				Vector3 p2 = m.MultiplyPoint(t.v2.point);
				Vector3 n = m.MultiplyVector(t.n);

				int t0 = 0, t1 = 0, t2 = 0;
				if (p0.z > zlo && p0.z < zhi)
					t0 = tag;

				if (p1.z > zlo && p1.z < zhi)
					t1 = tag;

				if (p2.z > zlo && p2.z < zhi)
					t2 = tag;

				// Look at the edges and see if they're all on the same side of the cutplane
				if (p0.z < zhi && p1.z < zhi && p2.z < zhi)
					tri(p0, p1, p2, n, t0, t1, t2);
				else if (p0.z > zlo && p1.z > zlo && p2.z > zlo)
					tri(p0, p1, p2, n, t0, t1, t2);
				else
				{
					Vector3 v0v1 = p1, v1v2 = p2, v2v0 = p0;
					int t01 = t1, t12 = t2, t20 = t0;
					if (crossing(p0, p1))
					{
						t01 = tag;
						v0v1 = cut(p0, p1, t.v0.id < t.v1.id);
					}

					if (crossing(p1, p2))
					{
						t12 = tag;
						v1v2 = cut(p1, p2, t.v1.id < t.v2.id);
					}

					if (crossing(p2, p0))
					{
						t20 = tag;
						v2v0 = cut(p2, p0, t.v2.id < t.v0.id);
					}

					tri(p0, v0v1, v2v0, n, t0, t01, t20);
					tri(p1, v1v2, v0v1, n, t1, t12, t01);
					tri(p2, v2v0, v1v2, n, t2, t20, t12);
					tri(v0v1, v1v2, v2v0, n, t01, t12, t20);
				}
			}

			r1 = low;
			r2 = hi;
		}
	}
}