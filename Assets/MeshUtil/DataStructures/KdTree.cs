using System;
using UnityEngine;
using Utility;

namespace MeshUtil.DataStructures
{
	public class KdTree
	{
		private class KdNode
		{
			internal Vertex v;
			internal KdNode lt;
			internal KdNode gte;
			internal bool Traverse(Func<Vertex, bool> action)	
			{
				if (lt != null)
					if (!lt.Traverse(action))
						return false;
				if (!action(v))
					return false;
				if (gte != null)
					if (!gte.Traverse(action))
						return false;
				return true;
			}
		}

		public int count => _count;

		private KdNode _root;
		private int _count;

		public Vertex AddOrGet(Vector3 pt, int tag)
		{
			return InternalFind(pt, tag, true).v;
		}

		public bool Contains(Vector3 pt)
		{
			return InternalFind(pt, 0,false)!=null;
		}

		public bool Traverse( Func<Vertex, bool> node )
		{
			if (_root == null)
				return false;
			return _root.Traverse(node);
		}

		private KdNode InternalFind(Vector3 pt, int tag, bool add)
		{
			int depth = 0;
			ref KdNode me = ref _root;
			
			while (true)
			{
				if(me==null)
				{
					if (add)
					{
						me = new KdNode();
						me.v = new Vertex(_count++, pt, tag);
					}
					return me;
				}

				if (me.v.point.AlmostEqual(pt))
				{
					if(!add)
						Debug.Log($"Searching for {pt} found {me.v.point}");
					return me;
				}

				float d = 0;
				switch (depth % 3)
				{
					case 0: d = me.v.point.x - pt.x; break;
					case 1: d = me.v.point.y - pt.y; break;
					case 2: d = me.v.point.z - pt.z; break;
				}

				if (d < 0)
					me = ref me.lt;
				else 
					me = ref me.gte;
				depth++;
			}
		}
	}
}