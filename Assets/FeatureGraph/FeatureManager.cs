using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Files;
using UnityEngine;
using Utility;

namespace FeatureGraph
{
	public class FeatureManager : MonoBehaviour
	{
		public static bool transientChange { get; set; }

		private Dictionary<string, IDataBlock> _data = new ();

		private Dictionary<string,Feature> _features = new ();
		private WorkerThread _worker;
		private float _buildStarted;
		private string _buildComment;
		private string _buildResult;
		private int _buildQueue;
		private Action _clearCallback;
		private Feature _feature;
		private List<Feature> _runningFeatures;
		private bool _runningChanges;

		public Logger<Feature> log { get; } = new ();
		
		private void Awake()
		{
			_worker = gameObject.AddComponent<WorkerThread>();
		}

		public T AddFeature<T>(string name) where T : Feature, new()
		{
			T feature = new T();
			AddFeature(name, feature);
			return feature;
		}

		public void AddFeature(string name, Feature feature)
		{
			if (string.IsNullOrEmpty(name))
				name = "New";
			name = UniqueName(name);

			feature.InternalSetName(name);

			RecursivelyCreateData(feature.name,feature);
			_features[name] = feature;
		}

		public DataRef<T> CreateData<T>(Feature creator, string name)
		{
			DataRef<T> dref = new DataRef<T>();
			string fullname = $"{creator.name}.{name}";
			string fixedname = fullname;
			int idx = 0;
			while (_data.ContainsKey(fixedname))
				fixedname = $"{fullname}_{idx++}";
			CreateData(fixedname, dref);
			return dref;
		}

		private void RecursivelyCreateData(string featurename, object feature)
		{
			if (feature == null)
				return;
			foreach (PropertyInfo prop in feature.GetType().GetProperties())
			{
				if (typeof(IDataRef).IsAssignableFrom(prop.PropertyType))
				{
					IDataRef dref = (IDataRef)prop.GetValue(feature);
					if (dref != null)
					{
						if(!dref.hasData)
							CreateData( string.IsNullOrEmpty(dref.blockname) ? $"{featurename}.{prop.Name}" : dref.blockname, dref );
						RecursivelyCreateData($"{featurename}.{prop.Name}", dref.data);
					}
					else
					{
						Debug.Log($"{featurename}.{prop.Name} is null (??)");
					}
				}
			}
		}

		private IDataBlock CreateData(string name, IDataRef dref)
		{
			if (!_data.TryGetValue(name, out IDataBlock block))
			{
				block = dref.NewDataBlock(name);
				_data[name] = block;
			}
			else
			{
				dref.UseData(block);
			}
			return block;
		}

		private string UniqueName(string s)
		{
			string newname = s;
			int i = 1;
			while (_features.ContainsKey(newname))
			{
				newname = $"{s}_{i++}";
			}
			return newname;
		}

		public void RemoveFeature(Feature feature)
		{
			if (feature == null)
				return;

			_features.Remove(feature.name);
		}

		public T GetOrCreateFeature<T>(string name) where T:Feature,new()
		{
			T existing = GetFeature<T>(name);
			if (existing != null)
				return existing;
			return AddFeature<T>(name);
		}
		
		public T GetFeature<T>(string featureName=null) where T: Feature
		{
			if (featureName != null && _features.TryGetValue(featureName, out Feature feature))
				return (T)feature;

			foreach (Feature f in _features.Values)
			{
				if (f is T && (featureName == null || featureName.Equals(f.name)))
					return (T)f;
			}
			return default;
		}

		public List<T> GetFeatures<T>() where T: Feature
		{
			List<T> features = new List<T>();
			foreach (Feature f in _features.Values)
			{
				if (f is T t)
					features.Add(t);
			}
			return features;
		}

		public void DisableAll()
		{
			foreach (Feature f in _features.Values)
				f.enabled = false;
		}

		public void Clear( Action whenReady )
		{
			_clearCallback = whenReady;
		}

		public bool Rebuild()
		{
			if ( _runningFeatures==null || _runningFeatures.Count==0 )
			{
				if (_clearCallback!=null)
				{
					_features.Clear();
					_data.Clear();
					Action callback = _clearCallback;
					_clearCallback = null;
					callback?.Invoke();
					log.Clear();
					return true;
				}

				FeatureExecutionBatch exec = new (_features.Values);

				StringBuilder sb = new StringBuilder();
				foreach (Feature feature in exec.batch)
					sb.Append($"'{feature.name}', ");
				if(sb.Length>0)
					Debug.Log($"Building features in parallel : [{sb.ToString()}]");

				foreach (Feature feature in exec.batch)
					feature.StartBuildJob(_worker, log);

				_runningChanges = false;
				_runningFeatures = exec.batch;
			}
			
			log.Update();

			int preCount = _runningFeatures.Count;
			for (int fi = 0; fi < _runningFeatures.Count; fi++)
			{
				if (_runningFeatures[fi].result == Feature.BuildResult.FinishedWithChanges)
					_runningChanges = true;
				if (_runningFeatures[fi].result != Feature.BuildResult.Building)
					_runningFeatures.RemoveAt(fi--);
			}
			return _runningChanges && _runningFeatures.Count==0 && preCount!=0;
		}
		
		public void SerializeFeatureData(ProjectFile file)
		{
			Dictionary<string,FeatureData> featuredata = new Dictionary<string, FeatureData>();
			foreach( IDataBlock block in _data.Values )
			{
				file.data[block.name] = block.Save();
			}
			foreach (Feature feature in _features.Values)
			{
				FeatureData data = new FeatureData();
				data.name = feature.name;
				data.type = feature.GetType().FullName;
				foreach( PropertyInfo propinfo in feature.GetType().GetProperties())
				{
					if (typeof(IDataRef).IsAssignableFrom(propinfo.PropertyType))
					{
						IDataRef dataRef = (IDataRef)propinfo.GetValue(feature);
						data.references[propinfo.Name] = new DataRefData{datablock = dataRef.blockname};
					}
					else if (typeof(IList).IsAssignableFrom(propinfo.PropertyType))
					{
						DataRefList l = new DataRefList { datablocks = new List<string>() };
						foreach (object o in (IList)propinfo.GetValue(feature))
						{
							if (o is IDataRef dref)
							{
								l.datablocks.Add( dref.blockname );
							}
						}
						if(l.datablocks.Count>0)
							data.lists[propinfo.Name] = l;
					}
				}
				featuredata[feature.name] = data;
			}
			file.features = featuredata;
		}

		public void LoadFeatures(ProjectFile file)
		{
			foreach (FeatureData featuredata in file.features.Values)
			{
				Type type = Type.GetType(featuredata.type);
				Feature feature = (Feature)Activator.CreateInstance(type);
				
				foreach( PropertyInfo propinfo in feature.GetType().GetProperties())
				{
					if (typeof(IDataRef).IsAssignableFrom(propinfo.PropertyType) && featuredata.references.TryGetValue(propinfo.Name, out DataRefData value))
					{
						LoadReference(file, (IDataRef)propinfo.GetValue(feature), value.datablock);
					}
					else if (typeof(IList).IsAssignableFrom(propinfo.PropertyType) && featuredata.lists.TryGetValue(propinfo.Name, out DataRefList list))
					{
						Type[] listtypes = propinfo.PropertyType.GetGenericArguments();
						if (listtypes.Length > 0)
						{
							Type listtype = listtypes[0];
							IList target = (IList)propinfo.GetValue(feature);
							foreach (string datablock in list.datablocks)
							{
								IDataRef dref = (IDataRef)listtype.GetConstructor(Type.EmptyTypes).Invoke(Array.Empty<object>());
								LoadReference(file, dref, datablock);
								target.Add(dref);
							}
						}
					}
				}
				
				AddFeature( featuredata.name, feature);
			}

			void LoadReference(ProjectFile file, IDataRef target, string datablockname)
			{
				if (file.data.TryGetValue(datablockname, out DataBlockData data))
				{
					if (!_data.TryGetValue(datablockname, out IDataBlock block))
					{
						block = CreateData(datablockname, target);
						block.Load(data);
					}
					target.UseData(block);
				}
			}
		}

		public ProjectFile VersionCheck(ProjectFile file, out string message)
		{
			FileVersion fileVersion = new FileVersion(file.version);
			FileVersion appVersion = new FileVersion(Application.version);

			switch (fileVersion.CompareTo(appVersion))
			{
				case FileVersion.CompareResult.Invalid:
					message = "This file has an unsupported version and cannot be loaded!";
					return null;
				case FileVersion.CompareResult.Later:
					message = "This file was saved with a more recent version of this software.\nLoading new projects in older versions is not supported!\nPlease upgrade to latest version and try again.";
					return null;
				case FileVersion.CompareResult.Earlier:
					file = VersionUpgrade(file, fileVersion, appVersion, out message);
					break;
				default:
					// This file matches the software and should load with no changes
					message = null;
					break;
			}
			file.version = Application.version;
			return file;
		}

		private ProjectFile VersionUpgrade(ProjectFile file, FileVersion from, FileVersion to, out string message)
		{
			message = "File was upgraded";
			return file;
		}
	}
}