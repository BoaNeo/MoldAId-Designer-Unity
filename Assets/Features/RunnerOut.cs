using System.Collections.Generic;
using FeatureGraph;
using Gizmos;
using MeshUtil;
using PropertySheet;
using StageBar;
using UnityEngine;

namespace Features
{
	public class RunnerOut : Runner
	{
		[ShowProperty(unit = "mm", min = 0.5f, max = 6.0f, order = -1000)]
		[FeatureInput] public DataRef<float> exitDiameter { get; } = new(2.8f);

		[ShowProperty(unit = "mm", min = 0.1f, max = 10.0f, order = -1001)]
		[FeatureInput] public DataRef<float> exitLength { get; } = new(2.8f);

		[ShowProperty(values = new []{"Taper","Bell"}, order = -1002)]
		[FeatureInput] public DataRef<int> exitStyle { get; } = new(0);
		
		[ShowProperty(order=1002)]
		public void MoveGate(IStageContext context)
		{
			ImportedPart part = context.GetFeature<ImportedPart>();
			context.PickFromScene("Select Gate Attachment Point on the <b>Part</b>", new[] {part.output.blockname}, (RaycastHit hit1) =>
			{
				if (hit1.collider == null)
					return false;
				end.Set(new XForm( part.transform, hit1.point,  Quaternion.LookRotation(hit1.normal)));
				return true;
			});
		}
		
		[ShowProperty(order=1001)]
		public void MoveExit(IStageContext context)
		{
			Mold mold = context.GetFeature<Mold>();
			context.PickFromScene("Select an Exit Point on the <b>Mold</b>", new[] {mold.output.blockname}, (RaycastHit hit2) =>
			{
				if (hit2.collider == null)
					return false;
				start.Set( new XForm ( hit2.point,  Quaternion.LookRotation(-hit2.normal)));
				return true;
			});
		}

		public static IEnumerator<IYield> Build(bool changing,
			float exitDiameter,
			float exitLength,
			int exitStyle,
			float diameter,
			float penetration,
			float smoothness,
			float straightEnds,
			float gateDiameter,
			float gateLength,
			int gateStyle,
			XForm start,
			XForm end,
			XForm startParent,
			XForm endParent,
			XForm[] controls,
			DataRef<MeshBuilder> output)
		{
			MeshBuilder runner = BuildRunner(startParent, start,endParent,end,controls,penetration,diameter/2,smoothness,straightEnds,gateDiameter/2,gateLength,(GateStyle)gateStyle,exitDiameter/2,exitLength,(GateStyle)exitStyle);

			output.Set(runner);
			yield break;
		}
	}
}