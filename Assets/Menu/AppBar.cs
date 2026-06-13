using System.Diagnostics;
using System.Text;
using FeatureGraph;
using Files;
using TMPro;
using UIComponents;
using UnityEngine;

namespace Menu
{
	public class AppBar : MonoBehaviour
	{
		[SerializeField] private FeatureManager _features;
		[SerializeField] private TMP_Text _status;
		[SerializeField] private TMP_Text _log;
		[SerializeField] private Transition _view;
		[SerializeField] private GameObject _blocker;
		[SerializeField] private Transition _progress;

		private void Update()
		{
			Logger<Feature>.LogEntry longestRunning = _features.log.GetLongestRunning();
			_status.text = longestRunning?.info ?? "";
			Stopwatch sw = longestRunning?.stopwatch;
			if (sw!=null && sw.IsRunning && sw.ElapsedMilliseconds > PreferencesFile.current.slowOperationTime && !_progress.isVisible)
				_progress.Show();
			if((sw==null || !sw.IsRunning) && _progress.isVisible)
				_progress.Hide();
		}

		public void OnShowLog()
		{
			StringBuilder sb = new StringBuilder();
			foreach (Logger<Feature>.LogEntry entry in _features.log.entries)
			{
				sb.AppendLine(entry.info);
			}
			_log.text = sb.ToString();
			_view.SetVisible(!_view.gameObject.activeSelf);
			_blocker.SetActive(_view.gameObject.activeSelf);
		}

		public void OnHideLog()
		{
			_view.SetVisible(false);
			_blocker.SetActive(false);
		}
	}
}