using System;
using System.Collections.Generic;
using System.IO;
using Files;
using IO;
using UnityEngine;

namespace Dialogs
{
	public struct RecentFile
	{
		public string name;
		public string path;
		
		public string lastAccessedDate;
		public int sortOrder;

		private RecentFile(string name, string path)
		{
			this.name = name;
			this.path = path;
			DateTime time = File.GetLastAccessTime(path);
			sortOrder = (int) (time.ToFileTime()/1000000000);
			lastAccessedDate = time.ToLongDateString();
		}

		private static char LISTSEPARATOR => '\t';
		private static string LISTFILENAME => Application.persistentDataPath.AppendPath("recentprojects.txt");

		public static Dictionary<string, RecentFile> LoadRecentFileList()
		{
			Dictionary<string, RecentFile> recent = new Dictionary<string, RecentFile>();
			try
			{
				string[] rows = File.ReadAllLines( LISTFILENAME );
				for (int i = 0; i < rows.Length; i++)
				{
					string[] cols = rows[i].Split(LISTSEPARATOR);
					string name = cols[0];
					string path = cols[1];
					if (File.Exists(path))
					{
						recent[path] = new RecentFile( name, path);
					}
				}
			}
			catch (Exception e)
			{
				Debug.LogWarning($"Failed to load list of recent files from {LISTFILENAME}: {e}");
			}
			return recent;
		}

		public static void SaveRecentFileList(Dictionary<string,RecentFile> files)
		{
			string[] rows = new string[files.Count];
			int i = 0;
			foreach (RecentFile file in files.Values)
				rows[i++] = $"{file.name}{LISTSEPARATOR}{file.path}{LISTSEPARATOR}";
			File.WriteAllLines(LISTFILENAME, rows);
		}

		public static void AddRecentFile(ProjectFile project)
		{
			Dictionary<string,RecentFile> recent = LoadRecentFileList();
			recent[project.name] = new RecentFile( project.name, project.path);
			SaveRecentFileList(recent);
		}
	}
}