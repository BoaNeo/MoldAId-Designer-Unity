using System.Collections.Generic;

namespace MeshUtil.DataStructures
{
	public class Tags<T>
	{
		private Dictionary<int,T> _tags = new ();

		public void SetTag(int idx, T tag)
		{
			_tags[idx] = tag;
		}

		public T GetTag(int idx)
		{
			_tags.TryGetValue(idx, out T tag);
			return tag;
		}
	}
}