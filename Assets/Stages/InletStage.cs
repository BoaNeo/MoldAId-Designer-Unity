using System.Collections.Generic;
using FeatureGraph;
using Features;
using Gizmos;
using StageBar;
using Undo;
using UnityEngine;
using Visuals;

namespace Stages
{
	public class InletStage : Stage
	{
		public override string name => "Inlet Runners";

		public override void BuildUI(StageBarUI ui)
		{
			ui.BeginUpdate();
			List<RunnerIn> inlets = context.GetFeatures<RunnerIn>();
			ui.AddList("Inlets", inlets.Count, (row,item) => item.Setup(inlets[row].name, inlets[row] == context.GetSelectedFeature()), (row,doubleclick) => context.SelectFeature( inlets[row]) );
			ui.AddAction("Add Inlet", () =>
			{
				AddInletRunner("Inlet");
				BuildUI(ui);
			});
			ui.EndUpdate();
		}

		public override void Activate()
		{
			List<Runner> runners = context.SetEnabledAll<Runner>(true);
			foreach (Runner runner in runners)
			{
				if (runner is RunnerOut)
					context.SetVisual(true, runner.output, null, Visual.Mode.Opaque,AppColors.OUTLET_COLOR, false, Main.SelectionPriority.Secondary);
				else
					context.SetVisual(true, runner.output, null, Visual.Mode.Opaque,AppColors.INLET_COLOR, runner==context.GetSelectedFeature(), Main.SelectionPriority.Primary, () => context.SelectFeature(runner), runner.DebugDraw);
				foreach(DataRef<XForm> xf in runner.controls)
					context.SetGizmo( runner==context.GetSelectedFeature(), xf, GizmoHandleFlags.All, GizmoSpace.World);
			}

			Mold mold = context.SetEnabled<Mold>(true);
			context.SetVisual(true, mold?.output, null, Visual.Mode.Transparent,AppColors.MOLD_COLOR, false, Main.SelectionPriority.Secondary);
			context.SetVisual(true, mold?.outSprueMesh, mold?.outSprueTransform, Visual.Mode.Opaque,AppColors.SPRUE_COLOR, false, Main.SelectionPriority.Primary);

			ImportedPart part = context.SetEnabled<ImportedPart>(true);
			context.SetVisual(true, part?.output, part?.transform, Visual.Mode.Opaque,AppColors.PART_COLOR, false, Main.SelectionPriority.Primary);

			foreach (CutPlaneInitial cutplane in context.SetEnabledAll<CutPlaneInitial>(false))
				context.SetVisual(true, cutplane.plane, cutplane.position, Visual.Mode.Transparent,AppColors.CUTPART_COLOR);

			context.SelectOneOf(context.GetFeatures<RunnerIn>().ToArray());
		}

		public override void DeActivate()
		{
		}
		
		private void AddInletRunner(string featurename)
		{
			if (context == null)
				return;

			Mold mold = context.GetFeature<Mold>();
			ImportedPart part = context.GetFeature<ImportedPart>();
			context.PickFromScene("Select Gate Attachment Point on the <b>Part</b>", new[] {part.output.blockname}, (RaycastHit hit2) =>
			{
				if (!hit2.collider)
					return false;
				
				UndoManager.Append(() =>
				{
					Vector3 startDirection = hit2.point - mold.outSprueTransform.value.position;

					// Yes, this is weird, but the Quaternion really likes its Y up
					startDirection.z = -startDirection.y;
					startDirection.y = 0;
					Quaternion q = Quaternion.LookRotation(startDirection);

					RunnerIn runner = context.AddFeature<RunnerIn>(featurename);
					runner.startParent.UseDataFrom(mold.outSprueTransform);
					runner.sprueDiameter.UseDataFrom( mold.inSprueDiameter );
					runner.sprueBottomOffset.Set(mold.inSprueHeight / 2);
					runner.sprueExitAngle.Set( q.eulerAngles.y + 90);

					if (context.GetSelectedFeature() is RunnerIn defaultRunner)
					{
						runner.diameter.Set( defaultRunner.diameter.value );
						runner.sprueBottomOffset.Set( defaultRunner.sprueBottomOffset.value );
						runner.sprueExitBottom.Set(defaultRunner.sprueExitBottom.value);
						runner.penetration.Set(defaultRunner.penetration.value);
						runner.gateDiameter.Set(defaultRunner.gateDiameter.value);
						runner.gateLength.Set(defaultRunner.gateLength.value);
						runner.gateStyle.Set(defaultRunner.gateStyle.value);
					}
					
					runner.endParent.UseDataFrom( part.transform );
					runner.end.Set(new XForm(part.transform, hit2.point, Quaternion.LookRotation(hit2.normal, Vector3.forward)));
					context.SelectFeature(runner);
					return runner;
				}, runner =>
				{
					context.RemoveFeature(runner);
				});
				return true;
			});
		}
	}
}