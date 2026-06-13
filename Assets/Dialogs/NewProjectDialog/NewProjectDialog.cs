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
		[SerializeField] private TMP_InputField _layerThickness;

		private Action<ProjectFile> _onDone;
		private int _selectedPrinter;
		private float _selectedLayer;
		private bool _ignoreInput;

		public void Setup(Action<ProjectFile> action)
		{
			_onDone = action;
			_selectedPrinter = 0;
			_selectedLayer = Library<PrinterFile>.Load()[_selectedPrinter].defaultLayerThickness;
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
					_selectedLayer = lib[row].defaultLayerThickness;
					Refresh();
				});
			});

			_layerThickness.text = (Mathf.RoundToInt(_selectedLayer * 1000)).ToString();
			
			_ignoreInput = false;
		}

		public void OnCancel()
		{
			_onDone(null);
			Hide();
		}

		public void OnLayerThicknessChanged()
		{
			if (_ignoreInput)
				return;

			PrinterFile printer = Library<PrinterFile>.Load()[_selectedPrinter];
			float.TryParse(_layerThickness.text, out _selectedLayer);
			_selectedLayer = Mathf.Clamp(	_selectedLayer/1000.0f, printer.minLayerThickness, printer.maxLayerThickness);
			Refresh();
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
			project.layerThickness = _selectedLayer;

			_onDone(project);
			Hide();
		}
	}
}