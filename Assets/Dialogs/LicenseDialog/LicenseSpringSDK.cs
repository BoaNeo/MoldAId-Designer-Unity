//#define USE_LICENSE_SPRING

using System;
using LicenseSpring;
using UnityEngine;

namespace Dialogs.LicenseDialog
{
	public class LicenseSpringSDK : MonoBehaviour
	{
		[SerializeField] private string _licenseKey = "";
		[SerializeField] private string _apiKey;
		[SerializeField] private string _sharedKey;
		[SerializeField] private string _productCode;
		[SerializeField] private bool _debugData;

		[Tooltip("Check true, if you use custom product name and product version")]
		[SerializeField] private bool _useCustomProductData;
		[SerializeField] private string _appName;
		[SerializeField] private string _appVersion;

		#if USE_LICENSE_SPRING
		public bool HasLicense => _licenseManager.CurrentLicense()?.IsValid() ?? false;
		public bool IsExpired => _licenseManager.CurrentLicense()?.IsExpired() ?? true;
		public bool IsTrial => _licenseManager.CurrentLicense()?.IsTrial() ?? false;
		public int DaysRemaining => _licenseManager.CurrentLicense()?.DaysRemaining() ?? 0;
		public string LicenseKey => _licenseManager.CurrentLicense()?.Key() ?? "(None)";
		public int ExpiresInDays => _licenseManager.CurrentLicense()?.DaysRemaining() ?? 0;
		

		private InstallationFile _installationFile;
		private ILicenseManager _licenseManager;

		private ExtendedOptions _extendedOptions;
		private Configuration _licenseConfiguration;
		private ProductDetails _productDetails;
		
		#else
		public bool HasLicense => true; // license!=null;
		public bool IsExpired => false; // license.IsExpired;
		public bool IsTrial => false; // license.IsTrial();
		public int DaysRemaining => 1; // lícense.DaysRemaining();
		#endif

		public void InitializeSDK()
		{
			DontDestroyOnLoad(gameObject);
			#if USE_LICENSE_SPRING

			if (!_useCustomProductData)
			{
				_appName = Application.productName;
				_appVersion = Application.version;
			}

			_extendedOptions = new ExtendedOptions();

			_extendedOptions.LicenseFilePath = Application.persistentDataPath + "/License.Key";
			_extendedOptions.HardwareID = SystemInfo.deviceUniqueIdentifier;

			if (Debug.isDebugBuild)
			{
				_extendedOptions.EnableLogging = true;
			}
			else
			{
				_extendedOptions.EnableLogging = false;
			}

			_extendedOptions.CollectNetworkInfo = true;

			_licenseConfiguration = new Configuration(
				apiKey: _apiKey,
				sharedKey: _sharedKey,
				productCode: _productCode,
				appName: _appName,
				appVersion: _appVersion,
				extendedOptions: _extendedOptions);

			_licenseConfiguration.OSVersion = SystemInfo.operatingSystem;

			_licenseManager = LicenseManager.GetInstance();
			_licenseManager.Initialize(_licenseConfiguration);

			_productDetails = _licenseManager.GetProductDetails();

			#endif
		}

		public void CheckLicense()
		{
			#if USE_LICENSE_SPRING
			try
			{
				_licenseManager.CurrentLicense().LocalCheck();
				InstallationFile file = _licenseManager.CurrentLicense().Check();
			}
			catch (Exception e)
			{
				Debug.LogError($"No Valid License ({e})!");
			}
			#endif
		}

		public void ActivateLicense(string licenseKey)
		{
			#if USE_LICENSE_SPRING
			LicenseSpring.LicenseID key = LicenseID.FromKey(licenseKey);

			if (licenseKey != null && licenseKey != "")
			{
				_licenseManager.ActivateLicense(key);
			}
			#endif
		}

		public void ActivateLicenseOffline(string file)
		{
			#if USE_LICENSE_SPRING
			_licenseManager.ActivateLicenseOffline(file);
			#endif
		}

		public string GetTrialLicense()
		{
			#if USE_LICENSE_SPRING
			return _licenseManager.GetTrialLicense();
			#else
			return "";
			#endif
		}

		/*
		public ILicense ActivateLicense(string user, string password)
		{
			LicenseSpring.LicenseID data = LicenseID.FromUser(user, password);
			license = licenseManager.ActivateLicense(data);
			return license;
		}

		public bool ChangePassword(string user, string password, string newPassword)
		{
			LicenseSpring.LicenseID data = LicenseID.FromUser(user, password);
			return licenseManager.ChangePassword(data, newPassword);
		}

		public string[] GetAllVersions(string licenseKey)
		{
			LicenseSpring.LicenseID key = LicenseID.FromKey(licenseKey);
			return licenseManager.GetAllVersions(key);
		}

		public string[] GetAllVersionsByUser(string user)
		{
			LicenseSpring.LicenseID data = LicenseID.FromUser(user);
			return licenseManager.GetAllVersions(data);
		}

		public InstallationFile GetInstallationFile(string licenseKey, string version = null)
		{
			LicenseSpring.LicenseID key = LicenseID.FromKey(licenseKey);
			return licenseManager.GetInstallationFile(key, version);
		}

		public InstallationFile GetInstallationFileByUser(string user, string version = null)
		{
			LicenseSpring.LicenseID data = LicenseID.FromUser(user);
			return licenseManager.GetInstallationFile(data, version);
		}

		public string CheckLicense(string licenseKey = null)
		{
			if (licenseKey == null || licenseKey == "")
			{
				return InstallationFileToString(license.Check());
			}

			LicenseID key = LicenseID.FromKey(licenseKey);
			return InstallationFileToString(licenseManager.ActivateLicense(key).Check());
		}

		public string GetActivationFile(string licenseKey)
		{
			LicenseID key = LicenseID.FromKey(licenseKey);
			return licenseManager.GetOfflineActivationFile(key);
		}

		public string GetActivationFile(string user, string password, string activationRequestFile)
		{
			LicenseID data = LicenseID.FromUser(user, password);
			return licenseManager.GetOfflineActivationFile(data, activationRequestFile);
		}

		public string InstallationFileToString(InstallationFile installationFile)
		{
			return "Md5Hash: " + installationFile.Md5Hash
			                   + "\nUrl: " + installationFile.Url
			                   + "\nVersion: " + installationFile.Version;
		}
		*/
	}
}