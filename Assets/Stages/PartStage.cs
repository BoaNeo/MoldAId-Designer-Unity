using Dialogs;
using Dialogs.CrossSection;
using FeatureGraph;
using Features;
using Gizmos;
using StageBar;
using Visuals;

namespace Stages
{
	public class PartStage : Stage
	{
		private Visual _partVis;
		private Gizmo _partGiz;
		private int _renderMode;
		public override string name => "Part Inspection";
		public override void BuildUI(StageBarUI ui)
		{
			ui.BeginUpdate();
			ui.AddAction("Re-import", () =>
			{
				string error = context.GetFeature<ImportedPart>().Reimport();
				if(error!=null)
					DialogManager.Show<MessageBox>().WithMessage("Unable to re-import STL file!", error, () => { });
			});
			ui.AddAction("Show Cross Section", () => { DialogManager.Show<CrossSectionUI>(false).WithVisuals( new []{_partVis}, new []{_partGiz} ); });
			ui.AddAction("Measure Distance", () => { context.MeasureWithTags( new[]{context.GetFeature<ImportedPart>().output.blockname}); });
			ui.AddEnum("Rendering", new[]{"Standard", "Overhang"}, _renderMode, mode => { _renderMode = mode; context.SelectFeature( context.GetFeature<ImportedPart>()); } );
			ui.EndUpdate();
		}

		public override void Activate()
		{
			ImportedPart part = context.SetEnabled<ImportedPart>(true);
			if (part == null)
				return;

			_partVis = context.SetVisual(true,part?.output, part?.transform, _renderMode==0 ? Visual.Mode.Opaque : Visual.Mode.Overhang,AppColors.PART_COLOR, part == context.GetSelectedFeature(), Main.SelectionPriority.Primary,() => { context.SelectFeature(part); });
			_partGiz = context.SetGizmo( part==context.GetSelectedFeature(), part?.transform, GizmoHandleFlags.All);

			context.SelectOneOf(new Feature[]{ part }, new[]{ false });
		}

		public override void DeActivate()
		{
		}
	}
}