using Dialogs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PropertySheet
{
	public class PropertyUIPath: PropertyUI<PathProperty>
	{
		[SerializeField] private TMP_InputField _text;
		[SerializeField] private Button _button;

		private PathProperty _path;

		public override Selectable firstSelectable => _button;
		public override Selectable lastSelectable => _button;

		protected override void Configure()
		{
		}

		protected override void ShowValueInUI(PathProperty value)
		{
			_path = value;
			_text.text = value.path;
		}

		protected override PathProperty GetValueFromUI()
		{
			_path.path = _text.text;
			return _path;
		}

		public void OnBrowse()
		{
			if (info.attribute.filter == FileDialog.FileFilter.FOLDER)
			{
				DialogManager.Show<FileDialog>().SelectFolder( "Select Folder", _path.path, newfile =>
				{
					_text.text = newfile;
					_path.path = newfile;
					OnChange();
				});
			}
			else
			{
				DialogManager.Show<FileDialog>().SelectFileToOpen( "Select File", _path.path, info.attribute.filter, newfile =>
				{
					_text.text = newfile;
					_path.path = newfile;
					OnChange();
				});
			}
		}
	}
}