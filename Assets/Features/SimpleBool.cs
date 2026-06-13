using System.Collections.Generic;
using FeatureGraph;
using Gizmos;
using MeshUtil;
using MeshUtil.Extensions;

namespace Features
{
	public class SimpleBool : Feature
	{
		[FeatureInput] public DataRef<MeshBuilder> inner { get; } = new (null);
		[FeatureInput] public DataRef<XForm> innerXform { get; } = new ();
		[FeatureInput] public DataRef<MeshBuilder> outer { get; } = new (null);

		[FeatureOutput] public DataRef<MeshBuilder> output { get; } = new (null);

		public static IEnumerator<IYield> Build(bool changing,
			MeshBuilder inner,
			XForm innerXform,
			MeshBuilder outer,
			DataRef<MeshBuilder> output)
		{
			if (changing || inner==null || outer==null)
				yield break;

			MeshBuilder boolean = outer.CopyGPU();
			boolean.AppendGPU(inner, innerXform.localToWorldMatrix, true);
			output.Set(boolean);
		}
	}
}