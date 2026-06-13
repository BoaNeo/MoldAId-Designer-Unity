using System.Collections.Generic;
using UIComponents;
using UnityEngine;
using UnityEngine.UI;

namespace PropertySheet
{
	public class PropertyUIEnum : PropertyUI<int>
	{
		[SerializeField] private PropertyUIEnumValue _prefabCheckbox;
		[SerializeField] private GridBuilder _grid;

		public override Selectable firstSelectable => null;
		public override Selectable lastSelectable => null;

		private List<PropertyUIEnumValue> _buttons;
		private int _selected;

		protected override void Configure()
		{
			_buttons = new List<PropertyUIEnumValue>();
			_grid.BeginUpdate();
			for (int i = 0; i < info.attribute.values.Length; i++)
			{
				_buttons.Add(_grid.AddRow(_prefabCheckbox, ui => ui.Setup(i, info.attribute.values[i], idx =>
				{
					if (idx != _selected)
					{
						_selected = idx;
						OnChange();
						ShowValueInUI(idx);
					}
				})));
			}
			_grid.EndUpdate();
			height = _label.rectTransform.rect.height + _grid.layoutHeight;
		}

		protected override void ShowValueInUI(int value)
		{
			_selected = value;
			for (int i = 0; i < _buttons.Count; i++)
				_buttons[i].selected = i == value;
		}

		protected override int GetValueFromUI()
		{
			return _selected;
		}
	}
}