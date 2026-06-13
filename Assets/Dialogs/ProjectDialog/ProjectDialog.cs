using System;
using System.Collections.Generic;
using Files;
using IO;
using UIComponents;
using UnityEngine;

namespace Dialogs
{
	public class ProjectDialog : Dialog
	{
		[SerializeField] private GridOnDemand _grid;
		[SerializeField] private ProjectListItem _itemPrefab;
		[SerializeField] private GameObject _openButton;

		private int _selectedRow;
		private Action<ProjectFile> _onSelect;
		private List<RecentFile> _recentSorted;
		private float _selectTime;

		public void Setup(Action<ProjectFile> onSelect)
		{
			_onSelect = onSelect;

			Dictionary<string,RecentFile> recent = RecentFile.LoadRecentFileList();
			_recentSorted = new List<RecentFile>();
			foreach (RecentFile file in recent.Values)
				_recentSorted.Add(file);
			_recentSorted.Sort( (f1, f2)=> f2.sortOrder-f1.sortOrder );
			_selectedRow = 0;
			_openButton.SetActive(recent.Count>0);
			Refresh();
		}
		
		private void Refresh()
		{
			_grid.Setup(_itemPrefab, 1, _recentSorted.Count, (col, row, cell) =>
			{
				cell.Setup(_recentSorted[row], row==_selectedRow, () =>
				{
					float t = Time.time;
					if (_selectedRow == row && (t-_selectTime)<0.5f)
						OnOpenProject();
					_selectedRow = row;
					_selectTime = t;
					Refresh();
				});
			});
		}

		private void LoadAndReturnProject(string path)
		{
			ProjectFile project = StreamableFile.Load<ProjectFile>(path);
			if(project!=null)
				RecentFile.AddRecentFile(project);
			Hide();
			_onSelect(project );
		}

		public void OnClose()
		{
			Hide();
			_onSelect(null);
		}

		public void OnOpenProject()
		{
			LoadAndReturnProject(_recentSorted[_selectedRow].path);
		}

		public void OnNewProject()
		{
			DialogManager.Show<NewProjectDialog>().Setup(project =>
			{
				if (project != null)
				{
					Hide();
					_onSelect(project);
				}
			});
		}

		public void OnBrowseProjects()
		{
			DialogManager.Show<FileDialog>().SelectFileToOpen("Open Project", "", FileDialog.FileFilter.AFP, s =>
			{
				if (!string.IsNullOrWhiteSpace(s))
				{
					LoadAndReturnProject(s);
				}
			});
		}
	}
}
