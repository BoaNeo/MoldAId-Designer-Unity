using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PropertySheet
{
	public class PropertyUIInt : PropertyUI<int>
	{
		[SerializeField] private TMP_InputField _input;

		public override Selectable firstSelectable => _input;
		public override Selectable lastSelectable => _input;

		protected override void Configure()
		{
			_input.contentType = TMP_InputField.ContentType.DecimalNumber;
		}

		protected override void ShowValueInUI(int value)
		{
			_input.text = value.ToString();
		}

		protected override int GetValueFromUI()
		{
			return (int)Clamp(int.Parse(_input.text));
		}

		public void Add(int i)
		{
			_input.text = ((int)Clamp(int.Parse(_input.text) + i)).ToString();
			OnChange();
		}
	}
}