using System.Collections.Generic;
using Dialogs;
using Dialogs.CrossSection;
using Features;
using Gizmos;
using StageBar;
using Undo;
using UnityEngine;
using Visuals;

namespace Stages
{
	public class CutPlaneStage : Stage
	{
		private List<Visual> _partVis;
		private List<Gizmo> _planeGizmos;
		private int _renderMode;
		public override string name => "Cut Plane";

		private Visual.Mode[] _renderModes = { Visual.Mode.Opaque, Visual.Mode.Transparent, Visual.Mode.Overhang};
		public override void BuildUI(StageBarUI ui)
		{
			ui.BeginUpdate();
			List<CutPlaneInitial> cutplanes = context.GetFeatures<CutPlaneInitial>();
			ui.AddAction("Place Cutplane", () =>
			{
				if(context.GetFeature<CutPlane>()!=null)
					UndoManager.Append( RemoveCutPlane, AddCutPlane);

				ImportedPart part = context.GetFeature<ImportedPart>();
				context.PickFromScene( "Select point on part to place a cut plane", new []{part.output.blockname}, hit =>
				{
					if (!hit.collider)
						return false;

					UndoManager.Append(() =>
					{
						AddCutPlane( new XForm( new Vector3(0,0, hit.point.z), Quaternion.identity));
					}, () =>
					{
						RemoveCutPlane();
					});
					BuildUI(ui);
					return true;
				});
			});
			if (cutplanes.Count > 0)
			{
				ui.AddAction("Delete Cutplane", () =>
				{
					UndoManager.Append( RemoveCutPlane, AddCutPlane);
					BuildUI(ui);
				});
			}

			CutPlaneInitial cp = context.GetFeature<CutPlaneInitial>();
			if(cp!=null)
				ui.AddAction("Measure Distance", () => { context.MeasureWithTags( new[]{context.GetFeature<SimpleBool>().output.blockname,cp.mesh1.blockname, cp.mesh2.blockname}); });
			else
				ui.AddAction("Measure Distance", () => { context.MeasureWithTags( new[]{context.GetFeature<SimpleBool>().output.blockname}); });
			ui.AddAction("Show Cross Section", () => { DialogManager.Show<CrossSectionUI>(false).WithVisuals( _partVis.ToArray(), _planeGizmos.ToArray() ); });

			ui.AddEnum("Rendering", new[]{"Standard", "Transparent", "Overhang"}, _renderMode, mode => { _renderMode = mode; context.SelectFeature( context.GetFeature<ImportedPart>()); } );
			
			ui.EndUpdate();

/*			
			ui.BeginUpdate();
			List<CutPlane> cutplanes = context.GetFeatures<CutPlane>();
			ui.AddList("Cutplanes", cutplanes.Count, (row,item) => item.Setup(cutplanes[row].name, cutplanes[row] == context.GetSelectedFeature()), (row,doubleclick) => context.SelectFeature(cutplanes[row]) );
			ui.AddAction("Add Cutplane", () =>
			{
				AddCutPlane("Cut Plane");
				BuildUI(ui);
			});
			ui.EndUpdate();
			*/
		}

		public override void Activate()
		{
			context.ConfigurePrintBox(PrintBox.PrintBoxMode.Floor);

			context.SetEnabled<SimpleBool>(true);

			_partVis = new List<Visual>();
			_planeGizmos = new List<Gizmo>();
			List<CutPlaneInitial> cutplanes = context.SetEnabledAll<CutPlaneInitial>(true);
			foreach (CutPlaneInitial cutplane in cutplanes)
			{
				context.SetVisual( cutplane.showCutplane, cutplane.plane, cutplane.position, Visual.Mode.Transparent, AppColors.CUTPLANE_COLOR, cutplane==context.GetSelectedFeature(), Main.SelectionPriority.Secondary,() => {context.SelectFeature(cutplane);});
				Gizmo v = context.SetGizmo( cutplane==context.GetSelectedFeature(), cutplane.position, GizmoHandleFlags.MoveZ | GizmoHandleFlags.RotateX | GizmoHandleFlags.RotateZ, GizmoSpace.Local, (xform,changing) => SetCutVisuals(cutplane,changing) );
				_planeGizmos.Add(v);
				SetCutVisuals(cutplane, cutplane.mesh1?.value==null || cutplane.mesh2?.value==null);
			}

			foreach (Runner runner in context.SetEnabledAll<Runner>(true))
				context.SetVisual(true, runner.output, null,Visual.Mode.Opaque, runner is RunnerOut ? AppColors.OUTLET_COLOR : AppColors.INLET_COLOR);

			Mold mold = context.SetEnabled<Mold>(cutplanes.Count==0);
			context.SetVisual(mold?.enabled??false, mold?.output, null,Visual.Mode.Transparent, AppColors.MOLD_COLOR, false, Main.SelectionPriority.Secondary);

			ImportedPart part = context.SetEnabled<ImportedPart>(cutplanes.Count==0);
			context.SetVisual(part?.enabled??false,part?.output, part?.transform, Visual.Mode.Opaque, AppColors.PART_COLOR, false, Main.SelectionPriority.Primary);
			
			context.SelectOneOf( cutplanes.ToArray() );
		}

		private void SetCutVisuals(CutPlane cutplane, bool changing)
		{
			Visual v1 = context.SetVisual( true, cutplane.mesh1, null, _renderModes[_renderMode], AppColors.CUTPART_COLOR, false, Main.SelectionPriority.Secondary, () => {context.SelectFeature(cutplane);});
			Visual v2 = context.SetVisual( true, cutplane.mesh2, null, _renderModes[_renderMode], AppColors.CUTPART_COLOR, false, Main.SelectionPriority.Secondary, () => {context.SelectFeature(cutplane);});
			Visual source = context.SetVisual( true, cutplane.input, null, Visual.Mode.Opaque, AppColors.BOOLEAN_COLOR, false, Main.SelectionPriority.Secondary,() => {context.SelectFeature(cutplane);});

			_partVis.Add(v1);
			_partVis.Add(v2);
			XForm xform = cutplane.position;
			source.SetCrossSection(changing, xform.position, xform.forward);

			bool cutsavailable = cutplane.mesh1?.value != null && cutplane.mesh2?.value != null;
			
			source.gameObject.SetActive(!cutsavailable || changing);
			v1.gameObject.SetActive(cutsavailable && !changing);
			v2.gameObject.SetActive(cutsavailable && !changing);
		}

		public override void DeActivate()
		{
		}

		private XForm RemoveCutPlane()
		{
			XForm pt = default;
			// Remove both initial and final cutplanes
			foreach (CutPlane plane in context.GetFeatures<CutPlane>())
			{
				pt = plane.position.value;
				context.RemoveFeature(plane);
			}
			context.RemoveFeature( context.GetFeature<SimpleBool>() );
			Activate();
			return pt;
		}
		
		private void AddCutPlane(XForm xform)
		{
			if (context == null)
				return;
			
			SimpleBool sb = context.GetOrCreateFeature<SimpleBool>("SimpleBool");

			CutPlaneInitial cutplane = context.AddFeature<CutPlaneInitial>(name);
			cutplane.position.Set( xform);
			cutplane.input.UseDataFrom(sb.output);
			cutplane.layerThickness.Set( context.GetProject().layerThickness );

			Activate();

			context.SelectFeature(cutplane);
		}
	}
}