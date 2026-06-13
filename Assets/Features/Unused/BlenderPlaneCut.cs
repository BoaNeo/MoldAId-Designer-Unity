namespace Features.Unused
{
	/*
	public class BlenderPlaneCut : BlenderFeature
	{
		public DataRef<MeshBuilder> input { get; } = new DataRef<MeshBuilder>( null);
		public DataRef<XForm> plane { get; } = new DataRef<XForm>( XForm.identity);
		
		public DataRef<float> placementDistance { get; } = new DataRef<float>(1);

		public DataRef<bool> open { get; } = new DataRef<bool>(true); 

		[FeatureOutput]
		public DataRef<MeshBuilder> output1 { get; } = new DataRef<MeshBuilder>(null);
		[FeatureOutput]
		public DataRef<MeshBuilder> output2 { get; } = new DataRef<MeshBuilder>(null);
		
		public override Bounds bounds => new Bounds();

		public override bool wantsRebuild => !(input.value == null || input.changing || plane.changing);

		public override IEnumerator<FeatureManager.YieldUntil> Execute(Action<AsyncGPUReadbackRequest> GPUFinishedCallback)
		{
			MeshBuilder input = this.input;
			XForm plane = this.plane;
			float placementDistance = this.placementDistance;
			bool open = this.open;

			yield return FeatureManager.YieldUntil.RunningInBackground;

			string outputpath1 = path.AppendPath("output1.stl");
			if(File.Exists(outputpath1))
				File.Delete(outputpath1);
			
			string outputpath2 = path.AppendPath("output2.stl");
			if(File.Exists(outputpath2))
				File.Delete(outputpath2);

			string name = "Input";
			string inputpath = path.AppendPath($"{name}.stl");

			StringBuilder sb = new StringBuilder();

			sb.AppendLine("import bpy");
			sb.AppendLine("import bpy.ops");

			input.Export( name, inputpath, plane.worldToLocalMatrix);
			sb.AppendLine($"bpy.ops.import_mesh.stl(filepath=\"{inputpath}\")");

			sb.AppendLine("bpy.ops.object.duplicate(linked=False)");
			sb.AppendLine($"bpy.data.objects['{name}.001'].hide_set(True)");
				
			sb.AppendLine("bpy.ops.object.select_all(action='DESELECT')");

			sb.AppendLine($"bpy.data.objects['{name}'].select_set(True)");
			sb.AppendLine("bpy.ops.object.editmode_toggle()");
			sb.AppendLine("bpy.ops.mesh.bisect(plane_co=(0.0, 0.0, 0.0), plane_no=(0.0, 0.0, 1.0), use_fill=True, clear_inner=False, clear_outer=True)");
			sb.AppendLine("bpy.ops.object.editmode_toggle()");
			sb.AppendLine($"bpy.ops.export_mesh.stl(filepath=\"{outputpath1}\", use_selection=True)");
			sb.AppendLine("bpy.ops.object.delete(use_global=False)");

			sb.AppendLine($"bpy.data.objects['{name}.001'].hide_set(False)");
			sb.AppendLine("bpy.ops.object.select_all(action='DESELECT')");
			sb.AppendLine($"bpy.data.objects['{name}.001'].select_set(True)");
			sb.AppendLine("bpy.ops.object.editmode_toggle()");
			sb.AppendLine("bpy.ops.mesh.bisect(plane_co=(0.0, 0.0, 0.0), plane_no=(0.0, 0.0, 1.0), use_fill=True, clear_inner=True, clear_outer=False)");
			sb.AppendLine("bpy.ops.object.editmode_toggle()");
			sb.AppendLine($"bpy.ops.export_mesh.stl(filepath=\"{outputpath2}\", use_selection=True)");

			RunBlender(sb);

			MeshBuilder out1 = new MeshBuilder().Import(outputpath1, plane.localToWorldMatrix);
			CheckBlenderOutput(output1, outputpath1);
			MeshBuilder out2 = new MeshBuilder().Import(outputpath2, plane.localToWorldMatrix);
			CheckBlenderOutput(output2, outputpath2);

			if (open)
			{
				Bounds inputBounds = out1.CalculateBoundingBox();
				Vector3 offset = new Vector3(2*inputBounds.extents.x+placementDistance, -2*out2.CalculateBoundingBox().extents.y,0);
				Quaternion q = Quaternion.Euler(180, 0, 0);
				Matrix4x4 m = Matrix4x4.Rotate(q)*Matrix4x4.Translate(offset);
				out2.transform = m * out2.transform;
			}

			FinishBlender();

			yield return FeatureManager.YieldUntil.RunningInMainThread;

			output1.Set( out1);
			output2.Set( out2);
		}
	}
	*/
}
