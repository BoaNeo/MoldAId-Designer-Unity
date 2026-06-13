using System;
using System.IO;
using Dialogs;
using IO;
using PropertySheet;
using UnityEngine;

namespace Files
{
	public class PreferencesFile : StreamableFile
	{
		private static string PREFERENCES_FILE => Application.persistentDataPath.AppendPath("Preferences.json");

		[ShowProperty(filter = FileDialog.FileFilter.APP)] 
		public PathProperty blenderPath;

		[ShowProperty(filter = FileDialog.FileFilter.FOLDER)]
		public PathProperty projectPath;

		[ShowProperty(min =0.01f, max = 100.0f, unit = "x")]
		public float mouseWheelSensitivity = 1.0f;
		
		[ShowProperty(min = 250, max = 10000, unit = "ms")]
		public int slowOperationTime = 750;

		[ShowProperty(values = new[] {"Box", "Floor", "Wireframe"})]
		public int printBoxMode;

		[ShowProperty(values = new[] {"Light", "Dark"})]
		public int skin;

		public float arcLength = 0.5f;

		public event Action OnPreferencesApplied;

		public static PreferencesFile current { get; set; }

		public string blenderPathFixed => Application.platform == RuntimePlatform.WindowsPlayer ? blenderPath.path : $"{blenderPath.path}/Contents/MacOS/blender";

		[ShowProperty]
		public void ApplyAndSave()
		{
			OnPreferencesApplied?.Invoke();
			Save(PREFERENCES_FILE);
		}

		public static void Load()
		{
			current = Load<PreferencesFile>(PREFERENCES_FILE) ?? new PreferencesFile();
		}

		public override void Serialize(DataStream data)
		{
			data.Serialize("projectPath", ref projectPath);
			data.Serialize("blenderPath", ref blenderPath);
			data.Serialize("skin", ref skin);
			data.Serialize("arcLength", ref arcLength);
			data.Serialize("slowOperationToime", ref slowOperationTime);
			data.Serialize("printBoxMode", ref printBoxMode);
			data.Serialize("mouseWheelSensitivity", ref mouseWheelSensitivity);
		}

		public PreferencesFile()
		{
			if (Application.platform == RuntimePlatform.WindowsPlayer)
			{
				projectPath.path = "%Homepath%/Documents/Mold Generator Output";
				blenderPath.path = "C:/Program Files/Blender Foundation/Blender 3.0/blender.exe";
			}
			else
			{
				projectPath.path = "~/Documents/Mold Generator Output";
				blenderPath.path = "/Applications/Blender/Blender.app";
			}
			
			mouseWheelSensitivity = 1.0f;
		}

		public string Validate()
		{
			if (!File.Exists(blenderPathFixed))
				return $"No Blender installation found at {blenderPath.path}";
			if (mouseWheelSensitivity < 0.01)
				mouseWheelSensitivity = 1.0f;
			return null;
		}
	}
}