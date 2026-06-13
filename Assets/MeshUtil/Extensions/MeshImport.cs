using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MeshUtil.DataStructures;
using UnityEngine;
using Utility;

namespace MeshUtil.Extensions
{
	public static class MeshImport
	{
		public static MeshBuilder Import(this MeshBuilder mb, string path)
		{
			try
			{
				long t = Log.ElapsedTime();

				IEnumerable<Facet> facets = null;
				string ext = Path.GetExtension(path).ToLowerInvariant();
				switch (ext)
				{
					case ".stl":
					{
						switch (GetSTLFileFormat(path))
						{
							case STLFormat.Binary:
								facets = ImportBinarySTL(path);
								break;
							case STLFormat.Text:
								facets = ImportAsciiSTL(path);
								break;
							case STLFormat.Invalid:
								break;
						}
						break;
					}
					default:
						Debug.LogError(string.Format("Unknown file format (.{ext}) at path {path}.", ext, path));
						break;
				}

				t = Log.ElapsedTime(t, "Loaded STL");

				if (facets == null)
				{
					Debug.LogError(string.Format("Invalid file at path {0}.", path));
					return null;
				}

				TrianglesById triangles = mb.GetTrianglesAs<TrianglesById>();
				foreach (Facet f in facets)
				{
					triangles.AddUniqueTriangle(f.a,f.b,f.c,f.normal,0, true);
				}

				t = Log.ElapsedTime(t, $"Imported {path} - {mb.triangleCount} triangles");

				return mb;
			}
			catch (Exception e)
			{
				Debug.LogError(string.Format("Failed importing mesh at path {0}.\n{1}", path, e));
			}

			return null;
			
			STLFormat GetSTLFileFormat(string path)
			{
				// Each facet contains a normal: (3 floats), 3 vertices: (3x3 floats) and AttributeCount: (1 short)
				const int facetSize = 3 * sizeof(float) + 3 * 3 * sizeof(float) + sizeof(short);

				STLFormat format = STLFormat.Invalid;
				if (File.Exists(path))
				{
					long fileSize = new FileInfo(path).Length;

					// The minimum size of an empty ASCII file is 15 bytes.
					if (fileSize > 15)
					{
						format = STLFormat.Text; // Assume text format at this point
						// 80-byte header + 4-byte "number of triangles" for a binary file
						if (fileSize >= 84)
						{
							FileStream file = File.Open(path, FileMode.Open);
							if (file.Seek(80, SeekOrigin.Current) == 80)
							{
								// Read the number of triangles, uint32_t (4 bytes), little-endian
								BinaryReader br = new BinaryReader(file);
								int nTriangles = br.ReadInt32();

								// Verify that file size equals the sum of header + nTriangles value + all triangles
								if (fileSize == (84 + (nTriangles * facetSize)))
								{
									format = STLFormat.Binary;
								}
							}
							file.Close();
						}
					}
				}
				return format;
			}
		}

		public enum STLFormat { Binary, Text, Invalid };
		public enum STLState { EMPTY, SOLID, FACET, OUTER, VERTEX, ENDLOOP, ENDFACET, ENDSOLID }

		struct Facet
		{
			public Vector3 normal;
			public Vector3 a, b, c;

			public Facet(Vector3 normal, Vector3 a, Vector3 b, Vector3 c)
			{
				this.normal = normal;
				this.a = a;
				this.b = b;
				this.c = c;
			}

			public override string ToString()
			{
				return string.Format("{0:F2}: {1:F2}, {2:F2}, {3:F2}", normal, a, b, c);
			}
		}

		private static IEnumerable<Facet> ImportBinarySTL(string path)
		{
			Facet[] facets;

			using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
			{
				using (BinaryReader br = new BinaryReader(fs, new ASCIIEncoding()))
				{
					// read header
					br.ReadBytes(80);

					uint facetCount = br.ReadUInt32();
					facets = new Facet[facetCount];

					for (uint i = 0; i < facetCount; i++)
						facets[i] = GetFacet(br);
				}
			}

			return facets;

			Facet GetFacet(BinaryReader br)
			{
				Facet facet = new Facet(
					GetVector3(br), // Normal
					GetVector3(br), // A
					GetVector3(br), // B
					GetVector3(br) // C
				);

				br.ReadUInt16(); // padding

				return facet;
			}

			Vector3 GetVector3(BinaryReader br)
			{
				return new Vector3(-br.ReadSingle(), -br.ReadSingle(), -br.ReadSingle());
			}
		}

		private static IEnumerable<Facet> ImportAsciiSTL(string path)
		{
			List<Facet> facets = new List<Facet>();

			using (StreamReader sr = new StreamReader(path))
			{
				string line;
				STLState state = STLState.EMPTY;
				int vertex = 0;
				Vector3 normal = Vector3.zero;
				Vector3 a = Vector3.zero, b = Vector3.zero, c = Vector3.zero;
				bool exit = false;

				while (sr.Peek() > 0 && !exit)
				{
					line = sr.ReadLine().Trim();
					state = ReadState(line);

					switch (state)
					{
						case STLState.SOLID:
							continue;

						case STLState.FACET:
							normal = Parse(line);
							break;

						case STLState.OUTER:
							vertex = 0;
							break;

						case STLState.VERTEX:
							// maintain counter-clockwise orientation of vertices:
							if (vertex == 0)
								a = Parse(line);
							else if (vertex == 2)
								c = Parse(line);
							else if (vertex == 1)
								b = Parse(line);
							vertex++;
							break;

						case STLState.ENDLOOP:
							break;

						case STLState.ENDFACET:
							facets.Add(new Facet(normal, a, b, c));
							break;

						case STLState.ENDSOLID:
							exit = true;
							break;

						case STLState.EMPTY:
						default:
							break;
					}
				}
			}

			return facets;
			
			STLState ReadState(string line)
			{
				if (line.StartsWith("solid"))
					return STLState.SOLID;
				if (line.StartsWith("facet"))
					return STLState.FACET;
				if (line.StartsWith("outer"))
					return STLState.OUTER;
				if (line.StartsWith("vertex"))
					return STLState.VERTEX;
				if (line.StartsWith("endloop"))
					return STLState.ENDLOOP;
				if (line.StartsWith("endfacet"))
					return STLState.ENDFACET;
				if (line.StartsWith("endsolid"))
					return STLState.ENDSOLID;
				return STLState.EMPTY;
			}
			
			Vector3 Parse(string str)
			{
				string[] split = str.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
				Vector3 v = new Vector3();

				float.TryParse(split[split.Length-3], out v.x);
				float.TryParse(split[split.Length-2], out v.y);
				float.TryParse(split[split.Length-1], out v.z);

				return -v;
			}
		}
	}
}