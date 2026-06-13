using UnityEngine;

namespace MeshUtil.Extensions
{
	public static class MeshExtrude
	{
		public static MeshBuilder Extrude(this MeshBuilder mb, Shape shape, Vector3 direction, bool cap)
		{
			return mb.Extrude(shape, shape, Vector3.zero, direction, cap, cap);
		}

		public static MeshBuilder Extrude(this MeshBuilder mb, Shape shape1, Shape shape2, Vector3 from, Vector3 to, bool capStart, bool capEnd)
		{
//			Quaternion look = Quaternion.FromToRotation(Vector3.forward, n);

			bool exit = false, first = true;
			Vector3 c1=default;
			Vector3 c2=default;
			int i1a = 0;
			int i2a = 0;
			int i1b = 0;
			int i2b = 0;

			while (!exit)
			{
				if (shape1.count > shape2.count)
				{
					i1b = i1a + 1;
					i2a = i1a * shape2.count / shape1.count;
					i2b = i1b * shape2.count / shape1.count;
					exit = i1b >= shape1.count;
				}
				else
				{
					i2b = i2a + 1;
					i1a = i2a * shape1.count / shape2.count;
					i1b = i2b * shape1.count / shape2.count;
					exit = i2b >= shape2.count;
				}

				i1a %= shape1.count;
				i1b %= shape1.count;
				i2a %= shape2.count;
				i2b %= shape2.count;

				Vector3 v0 = shape1[i1a];
				Vector3 v1 = shape2[i2a];
				Vector3 v2 = shape2[i2b];
				Vector3 v3 = shape1[i1b];

				if (first)
				{
					c1 = v0;
					c2 = v1;
					first = false;
				}
				else
				{
					c1 += v0;
					c2 += v1;
				}
				mb.AddQuad(v0+from,v1+to,v2+to,v3+from);

				if (shape1.count > shape2.count)
					i1a++;
				else
					i2a++;
			}

			Vector3 n = (to - from).normalized;
			if (capStart)
				Cap(from, c1, -n, shape1);
			if (capEnd)
				Cap(to, c2, n, shape2);
			return mb;

			void Cap(Vector3 offset, Vector3 center, Vector3 normal, Shape shape)
			{
				for (int i = 0; i < shape.count; i++)
				{
					Vector3 v0 = shape[i];
					Vector3 v1 = shape[(i + 1) % shape.count];

					mb.AddTriangle(v0 + offset, v1 + offset, center+offset, normal, 0, true);
				}
			};
		}

		public static MeshBuilder Extrude(this MeshBuilder mb, Shape[] shapes, Vector3[] directions, bool capStart, bool capEnd)
		{
			Vector3 from = Vector3.zero;
			for (int i = 0; i < shapes.Length-1; i++)
			{
				Vector3 to = from + directions[i];
				mb.Extrude(shapes[i],shapes[i+1], from, to, i==0 && capStart, i==shapes.Length-2 && capEnd);
				from = to;
			}
			return mb;
		}
	}
}