using System.Collections.Generic;
using UnityEngine;

namespace MeshUtil
{
	public enum BufferState
	{
		None         ,  // Buffer has not been changed, no action required
		TriangleMeta ,  // Buffer contains only triangle meta data (e.g. tags or normals) and nothing needs to be done beyond copying the data
		VertexMeta   ,  // Buffer is simply vertex meta data (e.g. tags), no structural changes, but tags must be updated in kdTree
		Triangles    ,	// Buffer has new triangles from startindex up to count, but all are using existing vertices (no kdTree update needed)
		Vertices     ,	// Buffer has both new vertices and (by extension) new triangles. Needs complete rebuild
	}

	public class ComputeBufferWrapper<T> where T : unmanaged
	{
		private int _count;
		private int _changedFrom;
		private int _changedTo;
		private ComputeBuffer _buffer;

		public int capacity => _buffer == null ? 0 : _buffer.count;
		public int count => _count;
		public bool isValid => _buffer != null ;// && _buffer.IsValid(); I can't do this because, for whatever reason, it can only be used on Unitys main thread
		public BufferState state { get; private set; }

		public void Update(IReadOnlyList<T> source, int size = -1)
		{
			unsafe
			{
				if(source==null)
					source = ReadBuffer();

				Invalidate();

				_count = source.Count;
				_buffer = new ComputeBuffer(size<0 ? _count : size, sizeof(T));
				ComputeShaderExtension.Ref(_buffer);

				if (source!=null && source.Count > 0)
				{
					if(source is T[] array)
						_buffer.SetData(array);
					else if(source is List<T> list)
						_buffer.SetData(list);
				}
			}
		}

		public ComputeBuffer GetBuffer()
		{
			if ( _buffer == null || !_buffer.IsValid())
				Debug.LogError("Trying to get invalid compute buffer");
			return _buffer;
		}

		public void SetBuffer(ComputeBuffer b, int cnt, int firstChange, int lastChange, BufferState state)
		{
			if (state == BufferState.None)
				return;
				
			Invalidate();
	   
			if(b==null || !b.IsValid())
				Debug.LogWarning("Setting an invalid buffer!");

			_changedFrom = firstChange;
			_changedTo = lastChange;
			_count = cnt;
			_buffer = b;
			this.state = state;

			ComputeShaderExtension.Ref(_buffer);
		}

		public T[] Read(out int from, out int to)
		{
			from = _changedFrom;
			to = _changedTo;
			T[] buffer = ReadBuffer();
			return buffer;
		}

		private T[] ReadBuffer()
		{
			if (_buffer != null && _buffer.IsValid())
			{
				T[] data = new T[_count];
				_buffer.GetData(data,0,0,_count);
				return data;
			}
			return _empty;
		}

		private static T[] _empty = new T[0];

		public void Invalidate()
		{
			ComputeShaderExtension.DeRef(_buffer);
			_buffer = null;
		}
	}
}