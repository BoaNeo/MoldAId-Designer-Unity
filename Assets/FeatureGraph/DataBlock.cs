using System;
using Files;
using IO;

namespace FeatureGraph
{
	public delegate void OnDataChanged();

	public interface IDataBlock
	{
		public string name { get; }
		void Load(DataBlockData value);
		DataBlockData Save();
	}

	public class DataBlock<T> : IDataBlock
	{
		public event OnDataChanged onChanged;

		public string name { get; }

		private T _value;

		public DataBlock(string name, T defaultvalue)
		{
			this.name = name;
			_value = defaultvalue;
		}

		public T Get()
		{
			return _value;
		}

		public void Set(T value)
		{
			_value = value;
			onChanged?.Invoke();
		}

		public void Load(DataBlockData from)
		{
			_value = from.ConvertValueTo(_value);
			onChanged?.Invoke();
		}

		public DataBlockData Save()
		{
			return new DataBlockData(_value);
		}
	}

	public interface IDataRef
	{
		public string blockname { get; }
		public bool changed { get; set; }
		public Type valueType { get; }
		bool hasData { get; }
		public object data { get; }
		public IDataBlock NewDataBlock(string name);
		public void UseData(IDataBlock block);
	}
	public class DataRef<T> : IDataRef, IStreamable
	{
		private DataBlock<T> _data;
		private bool _changed;
		private readonly T _default;
		private string _blockname;

		public bool hasData => _data != null;
		public Type valueType => typeof(T);
		public string blockname => _blockname;
		public object data => _data==null?null:(object)_data.Get();
		
		public static implicit operator T(DataRef<T> me) { return me.value; }
		public T value => _data==null?default:_data.Get();

		public DataRef() { }
		public DataRef(T defaultvalue)
		{
			_default = defaultvalue;
		}

		public IDataBlock NewDataBlock(string name)
		{
			Link(new DataBlock<T>(name, _default));
			return _data;
		}
		
		public void UseDataFrom(DataRef<T> other)
		{
			Link(other._data);
		}

		public void UseData(IDataBlock block)
		{
			Link((DataBlock<T>)block);
		}

		private void Unlink()
		{
			if (_data != null)
				_data.onChanged -= OnDataChanged;
		}

		private void Link(DataBlock<T> block)
		{
			Unlink();
			_data = block;
			_blockname = block?.name;
			_changed = true;
			_data.onChanged += OnDataChanged;
		}

		public void Set(T value)
		{
			_data.Set(value);
		}

		private void OnDataChanged()
		{
			_changed = true;
		}

		public bool changed
		{
			get => _changed;
			set => _changed = value;
		}

		// Serializing a DataRef stores only the block name - this is only intended to be used for referencing data stored elsewhere.
		public void Serialize(DataStream data)
		{
			data.Serialize("blockname", ref _blockname);
		}
	}
}