using System;
using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace MeshUtil
{
	public class Shape
	{
		private List<Vector3> _path = new List<Vector3>();
		public int count => _path.Count;
		public List<Vector3> shape => _path;

		public Shape Add(Vector3 p)
		{
			if(IsValidAt(p, _path.Count))
				_path.Add(p);
			return this;
		}
		
		public Shape Insert(int at,Vector3 p)
		{
			if(IsValidAt(p,at))
				_path.Insert(at,p);
			return this;
		}

		private bool IsValidAt(Vector3 vector3, int at)
		{
			if (_path.Count == 0)
				return true;
			at %= _path.Count;
			if (_path[at].AlmostEqual(vector3, 0.001))
				return false;
			int before =(at+_path.Count-1) % _path.Count;
			if (_path[before].AlmostEqual(vector3, 0.001))
				return false;
			return true;
		}

		public Shape AddArc(Vector3 c, Vector3 n, float r, int steps, float from_rad, float to_rad)
		{
			Quaternion look = Quaternion.FromToRotation(Vector3.forward, n);
			for (int i = 0; i <= steps; i++)
			{
				double a = from_rad + (to_rad-from_rad)* i / steps;
				Vector3 p = c + look * (r * new Vector3((float)Math.Sin(a), (float)Math.Cos(a), 0));
				Add(p);
			}
			return this;
		}
		
		public Shape AddBezierPath(Vector3[] bezpts)
		{
			Matrix4x4 coeff = BEZIER;

			for (int p = 0; p < bezpts.Length-3; p+=3)
			{
				Matrix4x4 ctrlpts = new Matrix4x4(bezpts[p], bezpts[p+1], bezpts[p+2], bezpts[p+3]);
				Matrix4x4 m = ctrlpts * coeff;

				Vector3 p0 = Eval(0);

				if(p==0)
					Add(p0);

				float a = 0;
				while (a < 1.0f)
				{
					Vector3 va = Velocity(a);
					(a,p0) = Subdivide(va,1.0f);
					Add(p0);
				}

				Vector3 Eval(float t)
				{
					float t2 = t * t;
					float t3 = t2 * t;
					Vector4 vt = new Vector4(t3, t2, t, 1);
					return m * vt;
				}

				Vector3 Velocity(float t)
				{
					float t2 = t * t;
					Vector3 v = new Vector3(
					 3*m.m00 * t2 + 2*m.m01*t + m.m02,
					 3*m.m10 * t2 + 2*m.m11*t + m.m12,
 					 3*m.m20 * t2 + 2*m.m21*t + m.m22);
					return v;
				}
/*
				Vector3 Acceleration(float t)
				{
					Vector3 v = new Vector3(
						2*3*m.m00 * t + 2*m.m01,
						2*3*m.m10 * t + 2*m.m11,
						2*3*m.m20 * t + 2*m.m21);
					return v;
				}
*/
				(float,Vector3) Subdivide(Vector3 va,float b)
				{
					Vector3 pt = Eval(b);

					Vector3 vb = Velocity(b);
					Vector3 dir = pt-p0;
					
					// TODO: This is really a dot product derivative - might be cheaper to do a vector normalization and use dot products instead.
					float angle_v = Vector3.Angle(va, vb); // The angle between the current velocity and the target points velocity
					float angle_dir = Vector3.Angle(va, dir); // The angle between the current velocity and the direction to the target

					// This is a de-generate case, but need to bail to avoid infinite sub division
					if (Mathf.Abs(a - b) < Vertex.ACCURACY)
					{
						return (1.0f, pt);
					}
					// Both angles should align fairly accurately to allow a smooth transition to the next point on the curve
					if ( (angle_v < 5 && angle_dir<5))
					{
						return (b,pt);
					}

					// If they don't, we'll subdivide further by picking a point halfway to the target and try again
					b = (a + b) / 2;
					return Subdivide(va,b);
				}
			}
			return this;
		}

		private static Matrix4x4 BEZIER = new Matrix4x4( new Vector4(-1,3,-3,1), new Vector4(3,-6,3,0), new Vector4(-3,3,0,0), new Vector4(1,0,0,0));

		public Vector3 this[int i]
		{
			get => _path[i];
		}

		public static int ArcLenToSegments(float radius, float arcLength)
		{
			float circ = 2 * Mathf.PI * radius;
			return Mathf.Clamp(Mathf.CeilToInt(circ / arcLength),3,120);
		}

		public float CalculateLength()
		{
			float l = 0;
			for (int i = 1; i < _path.Count; i++)
			{
				l += (_path[i] - _path[i - 1]).magnitude;
			}
			return l;
		}

		public void Remove(int i)
		{
			_path.RemoveAt(i);
		}
	}
}