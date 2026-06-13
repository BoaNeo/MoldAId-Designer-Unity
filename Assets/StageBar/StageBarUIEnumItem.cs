using System;
using TMPro;
using UIComponents;
using UnityEngine;

namespace StageBar
{
	public class StageBarUIEnumItem : GridCell
	{
		[SerializeField] private TMP_Text _label;
		[SerializeField] private GameObject _selection;

		private Action _onSelect;

		public void Setup(string label, bool selected, Action onSelect)
		{
			_selection.SetActive(selected);
			_label.text = label;
			_onSelect = onSelect;
		}

		public void OnSelect()
		{
			_onSelect();
		}
	}
}