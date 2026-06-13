using System.Collections.Generic;

namespace Utility
{
  public class MemoryPool<T> where T : new()
  {
	  private List<T> _memory = new List<T>();
    private List<T> _free = new List<T>();
    public int count => _memory.Count;
    public int free => _free.Count;

    private object _lock = new object();

    public T Allocate()
    {
	    T obj;
	    lock (_lock)
	    {
		    if (_free.Count > 0)
		    {
			    obj = _free[_free.Count-1];
			    _free.RemoveAt(_free.Count-1);
		    }
		    else
		    {
			    obj = new T();
			    _memory.Add(obj);
		    }
	    }
	    return obj;
    }

    public void Recycle(T curr)
    {
	    lock (_lock)
	    {
		    _free.Add(curr);
	    }
    }
  }
}