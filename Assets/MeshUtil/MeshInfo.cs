using UnityEngine;
using UnityEngine.Rendering;

namespace MeshUtil
{
	// TODO: This should not be used anymore - use MeshDataArray instead!
	public struct MeshInfo
	{
		public Vector3[] vertices;
		public Vector3[] normals;
		public int[] triangles;
		public Matrix4x4 transform;

		private Mesh _mesh;
		public Mesh Build(bool locked=false)
		{
			if (_mesh == null)
			{
				Mesh m = new Mesh();
				m.indexFormat = IndexFormat.UInt32;
				m.MarkDynamic();
				m.vertices = vertices;
				m.SetTriangles(triangles,0);
				if (normals == null)
					m.RecalculateNormals();
				else
					m.normals = normals;
				m.RecalculateBounds();
//				m.UploadMeshData(locked);
				_mesh = m;
			}
			return _mesh;
		}

		public void Dispose()
		{
			if (_mesh != null)
			{
			}
		}
	}
}