using System;
using System.Collections.Generic;
using System.Reflection;
using IO;
using UnityEngine;
using Utility;

namespace Files
{
	// TODO: All this should be in ProjectFile.cs
	public class FeatureData : IStreamable
	{
		public string name;
		public string type;
		public Dictionary<string,DataRefData> references = new Dictionary<string, DataRefData>();
		public Dictionary<string,DataRefList> lists = new Dictionary<string, DataRefList>();

		public void Serialize(DataStream data)
		{
			data.Serialize("name", ref name);
			data.Serialize("type", ref type);
			data.Serialize("references", ref references);
			data.Serialize("lists", ref lists);
		}
	}

	public class DataRefData : IStreamable
	{
		public string datablock;
		
		public void Serialize(DataStream data)
		{
			data.Serialize("datablock", ref datablock);
		}
	}

	public class DataRefList : IStreamable
	{
		public List<string> datablocks;
		
		public void Serialize(DataStream data)
		{
			data.Serialize("datablocks", ref datablocks);
		}
	}
	
	public class DataBlockData : IStreamable
	{
		private string _type; // This is not currently used, but could be useful in cases where the caller does not know the type
		private string _value;
		private bool _raw;

		public DataBlockData()
		{
		}
		
		public DataBlockData(object source)
		{
			_type = source==null?"null":source.GetType().FullName;
			_value = "";
			_raw = false;
			if (source != null)
			{
				if (source is IStreamable streamable)
				{
					DataStreamJsonOutput os = new DataStreamJsonOutput();
					streamable.Serialize(os);
					_value = $"{{{os.json}}}";
					_raw = true;
				}
				else if (source is Vector3)
					_value = ((Vector3)source).ToAccurateString();
				else
					_value = source.ToString();
			}
		}

		public T ConvertValueTo<T>(T old)
		{
			Type tt = typeof(T);
			if ( typeof(IStreamable).IsAssignableFrom(tt)  )
			{
				IStreamable streamable = (IStreamable)old;
				DataStreamJsonInput ins = new DataStreamJsonInput(_value);
				if (streamable == null)
					streamable = (IStreamable)( tt.GetConstructor(Type.EmptyTypes)?.Invoke(Array.Empty<object>()));
				streamable.Serialize(ins);
				return (T)streamable;
			}
			if ( tt == typeof(bool) )
				return (T) (object) bool.Parse(_value);
			if ( tt == typeof(int))
				return (T) (object) int.Parse(_value);
			if ( tt == typeof(float))
				return (T)(object)float.Parse(_value);
			if (tt == typeof(Vector3))
				return (T)(object)VectorExtension.Parse(_value);
			if (tt == typeof(string))
				return (T)(object)_value;
			ConstructorInfo typeConstructor = typeof(T).GetConstructor(new[] { typeof(string) });
			if(typeConstructor!=null)
				return (T) typeConstructor.Invoke(new[] {_value});
//			return (T)(object)_value;
			return default;
		}

		public void Serialize(DataStream data)
		{
			data.Serialize("raw", ref _raw);
			data.Serialize("type", ref _type);
			data.Serialize("value", ref _value, _raw);
		}
	}
}