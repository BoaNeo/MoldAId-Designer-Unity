using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Utility;

namespace IO
{
	public interface IStreamable
	{
		void Serialize(DataStream data);
	}

	public interface DataStream
	{
		void Serialize<T>(string attribute, ref T value) where T : IStreamable, new();
		void Serialize<T>(string attribute, ref List<T> list) where T : IStreamable, new();
		// ReSharper disable Unity.PerformanceAnalysis
		void Serialize<T>(string attribute, ref Dictionary<string,T> collection) where T : IStreamable, new();
		void Serialize(string attribute, ref string[] value);
		void Serialize(string attribute, ref List<string> value);
		void Serialize(string attribute, ref string value, bool raw=false);
		void Serialize(string attribute, ref ulong value);
		void Serialize(string attribute, ref int value);
		void Serialize(string attribute, ref byte value);
		void Serialize(string attribute, ref float value);
		void Serialize(string attribute, ref Vector3 value);
		void Serialize(string attribute, ref Quaternion value);
		void Serialize(string attribute, ref double value);
		void Serialize(string attribute, ref bool value);

		void Serialize(string attribute, Action<string> set , Func<string> get);
	}

	public class DataStreamJsonInput : DataStream
	{
		private List<JSONNode> _stack;
		public DataStreamJsonInput(string json, bool enter=false)
		{
			_stack = new List<JSONNode>();
			push(JSON.Parse(json));
		}

		public void Serialize<T>(string attribute, ref T value) where T: IStreamable, new()
		{
			if (!CurrentNodeHasAttribute(attribute))
				return;
			try
			{
				value = new T();
				if (attribute != null)
				{
					if (currentNode[attribute] == null)
						return;
					push(currentNode[attribute].AsObject);
				}
				value.Serialize(this);
			}
			catch (Exception e)
			{
				Debug.LogWarning(e);
				value = default;
			}
			if(attribute!=null)
				pop();
		}

		public void Serialize<T>(string attribute, ref Dictionary<string,T> collection) where T : IStreamable, new()
		{
			if (!CurrentNodeHasAttribute(attribute))
				return;
			collection.Clear();

			if (attribute != null)
			{
				if (currentNode[attribute] == null)
					return;
				push(currentNode[attribute].AsObject);
			}

			Dictionary<string,JSONNode>.KeyCollection.Enumerator keys = currentNode.Keys.GetEnumerator();
			while(keys.MoveNext())
			{
				push(currentNode[keys.Current].AsObject);
				T t = new T();
				Serialize(null,ref t);
				collection[keys.Current] = t;
				pop();
			}
			
			if(attribute!=null)
				pop();
			
		}
		
		public void Serialize<T>(string attribute, ref List<T> list) where T : IStreamable, new()
		{
			if (!CurrentNodeHasAttribute(attribute))
				return;
			list.Clear();

			if (attribute != null)
			{
				if (currentNode[attribute] == null)
					return;
				push(currentNode[attribute].AsArray);
			}
			
			var temp = currentNode.AsArray;
			for (int i = 0; i < temp.Count; i++)
			{
				push(temp[i].AsObject);
				T t = new T();
				Serialize(null,ref t);
				list.Add(t);
				pop();
			}
			
			if(attribute!=null)
				pop();
		}

		public void Serialize(string attribute, Action<string> set, Func<string> get)
		{
			if (!CurrentNodeHasAttribute(attribute))
				return;
			set(currentNode[attribute].Value);
		}

		public void Serialize(string attribute, ref string[] value)
		{
			if (!CurrentNodeHasAttribute(attribute))
				return;
			if (attribute != null)
			{
				if (currentNode[attribute] == null)
				{
					value = new string[0];
					return;
				}
				push(currentNode[attribute].AsArray);
			}
			
			var temp = currentNode.AsArray;
			value = new string[temp.Count];
			for (int i = 0; i < temp.Count; i++)
				value[i] = temp[i].Value;
			
			if(attribute!=null)
				pop();
		}
		
		public void Serialize(string attribute, ref List<string> value)
		{
			if (!CurrentNodeHasAttribute(attribute))
				return;
			if (attribute != null)
			{
				if (currentNode[attribute] == null)
				{
					value = new List<string>();
					return;
				}
				push(currentNode[attribute].AsArray);
			}
			
			var temp = currentNode.AsArray;
			value = new List<string>(temp.Count);
			for (int i = 0; i < temp.Count; i++)
				value.Add(temp[i].Value);
			
			if(attribute!=null)
				pop();
		}

		public void Serialize(string attribute, ref string value, bool raw=false)
		{
			if (!CurrentNodeHasAttribute(attribute))
				return;
			if (raw)
				value = currentNode[attribute].ToJSON(0);
			else
				value = currentNode[attribute].Value;
		}
		
		public void Serialize(string attribute, ref int value)
		{
			if (!CurrentNodeHasAttribute(attribute))
				return;
			value = currentNode[attribute].AsInt;
		}

		public void Serialize(string attribute, ref byte value)
		{
			if (!CurrentNodeHasAttribute(attribute))
				return;
			value = (byte)currentNode[attribute].AsInt;
		}

		public void Serialize(string attribute, ref ulong value)
		{
			if (!CurrentNodeHasAttribute(attribute))
				return;
			value = ulong.Parse(currentNode[attribute].Value);
		}

		public void Serialize(string attribute, ref float value)
		{
			if (!CurrentNodeHasAttribute(attribute))
				return;
			value = currentNode[attribute].AsFloat;
		}

		public void Serialize(string attribute, ref Vector3 value)
		{
			if (!CurrentNodeHasAttribute(attribute))
				return;
			value = VectorExtension.Parse( currentNode[attribute].Value);
		}

		public void Serialize(string attribute, ref Quaternion value)
		{
			if (!CurrentNodeHasAttribute(attribute))
				return;
			value = VectorExtension.ParseQuaternion( currentNode[attribute].Value);
		}

		public void Serialize(string attribute, ref double value)
		{
			if (!CurrentNodeHasAttribute(attribute))
				return;
			value = currentNode[attribute].AsDouble;
		}

		public void Serialize(string attribute, ref bool value)
		{
			if (!CurrentNodeHasAttribute(attribute))
				return;
			value = currentNode[attribute].AsBool;
		}

		private bool CurrentNodeHasAttribute(string attr)
		{
			if (attr == null)
				return true;
			bool result = false;
			try
			{
				JSONNode node = currentNode[attr];
				result = node.Value.Length> 0 || node is JSONClass || node is JSONArray;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
			if(!result)
				Debug.LogWarning($"Attribute {attr} not found in node {currentNode}!");
			return result;
		}
		
		public JSONNode currentNode => _stack[_stack.Count-1];

		private void push(JSONNode node)
		{
			_stack.Add(node);
		}

		private void pop()
		{
			_stack.RemoveAt(_stack.Count - 1);
		}
	}

	public class DataStreamJsonOutput : DataStream
	{
		private StringBuilder _out;
		private bool _needsComma;

		public DataStreamJsonOutput()
		{
			_out = new StringBuilder();
		}

		public void Serialize<T>(string attribute, ref T value) where T: IStreamable, new()
		{
			Comma();
			if(attribute!=null)
				_out.AppendFormat("\"{0}\":{{\n", attribute);
			else
				_out.Append("{\n");
			if (value != null)
			{
				_needsComma = false;
				value.Serialize(this);
			}
			_out.Append("\n}");
			_needsComma = true;
		}

		public void Serialize<T>(string attribute, ref List<T> list) where T : IStreamable, new()
		{
			Comma();
			if(attribute!=null)
				_out.AppendFormat("\"{0}\":\n", attribute);
			_out.AppendFormat("[\n");
			_needsComma = false;
			for (int i = 0; i < list.Count; i++)
			{
				T t = list[i];
				Serialize(null,ref t);
			}
			_out.AppendFormat("]");
		}

		public void Serialize<T>(string attribute, ref Dictionary<string,T> collection) where T : IStreamable, new()
		{
			Comma();
			if(attribute!=null)
				_out.AppendFormat("\"{0}\":\n", attribute);
			_out.AppendFormat("{{\n");
			_needsComma = false;
			foreach (KeyValuePair<string,T>pair in collection)
			{
				T t = pair.Value;
				Serialize(pair.Key,ref t);
				_needsComma = true;
			}
			_out.AppendFormat("}}");
			_needsComma = true;
		}

		public void Serialize(string attribute, Action<string> set, Func<string> get)
		{
			Comma();
			_out.AppendFormat("\"{0}\":\"{1}\"",attribute,get());
		}

		public void Serialize(string attribute, ref string[] list)
		{
			Comma();
			if(attribute!=null)
				_out.AppendFormat("\"{0}\":\n", attribute);
			_out.AppendFormat("[\n");
			for (int i = 0; i < list.Length; i++)
			{
				if (i != 0) _out.Append(",");
				_out.AppendFormat("\"{0}\"",list[i]);
			}
			_out.AppendFormat("]");
		}

		public void Serialize(string attribute, ref List<string> list)
		{
			Comma();
			if(attribute!=null)
				_out.AppendFormat("\"{0}\":\n", attribute);
			_out.AppendFormat("[\n");
			for (int i = 0; i < list.Count; i++)
			{
				if (i != 0) _out.Append(",");
				_out.AppendFormat("\"{0}\"",list[i]);
			}
			_out.AppendFormat("]");
		}

		private void Comma(bool needsComma=true)
		{
			if(_needsComma)
				_out.AppendFormat(",\n");
			_needsComma = needsComma;
		}
		public void Serialize(string attribute, ref string value, bool raw=false)
		{
			Comma();
			if(raw)
				_out.AppendFormat("\"{0}\":{1}",attribute,value);
			else
				_out.AppendFormat("\"{0}\":\"{1}\"",attribute,value);
		}
		
		public void Serialize(string attribute, ref int value)
		{
			Comma();
			_out.AppendFormat("\"{0}\":{1}",attribute,value);
		}

		public void Serialize(string attribute, ref byte value)
		{
			Comma();
			_out.AppendFormat("\"{0}\":{1}",attribute,value);
		}

		public void Serialize(string attribute, ref ulong value)
		{
			Comma();
			_out.AppendFormat("\"{0}\":\"{1}\"",attribute,value);
		}

		public void Serialize(string attribute, ref float value)
		{
			Comma();
			_out.AppendFormat("\"{0}\":{1}",attribute,value);
		}

		public void Serialize(string attribute, ref Vector3 value)
		{
			Comma();
			_out.AppendFormat("\"{0}\":\"{1}\"",attribute,value.ToAccurateString());
		}

		public void Serialize(string attribute, ref Quaternion value)
		{
			Comma();
			_out.AppendFormat("\"{0}\":\"{1}\"",attribute,value.ToAccurateString());
		}

		public void Serialize(string attribute, ref double value)
		{
			Comma();
			_out.AppendFormat("\"{0}\":{1}",attribute,value);
		}

		public void Serialize(string attribute, ref bool value)
		{
			Comma();
			_out.AppendFormat("\"{0}\":{1}",attribute,value?"true":"false");
		}

		public string json => _out.ToString();
	}
}