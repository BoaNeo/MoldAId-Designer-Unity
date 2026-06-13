using FeatureGraph;
using Gizmos;
using MeshUtil;
using PropertySheet;

namespace Features
{
	public abstract class Guide : Feature
	{
		[FeatureInput] public DataRef<XForm> position { get; } = new( XForm.identity);
		[FeatureInput] public DataRef<MeshBuilder> mold { get; } = new( null);

		[ShowProperty(unit = "mm", min = 0, max = 10)]
		[FeatureInput] public DataRef<float> penetration { get; } = new(0.3f);

		[FeatureOutput] public DataRef<MeshBuilder> output { get; } = new();

	}
}