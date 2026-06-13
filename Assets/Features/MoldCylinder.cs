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
	public class MoldCylinder : Mold
	{
		[ShowProperty(name="Height (Z)", unit = "mm")]
		[FeatureInput] public DataRef<float> height { get; } = new (20);

		[ShowProperty(unit="mm")]
		[FeatureInput] public DataRef<float> diameter { get; } = new (10);

		public override void ReadSettingsFrom(MoldFile projectMold)
		{
			height.Set(projectMold.height);
			diameter.Set(projectMold.width);
		}
		
		public override void SaveSettingsTo(MoldFile moldFile)
		{
			moldFile.height = height;
			moldFile.width = diameter;
		}
		
		public static IEnumerator<IYield> Build(bool changing,
			float height,
			float diameter,
			XForm inSpruePosition,
			float inSprueHeight,
			float inSprueDiameter,
			DataRef<MeshBuilder> outSprueMesh,
			DataRef<XForm> outSprueTransform,
			DataRef<MeshBuilder> output)
		{
			float radius = diameter/2.0f;
			float arcLength = PreferencesFile.current.arcLength;

			float circ = 2 * Mathf.PI * radius;
			int sides = Mathf.Clamp(Mathf.CeilToInt(circ / arcLength),3,120);
			MeshBuilder moldMesh = MeshCylinder.GenerateCylinder(new MeshBuilder(), radius, height, sides);
			
			BuildSprue(moldMesh, inSpruePosition, inSprueHeight, inSprueDiameter, outSprueMesh, outSprueTransform);
			
			output.Set(moldMesh);

			yield break;
		}
	}
}