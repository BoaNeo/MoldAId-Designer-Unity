using System;
using TMPro;
using UIComponents;
using UnityEngine;
using UnityEngine.UI;

namespace Dialogs
{
	public class ProjectListItem : GridCell
	{
		[SerializeField] private TMP_Text _name;
		[SerializeField] private TMP_Text _path;
		[SerializeField] private TMP_Text _date;
		[SerializeField] private RawImage _icon;
		[SerializeField] private GameObject _selection;

		private Action _onSelect;

		public void Setup(RecentFile file, bool selected, Action onSelect)
		{
			_name.text = file.name;
			_path.text = file.path;
			_date.text = file.lastAccessedDate;
			_selection.SetActive(selected);
			_onSelect = onSelect;
		}

		public void OnSelect()
		{
			_onSelect();
		}
	}
}