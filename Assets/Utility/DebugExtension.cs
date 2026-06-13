using UnityEngine;

namespace Utility
{
	public static class DebugExtension
	{

		public static void DrawBox(Vector3 p, float s, Color color)
		{
			Vector3 ul = new Vector3(-1, 1, 0);
			Vector3 ur = new Vector3(1, 1, 0);
			Vector3 lr = new Vector3(1, -1, 0);
			Vector3 ll = new Vector3(-1, -1, 0);
			Debug.DrawLine(p+s*ul,p+s*ur, color);
			Debug.DrawLine(p+s*ur,p+s*lr, color);
			Debug.DrawLine(p+s*lr,p+s*ll, color);
			Debug.DrawLine(p+s*ll,p+s*ul, color);
		}
	}
}