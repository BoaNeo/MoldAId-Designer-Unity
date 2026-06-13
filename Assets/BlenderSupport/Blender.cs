using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using Files;
using IO;
using MeshUtil;
using MeshUtil.Extensions;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace BlenderSupport
{
	public class Blender
	{
		public enum Operation { BASE, DIFFERENCE};
		public class ExportFile
		{
			public MeshBuilder mesh;
			public Matrix4x4 transform = Matrix4x4.identity;
			public string name;
			public Operation op;
		}

		private readonly string _path;

		public Blender()
		{
			_path = Application.persistentDataPath.AppendPath("Temp");
			if (!Directory.Exists(_path))
				Directory.CreateDirectory(_path);
		}
		
		public MeshBuilder Boolean(List<ExportFile> objects)
		{
			if (objects == null || objects.Count == 0)
				return null;

			for (int i = 0; i < objects.Count; i++)
				objects[i].name = FirstCharToUpper(objects[i].name);

			string outputpath = _path.AppendPath("output.stl");
			if(File.Exists(outputpath))
				File.Delete(outputpath);

			StringBuilder sb = new StringBuilder();

			sb.AppendLine("import bpy");
			sb.AppendLine("import bpy.ops");

			for (int i = 0; i < objects.Count; i++)
			{
				string stlpath = objects[i].mesh.Export( objects[i].name, _path, objects[i].transform);
				sb.AppendLine($"bpy.ops.import_mesh.stl(filepath=\"{stlpath}\")");
			}

			sb.AppendLine("objects=bpy.data.objects");
			sb.AppendLine($"base=objects['{objects[0].name}']");

			for (int i = 1; i < objects.Count; i++)
			{
				sb.AppendLine($"bool_{i} = base.modifiers.new(type='BOOLEAN', name='bool {i}')");
				sb.AppendLine($"bool_{i}.object = objects['{objects[i].name}']");
				sb.AppendLine($"bool_{i}.solver='EXACT'");
//					sb.AppendLine($"bool_{i}.use_self=True");
//					sb.AppendLine($"bool_{i}.use_hole_tolerant=True");
				sb.AppendLine($"bool_{i}.operation='{objects[i].op}'");
			}

			sb.AppendLine("bpy.ops.object.select_all(action='DESELECT')");
			sb.AppendLine($"objects['{objects[0].name}'].select_set(True)");
			sb.AppendLine($"bpy.ops.export_mesh.stl(filepath=\"{outputpath}\", use_selection=True)");

			RunBlender(_path,sb);

			MeshBuilder out1 = new MeshBuilder().Import(outputpath);
			CheckBlenderOutput(out1,outputpath);
				
			FinishBlender(_path);

			return out1;
		}

		private string FirstCharToUpper(string input)
		{
			return string.Concat(input[0].ToString().ToUpper(), input.Substring(1));
		}
		
		private void RunBlender(string path, StringBuilder script)
		{
			string scriptPath = path.AppendPath("script.py");
			File.WriteAllText(scriptPath,script.ToString());

			Debug.Log($"Exported to {path}");

			ProcessStartInfo startInfo = new ProcessStartInfo();
			startInfo.FileName = PreferencesFile.current.blenderPathFixed;
			startInfo.Arguments = $"--background --disable-autoexec --python \"{scriptPath}\"";
			startInfo.UseShellExecute = false;
			startInfo.RedirectStandardError = true;
			startInfo.RedirectStandardOutput = true;
			startInfo.CreateNoWindow = true;
			startInfo.UserName = null;
			startInfo.Password = null;
			startInfo.WindowStyle = ProcessWindowStyle.Hidden;
				
			Debug.Log($"Launching [{startInfo.FileName}]");
			Process p = Process.Start(startInfo);
			while (!p.HasExited)
				Thread.Sleep(1);
			p.Close();
		}

		private void CheckBlenderOutput(MeshBuilder output, string path)
		{
			if (output == null)
				throw new Exception($"Output File 1 is missing at {path}");
			Debug.Log($"Imported from {path}");
		}

		private void FinishBlender(string path)
		{
			if (!Application.isEditor)
			{
				string scriptPath = path.AppendPath("script.py");
				File.Delete(scriptPath);
			}
		}
		
	}
}