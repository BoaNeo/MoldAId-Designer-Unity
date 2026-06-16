using System.Collections.Generic;
using IO;
using UnityEngine;

namespace Files
{
	public class ProjectFile : StreamableFile
	{
		public string version;
		public string name;
		public string partPath;
		public string printerId;
		public Dictionary<string, DataBlockData> data = new();
		public Dictionary<string, FeatureData> features = new();

		public PrinterFile printer => printerId==null ? Library<PrinterFile>.Load()[0] : Library<PrinterFile>.Load()[printerId];

		public ProjectFile()
		{
			version = Application.version;
			name = "Unnamed";
		}

		public override void Serialize(DataStream stream)
		{
			stream.Serialize("version", ref version);
			stream.Serialize("name", ref name);
			stream.Serialize("partPath", ref partPath);
			stream.Serialize("printerId", ref printerId);
			stream.Serialize("features",ref features);
			stream.Serialize("data", ref data);
		}
	}
}