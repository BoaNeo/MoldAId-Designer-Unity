using Dialogs;
using FeatureGraph;
using Features;
using Files;
using Gizmos;
using IO;
using StageBar;
using Undo;
using UnityEngine;
using Visuals;

namespace Stages
{
	public class MoldStage : Stage
	{
		private MoldFile _selectedMold;
		private StageBarUIList _list;

		public override string name => "Mold Config";

		public override void BuildUI(StageBarUI ui)
		{
			ui.BeginUpdate();
			Library<MoldFile> lib = Library<MoldFile>.Load();

			_list = ui.AddList("Library", lib.Count, (idx, cell) => cell.Setup(lib[idx].key, lib[idx].key == _selectedMold?.key), (idx, doubleclick) =>
			{
				_selectedMold = lib[idx];
				if (doubleclick) 
					SelectMold(_selectedMold);
			});

			ui.AddAction("Use Selected", ()=>SelectMold(_selectedMold));
			ui.AddAction("Remove From Library", ()=>
			{
				RemoveFromLibrary();
				BuildUI(ui);
			});
			
			ui.AddSpacing();
			ui.AddAction("New Box Mold", ()=>SelectMold(new MoldFile {type = MoldFile.MoldType.Box} ));
			ui.AddAction("New Cylinder Mold", ()=>SelectMold(new MoldFile {type = MoldFile.MoldType.Cylinder} ));
			ui.AddSpacing();
			ui.AddAction("Center Part in Mold", OnCenterPartInMold );
			ui.AddAction("Align Plane to Base", OnAlignPlane );
//			ui.AddAction("New Custom", ()=>SelectMold(new MoldFile {type = MoldFile.MoldType.Stl} ));
			ui.EndUpdate();
		}

		private void OnCenterPartInMold()
		{
			Mold mold = context.GetFeature<Mold>();
			if (mold != null)
			{
				ImportedPart part = context.GetFeature<ImportedPart>();
				part.transform.Set(part.transform.value.WithPosition( mold.bounds.center ));
			}
		}

		private void OnAlignPlane()
		{
			string[] tags = {context.GetFeature<ImportedPart>().output.blockname};
			context.PickFromScene( "Select a point on the plane you wish to align to the print base", tags, hit =>
			{
				if (hit.collider == null)
					return false;

				Vector3 n1 = hit.normal;
				context.PickFromScene( "Select second point on the plane you wish to align to the print base", tags, hit =>
				{
					if (hit.collider == null)
						return false;

					Vector3 n2 = hit.normal;
					context.PickFromScene( "Select the final point on the plane you wish to align to the print base", tags, hit =>
					{
						if (hit.collider == null)
							return false;

						Vector3 n3 = hit.normal;

						Vector3 n = (n1 + n2 + n3) / 3.0f;
						ImportedPart part = context.GetFeature<ImportedPart>();
						part.transform.Set(part.transform.value.WithRotation( Quaternion.FromToRotation(n,Vector3.back)*part.transform.value.rotation ));
						return true;
					});
					return true;
				});
				return true;
			});
		}

		public override void Activate()
		{
			Mold mold = context.SetEnabled<Mold>(true);
			context.SetVisual(true,mold?.output, null, Visual.Mode.Transparent,AppColors.MOLD_COLOR, mold == context.GetSelectedFeature(), Main.SelectionPriority.Secondary, () => { context.SelectFeature(mold); });

			context.SetVisual(true, mold?.outSprueMesh, mold?.outSprueTransform, Visual.Mode.Opaque,AppColors.SPRUE_COLOR, mold == context.GetSelectedFeature(), Main.SelectionPriority.Primary, () => { context.SelectFeature(mold); });
			
			ImportedPart part = context.SetEnabled<ImportedPart>(true);
			Visual v = context.SetVisual(true,part?.output, part?.transform, Visual.Mode.Opaque,AppColors.PART_COLOR, part == context.GetSelectedFeature(), Main.SelectionPriority.Primary,() => { context.SelectFeature(part); });
			context.SetGizmo( part==context.GetSelectedFeature(), part?.transform, GizmoHandleFlags.All, GizmoSpace.World);

			foreach (CutPlaneInitial cutplane in context.SetEnabledAll<CutPlaneInitial>(false))
				context.SetVisual(true,cutplane.plane, cutplane.position, Visual.Mode.Transparent,AppColors.CUTPART_COLOR);

			foreach (Runner runner in context.SetEnabledAll<Runner>(true))
				context.SetVisual(true,runner.output, null, Visual.Mode.Opaque,runner is RunnerOut ? AppColors.VENT_COLOR : AppColors.GATE_COLOR);

			context.SelectOneOf(new Feature[]{ mold,part }, new[]{ false,false });
		}

		public override void DeActivate()
		{
		}
		
		private void SelectMold(MoldFile moldspec)
		{
			if (context == null)
				return;
			
			moldspec = moldspec.Copy();
			if(string.IsNullOrWhiteSpace(moldspec.key))
				moldspec.key = "Mold";
			
			UndoManager.Append(() =>
			{
				Mold oldmold = context.GetFeature<Mold>();
				context.RemoveFeature( oldmold );
				Mold f;
				switch (moldspec.type)
				{
					case MoldFile.MoldType.Box:
						f = context.AddFeature<MoldBox>("Mold");
						break;
					case MoldFile.MoldType.Cylinder:
						f = context.AddFeature<MoldCylinder>("Mold");
						break;
					case MoldFile.MoldType.Stl:
						f = context.AddFeature<MoldSTL>("Mold");
						break;
					default:
						Debug.LogWarning($"Unknown Mold Type: {moldspec.type}");
						return oldmold;
				}
				f.Configure(moldspec);
				context.SelectFeature(f);
			
				SimpleBool sb = context.GetOrCreateFeature<SimpleBool>("SimpleBool");
				sb.outer.UseDataFrom(f.output);

				ImportedPart part = context.GetFeature<ImportedPart>();
				sb.inner.UseDataFrom(part.output);
				sb.innerXform.UseDataFrom(part.transform);
				
				Activate();
				
				return oldmold;
			}, oldmold =>
			{
				context.RemoveFeature( context.GetFeature<Mold>() );
				if(oldmold!=null)
					context.AddFeature(oldmold.name,oldmold);
			});
		}

		private void RemoveFromLibrary()
		{
			if (_selectedMold == null)
				return;
			DialogManager.Show<MessageBox>().WithQuery("Remove Mold?", $"Are you sure you want to remove \"{_selectedMold.key}\" from the library?", yes =>
			{
				if (yes)
				{
					Library<MoldFile> lib = Library<MoldFile>.Load();
					lib.Remove(_selectedMold.key);
					lib.SaveLibrary();
					_list.UpdateRowCount(lib.Count);
				}
			});
		}
	}
}