using System;
using TMPro;
using UnityEngine;

namespace Dialogs
{
	public class MessageBox : Dialog
	{
		[SerializeField] private TMP_Text _header;
		[SerializeField] private TMP_Text _message;
		[SerializeField] private GameObject _okButton;
		[SerializeField] private GameObject _cancelButton;
		[SerializeField] private GameObject _closeButton;

		private Action<bool> _onChoice;
		private Action _onClose;

		public void WithQuery(string header, string message, Action<bool> onChoice)
		{
			Init(header, message, null, onChoice);
		}

		public void WithMessage(string header, string message, Action onClose)
		{
			Init(header, message, onClose, null);
		}

		private void Init(string header, string message, Action onClose, Action<bool> onChoice)
		{
			_header.text = header;
			_message.text = message;
			_onChoice = onChoice;
			_onClose = onClose;
			_okButton.SetActive(onChoice != null);
			_cancelButton.SetActive(onChoice != null);
			_closeButton.SetActive(onClose != null);
		}

		public void OnOk()
		{
			Hide();
			_onChoice(true);
		}
		public void OnCancel()
		{
			Hide();
			_onChoice(false);
		}
		public void OnClose()
		{
			Hide();
			if (_onChoice != null)
				_onChoice(false);
			else
				_onClose();
		}
	}
}