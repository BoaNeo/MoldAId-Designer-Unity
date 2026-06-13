using FeatureGraph;
using Gizmos;
using MeshUtil;
using PropertySheet;

namespace Features
{
	public abstract class Guide : Feature
	{
		[ShowProperty(unit = "mm")]
		[FeatureInput] public DataRef<XForm> position { get; } = new( XForm.identity);
		[FeatureInput] public DataRef<MeshBuilder> mold { get; } = new( null);

		[FeatureOutput] public DataRef<MeshBuilder> output { get; } = new();
	}
}