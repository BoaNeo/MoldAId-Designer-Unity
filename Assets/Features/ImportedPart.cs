using System.Collections.Generic;
using System.IO;
using FeatureGraph;
using Gizmos;
using IO;
using MeshUtil;
using MeshUtil.Extensions;
using PropertySheet;
using UnityEngine;

namespace Features
{
	public class ImportedPart : Feature
	{
		[ShowProperty(unit = "%", min = 90, max = 110)]  
		[FeatureInput] public DataRef<float> scale { get; } = new( 100 );

		[ShowProperty(unit="mm")] 
		public Vector3 position { get => transform.value.position; set => transform.Set( transform.value.WithPosition(value) ); }

		[ShowProperty(unit="deg")] 
		public Vector3 rotation { get => transform.value.rotation.eulerAngles; set => transform.Set(transform.value.WithRotation(Quaternion.Euler(value.x, value.y, value.z))); }

		[ShowProperty(name = "Auto Center Part Origin")]
		[FeatureInput] public DataRef<bool> autoCenter { get; } = new( true );
		
		[FeatureInput] public DataRef<string> localPath { get; } = new( "");

		[FeatureInput] public DataRef<string> originalPath { get; } = new( "");

		[FeatureOutput] public DataRef<MeshBuilder> output { get; } = new(null);

		public DataRef<XForm> transform { get; } = new( XForm.identity);

		public string Reimport()
		{
			if (originalPath.Equals(localPath))
			{
				localPath.Set(localPath);
				return null;
			}
			if (File.Exists(originalPath))
			{
				byte[] imported = File.ReadAllBytes(originalPath);
				if (imported != null && imported.Length > 0)
				{
					File.Delete(localPath);
					File.WriteAllBytes( localPath, imported);
					localPath.Set(localPath);
					return null;
				}
				return $"Original file at {originalPath.value} is empty!";
			}
			return $"Original file at {originalPath.value} does not exist!";
		}
		
		public void RefreshLocalPath(string projectPath)
		{
			if (string.IsNullOrEmpty(projectPath))
				localPath.Set(originalPath);
			else
				localPath.Set(PathHelper.AppendPath(projectPath,"Part.stl"));
			if(!File.Exists(localPath))
				Reimport();
		}

		public static IEnumerator<IYield> Build(bool changing,
			float scale,
			bool autoCenter,
			string localPath,
			string originalPath,
			DataRef<MeshBuilder> output)
		{
			if (changing) 
				yield break;

			yield return Until.RunningInBackground;

			MeshBuilder temp = new MeshBuilder();

			temp.Import(localPath);

			Vector3 origin = Vector3.zero;
			if (autoCenter)
				origin = temp.GetBounds().center;

			yield return Until.RunningOnMainThread;

			Matrix4x4 m = Matrix4x4.Scale((scale / 100.0f) * Vector3.one) * Matrix4x4.Translate(-origin);
			temp = temp.TransformGPU(m);

			output.Set(temp);
		}
	}
}