using System.IO;
using System.Text;
using IO;
using UnityEngine;

namespace MeshUtil.Extensions
{
	public static class MeshExport
	{
		public static string Export(this MeshBuilder mb, string name, string rootpath, Matrix4x4 xform)
		{
			string path = rootpath.AppendPath($"{name}.stl");
			using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create), new ASCIIEncoding()))
			{
				// 80 byte header
				writer.Write(new byte[80]);
				//TODO: Write object name to header

				uint totalTriangleCount = (uint)mb.triangleCount;

				writer.Write( totalTriangleCount );

				foreach (Triangle t in mb.GetTriangles())
				{
					WriteVector3(writer, xform.MultiplyVector(t.n));
					WriteVector3(writer, xform.MultiplyPoint(t.v2.point));
					WriteVector3(writer, xform.MultiplyPoint(t.v1.point));
					WriteVector3(writer, xform.MultiplyPoint(t.v0.point));

					// specification says attribute byte count should be set to 0.
					writer.Write((ushort) 0);
				}
			}
			return path;

			void WriteVector3(BinaryWriter writer, Vector3 v)
			{
				writer.Write(-v.x);
				writer.Write(-v.y);
				writer.Write(-v.z);
			}
		}
	}
}