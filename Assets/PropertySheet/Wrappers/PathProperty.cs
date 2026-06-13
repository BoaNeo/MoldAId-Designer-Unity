using IO;

namespace PropertySheet
{
	public struct PathProperty : IStreamable
	{
		public string path
		{
			get => _path;
			set => _path = value.FixPathSeparator();
		}

		private string _path;

		public PathProperty(string p)
		{
			_path = p.FixPathSeparator();
		}

		public override string ToString()
		{
			return path;
		}

		public void Serialize(DataStream data)
		{
			data.Serialize("path", ref _path);
			_path = _path.FixPathSeparator();
		}
	}
}