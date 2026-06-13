using System.Collections.Generic;
using Dialogs;
using FeatureGraph;
using Files;
using Gizmos;
using MeshUtil;
using MeshUtil.Extensions;
using PropertySheet;

namespace Features
{
	public class MoldSTL : Mold
	{
		[ShowProperty(filter = FileDialog.FileFilter.STL)]
		[FeatureInput] public DataRef<PathProperty> path { get; } = new (new PathProperty(""));

		public override void ReadSettingsFrom(MoldFile projectMold)
		{
			path.Set( new PathProperty(projectMold.path));
		}

		public override void SaveSettingsTo(MoldFile file)
		{
			file.path = path.value.path;
		}

		public static IEnumerator<IYield> Build(bool changing,
			PathProperty path,
			XForm inSpruePosition,
			float inSprueHeight,
			float inSprueDiameter,
			DataRef<MeshBuilder> outSprueMesh,
			DataRef<XForm> outSprueTransform,
			DataRef<MeshBuilder> output)
		{
			yield return Until.RunningInBackground;
			MeshBuilder moldMesh = MeshImport.Import(new MeshBuilder(), path.path);
			BuildSprue(moldMesh, inSpruePosition, inSprueHeight, inSprueDiameter, outSprueMesh, outSprueTransform);
			yield return Until.RunningOnMainThread;
			
			output.Set(moldMesh);
		}
	}
}