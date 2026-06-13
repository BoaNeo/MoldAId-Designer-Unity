using IO;

namespace Files
{
	public class MoldFile : LibraryFile
	{
		public enum MoldType { Stl, Box, Cylinder }

		public string stlFile; // Path to load STL from if mold is defined by a cad model
		public float width; // Radius if cylinder
		public float height;
		public float depth; // Unused if cylinder
		public float fillet; // Filleting of sharp edges
		public MoldType type;
		public float sprueDiameter;
		public float sprueHeight;

		public MoldFile()
		{
			type = MoldType.Box;
			width = 10;
			height = 10;
			depth = 10;
			fillet = 1;
			sprueDiameter = 5;
			sprueHeight = 5;
		}

		public override void CreateDefaults()
		{
		}

		public MoldFile Copy()
		{
			MoldFile copy = new MoldFile();
			DataStreamJsonOutput write = new DataStreamJsonOutput();
			this.Serialize(write);
			string json = $"{{{write.json}}}";
			DataStreamJsonInput read = new DataStreamJsonInput(json);
			copy.Serialize(read);
			copy.key = key;
			return copy;
		}

		public override void Serialize(DataStream data)
		{
			data.Serialize("stlFile", ref stlFile);
			data.Serialize("width", ref width);
			data.Serialize("height",ref height);
			data.Serialize("depth",ref depth);
			data.Serialize("fillet",ref fillet);
			data.Serialize("sprueHeight", ref sprueHeight);
			data.Serialize("sprueDiameter", ref sprueDiameter);
			data.Serialize("type", value => { type = MoldTypeFromString(value); }, () => type.ToString() );
		}

		private MoldType MoldTypeFromString(string value)
		{
			switch (value)
			{
				case "Box": return MoldType.Box;
				case "Cylinder": return MoldType.Cylinder;
				case "Stl": return MoldType.Stl;
			}
			return MoldType.Box;
		}
	}
}