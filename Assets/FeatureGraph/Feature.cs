using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using Utility;

namespace FeatureGraph
{
	public abstract class Feature
	{
		public enum BuildResult
		{
			Building, Skipped, FinishedWithChanges
		}

		public string name { get; private set; }

		public bool enabled { get; set; }
		public BuildResult result { get; private set; }

		public void InternalSetName(string s)
		{
			name = s;
		}
		/*
		public enum ChangeFlags { Clean, Dirty }
		public ChangeFlags PrepareBuildJob()
		{
			BuildMetaData();
			bool changed = false;
			int i = 1;
			foreach (ParameterMetaData data in _parameterMeta)
			{
				if (data.output)
				{
					if(data.reference!=null)
						_parameters[i++] = data.reference;
					else
						_parameters[i++] = data.list;
				}
				else
				{
					if (data.reference != null)
					{
						changed |= data.reference.changed;
						_parameters[i++] = data.reference.data;
					}
					else if (data.list != null)
					{
						foreach(var obj in data.list)
						{
							if (obj is IDataRef dataref)
							{
								changed |= dataref.changed;
							}
							else
							{
								Debug.LogWarning("Input list contains a element that is not an IDataRef!");
							}
						}
						
						if(data.listAsArray.Length!=data.list.Count)
							data.listAsArray = Array.CreateInstance(data.listElementType, data.list.Count);
						for(int x=0;x<data.listAsArray.Length;x++)
							data.listAsArray.SetValue(((IDataRef)data.list[x]).data,x); 

						_parameters[i++] = data.listAsArray;//data.list;
					}
				}
			}

			_parameters[0] = FeatureManager.transientChange;

			if (changed)
				return ChangeFlags.Dirty;
			return ChangeFlags.Clean;
		}
		*/
		public void StartBuildJob(WorkerThread worker, Logger<Feature> log)
		{
			if (_buildMethod == null)
				return;

			foreach (ParameterMetaData data in _parameterMeta)
			{
				data.ForEachDataRef( dataref =>
				{
					dataref.changed = false;
				});
			}

			result = BuildResult.Building;
			_job = (IEnumerator<IYield>) _buildMethod.Invoke(null, _parameters);
			
			_log = log.NewEntry(this, name);
			_log.Begin();
			_worker = worker;
			_worker.RunOnMain( Run );
		}

		public void FinishBuildJob()
		{
			result = BuildResult.Skipped;
			foreach (ParameterMetaData data in _parameterMeta)
			{
				data.ForEachDataRef( dataref =>
				{
					if (data.output && dataref.changed)
					{
						Debug.Log($"{dataref.blockname} changed");
						result = BuildResult.FinishedWithChanges;
						dataref.changed = false;
					}
				});
			}
			Debug.Log($"Finished build of {name} with result: {result}");
			_job = null;
		}

		public void SetParameter(int idx, object value)
		{
			_parameters[idx] = value;
		}

		internal class ParameterMetaData
		{
			public bool output;
			public string name;
			public IDataRef reference;
			public IList list;
			public Type listElementType;
			public Array listAsArray;

			public void ForEachDataRef(Action<IDataRef> action)
			{
				if (reference != null)
					action(reference);
				if (list != null)
				{
					foreach (object o in list)
						action((IDataRef) o);
				}
			}
		}

		private List<ParameterMetaData> _parameterMeta;
		private object[] _parameters;
		private Type[] _parameterTypes;
		private MethodInfo _buildMethod;
		private IEnumerator<IYield> _job;
		private Logger<Feature>.LogEntry _log;
		private WorkerThread _worker;

		internal List<ParameterMetaData> BuildMetaData()
		{
			if (_parameterMeta != null)
				return _parameterMeta;

			_parameterMeta = new List<ParameterMetaData>();
			foreach (PropertyInfo prop in GetType().GetProperties())
			{
				bool output = prop.GetCustomAttribute<FeatureOutputAttribute>() != null;
				bool input = prop.GetCustomAttribute<FeatureInputAttribute>() != null;

				if (output || input)
				{
					if( typeof(IDataRef).IsAssignableFrom(prop.PropertyType))
					{
						IDataRef reference = (IDataRef)prop.GetValue(this);
						if (reference != null)
							_parameterMeta.Add(new ParameterMetaData { output = output, name = prop.Name, reference = reference });
						else
							Debug.LogWarning($"Uninitialized DataRef in {this.name}.{prop.Name}" );
					}
					else if (typeof(IList).IsAssignableFrom(prop.PropertyType) && prop.PropertyType.GetGenericArguments().Length>0 && typeof(IDataRef).IsAssignableFrom(prop.PropertyType.GetGenericArguments()[0]) )
					{
						IList list = (IList) prop.GetValue(this);
						if (list != null)
							_parameterMeta.Add(new ParameterMetaData { output = output, name = prop.Name, list = list });
						else
							Debug.LogWarning($"Uninitialized DataRef in {this.name}.{prop.Name}" );
					}
				}
			}

			_parameters = new object[1 + _parameterMeta.Count];
			_parameterTypes = new Type[1 + _parameterMeta.Count];
			int i = 0;
			_parameterTypes[i++] = typeof(bool);
			foreach (ParameterMetaData data in _parameterMeta)
			{
				_parameterTypes[i++] = GetParameterType(data);
			}

			Type type = GetType();
			do
			{
				// TODO: Should get all static public methods named "Build" and compare the parameter list names with the DataRefs and re-arrange accordingly.
				_buildMethod = type.GetMethod("Build", BindingFlags.Public|BindingFlags.Static, null, CallingConventions.Any, _parameterTypes, default);
				type = type.BaseType;
			} while (_buildMethod==null && type!=null);

			if (_buildMethod == null)
			{
				StringBuilder sb = new StringBuilder();
				sb.Append("public static IEnumerator<IYield> Build(bool changing");
				for (i = 0; i < _parameterMeta.Count; i++)
				{
					ParameterMetaData d = _parameterMeta[i];
					sb.Append($",\n                         {NameOf(_parameterTypes[i+1]) } {d.name}");
				}
				sb.Append(")\n");
				throw new Exception($"{GetType()} Has no build method - please declare a static method with signature:\n{sb}");
			}

			return _parameterMeta;
		}
		
		private Type GetParameterType(ParameterMetaData metaData)
		{
			if (metaData.output)
			{
				if (metaData.list != null)
					return metaData.list.GetType();
				if(metaData.reference!=null)
					return metaData.reference.GetType();
			}
			else
			{
				if (metaData.list != null)
				{
					Type elementType = metaData.list.GetType().GetGenericArguments()[0];
					metaData.listElementType = elementType.GetGenericArguments()[0];
					metaData.listAsArray = Array.CreateInstance(metaData.listElementType, 0);
					return metaData.listAsArray.GetType();
				}
				if(metaData.reference!=null)
					return metaData.reference.valueType;
			}
			return null;
		}


		private string NameOf(Type t)
		{
			if (t == typeof(Single))
				return "float";
			if (t == typeof(Boolean))
				return "bool";
			if (t == typeof(Int32))
				return "int";
			if (t == typeof(Int64))
				return "long";
			if (t == typeof(String))
				return "string";
			if (t.IsGenericType)
				return $"{t.Name.Substring(0,t.Name.IndexOf('`'))}<{NameOf(t.GetGenericArguments()[0])}>";

			return t.Name;
		}
		
		public void Run()
		{
			try
			{
				if (_job.MoveNext())
				{
					_job.Current.OnYield(_worker, Run);
				}
				else
				{
					FinishBuildJob();
					_log.Finished();
					_job = null;
				}
			}
			catch (Exception e)
			{
				_log.Failed(e.Message);
				_job = null;
				Debug.LogError(DateTime.Now + " Job failed with: " + e);
			}
		}

		public bool HasInputFrom(Feature other)
		{
			long t0 = Log.ElapsedTime();
			foreach (ParameterMetaData myMetaData in _parameterMeta)
			{
				if (!myMetaData.output)
				{
					foreach (ParameterMetaData otherMetaData in other._parameterMeta)
					{
						if (otherMetaData.output)
						{
							if (myMetaData.reference!=null && otherMetaData.reference!=null && myMetaData.reference.blockname.Equals(otherMetaData.reference.blockname))
								return true;
							if (myMetaData.list != null && otherMetaData.list != null)
							{
								for (int i = 0; i < myMetaData.list.Count; i++)
								{
									for (int j = 0; j < otherMetaData.list.Count; j++)
									{
										IDataRef myListElement = (IDataRef) myMetaData.list[i];
										IDataRef otherListElement = (IDataRef) myMetaData.list[i];
										if (myListElement.blockname.Equals(otherListElement.blockname))
											return true;
									}
								}
							}
						}
					}
				}
			}
			Log.ElapsedTime(t0, "Checked HasInputFrom");
			return false;
		}
	}
}