using MeshUtil.Extensions;
using UnityEngine;

namespace MeshUtil
{
	public partial class Geometry
	{
		// TODO: This is extraordinarily ugly
		public static MeshBuilder GenerateFilletCube(float width, float height, float depth, float fillet, int filletsteps)
		{
			width /= 2;
			height /= 2;
			depth /= 2;

			Vector3 v0 = new Vector3(width,height,depth);
			Vector3 v1 = new Vector3(-width,height,depth);
			Vector3 v2 = new Vector3(-width,-height,depth);
			Vector3 v3 = new Vector3(width,-height,depth);

			Vector3 v4 = new Vector3(-width,height,-depth);
			Vector3 v5 = new Vector3(width,height,-depth);
			Vector3 v6 = new Vector3(width,-height,-depth);
			Vector3 v7 = new Vector3(-width,-height,-depth);

			MeshBuilder mb = new MeshBuilder();

			CreateSide(mb, v0,v1,v2,v3, fillet, filletsteps);
			CreateSide(mb, v1,v4,v7,v2, fillet, filletsteps);
			CreateSide(mb, v4,v5,v6,v7, fillet, filletsteps);
			CreateSide(mb, v5,v0,v3,v6, fillet, filletsteps);
			CreateSide(mb, v5,v4,v1,v0, fillet, filletsteps);
			CreateSide(mb, v3,v2,v7,v6, fillet, filletsteps);
			CreateSide(mb, v7,v6,v3,v2, fillet, filletsteps); // This extra side is a rotated bottom to get the remaining fillets and corner included

			return mb;
		}

		private static Vector3 IsolateAxes(Vector3 v)
		{
			return new Vector3(v.x < 0 ? -1 : v.x > 0 ? 1:0, v.y < 0 ? -1 : v.y > 0 ? 1:0,v.z < 0 ? -1 : v.z > 0 ? 1:0);
		}

		private static void CreateSide(MeshBuilder mb, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, float fillet, int subdivs)
		{
			Vector3 n = Vector3.Cross(v2 - v0, v1 - v0).normalized;

			Vector3 d20 = IsolateAxes(v2 - v0);
			Vector3 d31 = IsolateAxes(v3 - v1);
			
			v0 += fillet * d20;
			v1 += fillet * d31;
			v2 -= fillet * d20;
			v3 -= fillet * d31;

			Vector3 up = (v0 - v3).normalized;
			Vector3 right = (v1 - v0).normalized;
			mb.AddQuad(v0, v1, v2, v3);

			FilletEdgeFrom(mb, v0, v1, v2, n,fillet, subdivs);
			FilletCornerFrom(mb, v0, v1, v2, n,fillet, subdivs);
			FilletEdgeFrom(mb, v1, v2, v3, n,fillet, subdivs);
			FilletCornerFrom(mb, v1, v2, v3, n,fillet, subdivs);
		}

		private static void FilletEdgeFrom(MeshBuilder mb, Vector3 v0o, Vector3 v1o, Vector3 v2o, Vector3 fwd, float fillet, int subdivs)
		{
			Vector3 up = (v1o - v2o).normalized;

			float a = Mathf.PI / (2.0f * (subdivs+1) ); // Angular step for each fillet step
			Vector3 v0 = v0o;
			Vector3 v1a = v1o;

			for (int s = 0; s <= subdivs; s++)
			{
				float u = fillet * Mathf.Sin((s+1) * a);
				float f = fillet * (1-Mathf.Cos((s+1) * a));
				
				Vector3 v = u * up + f * fwd;
				Vector3 v3 = v0o + v;
				Vector3 v4 = v1o + v;
				mb.AddQuad(v3, v4, v1a, v0);

				v0 = v3;
				v1a = v4;
			}
		}

		private static void FilletCornerFrom(MeshBuilder mb, Vector3 v0o, Vector3 v1o, Vector3 v2o, Vector3 fwd, float fillet, int subdivs)
		{
			Vector3 up = (v1o - v2o).normalized;
			Vector3 right = (v1o - v0o).normalized;

			float a = Mathf.PI / (2.0f * (subdivs+1) ); // Angular step for each fillet step
			Vector3 v1a = v1o;
			Vector3 v1b = v1o;

			Vector3 v7o = v1o + fillet * fwd + fillet * right;
			Vector3 v7b = v7o;
			Vector3 v8 = v1o + fillet * 0.5f*(fwd + up + right);

			Vector3 special = Vector3.zero;
			for (int s = 0; s <= subdivs; s++)
			{
				float u = fillet * Mathf.Sin((s+1) * a);
				float f = fillet * (1-Mathf.Cos((s+1) * a));
				
				Vector3 v4 = v1o + u * up + f * fwd;

				Vector3 v5 = v1o + u * right + f * fwd;

				Vector3 v7 = v7o + u * up - f * right;

				if (s == 0)
				{
					mb.AddTriangle(v4,v5, v1a);
					mb.AddTriangle(v4,v8, v5);
					special = v7;
				}
				else if (s == subdivs)
				{
					mb.AddTriangle(v7,v7b, v1a);
					mb.AddTriangle(v1a,v7b, v8);
					mb.AddTriangle(v1b,special, v5);
					mb.AddTriangle(v1b,v8, special);
				}
				else
				{
					mb.AddTriangle(v4,v8,v1a);
					mb.AddTriangle(v8,v5,v1b);
					mb.AddTriangle(v8,v7,v7b);
				}

				v1a = v4;

				v1b = v5;

				v7b = v7;
			}
		}
	}
}