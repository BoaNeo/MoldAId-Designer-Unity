using System;
using IO;
using Plugins.StandaloneFileBrowser;

namespace Dialogs
{
	public class FileDialog : Dialog
	{
		private enum State { Idle, Open, Ready}
		private string[] _files;
		private State _state;
		private object _stateLock = new object();
		private Action<string> _onDone;

		public enum FileFilter { STL, AFP, APP, ANY, FOLDER }
		private static ExtensionFilter[][] FILTERS = new ExtensionFilter[][]
 		{
			new[] { new ExtensionFilter("STL Files", "stl") },
			new[] { new ExtensionFilter("Project", "afp")},
			new[] { new ExtensionFilter("Application", "exe","app")},
			new[] { new ExtensionFilter("Any File", "*")},
			new[] { new ExtensionFilter("Folder", "")}
		};

		public void SelectFolder(string header, string path, Action<string> onSelect)
		{
			lock (_stateLock)
			{
				_state = State.Open;
				_files = null;
				_onDone = onSelect;
			}

			StandaloneFileBrowser.OpenFolderPanelAsync( header, path, false, files =>
			{
				lock (_stateLock)
				{
					_files = files;
					_state = State.Ready;
				}
			});
		}

		public void SelectFileToSave(string header, string path, string filename, FileFilter filter, Action<string> onSave)
		{
			lock (_stateLock)
			{
				_state = State.Open;
				_files = null;
				_onDone = onSave;
			}

			StandaloneFileBrowser.SaveFilePanelAsync( header, path, filename, FILTERS[(int)filter], file =>
			{
				lock (_stateLock)
				{
					_files = new [] { file };
					_state = State.Ready;
				}
			});
		}

		public void SelectFileToOpen(string header, string path, FileFilter filter, Action<string> onLoad)
		{
			lock (_stateLock)
			{
				_state = State.Open;
				_files = null;
				_onDone = onLoad;
			}

			StandaloneFileBrowser.OpenFilePanelAsync( header, path, FILTERS[(int)filter], false, files =>
			{
				lock (_stateLock)
				{
					_files = files;
					_state = State.Ready;
				}
			});
		}

		private void Update()
		{
			lock (_stateLock)
			{
				if (_state==State.Ready)
				{
					Hide();
					if (_files != null && _files.Length > 0)
					{
						_onDone( _files[0].FixPathSeparator() );
					}
					else
					{
						_onDone(null);
					}
					_state = State.Idle;
				}
			}
		}
	}
}