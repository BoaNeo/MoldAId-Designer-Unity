using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace IO
{
	public class Library<T> where T: LibraryFile, new()
	{
		private string path => Application.persistentDataPath.AppendPath($"{typeof(T).Name}s");
		public int Count => _library.Count;
		private Dictionary<string,T> _library;
		private List<T> _libraryList= new ();

		private static Dictionary<object, object> _libraries = new ();
		public static Library<T> Load()
		{
			var key = typeof(T);
			if (_libraries.TryGetValue(key, out object o))
				return (Library<T>) o;

			Library<T> lib = new Library<T>();
			lib.LoadLibrary();
			_libraries[key] = lib;
			new T().CreateDefaults();
			return lib;
		}
		
		private void LoadLibrary()
		{
			if (_library == null || _library.Count == 0)
			{
				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);
				string[] files = Directory.GetFiles( path );
				Dictionary<string,T> library = new ();
				for (int i = 0; i < files.Length; i++)
				{
					T file = StreamableFile.Load<T>(files[i]);
					file.key = Path.GetFileName(files[i]);
					library[file.key] = file;
				}
				_library = library;
			}
		}

		public List<T> GetFiles()
		{
			if (_library == null)
				LoadLibrary();
			_libraryList.Clear();
			foreach (T file in _library.Values)
				_libraryList.Add(file);
			_libraryList.Sort((f1, f2) => String.Compare(f1.key, f2.key, StringComparison.Ordinal) );
			return _libraryList;
		}

		public void SaveLibrary()
		{
			string[] existing = Directory.GetFiles(path);
			foreach(string old in existing)
				File.Delete(old);
			foreach(var kvp in _library)
				kvp.Value.Save( path.AppendPath(kvp.Value.key));
		}

		public void Add(string key, T file)
		{
			if (!_library.ContainsKey(key))
			{
				file.key = key;
				_library[key] = file;
				GetFiles();
			}
		}

		public void Remove(string key)
		{
			_library.Remove(key);
			GetFiles();
		}

		public bool ContainsKey(string key)
		{
			return _library.ContainsKey(key);
		}

		public T this[int idx] => GetFiles()[idx];
		public T this[string key] => _library[key];
	}
}