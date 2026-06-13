using System.Collections.Generic;
using FeatureGraph;
using Gizmos;
using MeshUtil;
using PropertySheet;
using StageBar;
using UnityEngine;

namespace Features
{
	public class RunnerIn : Runner
	{
		[ShowProperty(unit = "mm", min = 1, max = 50)]
		[FeatureInput] public DataRef<float> sprueBottomOffset { get; } = new(2);

		[ShowProperty(unit = "deg", min = -360, max = 360)]
		[FeatureInput] public DataRef<float> sprueExitAngle { get; } = new(0);

		[ShowProperty]
		[FeatureInput] public DataRef<bool> sprueExitBottom { get; } = new(true);

		[FeatureInput] public DataRef<float> sprueDiameter { get; } = new(5);

		[ShowProperty(order=1001)]
		public void MoveGate(IStageContext context)
		{
			ImportedPart part = context.GetFeature<ImportedPart>();
			context.PickFromScene("Select Gate Attachment Point on the <b>Part</b>", new[] {part.output.blockname}, (RaycastHit hit2) =>
			{
				if (!hit2.collider)
					return false;
				end.Set(new XForm(endParent, hit2.point, Quaternion.LookRotation(hit2.normal))); 
				return true;
			});
		}

		public static IEnumerator<IYield> Build(bool changing,
			float sprueBottomOffset,
			float sprueExitAngle,
			bool sprueExitBottom,
			float sprueDiameter,
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
			//yield return FeatureManager.YieldUntil.RunningInBackground;
			XForm s;
			if (sprueExitBottom)
			{
				s = new XForm( Vector3.zero, Quaternion.LookRotation(Vector3.back));
			}
			else
			{
				Quaternion look = Quaternion.Euler(0, 0,sprueExitAngle);
				Vector3 v = look * Vector3.left;
				s = new XForm( sprueBottomOffset*Vector3.forward + 0.5f*sprueDiameter*v, Quaternion.LookRotation(v));
			}

			MeshBuilder runner = BuildRunner(startParent, s,endParent,end,controls,penetration,diameter/2,smoothness,straightEnds,gateDiameter/2,gateLength,(GateStyle)gateStyle,0,0,GateStyle.Bell);
			
			//yield return FeatureManager.YieldUntil.RunningInMainThread;

			output.Set(runner);
			yield break;
		}		
	}
}