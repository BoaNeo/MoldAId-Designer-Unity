using System;
using System.IO;
using Files;
using IO;
using TMPro;
using UIComponents;
using UnityEngine;

namespace Dialogs
{
	public class NewProjectDialog : Dialog
	{
		[SerializeField] private TMP_InputField _partPath;
		[SerializeField] private GridOnDemand _printerGrid;
		[SerializeField] private PrinterListItem _printerItemPrefab;

		private Action<ProjectFile> _onDone;
		private int _selectedPrinter;
		private bool _ignoreInput;

		public void Setup(Action<ProjectFile> action)
		{
			_onDone = action;
			_selectedPrinter = 0;
			Refresh();
		}

		public void OnBrowsePart()
		{
			DialogManager.Show<FileDialog>().SelectFileToOpen($"Import Part", "", FileDialog.FileFilter.STL, file =>
			{
				if (file != null)
				{
					_partPath.text = file;
					Debug.Log($"Selected file {file}");
				}
			});
		}

		private void Refresh()
		{
			_ignoreInput = true;
			
			Library<PrinterFile> lib = Library<PrinterFile>.Load();
			_printerGrid.Setup(_printerItemPrefab, 1, lib.Count, (col, row, cell) =>
			{
				cell.Setup(lib[row], row == _selectedPrinter, () =>
				{
					_selectedPrinter = row;
					Refresh();
				});
			});
			
			_ignoreInput = false;
		}

		public void OnCancel()
		{
			_onDone(null);
			Hide();
		}

		public void OnCreate()
		{
			string error = null;
			if (!File.Exists(_partPath.text))
				error = "Please select a part file";

			if (error != null)
			{
				DialogManager.Show<MessageBox>().WithMessage("Invalid Project", error, () => { });
				return;
			}
			
			ProjectFile project = new ProjectFile();
			PrinterFile printer = Library<PrinterFile>.Load()[_selectedPrinter];

			project.partPath = _partPath.text;
			project.printerId = printer.name;

			_onDone(project);
			Hide();
		}
	}
}