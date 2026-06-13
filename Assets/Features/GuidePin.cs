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
		[ShowProperty(values = new [] {"3mm", "4mm", "5mm", "6mm"})]
		[FeatureInput] public DataRef<int> pinType { get; } = new DataRef<int>( 0 );

		[ShowProperty(unit = "mm", min = 5, max = 250)]
		[FeatureInput] public DataRef<float> length { get; } = new(0f);

		private struct Pin
		{
			public float diameter;
		}

		private static Pin[] _pins = new[]
		{
			new Pin { diameter = 3.0f },
			new Pin { diameter = 4.0f },
			new Pin { diameter = 5.0f },
			new Pin { diameter = 6.0f },
		};

		public static IEnumerator<IYield> Build(bool changing,
			int pinType,
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
		
			Pin pin = _pins[pinType];
			float radius = pin.diameter / 2.0f;
			mb.GenerateCylinder(radius, h, Shape.ArcLenToSegments(radius,PreferencesFile.current.arcLength));

			Matrix4x4 m = Matrix4x4.Translate( new Vector3(0, 0, -penetration ) );
			mb = mb.TransformGPU(m);
			
//			yield return FeatureManager.YieldUntil.RunningInMainThread;
			
			output.Set( mb );
			yield break;
		}
	}
}