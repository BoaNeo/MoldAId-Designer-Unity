using System;
using Files;
using TMPro;
using UIComponents;
using UnityEngine;

namespace Dialogs
{
	public class PrinterListItem : GridCell
	{
		[SerializeField] private GameObject _selection;
		[SerializeField] private TMP_Text _name;
		[SerializeField] private TMP_Text _description;

		private Action _onTap;

		public void Setup(PrinterFile printerFile, bool b, Action action)
		{
			_selection.SetActive(b);
			_name.text = printerFile.name;
			_description.text = $"{(int)printerFile.volume.x}x{(int)printerFile.volume.y}x{(int)printerFile.volume.z}mm";
			_onTap = action;
		}

		public void OnTap()
		{
			_onTap();
		}
	}
}
