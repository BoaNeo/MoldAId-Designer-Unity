using System;
using TMPro;
using UIComponents;
using UnityEngine;

namespace StageBar
{
	public class StageBarUIAction : GridCell
	{
		[SerializeField] private TMP_Text _label;
		private Action _onClick;

		public void Setup(string label, Action onClick)
		{
			_label.text = label;
			_onClick = onClick;
		}

		public void OnClick()
		{
			_onClick();
		}
	}
}