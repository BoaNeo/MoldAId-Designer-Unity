using IO;
using UnityEngine;

namespace Files
{
	public class PrinterFile : LibraryFile
	{
		public string name;
		public Vector3 volume;
		
		public override void Serialize(DataStream data)
		{
			data.Serialize("name", ref name);
			data.Serialize("volume", ref volume);
		}
		
		public override void CreateDefaults()
		{
			var lib = Library<PrinterFile>.Load();
			lib.Add("Prusa SL1S", new PrinterFile()
			{
				name = "Prusa SL1S",
				volume = new Vector3(126, 79, 150),
			});
			lib.Add("Addline", new PrinterFile()
			{
				name = "Addline",
				volume = new Vector3(96, 54, 150),
			});
			lib.Add("Nexa XiP", new PrinterFile()
			{
				name = "Nexa XiP",
				volume = new Vector3(190, 110, 170),
			});
			lib.Add("Nexa NXE 400", new PrinterFile()
			{
				name = "Nexa NXE 400",
				volume = new Vector3(275, 160, 400),
			});
		}
	}
}