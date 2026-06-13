using System;
using TMPro;
using UnityEngine;

namespace Dialogs
{
	public class Toast: Dialog
	{
		[SerializeField] private TMP_Text _header;
		[SerializeField] private GameObject _closeButton;
		private Action _onCancel;

		public void WithMessage(string header, Action onCancel=null)
		{
			_header.text = header;
			_onCancel = onCancel;
			_closeButton.SetActive(onCancel!=null);
		}

		public new void Hide()
		{
			base.Hide();
		}

		public void OnCancel()
		{
			Hide();
			_onCancel();
		}
	}
}