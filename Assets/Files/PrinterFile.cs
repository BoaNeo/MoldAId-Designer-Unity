using IO;
using UnityEngine;

namespace Files
{
	public class PrinterFile : LibraryFile
	{
		public string name;
		public Vector3 volume;
		public float defaultLayerThickness;
		public float minLayerThickness;
		public float maxLayerThickness;
		
		public override void Serialize(DataStream data)
		{
			data.Serialize("name", ref name);
			data.Serialize("volume", ref volume);
			data.Serialize("defautLayerThickness", ref defaultLayerThickness);
			data.Serialize("minLayerThickness", ref minLayerThickness);
			data.Serialize("maxLayerThickness", ref maxLayerThickness);
		}
		
		public override void CreateDefaults()
		{
			var lib = Library<PrinterFile>.Load();
			lib.Add("Prusa SL1S", new PrinterFile()
			{
				name = "Prusa SL1S",
				volume = new Vector3(126, 79, 150),
				minLayerThickness = 0.05f,
				maxLayerThickness = 0.10f,
				defaultLayerThickness = 0.10f,
			});
			lib.Add("Addline", new PrinterFile()
			{
				name = "Addline",
				volume = new Vector3(96, 54, 150),
				minLayerThickness = 0.05f,
				maxLayerThickness = 0.10f,
				defaultLayerThickness = 0.10f,
			});
			lib.Add("Nexa XiP", new PrinterFile()
			{
				name = "Nexa XiP",
				volume = new Vector3(190, 110, 170),
				minLayerThickness = 0.05f,
				maxLayerThickness = 0.10f,
				defaultLayerThickness = 0.10f,
			});
			lib.Add("Nexa NXE 400", new PrinterFile()
			{
				name = "Nexa NXE 400",
				volume = new Vector3(275, 160, 400),
				minLayerThickness = 0.05f,
				maxLayerThickness = 0.10f,
				defaultLayerThickness = 0.10f,
			});
		}
	}
}