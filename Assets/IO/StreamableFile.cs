using System;
using System.IO;
using UnityEngine;

namespace IO
{
	public abstract class StreamableFile : IStreamable
	{
		public string path;

		public static T Load<T>(string path) where T:StreamableFile, new()
		{
			try
			{
				string text = File.ReadAllText(path);
				DataStreamJsonInput input = new DataStreamJsonInput(text);
				T t = new T();
				t.Serialize(input);
				t.path = path;
				return t;
			}
			catch (Exception e)
			{
				Debug.LogError($"Failed to load {path}: {e}");
				return null;
			}
		}

		public void Save(string newpath)
		{
			path = newpath;
			DataStreamJsonOutput output = new DataStreamJsonOutput();
			Serialize(output);
			
			File.WriteAllText(path, $"{{{output.json}}}");

			Debug.Log(output.json);
		}

		public abstract void Serialize(DataStream data);
	}
}