using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PropertySheet
{
	public class PropertyUIVector3 : PropertyUI<Vector3>
	{
		[SerializeField] private TMP_InputField _xInput;
		[SerializeField] private TMP_InputField _yInput;
		[SerializeField] private TMP_InputField _zInput;
		[SerializeField] private TMP_Text _xUnits;
		[SerializeField] private TMP_Text _yUnits;
		[SerializeField] private TMP_Text _zUnits;

		public override Selectable firstSelectable => _xInput;
		public override Selectable lastSelectable => _zInput;
		
		protected override void Configure()
		{
			_xInput.contentType = TMP_InputField.ContentType.DecimalNumber;
			_yInput.contentType = TMP_InputField.ContentType.DecimalNumber;
			_zInput.contentType = TMP_InputField.ContentType.DecimalNumber;
			_xUnits.text = info.attribute.unit;
			_yUnits.text = info.attribute.unit;
			_zUnits.text = info.attribute.unit;
		}

		protected override void ShowValueInUI(Vector3 value)
		{
			_xInput.text = value.x.ToString("F2");
			_yInput.text = value.y.ToString("F2");
			_zInput.text = value.z.ToString("F2");
		}

		protected override Vector3 GetValueFromUI()
		{
			try
			{
				return new Vector3(float.Parse(_xInput.text), float.Parse(_yInput.text), float.Parse(_zInput.text));
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return Vector3.zero;
			}
		}
	}
}