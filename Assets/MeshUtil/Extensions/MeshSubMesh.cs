using System.Collections.Generic;
using MeshUtil.DataStructures;
using UnityEngine;

namespace MeshUtil.Extensions
{
  public static class MeshSubMesh
  {
    private class Group
    {
      public List<Triangle> triangles = new ();
      public HashSet<int> siblings = new ();
    }

    public static List<MeshBuilder> GetSubMeshes(this MeshBuilder mb)
    {
      Tags<int> tags = new ();
      List<Group> groups = new();
      groups.Add(null); // ignore index 0

      void Group(Triangle t, ref int nextGroupId)
      {
        int tag0 = tags.GetTag(t.v0.id);
        int tag1 = tags.GetTag(t.v1.id);
        int tag2 = tags.GetTag(t.v2.id);

        Group group;
        int id = Mathf.Max(tag0,tag1,tag2);
        if (id == 0) // All zero => potential new submesh 
        {
          id = nextGroupId++;
          group = new Group();
          groups.Add(group);
        }
        else
        {
          group = groups[id];
        }

        group.triangles.Add(t);

        void UpdateTag(int vid, int tag, int id)
        {
          if (tag != id)
          {
            if (tag == 0)
              tags.SetTag(vid, id);
            else
            {
              group.siblings.Add(tag);
              groups[tag].siblings.Add(id);
            }
          }
        }

        UpdateTag(t.v0.id, tag0, id);
        UpdateTag(t.v1.id, tag1, id);
        UpdateTag(t.v2.id, tag2, id);
      }

      int newid = 1;
      foreach (Triangle t in mb.GetTriangles())
      {
        Group(t, ref newid);
      }

      List<MeshBuilder> submeshes = new ();

      for (int gidx=1;gidx<groups.Count;gidx++)
      {
        if (groups[gidx]!=null)
        {
          MeshBuilder sub = mb.CreateSubMesh();
          HashSet<int> remaining = new();
          remaining.Add(gidx);
          do
          {
            // The C# default hashset is such a horrible piece of shit design - why does it not have a "RemoveAndReturnAny" or like Javas iterator that actually allow you to remove elements while iterating
            HashSet<int>.Enumerator e = remaining.GetEnumerator();
            if (e.MoveNext())
            {
              int i = e.Current;
              remaining.Remove(i);
              Group g = groups[i];
              groups[i] = null;
              foreach (Triangle t in g.triangles)
                sub.AddTriangle(t.v0, t.v1, t.v2, t.n, t.tag);
              foreach (int sibling in g.siblings)
              {
                if(groups[sibling]!=null)
                  remaining.Add(sibling);
              }
            }
          } while (remaining.Count>0);

          if(sub.triangleCount>0)
            submeshes.Add(sub);
        }
      }

      return submeshes;
    }
  }
}