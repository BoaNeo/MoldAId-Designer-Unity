using System.Collections.Generic;
using FeatureGraph;
using Files;
using Gizmos;
using MeshUtil;
using MeshUtil.Extensions;
using PropertySheet;
using UnityEngine;

namespace Features
{
	public class GuidePin : Guide
	{
		[ShowProperty(unit = "mm", min = 1, max = 12)]
		[FeatureInput] public DataRef<float> pinSize { get; } = new(3.0f);

		[ShowProperty(unit = "mm", min = 5, max = 250)]
		[FeatureInput] public DataRef<float> length { get; } = new(0f);

		public static IEnumerator<IYield> Build(bool changing,
			float pinSize,
			float length,
			XForm position,
			MeshBuilder mold,
			DataRef<MeshBuilder> output)
		{
			Bounds bounds = mold.GetBounds();

			float penetration = 0.1f;

//			yield return FeatureManager.YieldUntil.RunningInBackground;

			float h = length>0 ? length+penetration : 2*(bounds.extents.z+penetration);
			MeshBuilder mb = new MeshBuilder();
		
			float radius = pinSize / 2.0f;
			mb.GenerateCylinder(radius, h, Shape.ArcLenToSegments(radius,PreferencesFile.current.arcLength));

			Matrix4x4 m = Matrix4x4.Translate( new Vector3(0, 0, -penetration ) );
			mb = mb.TransformGPU(m);
			
//			yield return FeatureManager.YieldUntil.RunningInMainThread;
			
			output.Set( mb );
			yield break;
		}
	}
}