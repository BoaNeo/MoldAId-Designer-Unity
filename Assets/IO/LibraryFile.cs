namespace IO
{
	public abstract class LibraryFile : StreamableFile
	{
		public string key;
		public abstract void CreateDefaults();
	}
}