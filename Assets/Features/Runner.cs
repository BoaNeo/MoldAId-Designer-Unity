using System.Collections.Generic;
using FeatureGraph;
using Files;
using Gizmos;
using MeshUtil;
using MeshUtil.Extensions;
using PropertySheet;
using StageBar;
using Undo;
using UnityEngine;

namespace Features
{
	public abstract class Runner : Feature
	{
		[ShowProperty(name = "Runner Diameter", unit = "mm", min = 0.5f, max = 3.0f)]
		[FeatureInput] public DataRef<float> diameter { get; } = new(3.2f);

		[ShowProperty(unit = "mm", min = 0.05f, max = 2.0f)]
		[FeatureInput] public DataRef<float> penetration { get; } = new(0.3f);

		[ShowProperty(min = 0.05f, max = 10.0f)]
		[FeatureInput] public DataRef<float> smoothness { get; } = new(0.3f);

		[ShowProperty(min = 0.5f, max = 10.0f)]
		[FeatureInput] public DataRef<float> straightEnds { get; } = new(4.0f);

		[ShowProperty(unit = "mm", min = 0.5f, max = 6.0f)]
		[FeatureInput] public DataRef<float> gateDiameter { get; } = new(2.8f);

		[ShowProperty(unit = "mm", min = 0.1f, max = 10.0f)]
		[FeatureInput] public DataRef<float> gateLength { get; } = new(2.8f);

		public enum GateStyle{Taper,Bell}
		[ShowProperty(values = new []{"Taper","Bell"})]
		[FeatureInput] public DataRef<int> gateStyle { get; } = new(0);

		[FeatureInput] public DataRef<XForm> start { get; } = new(XForm.identity);
		[FeatureInput] public DataRef<XForm> end { get; } = new(XForm.identity);
		[FeatureInput] public DataRef<XForm> startParent { get; } = new(XForm.identity);
		[FeatureInput] public DataRef<XForm> endParent { get; } = new(XForm.identity);

		[FeatureInput] public List<DataRef<XForm>> controls { get; } = new();

		[FeatureOutput] public DataRef<MeshBuilder> output { get; } = new( null );

		/*
		[ShowProperty(order=1000)]
		public void AddControlPoint(IStageContext context)
		{
			if (controls.Count > 3)
				return;
			UndoManager.Append(() =>
			{
				int idx = controls.Count / 2;
				controls.Insert( idx, context.CreateData<XForm>(this, "controlpoint"));
				UpdateControlPoints();
				return idx;
			}, (idx) =>
			{
				controls.RemoveAt(idx);
				UpdateControlPoints();
			});
		}

		[ShowProperty(order=999)]
		public void RemoveControlPoint()
		{
			if (controls.Count == 0)
				return;

			controls.RemoveAt( controls.Count/2 );
			diameter.Set(diameter); // TODO: This is a temp hack because removing from the list doesn't change any DataRefs and thus won't trigger a rebuild
		}
    */

		private Shape _lastPath;
		private Vector3[] _points;

		private void UpdateControlPoints()
		{
			if (controls.Count == 0 || _lastPath==null)
				return;
			float step = _lastPath.count / (float)(controls.Count+1);
			float s = step;
			Debug.Log("Updating Controls");
			for ( int c=0;c< controls.Count;c++)
			{
				DataRef<XForm> ctrl = controls[c];
				Vector3 pt = _lastPath[(int)Mathf.Clamp(s, 0, _lastPath.count-1)];
				Debug.Log($"Updating Control {c} [{ctrl.blockname}] with point at {(int)s} : {pt}");
				ctrl.Set( new XForm(pt,Quaternion.identity) );
				s += step;
			}
		}
		
		public static MeshBuilder BuildRunner(
			XForm startParent,
			XForm start,
			XForm endParent,
			XForm end,
			XForm[] controls,
			float penetration,
			float radius,
			float smoothness,
			float straightEnds,
			float gateRadius,
			float gateLength,
			GateStyle gateStyle,
			float exitRadius,
			float exitLength,
			GateStyle exitStyle)
		{
			Vector3 fwd = start.GetRotation(startParent) * Vector3.forward;
			Vector3 back = end.GetRotation(endParent) * Vector3.forward;

			Vector3[] points = new Vector3[2 + 3 + controls.Length*3 + 3 + 2];
			int i = 0;

			Vector3 startPos = start.GetPosition(startParent);
			Vector3 endPos = end.GetPosition(endParent);
			
			Vector3 fp = startPos + (1 + Mathf.Max(exitLength , radius)) * fwd;
			Vector3 lp = endPos + (1 + Mathf.Max(gateLength , radius)) * back;
			
			points[i++] = startPos - penetration * fwd;
			points[i++] = startPos; // CP
			points[i++] = startPos; // CP
			points[i++] = fp;
			points[i++] = fp + fwd*radius*straightEnds; // CP
			for (int j = 0; j < controls.Length; j++)
			{
				Vector3 p = controls[j].position;

				Vector3	b = points[i - 2] - p;
				float	blen = b.magnitude;

				Vector3 f = lp - p;
				if(j<controls.Length-1)
					f = controls[j+1].position - p;
				float flen = f.magnitude;

				Vector3 tangent = controls[j].forward;

				points[i++] = p + smoothness * blen * tangent; // CP
				points[i++] = p;
				points[i++] = p - smoothness * flen * tangent; // CP
			}
			points[i++] = lp + back*radius*straightEnds; // CP
			points[i++] = lp;
			points[i++] = endPos; // CP
			points[i++] = endPos; // CP
			points[i++] = endPos - penetration * back;

			Shape path = new Shape();
			path.AddBezierPath(points);

			RebuildExitSegment(penetration, radius, exitLength, exitStyle, path);
			RebuildGateSegment(penetration, radius, gateLength, gateStyle, path);

			int sides = Shape.ArcLenToSegments(radius, PreferencesFile.current.arcLength); 

			float length = path.CalculateLength();
			// TODO: Fix this mess! Should use a MeshBuilder to do an extrusion of the path
			var meshInfo = Geometry.GenerateTubeMesh(path.shape, sides, true, l =>
			{
				if (l>length-(penetration+gateLength))
				{
					l = Mathf.Clamp(1 - ((length - l) - penetration) / gateLength,0,1);
					return GetGateRadius(l, radius, gateRadius, gateStyle);
				}

				if (l <= (penetration + exitLength))
				{
					l = 1.0f-Mathf.Clamp((l - penetration) / exitLength,0,1);
					return GetGateRadius(l, radius,exitRadius, exitStyle);
				}

				return radius;
			});

			return new MeshBuilder().CloneFrom(meshInfo);
		}

		private static Vector3 NormalOf(Vector3 v, Vector3 d)
		{
			float scale = v.magnitude;
			if (scale < 0.001f)
				return d;
			v /= scale;
			Vector3 n = Vector3.Cross(d, v).normalized;
			return Vector3.Cross(v, n);
		}

		private static float GetGateRadius(float l, float radius, float gateRadius, GateStyle gateStyle)
		{
			switch (gateStyle)
			{
				case GateStyle.Taper:
					return  radius - (radius - gateRadius) * l;
				case GateStyle.Bell:
					float r = radius - gateRadius;
					if (l < 0.5f)
						return gateRadius + r * Mathf.Cos(l * Mathf.PI);
					l = Mathf.Clamp(l-0.5f,0,0.5f);
					return radius - r * Mathf.Cos(l * Mathf.PI);
			}
			return 1.0f;
		}

		private static void RebuildGateSegment(float penetration, float radius, float gateLength, GateStyle gateStyle, Shape path)
		{
			float step=0.25f;
			switch (gateStyle)
			{
				case GateStyle.Taper: step = gateLength; break;
				case GateStyle.Bell: step = radius/10; break;
			}

			float l = 0;
			for (int j = path.count-2; j >=0 ; j--)
			{
				l += (path[j] - path[j + 1]).magnitude;
				if (l > penetration + gateLength)
				{
					Vector3 gatestart = path[j];
					Vector3 gatedirection = (path[path.count - 1] - gatestart).normalized;
					
					while(j<path.count)
						path.Remove(j);

					gatestart += gatedirection * (l - (penetration + gateLength));
					path.Add(gatestart);
					for (l = 0; l < gateLength; l += step)
						path.Add(gatestart + gatedirection * (l+step));
					path.Add(gatestart + gatedirection * (l + penetration));
					break;
				}
			}
		}
		private static void RebuildExitSegment(float penetration, float radius, float exitLength, GateStyle exitStyle, Shape path)
		{
			if (exitLength <= 0)
				return;

			float step=0.25f;
			switch (exitStyle)
			{
				case GateStyle.Taper: step = exitLength; break;
				case GateStyle.Bell: step = radius/10; break;
			}

			float l = 0;
			for (int j = 0; j <path.count-1 ; j++)
			{
				l += (path[j + 1] - path[j]).magnitude;
				if (l > penetration + exitLength)
				{
					Vector3 gatestart = path[0];
					Vector3 gatedirection = (path[j+1] - gatestart).normalized;

					j++;
					while(j>0)
						path.Remove(j--);

					j = 1;
//						path.Insert(j,gatestart);
					for (l = 0; l < exitLength; l += step)
						path.Insert(j++, gatestart + gatedirection * (l+step));
					path.Insert(j++, gatestart + gatedirection * (l + penetration));
					break;
				}
			}
		}

		public void DebugDraw()
		{
			if (_points != null)
			{
				UnityEngine.Gizmos.color = Color.green;
				for (int i = 0; i < _points.Length-1; i++)
				{
					UnityEngine.Gizmos.DrawLine(_points[i],_points[i+1]);
					UnityEngine.Gizmos.DrawSphere(_points[i], 0.1f);
				}
				UnityEngine.Gizmos.DrawSphere(_points[_points.Length-1], 0.25f);
			}
		}
	}
}