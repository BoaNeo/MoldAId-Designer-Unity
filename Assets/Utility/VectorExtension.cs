using System;
using MeshUtil;
using UnityEngine;

namespace Utility
{
	public static class VectorExtension
	{
		public static Vector3 Parse(string value)
		{
			if(value.Length<7) // Need at least 7 bytes to define a Vector3: "(0,0,0)"
				return Vector3.zero;
			string[] axes = value.Substring(1, value.Length - 2).Split(',');
			return new Vector3( float.Parse(axes[0]),float.Parse(axes[1]),float.Parse(axes[2]) );
		}

		public static Quaternion ParseQuaternion(string value)
		{
			if(value.Length<7) // Need at least 9 bytes to define a Vector3: "(0,0,0,0)"
				return Quaternion.identity;
			string[] axes = value.Substring(1, value.Length - 2).Split(',');
			return new Quaternion( float.Parse(axes[0]),float.Parse(axes[1]),float.Parse(axes[2]),float.Parse(axes[3]) );
		}

		public static string ToAccurateString(this Vector3 vector)
		{
//			return UnityString.Format("({0}, {1}, {2})", (object) this.x.ToString(format, formatProvider), (object) this.y.ToString(format, formatProvider), (object) this.z.ToString(format, formatProvider));
			return $"({vector.x},{vector.y},{vector.z})";
		}

		public static string ToAccurateString(this Quaternion q)
		{
//			return UnityString.Format("({0}, {1}, {2})", (object) this.x.ToString(format, formatProvider), (object) this.y.ToString(format, formatProvider), (object) this.z.ToString(format, formatProvider));
			return $"({q.x},{q.y},{q.z},{q.w})";
		}

		public static double DistanceToPlane(this Vector3 pt, Vector3 p0, Vector3 pn)
		{
			double d = -pn.x * (double)p0.x - pn.y * (double)p0.y - pn.z * (double)p0.z;
			return pn.x * (double)pt.x + pn.y * (double)pt.y + pn.z * (double)pt.z + d;
		}
		
		public static double DistanceToLineXY(this Vector3 pt, Vector3 p0, Vector3 pn)
		{
			double d = -pn.x * (double)p0.x - pn.y * (double)p0.y;
			return pn.x * (double)pt.x + pn.y * (double)pt.y + d;
		}

		public static bool AlmostEqual(this Vector3 v0, Vector3 v1)
		{
			// Yes, it seems like it's worth doing this explicitly instead of doing (v1-v0).sqrMagnitude
			double dx = v0.x - v1.x;
			double dy = v0.y - v1.y;
			double dz = v0.z - v1.z;
			return Math.Sqrt(dx * dx + dy * dy + dz * dz) <= Vertex.ACCURACY;
		}

		public static bool IsInTriangle(this Vector3 ip, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 n)
		{
			Vector3 edge0 = v1 - v0; 
			Vector3 edge1 = v2 - v1; 
			Vector3 edge2 = v0 - v2; 
			Vector3 C0 = ip - v0; 
			Vector3 C1 = ip - v1; 
			Vector3 C2 = ip - v2;
			return (Vector3.Dot(n, Vector3.Cross(edge0, C0)) >= 0 &&
			        Vector3.Dot(n, Vector3.Cross(edge1, C1)) >= 0 &&
			        Vector3.Dot(n, Vector3.Cross(edge2, C2)) >= 0);
		}

		public static float SideOf(this Vector2 pt, Vector2 vector, float zup=1)
		{
			return zup * (pt.x * vector.y - pt.y * vector.x);
		}

		public static bool IsInTriangle(this Vector2 ip, Vector2 v0, Vector2 v1, Vector2 v2, float zup=1)
		{
			return (ip - v0).SideOf(v1 - v0, zup) <= 0 && (ip - v1).SideOf(v2 - v1, zup) <= 0 && (ip - v2).SideOf(v0 - v2,zup) <= 0;
		}
		
		public static bool IntersectsPlane(this Ray ray, Vector3 planePoint, Vector3 planeNormal, out float distance, out Vector3 pt)
		{
			// 1: planeNormal.x * (pt.x - planePoint.x) + planeNormal.y * (pt.y - planePoint.y) + planeNormal.z * (pt.z - planePoint.z) = 0;
			// 2: pt = rayOrigin + distance * rayDirection;

			Vector3 v = ray.origin - planePoint;
			  
			distance = (-planeNormal.x*v.x - planeNormal.y*v.y - planeNormal.z*v.z) / (planeNormal.x*ray.direction.x  +  planeNormal.y*ray.direction.y + planeNormal.z*ray.direction.z);

			if (double.IsNaN(distance) || double.IsInfinity(distance) || distance<0)
			{
				pt = Vector3.zero;
				return false;
			}
			
			pt = ray.origin + distance * ray.direction;
			return true;
		}

		public static bool IntersectsTriangle(this Ray ray, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 n, out float f, out Vector3 hitpoint)
		{
			if (!ray.IntersectsPlane(p0, n, out f, out hitpoint))
			{
				return false;
			}

			return hitpoint.IsInTriangle(p0, p1, p2, n);
		}
	}
}