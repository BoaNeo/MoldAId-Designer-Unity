using System;
using System.IO;
using Files;
using IO;
using TMPro;
using UnityEngine;

namespace Dialogs
{
	public class SaveProjectDialog : Dialog
	{
		[SerializeField] private TMP_InputField _projectName;
		[SerializeField] private TMP_InputField _projectPath;

		private Action<string> _onSave;

		public void WithProject(ProjectFile project, Action<string> doSave)
		{
			_onSave = doSave;

			string[] path = project.path.SplitPath();
			string folder = path.Length>1 ? path[path.Length-2] : "";
			string basepath = path.MergePath(0, path.Length - 2);

			if (string.IsNullOrWhiteSpace(basepath))
				basepath = PreferencesFile.current.projectPath.path;

			_projectName.text = folder;
			_projectPath.text = basepath;
		}

		public void OnBrowsePath()
		{
			DialogManager.Show<FileDialog>().SelectFolder("Select Folder for Project", _projectPath.text, path =>
			{
				_projectPath.text = path;
			});
		}

		public void OnCancel()
		{
			Hide();
		}

		public void OnSave()
		{
			if( string.IsNullOrEmpty(_projectName.text) )
				DialogManager.Show<MessageBox>().WithMessage("Invalid Name", "Please select a valid name for the project!", () => { });
			else if( string.IsNullOrEmpty(_projectPath.text) )
				DialogManager.Show<MessageBox>().WithMessage("Invalid Path", "Please select a valid path for the project!", () => { });
			else
			{
				_onSave(Path.Combine(_projectPath.text, _projectName.text));
				Hide();
			}
		}
	}
}