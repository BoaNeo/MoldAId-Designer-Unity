using UnityEngine;

namespace MeshUtil
{
	public partial class Geometry
	{
		// TODO: Move this to a MeshBuilder extension and get rid of MeshInfo
		public static MeshInfo GenerateTorus(float ringRadius, float tubeRadius, int slices, int segments)
		{
			int i = 0, f=0;
			Vector3[] verts = new Vector3[segments * slices];
			int[] faces = new int[3 * 2 * segments * slices];
			for (int slice = 0; slice < slices; slice++)
			{
				float a0 = 2*Mathf.PI*slice/slices;
				Vector2 d = new Vector2(Mathf.Cos(a0), Mathf.Sin(a0));
				for (int segment = 0; segment < segments; segment++)
				{
					float a1 = 2 * Mathf.PI * segment / segments;
					
					float r = ringRadius + tubeRadius * Mathf.Cos(a1);

					Vector3 v = new Vector3(r * d.x, r * d.y, tubeRadius * Mathf.Sin(a1));
					verts[i+segment] = v;
					faces[f++] = i+(segment+1)%segments;
					faces[f++] = i+segment;
					faces[f++] = (i+segment+segments)%(verts.Length);
					faces[f++] = i+(segment+1)%segments;
					faces[f++] = (i+segment+segments)%(verts.Length);
					faces[f++] = (i+segments)%(verts.Length)+(segment+1)%segments;
				}
				i += segments;
			}

			MeshInfo m = new MeshInfo();
			m.vertices = verts;
			m.triangles = faces;
			return m;
		}
	}
}