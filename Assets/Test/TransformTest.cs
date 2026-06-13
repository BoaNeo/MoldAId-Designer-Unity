using MeshUtil;
using MeshUtil.Extensions;
using UnityEngine;

namespace Test
{
    public class TransformTest : MonoBehaviour
    {
        [SerializeField] private Transform _transform;
        [SerializeField] private bool _auto;
        [SerializeField] private bool _enabled;

        private MeshBuilder _loaded;
        private Mesh _mesh;

        void Start()
        {
//        _loaded = new MeshBuilder().Import("/Users/mrmac/Documents/stl samples/442L_OutputModel_PRECAST.stl",Matrix4x4.identity);
//		_loaded = new MeshBuilder().Import("/Users/mrmac/Documents/stl samples/420.1 - Hynds smart cover G2 housing opt 4 C Class.STL",Matrix4x4.identity);
//		_loaded = new MeshBuilder().Import("/Users/mrmac/Documents/stl samples/Oshape.stl",Matrix4x4.identity);
//		_loaded = new MeshBuilder().Import("/Users/mrmac/Documents/stl samples/Cavity.stl",Matrix4x4.identity);
            _loaded = new MeshBuilder().Import("/Users/mrmac/Documents/stl samples/460 - Brose Gearwheel part.STL");
            _mesh = GetComponent<MeshFilter>().mesh;
        }

        // Update is called once per frame
        void Update()
        {
            if (_enabled)
            {
                MeshBuilder m1 = _loaded.TransformGPU(_transform.localToWorldMatrix);
            
                if (m1.changed)
                {
                    Mesh.MeshDataArray meshData = Mesh.AllocateWritableMeshData(1);
                    m1.FlatShade(meshData);
                    Mesh.ApplyAndDisposeWritableMeshData(meshData,_mesh);
                }
                _enabled = _auto;
            }
        }
    }
}
