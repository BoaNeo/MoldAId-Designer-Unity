using UnityEngine;

namespace MeshUtil.Extensions
{
	public static class MeshTransform
	{
		private static void Copy(this MeshBuilder source)
		{
			MeshBuilder me = new MeshBuilder();
			foreach(Triangle t in source.GetTriangles())
				me.AddTriangle(
					me.AddVertex(t.v0.point, t.v0.tag), 
					me.AddVertex(t.v1.point, t.v1.tag), 
					me.AddVertex(t.v2.point, t.v2.tag), 
					t.n,
					t.tag);
		}

		public static MeshBuilder CopyGPU(this MeshBuilder me)
		{ // TODO: This is not really a copy - it's re-using the same buffer which will work in some case, but certainly not in others!
			MeshBuilder mb = new MeshBuilder();
			mb.SetComputeBuffer(me.GetComputeBuffer(), me.triangleCount);
			return mb;
		}
		
		public static MeshBuilder Transform(this MeshBuilder me, Matrix4x4 transform, bool invert = false, int faceTag=0, int faceTagMask=-1, int vertexTag=0, int vertexMask=-1)
		{
			MeshBuilder mb = new MeshBuilder();
			mb.Append(me, transform, invert, faceTag, faceTagMask, vertexTag, vertexMask);
			return mb;
		}
	
		public static MeshBuilder TransformGPU(this MeshBuilder me, Matrix4x4 transform, bool invert = false, int faceTag=0, int faceTagMask=-1, int vertexTag=0, int vertexMask=-1)
		{
			if (me.triangleCount == 0)
				return new MeshBuilder();

			ComputeShaderExtension cs = ComputeShaderExtension.Get("ComputeShaders/TransformInPlaceShader");

			cs.SetInt("faceTag", faceTag);
			cs.SetInt("faceTagMask", faceTagMask);
			cs.SetInt("vertexTag", vertexTag);
			cs.SetInt("vertexTagMask", vertexMask);
			cs.SetInt("count", me.triangleCount);
			cs.SetFloat("flip", invert ? 1 : 0);
			cs.SetMatrix("transform", transform);
			cs.SetBuffer("triangles", me.GetComputeBuffer());

			cs.Dispatch(me.triangleCount);

			MeshBuilder mb = new MeshBuilder();
			mb.SetComputeBuffer(me.GetComputeBuffer(), me.triangleCount);

			cs.Dispose();

			return mb;
		}
		
		private static void Append(this MeshBuilder me, MeshBuilder source, Matrix4x4 transform, bool invert = false, int faceTag=0, int faceTagMask=-1, int vertexTag=0, int vertexMask=-1)
		{
			float scale = invert ? -1 : 1;
			foreach(Triangle t in source.GetTriangles())
				me.AddTriangle(
					me.AddVertex(transform.MultiplyPoint(t.v0.point), (t.v0.tag&vertexMask)|vertexTag ), 
					me.AddVertex(transform.MultiplyPoint(t.v1.point), (t.v1.tag&vertexMask)|vertexTag), 
					me.AddVertex(transform.MultiplyPoint(t.v2.point), (t.v2.tag&vertexMask)|vertexTag), 
					scale * transform.MultiplyVector(t.n),
					(t.tag & faceTagMask)|faceTag);
		}

		public static void AppendGPU(this MeshBuilder me, MeshBuilder source, Matrix4x4 transform, bool invert = false, int faceTag=0, int faceTagMask=-1, int vertexTag=0, int vertexMask=-1)
		{
			if (source.triangleCount == 0)
				return;

			ComputeShaderExtension cs = ComputeShaderExtension.Get("ComputeShaders/TransformAppendShader");

			cs.SetInt("faceTag", faceTag);
			cs.SetInt("faceTagMask", faceTagMask);
			cs.SetInt("vertexTag", vertexTag);
			cs.SetInt("vertexTagMask", vertexMask);
			cs.SetInt("sourceCount", source.triangleCount);
			cs.SetFloat("flip", invert ? 1 : 0);
			cs.SetMatrix("transform", transform);
			cs.SetBuffer("source", source.GetComputeBuffer());
			cs.SetBuffer("target", me.GetComputeBuffer(source.triangleCount+me.triangleCount));
			cs.SetBuffer("targetCount", cs.CreateBuffer(me.triangleCount));

			cs.Dispatch(source.triangleCount);
				
			int[] targetCount = new int[1];
			cs.GetBuffer("targetCount").GetData(targetCount);

			me.SetComputeBuffer(cs.GetBuffer("target"), targetCount[0], me.triangleCount );
			
			cs.Dispose();
		}
		
	}
}