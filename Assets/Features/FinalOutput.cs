using System.Collections.Generic;
using BlenderSupport;
using FeatureGraph;
using Gizmos;
using MeshUtil;
using UnityEngine;

namespace Features
{
	public class FinalOutput : Feature
	{
		[FeatureInput] public DataRef<MeshBuilder> simplebool { get; } = new();
		[FeatureInput] public DataRef<MeshBuilder> sprue { get; } = new();
		[FeatureInput] public DataRef<XForm> sprueTransform { get; }  = new();
		// TODO: This use of List<DataRef> is a mess - all inputs should be DataRef<Something>, either change to DataRef<List<T>> or create a special DataRefCollection<T>
		[FeatureInput] public List<DataRef<MeshBuilder>> runners { get; set; } = new();
		[FeatureInput] public List<DataRef<MeshBuilder>> guides { get; set; } = new();
		[FeatureInput] public List<DataRef<XForm>> guideTransforms { get; set;  }  = new();

		[FeatureOutput] public DataRef<MeshBuilder> output { get; } = new ();

		public static IEnumerator<IYield> Build(bool changing,
			MeshBuilder simplebool,
			MeshBuilder sprue,
			XForm sprueTransform,
			MeshBuilder[] runners,
			MeshBuilder[] guides,
			XForm[] guideTransforms,
			DataRef<MeshBuilder> output)
		{
			if (changing)
				yield break;

			Blender blender = new Blender();
			
			yield return Until.RunningInBackground;
		  
			List<Blender.ExportFile> objects = new ();

//			ImportedPart part = context.GetFeature<ImportedPart>();
//			objects.Add( new BlenderBoolean.ExportFile { mesh = mold.output.value, transform = Matrix4x4.identity,name = "mold", op = BlenderBoolean.Operation.BASE });
//			objects.Add( new BlenderBoolean.ExportFile { mesh = part.output.value, transform = part.transform.value.localToWorldMatrix, name = "part", op = BlenderBoolean.Operation.DIFFERENCE });

			objects.Add( new Blender.ExportFile { mesh = simplebool, transform = Matrix4x4.identity,name = "mold", op = Blender.Operation.BASE });
			objects.Add( new Blender.ExportFile { mesh = sprue, transform = sprueTransform.localToWorldMatrix, name = "sprue", op = Blender.Operation.DIFFERENCE });

			for (int i = 0; i < runners.Length; i++)
			{
				objects.Add( new Blender.ExportFile { mesh = runners[i], name = $"runner{i}", op = Blender.Operation.DIFFERENCE });
			}

			for (int i = 0; i < guides.Length; i++)
			{
				objects.Add( new Blender.ExportFile { mesh = guides[i], transform = guideTransforms[i].localToWorldMatrix, name = $"guide{i}", op = Blender.Operation.DIFFERENCE });
			}

			MeshBuilder out1 = blender.Boolean(objects);

			yield return Until.RunningOnMainThread;

			output.Set( out1 );
		}
	}
}