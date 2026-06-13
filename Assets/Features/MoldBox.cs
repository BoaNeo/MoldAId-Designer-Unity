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
	public class MoldBox : Mold
	{
		[ShowProperty(name="Height (Z)", unit = "mm")]
		[FeatureInput] public DataRef<float> height { get; } = new (10);

		[ShowProperty(name="Width (X)", unit = "mm")]
		[FeatureInput] public DataRef<float> width { get; } = new (10);

		[ShowProperty(name="Depth (Y)", unit = "mm")]
		[FeatureInput] public DataRef<float> depth { get; } = new (10);

		[ShowProperty(unit = "mm")]
		[FeatureInput] public DataRef<float> fillet { get; } = new (1);

		public override void ReadSettingsFrom(MoldFile projectMold)
		{
			depth.Set( projectMold.depth );
			height.Set(projectMold.height);
			width.Set(projectMold.width);
			fillet.Set(projectMold.fillet);
		}

		public override void SaveSettingsTo(MoldFile moldFile)
		{
			moldFile.depth = depth;
			moldFile.fillet = fillet;
			moldFile.height = height;
			moldFile.width = width;
		}

		public static IEnumerator<IYield> Build(bool changing,
			float height,
			float width,
			float depth,
			float fillet,
			float inSprueHeight,
			float inSprueDiameter,
			DataRef<MeshBuilder> outSprueMesh,
			DataRef<XForm> outSprueTransform,
			DataRef<MeshBuilder> output)
		{
			Shape shape = new Shape();

			float x = width / 2 - fillet;
			float y = depth / 2 - fillet;
			Vector3 n = Vector3.forward;
			Vector3 c = new Vector3(x,y, 0);
			shape.AddArc(c,n,fillet,5,0, Mathf.PI/2);
			c = new Vector3(x,-y, 0);
			shape.AddArc(c,n,fillet,5,Mathf.PI/2,Mathf.PI);
			c = new Vector3(-x,-y, 0);
			shape.AddArc(c,n,fillet,5,-Mathf.PI,-Mathf.PI/2);
			c = new Vector3(-x,y, 0);
			shape.AddArc(c,n,fillet,5,-Mathf.PI/2, 0);
				
			MeshBuilder moldMesh = MeshExtrude.Extrude(new MeshBuilder(), shape, n*height, true);

			BuildSprue(moldMesh, inSprueHeight, inSprueDiameter, outSprueMesh, outSprueTransform);

			output.Set(moldMesh);
			
			yield break;
		}
	}
}