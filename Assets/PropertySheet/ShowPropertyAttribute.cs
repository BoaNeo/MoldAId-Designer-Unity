using System;
using Dialogs;

namespace PropertySheet
{
	public class ShowPropertyAttribute : Attribute
	{
		public int order { get; set; }
		public string name { get; set; } = null;
		public string unit { get; set; } = "mm";
		public float min { get; set; } = float.MinValue;
		public float max { get; set; } = float.MaxValue;
		public string[] values { get; set; }
		public FileDialog.FileFilter filter { get; set; }
	}
}