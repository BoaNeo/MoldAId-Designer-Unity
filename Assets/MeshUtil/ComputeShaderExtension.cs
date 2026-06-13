using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MeshUtil
{
	public class ComputeShaderExtension
	{
		private static Dictionary<string, ComputeShader> _shaders = new();
		public static ComputeShaderExtension Get(string name)
		{
			if (!_shaders.TryGetValue(name, out ComputeShader cs))
			{
				cs = Resources.Load<ComputeShader>(name);
				_shaders[name] = cs;
			}
			return new ComputeShaderExtension(cs);
		}

		private static List<ComputeBuffer> _trashcan = new();
		private static Dictionary<ComputeBuffer, int> _bufferRefs = new();
		public static void Ref(ComputeBuffer buffer)
		{
			if (buffer == null || !buffer.IsValid())
			{
				Debug.LogWarning("Trying to ref an invalid buffer - ignoring!");
				return;
			}
			int count;
			_bufferRefs.TryGetValue(buffer, out count);
			count++;
			_bufferRefs[buffer] = count;
		}
		public static void DeRef(ComputeBuffer buffer)
		{
			if (buffer == null)
				return;
			int count;
			if (!_bufferRefs.TryGetValue(buffer, out count))
			{
				Debug.LogWarning("Trying to deref a buffer that is not referenced - disposing it!");
				DisposeBufferOnMain(buffer);
				return;
			}
			count--;
			if (count == 0)
			{ 
				DisposeBufferOnMain(buffer);
				_bufferRefs.Remove(buffer);
			}
			else
				_bufferRefs[buffer] = count;
		}

		private static void DisposeBufferOnMain(ComputeBuffer buffer)
		{
			lock (_trashcan)
			{
				_trashcan.Add(buffer);
			}
		}

		public static void EmptyTrash()
		{
			lock (_trashcan)
			{
				foreach(ComputeBuffer trash in _trashcan)
					trash.Dispose();
				_trashcan.Clear();
			}
		}

		public static void DisposeAllBuffers()
		{
			foreach (ComputeBuffer buffer in _bufferRefs.Keys)
			{
				buffer.Dispose();
			}
			_bufferRefs.Clear();
		}

		private readonly ComputeShader _cs;
		private ComputeShaderExtension(ComputeShader cs)
		{
			_cs = cs;
		}

		private Dictionary<string, ComputeBuffer> _buffers = new();

		public void SetBuffer(string parameter, ComputeBuffer buffer, string kernelName=null)
		{
			if(!buffer.IsValid())
				Debug.LogError($"Trying to set invalid buffer for CS {kernelName}.{parameter}");
			_cs.SetBuffer(GetKernel(kernelName), parameter, buffer);
			_buffers[parameter] = buffer;
			Ref(buffer);
		}
		
		public ComputeBuffer GetBuffer(string name)
		{
			if(!_buffers[name].IsValid())
				Debug.LogError($"Trying to get invalid buffer for CS {name}");
			return _buffers[name];
		}

		public void Dispatch(int count, string kernelName=null)
		{
			int k = GetKernel(kernelName);
			_cs.GetKernelThreadGroupSizes(k,out uint gx, out uint gy, out uint gz);
			int groups = Mathf.CeilToInt(count / (float) gx);
			if (groups == 0)
			{
				Debug.LogWarning("Compute shader group count is zero - doing nothing!");
				return;
			}
			_cs.Dispatch(k, groups,1,1);
		}

		public void SetMatrix(string parameter, Matrix4x4 mtx)
		{
			_cs.SetMatrix(parameter, mtx);
		}

		public void SetInt(string parameter, int val)
		{
			_cs.SetInt(parameter, val);
		}
		
		public void SetFloat(string parameter, float val)
		{
			_cs.SetFloat(parameter, val);
		}


		public void Dispose()
		{
			foreach (ComputeBuffer buffer in _buffers.Values)
			{
				DeRef(buffer);
			}
		}

		private int GetKernel(string kernelName=null)
		{
			return kernelName==null ? 0 : _cs.FindKernel(kernelName);
		}

		public void WaitForBuffers(string[] bufferNames, Action<ComputeBuffer[]> whenAllDone)
		{
			int waiting = bufferNames.Length;
			ComputeBuffer[] buffers = new ComputeBuffer[bufferNames.Length];
			for (int i=0;i<bufferNames.Length;i++)
			{
				string bufferName = bufferNames[i];
				buffers[i] = GetBuffer(bufferName);
				AsyncGPUReadback.Request(buffers[i], callback =>
				{
					waiting--;
					if (waiting == 0)
						whenAllDone(buffers);
				});
			}
		}

		public ComputeBuffer CreateBuffer(params int[] values)
		{
			ComputeBuffer b = new ComputeBuffer(values.Length, sizeof(int));
			b.SetData(values);
			return b;
		}
	}
}