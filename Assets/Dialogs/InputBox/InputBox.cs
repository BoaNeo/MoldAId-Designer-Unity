using System;
using TMPro;
using UnityEngine;

namespace Dialogs.InputBox
{
	public class InputBox: Dialog
	{
		[SerializeField] private TMP_Text _header;
		[SerializeField] private TMP_Text _message;
		[SerializeField] private TMP_InputField _value;

		private Action<string> _onChoice;

		public void WithQuery(string header, string message, Action<string> onChoice)
		{
			_header.text = header;
			_message.text = message;
			_onChoice = onChoice;
			_value.text = "";
		}

		public void OnOk()
		{
			Hide();
			_onChoice(_value.text);
		}

		public void OnCancel()
		{
			Hide();
			_onChoice(null);
		}
	}
}