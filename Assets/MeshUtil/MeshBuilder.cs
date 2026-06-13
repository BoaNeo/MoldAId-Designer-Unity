using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace MeshUtil
{
	public class MeshBuilder
	{
		/// <summary>
		/// In addition to its native list, the MeshBuilder allow custom representations of its triangle set to be added, each optimized for different purposes.
		///
		/// GetTriangleSet<T> returns (and creates if needed) a triangle representation of type T. Modifications to the MeshBuilder itself will
		/// propagate to the extension.
		/// </summary>
		 
		public interface ITriangleSet
		{
			/// <summary>
			/// Called when set is first initialized.
			/// </summary>
			/// <param name="mb"></param>
			void Initialize(MeshBuilder mb);

			/// <summary>
			/// Called when the MeshBuilder is asked to draw debug info
			/// </summary>
			void DebugDraw();
		}

		public int triangleCount => _bufferedMesh.isValid ? _bufferedMesh.count : _mutableMesh.triangles.Count;
		public bool changed { get; private set; }

		private ComputeBufferWrapper<Triangle> _bufferedMesh = new();
		private MutableMesh _mutableMesh = new();

    private List<ITriangleSet> _alternativeTriangleSets;

    private bool _didReadBuffer = true;

    public Bounds GetBounds()
    {
	    return GetMutableMesh().bounds;
    } 

    public IReadOnlyList<Triangle> GetTriangles()
    {
	    return GetMutableMesh().triangles;
    }

    public MeshBuilder CreateSubMesh()
    {
	    MeshBuilder mb = new ();
	    mb._mutableMesh.ShareVerticesWith(_mutableMesh);
	    return mb;
    }

    public Vertex AddVertex(Vector3 v0, int tag = 0)
    {
	    Invalidate();
	    return GetMutableMesh().AddVertex(v0, tag);
    }
    
    public Triangle AddTriangle(Vertex v0, Vertex v1, Vertex v2, Vector3 n,int tagface=0, bool reorient=false)
    {
	    Invalidate();
	    return GetMutableMesh().AddTriangle(v0, v1, v2, n, tagface, reorient);
    }

    public void RemoveTriangles(Func<Triangle, bool> filter)
    {
	    Invalidate();
	    GetMutableMesh().RemoveTriangles(filter);
    }

		public ComputeBuffer GetComputeBuffer(int size = -1)
		{
			if( !_bufferedMesh.isValid || size>_bufferedMesh.capacity)
			{
				if( _bufferedMesh.isValid )
					_bufferedMesh.Update(null, size);
				else
					_bufferedMesh.Update(_mutableMesh.triangles, size);
			}
			return _bufferedMesh.GetBuffer();
		}

		public void SetComputeBuffer(ComputeBuffer triangles, int triangleCount, int firstChange=0, int lastChange=-1, BufferState state=BufferState.Vertices)
		{
			Invalidate();

			_bufferedMesh.SetBuffer(triangles, triangleCount, firstChange, lastChange<0?triangleCount:lastChange, state);
			_didReadBuffer = false;
			
			// TODO: This is here because having data that can only be accessed from the main Unity thread is a pain in the arse - it's really hard to know ahead of time if this is needed, so for now we always copy it immediately
			// Ideally, this would either work from a background thread (not going to happen) or there would be some mechanism to allow GetMutableMesh to put itself on the main thread (Refactoring with callback?)
			GetMutableMesh();
		}

		private void Invalidate()
		{
			changed = true;
			_alternativeTriangleSets = null;
			_bufferedMesh.Invalidate();
		}

		public void FlatShade( Mesh.MeshDataArray target,bool force=false)
		{
			if ( changed || force)
			{
				changed = false;
				// Allocate mesh data for one mesh.
				var data = target[0];

				data.SetVertexBufferParams(triangleCount*3,
					new VertexAttributeDescriptor(VertexAttribute.Position),
					new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3, stream:1),
					new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UInt8, 4,stream:2)
					);
			
				// TODO: No need to force Uint32 here unless we actually use that many vertices
				data.SetIndexBufferParams(triangleCount*3, IndexFormat.UInt32);

				NativeArray<Vector3> verts = data.GetVertexData<Vector3>();
				NativeArray<Vector3> norms = data.GetVertexData<Vector3>(1);
				//NativeArray<Color32> colors = data.GetVertexData<Color32>(2);
				NativeArray<int> tris = data.GetIndexData<int>();

				int t = 0;
				// TODO: If compute buffer is available, we really don't want to first read from the GPU to then write it back to the GPU.
				foreach (Triangle tri in GetTriangles())
				{
					Vector3 n = tri.n;
					norms[t] = n;
					norms[t + 1] = n;
					norms[t + 2] = n;
					verts[t] = tri.v0.point;
					verts[t + 1] = tri.v1.point;
					verts[t + 2] = tri.v2.point;
					tris[t] = t;
					tris[t + 1] = t + 1;
					tris[t + 2] = t + 2;
					/*
					colors[t] = tri.v0.color;
					colors[t+1] = tri.v1.color;
					colors[t+2] = tri.v2.color;
					*/
					t += 3;
				}

				data.subMeshCount = 1;
				data.SetSubMesh(0, new SubMeshDescriptor(0, tris.Length));
			}
		}

		public T GetTrianglesAs<T>() where T: ITriangleSet, new()
		{
			if (_alternativeTriangleSets != null)
			{
				foreach (ITriangleSet set in _alternativeTriangleSets)
				{
					if (set is T)
						return (T)set;
				}
			}

			T ext = new T();
			ext.Initialize(this);

			if (_alternativeTriangleSets == null)
				_alternativeTriangleSets = new();
			_alternativeTriangleSets.Add(ext);

			return ext;
		}
		
		    
		/*
		public bool SharesVerticesWith(MeshBuilder other)
		{
			if (other == null)
				return false;

			KdTree otherTree = other.GetMutableMesh().vertices;
			KdTree myTree = GetMutableMesh().vertices;
			
			if(otherTree==myTree)
				return true;
			
			// Traverse the smallest tree and look for those vertices in the other
			// If we don't complete the Traverse, it's because we found a match
			if(otherTree.count>myTree.count)
				return !myTree.Traverse(v1 => !otherTree.Contains(v1.point));
			return !otherTree.Traverse(v1 => !myTree.Contains(v1.point));
		}
		*/

		public void DebugDraw()
		{
			if(_alternativeTriangleSets!=null)
				foreach (ITriangleSet set in _alternativeTriangleSets)
					set.DebugDraw();
		}
		
		private MutableMesh GetMutableMesh()
		{
			if ( !_didReadBuffer )
			{
				Triangle[] triangles = _bufferedMesh.Read(out int from, out int to);
				_mutableMesh.Update(triangles, 0, triangles.Length);
				_alternativeTriangleSets = null;
				_didReadBuffer = true;
			}
			return _mutableMesh;
		}
	}
}