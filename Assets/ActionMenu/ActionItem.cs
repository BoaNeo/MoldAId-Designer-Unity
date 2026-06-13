using System;
using TMPro;
using UIComponents;
using UnityEngine;

namespace ActionMenu
{
	public class ActionItem : GridCell
	{
		[SerializeField] private TMP_Text _label;
		private Action _onAction;
		
		public void Setup(string label, Action action)
		{
			_label.text = label;
			_onAction = action;
			width = _label.preferredWidth + 10;
		}

		public void OnAction()
		{
			_onAction();
		}
	}
}