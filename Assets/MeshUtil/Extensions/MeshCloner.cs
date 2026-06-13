using UnityEngine;

namespace MeshUtil.Extensions
{
	public static class MeshCloner
	{
		public static MeshBuilder CloneFrom(this MeshBuilder mb, MeshInfo source, Matrix4x4 transform=default)
		{
			return mb.CloneFrom(source.vertices, source.normals, source.triangles, transform);
		}

		public static MeshBuilder CloneFrom(this MeshBuilder mb, Mesh source=null, Matrix4x4 transform=default)
		{
			return mb.CloneFrom(source.vertices, source.normals, source.triangles, transform);
		}

		public static MeshBuilder CloneFrom(this MeshBuilder mb, Vector3[] srcverts, Vector3[] srcnorms, int[] triangles, Matrix4x4 transform=default)
		{
			Vector3[] norms = srcnorms;
			Vector3[] verts = srcverts;
			if (transform != default && !transform.isIdentity)
			{
				if(srcnorms!=null)
					norms = new Vector3[srcnorms.Length];
				verts = new Vector3[verts.Length];
				for (int vidx = 0; vidx < srcverts.Length; vidx++)
				{
					verts[vidx] = transform.MultiplyPoint( srcverts[vidx]);
					if(srcnorms!=null)
						norms[vidx] = transform.MultiplyVector(srcnorms[vidx]);
				}
			}
			for (int ti = 0; ti < triangles.Length;ti+=3)
			{
				Vector3 p0 = verts[triangles[ti]];
				Vector3 p1 = verts[triangles[ti+1]];
				Vector3 p2 = verts[triangles[ti+2]];

				Vector3 n;
				if (norms == null)
				{
					n = Vector3.Cross(p1 - p0, p2 - p0).normalized;
				}
				else
				{
					Vector3 n0 = norms[triangles[ti]];
					Vector3 n1 = norms[triangles[ti+1]];
					Vector3 n2 = norms[triangles[ti+2]];
					n = (n0 + n1 + n2).normalized;
				}
				mb.AddTriangle(p0,p1,p2,n);
			}

			return mb;
		}
	}
}