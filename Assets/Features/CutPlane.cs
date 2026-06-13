using System.Collections.Generic;
using FeatureGraph;
using Gizmos;
using MeshUtil;
using MeshUtil.DataStructures;
using MeshUtil.Extensions;
using PropertySheet;
using UnityEngine;
using Utility;

namespace Features
{
	public class CutPlane : Feature 
	{
		[ShowProperty(name="Mold Separation Distance", unit = "mm")]
		[FeatureInput] public DataRef<float> placementDistance { get; } = new(10);

		[ShowProperty(name="Cutplane Offset from Origin", unit = "mm")]
		public float offset { get => position.value.position.z; set => position.Set(position.value.WithPosition(new Vector3(0,0, value))); }

		[ShowProperty(name="Cutplane Tilt", unit = "deg")]
		public float tilt
		{
			get => position.value.rotation.eulerAngles.x;
			set
			{
				Vector3 e = position.value.rotation.eulerAngles;
				position.Set(position.value.WithRotation(Quaternion.Euler(value, e.y, e.z)));
			}
		}

		[ShowProperty(name="Cutplane Rotation", unit = "deg")]
		public float rotation
		{
			get => position.value.rotation.eulerAngles.y;
			set
			{
				Vector3 e = position.value.rotation.eulerAngles;
				position.Set(position.value.WithRotation(Quaternion.Euler(e.x,e.y, value)));
			}
		}

		[ShowProperty]
		[FeatureInput] public DataRef<bool> showMoldOpen { get; } = new(true); 

		[ShowProperty]
		[FeatureInput] public DataRef<bool> showCutplane { get; } = new(true);

		[ShowProperty(name="Move Cutplane Up 0.1mm")]
		public void MoveUp() { offset = Mathf.Clamp( offset+0.1f, layerThickness, input.value.GetBounds().extents.z*2-0.1f); }
		
		[ShowProperty(name="Move Cutplane Down 0.1mm")]
		public void MoveDown() { offset = Mathf.Clamp( offset-0.1f, layerThickness, input.value.GetBounds().extents.z*2-0.1f); }

		[FeatureInput] public DataRef<XForm> position { get; } = new(XForm.identity);
		
		[FeatureInput] public DataRef<float> layerThickness { get; } = new(0.05f);

		[FeatureInput] public DataRef<MeshBuilder> input { get; } = new(null);

		// Outputs:

		[FeatureOutput] public DataRef<MeshBuilder> plane { get; } = new(null);

		[FeatureOutput] public DataRef<MeshBuilder> mesh1 { get; } = new(null);

		[FeatureOutput] public DataRef<MeshBuilder> mesh2 { get; } = new(null);
		
		const int FILLED_FACE = 0x40;
		const int REMOVED_FACE = 0x80;

		public static IEnumerator<IYield> Build(bool changing,
			float placementDistance,
			bool showMoldOpen,
			bool showCutplane,
			XForm position,
			float layerThickness,
			MeshBuilder input,
			DataRef<MeshBuilder> plane,
			DataRef<MeshBuilder> mesh1,
			DataRef<MeshBuilder> mesh2)
		{
			if (input == null || changing)
				yield break;
			
			long t0 = Log.ElapsedTime();

//			Matrix4x4 org = input.transform;
//			input.transform = input.transform * xform.worldToLocalMatrix;

			MeshBuilder m1 = new();
			MeshBuilder m2 = new();

//			Action<Action> slice = input.SliceGPU(position.worldToLocalMatrix, 1, m1, m2);
//			yield return Until.CallsBackAfterRunningOnMain( slice );
			input.Slice(position.worldToLocalMatrix, 1, out m1, out m2);

			// TODO: I've removed these because the MeshBuilder now always gets the GPU buffers immediately, so there is never a need for this.
			// Ideally this should "just work" - I.e. no need to call GetTriangle ahead of time, and when/if they are called they should execute on main
//			m1.GetTriangles(); // This sucks donkey balls but I need to make sure buffers are read before we leave the main thread
//			m2.GetTriangles();

			t0 = Log.ElapsedTime(t0, "Sliced");

			yield return Until.RunningInBackground;

//			input.Slice( position.worldToLocalMatrix, 1, out MeshBuilder m1, out MeshBuilder m2);

			t0 = Log.ElapsedTime(t0, "Waited on thread switch");

			m1.FillTrapezoid( 1, Vector3.forward, FILLED_FACE);
			t0 = Log.ElapsedTime(t0, "Filled Forward");

			m2.FillTrapezoid( 1, Vector3.back, FILLED_FACE);
			t0 = Log.ElapsedTime(t0, "Filled Back");

			List<MeshBuilder> subs1 = m1.GetSubMeshes();
			List<MeshBuilder> subs2 = m2.GetSubMeshes();

			t0 = Log.ElapsedTime(t0, $"Extracted {subs1.Count} and {subs2.Count} Submeshes");

			m1 = GetMajorPart(input,subs1);
			m2 = GetMajorPart(input,subs2);

			t0 = Log.ElapsedTime(t0, "Identified Major Parts");

			List<MeshBuilder> m1match = new List<MeshBuilder>();
			List<MeshBuilder> m2match = new List<MeshBuilder>();
			FindAndPrepareIslands(m1, subs2, m1match, m2match);
			FindAndPrepareIslands(m2, subs1, m2match, m1match);
			
			yield return Until.RunningOnMainThread;
			
			foreach(MeshBuilder m in m1match)
				m1.AppendGPU(m, Matrix4x4.identity);
			foreach(MeshBuilder m in m2match)
				m2.AppendGPU(m, Matrix4x4.identity);

			t0 = Log.ElapsedTime(t0, $"Appended Minor Parts");

			Matrix4x4 xformInv = position.localToWorldMatrix;
			m1 = m1.TransformGPU(xformInv);

			Bounds inputBounds = input.GetBounds();

			if (showMoldOpen)
			{
				Vector3 p = new Vector3(2*inputBounds.extents.x+placementDistance,0, -2*inputBounds.extents.z);
				Quaternion q = Quaternion.Euler(180, 0, 0);
				Matrix4x4 m = Matrix4x4.Rotate(q)*Matrix4x4.Translate(p);
				m2 = m2.TransformGPU( m * xformInv);
			}
			else
			{
				m2 = m2.TransformGPU(xformInv);
			}

			float s = Mathf.Max(inputBounds.extents.x, inputBounds.extents.y);

			if (plane.value == null)
			{
				MeshBuilder mb = new MeshBuilder();
				mb.AddQuad(new Vector3(-s,s,0), new Vector3(s,s,0),new Vector3(s,-s,0),new Vector3(-s,-s,0), Vector3.forward);
				mb.AddQuad(new Vector3(s,s,0), new Vector3(-s,s,0),new Vector3(-s,-s,0),new Vector3(s,-s,0), Vector3.back);
				plane.Set(mb);
			}
			
			t0 = Log.ElapsedTime(t0, "Finished Up");

			mesh1.Set(m1);
			mesh2.Set(m2);
		}

    static MeshBuilder GetMajorPart(MeshBuilder org, List<MeshBuilder> list)
    {
	    MeshBuilder major = null;
	    int majorIndex = 0;
	    float majorMaxExt = 0;
      for ( int i=0;i<list.Count;i++)
      {
	      MeshBuilder sub = list[i];
	      Bounds bb = sub.GetBounds();
	      float ext = Mathf.Max(bb.extents.x, bb.extents.y, bb.extents.z);
	      if (ext > majorMaxExt)
	      {
		      major = sub;
		      majorIndex = i;
		      majorMaxExt = ext;
	      }
      }
      list.RemoveAt(majorIndex);
      return major;
    }

    static void FindAndPrepareIslands(MeshBuilder major, List<MeshBuilder> minors, List<MeshBuilder> matches, List<MeshBuilder> misses)
    {
	    long t = Log.ElapsedTime();
	    BoundingSphereGrid raycaster = major.GetTrianglesAs<BoundingSphereGrid>();
	    t = Log.ElapsedTime(t, "Created bounding sphere grid");

	    Ray ray = new Ray();
	    foreach (MeshBuilder minor in minors)
	    {
		    bool attach = false;
		    foreach (Triangle triangle in minor.GetTriangles())
		    {
			    if (triangle.tag == FILLED_FACE )
			    {
				    attach = true;
				    ray.origin = triangle.mid - 0.01f * triangle.n;
				    ray.direction = triangle.n;
				    if (raycaster.RayCast(ray, t => (t.tag&FILLED_FACE)!=0, out BoundingSphereGrid.TriangleHit hit))
				    {
					    hit.tag |= REMOVED_FACE;
				    }
			    }
		    }

		    if (attach)
		    {
			    major.RemoveTriangles( (triangle) => (raycaster.GetTriangle(triangle.id).tag&REMOVED_FACE)!=0);
			    minor.RemoveTriangles( triangle => (triangle.tag & FILLED_FACE)!=0);
			    matches.Add(minor);
		    }
		    else
		    {
			    misses.Add(minor);
		    }
	    }
	    t = Log.ElapsedTime(t, "Removed inner faces and identified parts to attach");
    }
    
/*
    static void AttachMinorParts(MeshBuilder target, MeshBuilder source, List<MeshBuilder> subs)
    {
      if (target.Append(subs, FILLED_FACE, source))
	      RemoveInnerTriangles(target);
    }
 
    static void RemoveInnerTriangles(MeshBuilder mb)
    {
      long t = Log.ElapsedTime();
      BoundingSphereGrid raycaster = mb.GetTrianglesAs<BoundingSphereGrid>();
      t = Log.ElapsedTime(t, "Created bounding sphere grid");

      Ray ray = new Ray();
      foreach (BoundingSphereGrid.TriangleHit triangle in raycaster.triangles)
      {
        if (triangle.tag == FILLED_FACE )
        {
	        ray.origin = triangle.mid - 0.01f * triangle.n;
		      ray.direction = triangle.n;
	        if (raycaster.RayCast(ray, t => (t.tag&FILLED_FACE)!=0 && Vector3.Dot(t.n, triangle.n) < 0, out BoundingSphereGrid.TriangleHit hit))
	        {
		        triangle.tag |= REMOVED_FACE;
		        hit.tag |= REMOVED_FACE;
	        }
        }
      }
      t = Log.ElapsedTime(t, "Collected inner faces");

      mb.RemoveTriangles( (triangle) => (raycaster.GetTriangle(triangle.id).tag&REMOVED_FACE)!=0);

      t = Log.ElapsedTime(t, "Removed inner faces");
    }*/
	}
}