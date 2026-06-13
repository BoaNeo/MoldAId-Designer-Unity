using Dialogs;
using FeatureGraph;
using Files;
using Gizmos;
using IO;
using MeshUtil;
using MeshUtil.Extensions;
using PropertySheet;
using StageBar;
using UnityEngine;

namespace Features
{
	public abstract class Mold : Feature
	{
		private MoldFile _moldFile = new MoldFile();

		[ShowProperty(unit = "mm", min=1)]
		[FeatureInput] public DataRef<float> inSprueHeight { get; } = new (5);

		[ShowProperty(unit="mm", min=1)]
		[FeatureInput] public DataRef<float> inSprueDiameter { get; } = new (5);

		[ShowProperty(order = 1000)] public DataRef<string> moldName { get; } = new(""); 

		[FeatureOutput] public DataRef<MeshBuilder> outSprueMesh { get; } = new (null);

		[FeatureOutput] public DataRef<XForm> outSprueTransform { get; } = new (XForm.identity);

		[FeatureOutput] public DataRef<MeshBuilder> output { get; } = new (null);

		public Bounds bounds => output.value.GetBounds();

		[ShowProperty]
		public void SaveToLibrary(IStageContext context)
		{
			_moldFile.key = moldName;
			_moldFile.sprueDiameter = inSprueDiameter.value;
			_moldFile.sprueHeight = inSprueHeight.value;
			SaveSettingsTo(_moldFile);
			MoldFile file = _moldFile;
			Library<MoldFile> lib = Library<MoldFile>.Load();

			if (lib.ContainsKey(moldName))
				DialogManager.Show<MessageBox>().WithQuery($"Name [{moldName}] Already Exist!", "A mold with this name already exist in the library!\nDo you want to replace it?", DoSave);
			else
				DoSave(true);
			void DoSave(bool yes)
			{
				if (yes)
				{
					lib.Add(file.key,file);
					lib.SaveLibrary();
					context.SelectFeature(this);
				}
			}
		}

		public void Configure(MoldFile projectMold)
		{
			_moldFile = projectMold;
			moldName.Set(_moldFile.key);
			inSprueDiameter.Set(_moldFile.sprueDiameter );
			inSprueHeight.Set(_moldFile.sprueHeight);
			ReadSettingsFrom(_moldFile);
		}

		public abstract void ReadSettingsFrom(MoldFile file);
		public abstract void SaveSettingsTo(MoldFile file);

		protected static void BuildSprue(
			MeshBuilder moldMesh,
			float inSprueHeight,
			float inSprueDiameter,
			DataRef<MeshBuilder> outSprueMesh,
			DataRef<XForm> outSprueTransform)
		{
			float sprueRadius = inSprueDiameter/2.0f; 
			float sprueHeight = inSprueHeight;
			float sprueArcLength = PreferencesFile.current.arcLength;

			float circ = 2 * Mathf.PI * sprueRadius;
			float penetration = .3f;
			int sides = Mathf.Clamp(Mathf.CeilToInt(circ / sprueArcLength),3,120);
			MeshBuilder sprueMesh = MeshCylinder.GenerateCylinder(new MeshBuilder(), sprueRadius, sprueHeight+penetration, sides);

			XForm sprueTransform = new XForm( new Vector3(0,0, moldMesh.GetBounds().extents.z*2-sprueHeight+penetration), Quaternion.identity);
			//sprueMesh.transform = sprueTransform.localToWorldMatrix;

			outSprueMesh.Set(sprueMesh);
			outSprueTransform.Set( sprueTransform );
		}
	}
}