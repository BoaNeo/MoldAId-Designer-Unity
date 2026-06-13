using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PropertySheet
{
	public class PropertyUIFloat : PropertyUI<float>
	{
		[SerializeField] private TMP_InputField _input;
		[SerializeField] private TMP_Text _units;

		public override Selectable firstSelectable => _input;
		public override Selectable lastSelectable => _input;

		protected override void Configure()
		{
			_input.contentType = TMP_InputField.ContentType.DecimalNumber;
			_units.text = info.attribute.unit;
		}

		protected override void ShowValueInUI(float value)
		{
			_input.text = value.ToString("F2");
		}

		protected override float GetValueFromUI()
		{
			return Clamp(float.Parse(_input.text));
		}
	}
}