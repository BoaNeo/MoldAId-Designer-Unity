using System;
using TMPro;
using UIComponents;
using UnityEngine;

namespace StageBar
{
	public class StageBarUIListItem : GridCell
	{
		[SerializeField] private TMP_Text _label;
		[SerializeField] private GameObject _selection;

		public Action onSelect { get; set; }

		public void Setup(string label, bool selected)
		{
			_selection.SetActive(selected);
			_label.text = label;
		}

		public void OnSelect()
		{
			onSelect();
		}
	}
}