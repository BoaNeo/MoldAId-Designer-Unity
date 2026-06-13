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
	public class GuideBolt : Guide
	{
		[ShowProperty(values = new [] {"M3", "M4", "M5", "M6"})]
		[FeatureInput] public DataRef<int> boltType { get; } = new DataRef<int>( 0 );

		[ShowProperty(unit = "mm", min = 5, max = 250)]
		[FeatureInput] public DataRef<float> length { get; } = new(25.0f);

		private struct Bolt
		{
			public float diameter;
			public float headDiameter;
			public float headHeight;
			public float nutDiameter;
			public float nutHeight;
		}

		private static Bolt[] _bolts = new[]
		{
			new Bolt { diameter = 3.0f, headDiameter = 6.0f, headHeight = 6.0f, nutDiameter = 6.582f, nutHeight = 6.0f },
			new Bolt { diameter = 4.0f, headDiameter = 7.5f, headHeight = 7.5f, nutDiameter = 8.314f, nutHeight = 7.5f },
			new Bolt { diameter = 5.0f, headDiameter = 9.0f, headHeight = 9.0f, nutDiameter = 9.469f, nutHeight = 9.0f },
			new Bolt { diameter = 6.0f, headDiameter = 10.5f, headHeight = 10.5f, nutDiameter = 11.778f, nutHeight = 10.5f }
		};

		public static IEnumerator<IYield> Build(bool changing,
			int boltType,
			float length,
			XForm position,
			MeshBuilder mold,
			DataRef<MeshBuilder> output)
		{
			Bounds bounds = mold.GetBounds();

//			yield return Until.RunningInBackground;

			float penetration = 0.1f;

			Bolt bolt = _bolts[boltType];

			float total_length = 2*(bounds.extents.z+penetration);
			float excess = total_length - length;

			float head_extrusion = penetration + excess / 2;
			float bore_extrusion = length;
			float hex_extrusion = total_length - (head_extrusion + bore_extrusion);
				
			MeshBuilder out1 = new MeshBuilder();

			Vector3 zero = Vector3.forward* penetration;
			Shape head = new Shape().AddArc(Vector3.zero, Vector3.forward, bolt.headDiameter/2.0f, Shape.ArcLenToSegments(bolt.headDiameter/2.0f, PreferencesFile.current.arcLength), 0, 2 * Mathf.PI);
			Shape bore = new Shape().AddArc(Vector3.zero, Vector3.forward, bolt.diameter / 2.0f, Shape.ArcLenToSegments(bolt.diameter / 2.0f, PreferencesFile.current.arcLength), 0, 2 * Mathf.PI);
			Shape hex = new Shape().AddArc(Vector3.zero, Vector3.forward, bolt.nutDiameter/2.0f, 6, 0, 2 * Mathf.PI);
			Shape[] shapes = { head, head, bore, bore, hex, hex };
			Vector3[] steps =
			{
				head_extrusion * Vector3.forward,
				Vector3.zero, 
				bore_extrusion * Vector3.forward, 
				Vector3.zero,
				hex_extrusion * Vector3.forward, 
				Vector3.zero,
			};
			out1.Extrude(shapes, steps, true, true);


			Matrix4x4 m = Matrix4x4.Translate( new Vector3(0, 0, -penetration ) );

			out1 = out1.TransformGPU(m);
//			yield return Until.RunningOnMainThread;
			output.Set( out1 );

			yield break;
		}
	}
}