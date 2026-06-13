using System;

namespace Files
{
	public class FileVersion
	{
		public enum CompareResult {Invalid, Later, Earlier, Same}
		private readonly int[] _version;

		public FileVersion(string versionString)
		{
			string[] version = versionString==null ? Array.Empty<string>() : versionString.Split('.');
			_version = new int[version.Length];
			for (int i=0;i<version.Length;i++)
			{
				if (int.TryParse(version[i], out int v))
					_version[i] = v;
				else
				{
					_version = Array.Empty<int>();
					return;
				}
			}
		}

		public CompareResult CompareTo(FileVersion other)
		{
			if (other._version.Length != _version.Length)
				return CompareResult.Invalid;
			for (int i = 0; i < _version.Length; i++)
			{
				if (_version[i] < other._version[i])
					return CompareResult.Earlier;
				if (_version[i] > other._version[i])
					return CompareResult.Later;
			}
			return CompareResult.Same;
		}
	}
}