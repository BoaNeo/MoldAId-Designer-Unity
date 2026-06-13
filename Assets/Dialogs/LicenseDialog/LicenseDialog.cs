using System;
using TMPro;
using UnityEngine;

namespace Dialogs.LicenseDialog
{
	public class LicenseDialog : Dialog
	{
		[SerializeField] private TMP_Text _message;
		[SerializeField] private GameObject _trialButton;

		private Action<bool> _onClose;
		private LicenseSpringSDK _manager;

		public void WithManager(LicenseSpringSDK manager, Action<bool> onClose)
		{
			_manager = manager;
			_onClose = onClose;
			if(!manager.HasLicense)
				_message.text = "You do not have a license for this application!";
			else if (manager.IsExpired)
			{
				_trialButton.SetActive(false);
				if (manager.IsTrial)
				{
					_message.text = "Your trial license for this application has expired!\nPlease obtain and activate a commercial license for continued use!";
				}
				else
				{
					_message.text = "Your license for this application has expired!";
				}
			}
		}

		public void OnActivateLicense()
		{
			DialogManager.Show<InputBox.InputBox>().WithQuery("Activate License", "Enter License Key", key =>
			{
				if (!string.IsNullOrEmpty(key))
				{
					string error = "No key was returned!";
					try
					{
						_manager.ActivateLicense(key);
					}
					catch(Exception e)
					{
						error = e.Message;
					}		
					if(!_manager.HasLicense || _manager.IsExpired)
						DialogManager.Show<MessageBox>().WithMessage("Unable to activate license!", error, () => { });
					else
					{
						Hide();
						_onClose(true);
					}
				}
			});
		}

		public void OnActivateFromFile()
		{
			DialogManager.Show<FileDialog>().SelectFileToOpen("Select license activation file","", FileDialog.FileFilter.ANY, file =>
			{
				if (file.Length != 0)
				{
					string error = "No key was returned!";
					try
					{
						_manager.ActivateLicenseOffline(file);
					}
					catch(Exception e)
					{
						error = e.Message;
					}		
					if(!_manager.HasLicense || _manager.IsExpired)
						DialogManager.Show<MessageBox>().WithMessage("Unable to activate license!", error, () => { });
					else
					{
						Hide();
						_onClose(true);
					}
				}				
			});
		}

		public void OnExit()
		{
			Hide();
			_onClose(false);
		}
		
		public void OnGetTrial()
		{
			string error = "No key was returned!";
			try
			{
				string trialKey = _manager.GetTrialLicense();
				_manager.ActivateLicense(trialKey);
			}
			catch(Exception e)
			{
				error = e.Message;
			}		
			if(!_manager.HasLicense || _manager.IsExpired)
				DialogManager.Show<MessageBox>().WithMessage("Unable to get trial key!", error, () => { });
			else
			{
				Hide();
				_onClose(true);
			}
		}
	}
}