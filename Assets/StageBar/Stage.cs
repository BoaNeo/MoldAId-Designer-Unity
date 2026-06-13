using System;
using System.Collections.Generic;
using FeatureGraph;
using Files;
using Gizmos;
using MeshUtil;
using UnityEngine;
using Visuals;

namespace StageBar
{
	public interface IStageContext
	{
		ProjectFile GetProject();
		public void SaveProject();
		
		DataRef<T> CreateData<T>(Feature creator, string name);

		T AddFeature<T>(string name) where T:Feature, new();
		void AddFeature(string name, Feature f);
		void RemoveFeature<T>(T getFeature) where T:Feature;

		T GetOrCreateFeature<T>(string name) where T:Feature, new();
		T GetFeature<T>(string name=null) where T:Feature;
		List<T> GetFeatures<T>() where T:Feature;

		void SelectFeature(Feature feature);
		void SelectOneOf(Feature[] features, bool[] showDeleteButtons=null);
		Feature GetSelectedFeature();
		
		void DisableAllFeatures();
		T SetEnabled<T>(bool enabled, string name=null) where T:Feature;
		List<T> SetEnabledAll<T>(bool enabled) where T:Feature;

		Visual SetVisual(bool visible, DataRef<MeshBuilder> mesh, DataRef<XForm> xform,Visual.Mode mode, Color color, bool selected=false, Main.SelectionPriority priority=Main.SelectionPriority.Ignore, Action onSelect=null, Action onDebugDraw=null);
		void HideAllVisuals();

		Gizmo SetGizmo(bool visible, DataRef<XForm> xformref, GizmoHandleFlags handles, GizmoSpace space=GizmoSpace.Local, Action<XForm, bool> feedback=null);
		void HideAllGizmos();

		void ShowPropertiesOf<T>(string title, T target, Action<T> onDelete);

		void PickFromScene(string header, string[] tagPriorities, Func<RaycastHit, bool> action);

		Marker PushMarker(Marker.Flags flags=0);
		Marker PopMarker();
		void ClearMarkers();

		// TODO: This should probably have its own UI like the cross section tool, but currently don't have a lot to put in it, so...
		public void MeasureWithTags(string[] tags );

		void ConfigurePrintBox(PrintBox.PrintBoxMode mode);
	}
	
	public abstract class Stage
	{
		public IStageContext context { get; set; }
		public abstract string name { get; }
		public abstract void BuildUI(StageBarUI ui);
		public abstract void Activate();
		public abstract void DeActivate();
	}
}