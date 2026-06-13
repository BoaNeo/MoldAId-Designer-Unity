using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PropertySheet
{
	public class PropertyUIString : PropertyUI<string>
	{
		[SerializeField] private TMP_InputField _input;

		public override Selectable firstSelectable => _input;
		public override Selectable lastSelectable => _input;

		protected override void Configure()
		{
			_input.contentType = TMP_InputField.ContentType.Standard;
		}

		protected override void ShowValueInUI(string value)
		{
			_input.text = value;
		}

		protected override string GetValueFromUI()
		{
			return _input.text;
		}
	}
}