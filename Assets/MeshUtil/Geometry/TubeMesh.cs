using System;
using System.Collections.Generic;
using UnityEngine;

namespace MeshUtil
{
	public partial class Geometry
	{
		// TODO: Move to a MeshBuilder extension and get rid of the MeshInfo... Or simply build the MeshDataArray directly
		
		public static MeshInfo GenerateTubeMesh(List<Vector3> path, int sides, bool cap, Func<float,float>radiusAt)
	  {
		  Vector3[] vertices = new Vector3[sides * path.Count + (cap ? 2 + 2*sides: 0)];
		  int[] triangles = new int[ 3*( 2 * sides*(path.Count-1) + (cap ? 2*sides:0)) ];
		  int v = 0, t=0;
		  float length = 0;

		  Vector3 n1 = Vector3.zero;
		  for (int i = 0; i < path.Count; i++)
		  {
			  Vector3 fwd = i == path.Count-1 ? path[i] - path[i-1] : path[i + 1] - path[i];
			  float len = fwd.magnitude;
			  fwd /= len;

			  if (i == 0)
			  {
				  // On the first iteration we pick any plane vector for our cross section that is not parallel to our normal (that's why there are two alternatives here)
				  n1 = Mathf.Abs(fwd.z)>Mathf.Abs(fwd.x) ? new Vector3(fwd.x, -fwd.z, fwd.y) : new Vector3(fwd.y, -fwd.x, fwd.z);
			  }
			  else
			  {
				  // On each of the following cross sections we can't simply pick two random perpendicular vectors to define our cross section plane
				  // since a change in sign in the chosen normal may rotate our plane around the normal causing a twist in the tube.
				  // Instead we pick a new n1 by projecting the previous n1 onto the new cross section plane, thus ensuring that
				  // the new plane is aligned as closely to the previous plane as possible.
				  n1 = n1 - Vector3.Dot(n1, fwd) * fwd;
			  }

			  n1.Normalize();
			  Vector3 n2 = Vector3.Cross(fwd, n1);
			  n2.Normalize();

			  float radius = radiusAt(length);

			  for (int j = 0; j < sides; j++)
			  {
				  float a = j * 2 * Mathf.PI / sides;
				  float cosa = Mathf.Cos(a);
				  float sina = Mathf.Sin(a);
				  // Build the vertices for our 2D circular cross section on the plane spanned by (n1 x n2).
				  Vector3 p = new Vector3(
					  radius * ( cosa*n1.x +sina*n2.x),
					  radius * ( cosa*n1.y +sina*n2.y),
					  radius * ( cosa*n1.z +sina*n2.z));
				  vertices[v++] = path[i] + p;

				  // Create faces between this cross section and the previous one (if there is any)
				  if (i > 0)
				  {
					  int c1 = v - 1;
					  int c0 = j == 0 ? v-2 + sides : v-2;
					  triangles[t++] = c0-sides;
					  triangles[t++] = c1-sides;
					  triangles[t++] = c0;

					  triangles[t++] = c0;
					  triangles[t++] = c1-sides;
					  triangles[t++] = c1;
				  }
			  }

			  length += len;
		  }

		  if (cap)
		  {
			  int e = v-sides;
			  int c0 = v;
			  vertices[v++] = path[0];
			  int c1 = v;
			  vertices[v++] = path[path.Count - 1];
			  int e0 = v;
			  int e1 = v+sides;
			  for (int i = 0; i < sides; i++)
			  {
				  vertices[e0+i] = vertices[i];
				  vertices[e1+i] = vertices[e + i];
			  }
			  for (int i = 0; i < sides;i++)
			  {
				  triangles[t++] = e0+i;
				  triangles[t++] = c0;
				  triangles[t++] = e0+(i+1)%sides;

				  triangles[t++] = c1;
				  triangles[t++] = e1 + i;
				  triangles[t++] = e1 + (i+1)%sides;
			  }
		  }

		  MeshInfo mesh = new MeshInfo();
		  mesh.transform = Matrix4x4.identity;
		  mesh.vertices = vertices;
		  mesh.triangles = triangles;
		  return mesh;
	  }
	}
}