using System.Collections.Generic;
using Features;
using Gizmos;
using StageBar;
using Undo;
using UnityEngine;
using Visuals;

namespace Stages
{
	public class GuidesStage : Stage
	{
		public override string name => "Pins & Bolts";
		public override void BuildUI(StageBarUI ui)
		{
			ui.BeginUpdate();
			
			SimpleBool sb = context.SetEnabled<SimpleBool>(true);
			if (sb == null)
			{
				ui.AddText("No Cutplanes!");
			}
			else
			{
				List<Guide> bolts = context.GetFeatures<Guide>();
				ui.AddList("Bolts", bolts.Count, (row,item) => item.Setup(bolts[row].name, bolts[row] == context.GetSelectedFeature()), (row,doubleclick) => context.SelectFeature( bolts[row]) );
				ui.AddAction("Add Bolt", () =>
				{
					AddGuide<GuideBolt>("Bolt");
					BuildUI(ui);
				});
				ui.AddAction("Add Pin", () =>
				{
					AddGuide<GuidePin>("Pin");
					BuildUI(ui);
				});
			}
			
			ui.EndUpdate();		
		}

		public override void Activate()
		{
			SimpleBool sb = context.SetEnabled<SimpleBool>(true);
			if (sb == null)
				return;
			
			context.SetVisual(true, sb.output, null, Visual.Mode.Transparent,AppColors.BOOLEAN_COLOR, false, Main.SelectionPriority.Secondary);

			Mold mold = context.GetFeature<Mold>();
			context.SetVisual(true, mold?.outSprueMesh, mold?.outSprueTransform, Visual.Mode.Opaque,AppColors.SPRUE_COLOR, false, Main.SelectionPriority.Primary);

			List<CutPlaneInitial> cutplanes = context.SetEnabledAll<CutPlaneInitial>(false);
			foreach (CutPlaneInitial cutplane in cutplanes)
				context.SetVisual( cutplane.showCutplane, cutplane.plane,cutplane.position, Visual.Mode.Transparent, AppColors.CUTPLANE_COLOR);
			
			foreach (Runner runner in context.SetEnabledAll<Runner>(true))
				context.SetVisual(true, runner.output, null, Visual.Mode.Opaque,runner is RunnerOut ? AppColors.OUTLET_COLOR : AppColors.INLET_COLOR, false, Main.SelectionPriority.Primary);

			List<Guide> guides = context.SetEnabledAll<Guide>(true);
			foreach (Guide guide in guides)
			{
				context.SetVisual(true, guide.output, guide.position, Visual.Mode.Opaque,AppColors.GUIDE_COLOR, guide==context.GetSelectedFeature(), Main.SelectionPriority.Primary, () => context.SelectFeature(guide));
				context.SetGizmo( guide==context.GetSelectedFeature(), guide.position, GizmoHandleFlags.MoveX|GizmoHandleFlags.MoveY);
			}

			context.SelectOneOf(guides.ToArray());
		}

		public override void DeActivate()
		{
		}
		
		private void AddGuide<T>(string featurename) where T: Guide, new()
		{
			if (context == null)
				return;
			
			SimpleBool sb = context.GetFeature<SimpleBool>();
			context.PickFromScene($"Select {featurename} Placement", new[] { sb.output.blockname }, (RaycastHit hit1) =>
			{
				if (!hit1.collider)
					return false;
				UndoManager.Append(() =>
				{
					Guide guide = context.AddFeature<T>(featurename);
					guide.position.Set(new XForm( hit1.point,  Quaternion.LookRotation(-hit1.normal)));
					guide.mold.UseDataFrom(sb.output);
					context.SelectFeature(guide);
					return guide;
				}, guide =>
				{
					context.RemoveFeature(guide);
				});
				return true;
			});
		}
	}
}