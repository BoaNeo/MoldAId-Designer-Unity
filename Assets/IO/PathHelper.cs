using System.Collections.Generic;
using System.IO;
using System.Text;

namespace IO
{
	public static class PathHelper
	{
		public static string AppendPath(this string p1, string p2)
		{
			return $"{RemoveTrailingSlash(p1)}/{p2.FixPathSeparator()}";
		}

		public static string RemoveTrailingSlash(this string p1)
		{
			p1 = p1.FixPathSeparator();
			if (p1.EndsWith('/'))
				p1 = p1.Substring(0, p1.Length - 1);
			return p1;
		}

		public static string FixPathSeparator(this string file)
		{
			if (file == null)
				return "";
			return file.Replace('\\', '/');
		}

		public static string LastPathElement(this string path)
		{
			path = path.RemoveTrailingSlash();
			int i = path.LastIndexOf('/');
			if (i > 0)
				return path.Substring(i + 1);
			return path;
		}

		public static bool IsEmptyFolder(this string path)
		{
			if (!Directory.Exists(path))
				return true;
			IEnumerable<string> items = Directory.EnumerateFileSystemEntries(path);
			using (IEnumerator<string> en = items.GetEnumerator())
			{
				return !en.MoveNext();
			}
		}

		public static string[] SplitPath(this string path)
		{
			return path.FixPathSeparator().RemoveTrailingSlash().Split('/');
		}

		public static string MergePath(this string[] path, int start=0,int end=-1)
		{
			StringBuilder sb = new StringBuilder();
			if (end <= start)
				end = path.Length;
			for (int i=start;i<end;i++)
			{
				if (i!=start)
					sb.Append('/');
				sb.Append(path[i].FixPathSeparator());
			}
			return sb.ToString();
		}

		public static string RemoveLastPathElement(this string path)
		{
			path = path.RemoveTrailingSlash();
			int i = path?.LastIndexOf('/') ?? -1;
			if (i < 0)
				return "";
			return path.Substring(0, i);
		}
	}
}