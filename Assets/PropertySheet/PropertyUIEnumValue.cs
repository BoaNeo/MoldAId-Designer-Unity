using System;
using TMPro;
using UIComponents;
using UnityEngine;

namespace PropertySheet
{
	public class PropertyUIEnumValue : GridCell
	{
		[SerializeField] private TMP_Text _label;
		[SerializeField] private GameObject _check;

		private int _value;
		private Action<int> _onSelect;

		public bool selected
		{
			get => _check.gameObject.activeSelf;
			set => _check.gameObject.SetActive(value);
		}

		public void Setup(int i, string lbl, Action<int> onSelect)
		{
			_label.text = lbl;
			_value = i;
			_onSelect = onSelect;
		}

		public void OnSelect()
		{
			_onSelect(_value);
		}
	}
}