using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Dialogs;
using Dialogs.CrossSection;
using FeatureGraph;
using Features;
using Gizmos;
using IO;
using MeshUtil;
using MeshUtil.Extensions;
using PropertySheet;
using StageBar;
using UnityEngine;
using Visuals;

namespace Stages
{
	public class ExportStage : Stage
	{
		private int _renderMode;
		private Visual.Mode[] _renderModes = { Visual.Mode.Opaque, Visual.Mode.Transparent, Visual.Mode.Overhang};
		private List<Visual> _partVis;

		public override string name => "Export Job";
		public override void BuildUI(StageBarUI ui)
		{
			ui.BeginUpdate();
			ui.AddAction("Generate Result", CreateBoolean);
			if (context.GetFeature<FinalOutput>() != null)
			{
				ui.AddAction("Export STL Files", ExportAndShow);
			}
			ui.AddAction("Save Project", context.SaveProject);
			ui.AddAction("Show Cross Section", () =>
			{
				if(_partVis.Count==0)
					DialogManager.Show<MessageBox>().WithMessage("Result not Generated!", "You must generate the final printable object before you can create a cross section of it", () => { });
				else
					DialogManager.Show<CrossSectionUI>(false).WithVisuals( _partVis.ToArray(), Array.Empty<Gizmo>() );
			});
			ui.AddEnum("Rendering", new[]{"Standard", "Transparent", "Overhang"}, _renderMode, mode => { _renderMode = mode; context.SelectFeature( context.GetFeature<ImportedPart>()); } );
			ui.EndUpdate();
		}

		public override void Activate()
		{
			context.ConfigurePrintBox(PrintBox.PrintBoxMode.Floor);

			Visual.Mode renderMode = _renderModes[_renderMode];
			bool showbool = true;
			List<CutPlaneFinal> cutplanes = context.GetFeatures<CutPlaneFinal>();
			_partVis = new List<Visual>();
			foreach (CutPlaneFinal cutplane in cutplanes)
			{
				cutplane.enabled = true;
				_partVis.Add(context.SetVisual( true, cutplane.mesh1, null, renderMode, AppColors.BOOLEAN_COLOR, context.GetSelectedFeature()==cutplane, Main.SelectionPriority.Primary, () => { context.SelectFeature(cutplane);}));
				_partVis.Add(context.SetVisual( true, cutplane.mesh2, null, renderMode, AppColors.BOOLEAN_COLOR, context.GetSelectedFeature()==cutplane, Main.SelectionPriority.Primary,() => { context.SelectFeature(cutplane);}));
				showbool = false;
			}

			FinalOutput boolout = context.SetEnabled<FinalOutput>(true);
			Visual boolean = context.SetVisual(showbool,boolout?.output, null, renderMode,AppColors.BOOLEAN_COLOR,false,Main.SelectionPriority.Primary);
			if(showbool)
				_partVis.Add(boolean);

			foreach (Runner runner in context.SetEnabledAll<Runner>(boolout==null))
				context.SetVisual(runner.enabled,runner.output, null, Visual.Mode.Opaque,AppColors.GATE_COLOR,false,Main.SelectionPriority.Primary);

			Mold mold = context.SetEnabled<Mold>(boolout==null);
			context.SetVisual(mold?.enabled??false,mold?.output, null, Visual.Mode.Transparent,AppColors.MOLD_COLOR,false,Main.SelectionPriority.Secondary);

			context.SetVisual(mold?.enabled??false,mold?.outSprueMesh,  mold?.outSprueTransform, Visual.Mode.Opaque,AppColors.SPRUE_COLOR,false,Main.SelectionPriority.Secondary);

			ImportedPart part = context.SetEnabled<ImportedPart>(boolout==null);
			context.SetVisual(part?.enabled??false,part?.output, part?.transform, Visual.Mode.Opaque,AppColors.PART_COLOR,false,Main.SelectionPriority.Primary);
			
			foreach (Guide guide in context.SetEnabledAll<Guide>(boolout==null))
				context.SetVisual(guide.enabled, guide.output, guide.position, Visual.Mode.Opaque,AppColors.GUIDE_COLOR,false,Main.SelectionPriority.Primary);

			context.ShowPropertiesOf( "Generated Output", new PropertyProxy { cutplanes = cutplanes}, null);
		}

		private struct PropertyProxy
		{
			public List<CutPlaneFinal> cutplanes;

			[ShowProperty]
			public bool showMoldOpen
			{
				get => cutplanes!=null && cutplanes.Count>0 && cutplanes[0].showMoldOpen;
				set
				{
					foreach(CutPlaneFinal cutplane in cutplanes) cutplane.showMoldOpen.Set(value);
				}
			}
		}

		public override void DeActivate()
		{
		}

		private void ExportAndShow()
		{
			string path = context.GetProject().path.RemoveLastPathElement();
			if (string.IsNullOrWhiteSpace(path))
				path = ".";
			path.AppendPath("output");
			Directory.CreateDirectory(path);
			DialogManager.Show<FileDialog>().SelectFolder( "Select output path", path, s =>
			{
				if (s == null || string.IsNullOrWhiteSpace(s))
					return;

				path = s;
				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);
			
				int idx = 1;
				foreach (CutPlaneFinal planeCut in context.GetFeatures<CutPlaneFinal>())
				{
					planeCut.mesh1.value.Export($"{context.GetProject().name}_Cut{idx}_A", path, Matrix4x4.identity);//planeCut.mesh1.value.transform);
					planeCut.mesh2.value.Export($"{context.GetProject().name}_Cut{idx}_B",path, Matrix4x4.identity);//planeCut.mesh2.value.transform);
					idx++;
				}

				if (idx == 1)
				{
					FinalOutput boolout = context.GetFeature<FinalOutput>();
					if (boolout != null)
					{
						boolout.output.value.Export($"{context.GetProject().name}_Mold",path,Matrix4x4.identity);
					}
				}

				Process.Start(path);
			});
		}
		
		private void CreateBoolean()
		{
			if (context == null)
				return;
				
			Mold mold = context.GetFeature<Mold>();
			SimpleBool sb = context.GetFeature<SimpleBool>();

			if (mold == null || sb == null)
				return;

			List<Runner> runners = context.GetFeatures<Runner>();
			List<DataRef<MeshBuilder>> runnermeshes = new ();
			foreach (Runner runner in runners)
			{
				DataRef<MeshBuilder> m = new DataRef<MeshBuilder>();
				m.UseDataFrom(runner.output);
				runnermeshes.Add( m );
			}

			List<Guide> guides = context.GetFeatures<Guide>();
			List<DataRef<MeshBuilder>> guidemeshes = new ();
			List<DataRef<XForm>> guidexforms = new ();
			foreach (Guide guide in guides)
			{
				DataRef<MeshBuilder> m = new DataRef<MeshBuilder>();
				m.UseDataFrom(guide.output);
				guidemeshes.Add( m );
				DataRef<XForm> x = new DataRef<XForm>();
				x.UseDataFrom(guide.position);
				guidexforms.Add(x);
			}

			// TODO: We're destroying the old feature because of how runners and guides are supplied as dumb "Lists" and not DataRefs. 
			FinalOutput blenderbool = context.GetFeature<FinalOutput>( "Final");
			if(blenderbool!=null)
				context.RemoveFeature(blenderbool);

			blenderbool = context.AddFeature<FinalOutput>( "Final");
			blenderbool.simplebool.UseDataFrom( sb.output);
			blenderbool.sprue.UseDataFrom(mold.outSprueMesh);
			blenderbool.sprueTransform.UseDataFrom(mold.outSprueTransform);
			blenderbool.runners = runnermeshes;
			blenderbool.guides = guidemeshes;
			blenderbool.guideTransforms = guidexforms;
			blenderbool.output.Set(null);

			// For each non-final cutplane, make sure there's a corresponding final cut plane
			foreach (CutPlaneInitial pl in context.GetFeatures<CutPlaneInitial>())
			{
				CutPlaneFinal finalplane = context.GetOrCreateFeature<CutPlaneFinal>($"Final {pl.name}");
				finalplane.input.UseDataFrom( blenderbool.output );
				finalplane.position.UseDataFrom( pl.position );
				finalplane.placementDistance.UseDataFrom(pl.placementDistance);
				finalplane.showCutplane.Set(false);
				finalplane.showMoldOpen.Set(true);
			}

			blenderbool.enabled = true;
		}
	}
}