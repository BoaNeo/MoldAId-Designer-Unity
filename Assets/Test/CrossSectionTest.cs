using MeshUtil;
using MeshUtil.Extensions;
using UnityEngine;

namespace Test
{
	public class CrossSectionTest : MonoBehaviour
	{
		[SerializeField] private MeshFilter _external;
		[SerializeField] private MeshRenderer[] _targets;

		private MeshBuilder _loaded;
		private int _matPlanePosId;
		private int _matPlaneNormalId;

		private void Awake()
		{
			_loaded = new MeshBuilder().Import("/Users/mrmac/Documents/stl samples/420.1 - Hynds smart cover G2 housing opt 4 C Class.STL");

			Mesh mesh = _external.mesh;
			Mesh.MeshDataArray meshData = Mesh.AllocateWritableMeshData(1);
			_loaded.FlatShade(meshData);
			Mesh.ApplyAndDisposeWritableMeshData(meshData,mesh);
			_external.mesh = mesh;

			_matPlanePosId = Shader.PropertyToID("_PlanePosition");
			_matPlaneNormalId =  Shader.PropertyToID("_PlaneNormal");
		}

		void Update()
		{
			foreach (MeshRenderer meshRenderer in _targets)
			{
				Material m = meshRenderer.material;
				m.SetVector(_matPlanePosId, transform.position);
				m.SetVector(_matPlaneNormalId, transform.up);
			}
		}
	}
}
