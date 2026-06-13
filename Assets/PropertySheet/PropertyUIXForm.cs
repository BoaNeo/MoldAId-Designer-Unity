using System;
using Gizmos;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PropertySheet
{
	public class PropertyUIXForm : PropertyUI<XForm>
	{
		[SerializeField] private TMP_InputField _xInput;
		[SerializeField] private TMP_InputField _yInput;
//		[SerializeField] private TMP_InputField _zInput;
		[SerializeField] private TMP_Text _xUnits;
		[SerializeField] private TMP_Text _yUnits;
//		[SerializeField] private TMP_Text _zUnits;

		private XForm _value;

		public override Selectable firstSelectable => _xInput;
		public override Selectable lastSelectable => _yInput;
		
		protected override void Configure()
		{
			_xInput.contentType = TMP_InputField.ContentType.DecimalNumber;
			_yInput.contentType = TMP_InputField.ContentType.DecimalNumber;
//			_zInput.contentType = TMP_InputField.ContentType.DecimalNumber;
			_xUnits.text = info.attribute.unit;
			_yUnits.text = info.attribute.unit;
//			_zUnits.text = info.attribute.unit;
		}

		protected override void ShowValueInUI(XForm value)
		{
			_value = value;
			_xInput.text = value.position.x.ToString("F2");
			_yInput.text = value.position.y.ToString("F2");
		}

		protected override XForm GetValueFromUI()
		{
			try
			{
				XForm xform = _value;
				xform.position = new Vector3(float.Parse(_xInput.text), float.Parse(_yInput.text), _value.position.z);
				return xform;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return _value;
			}
		}
	}
}