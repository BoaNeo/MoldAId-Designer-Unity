using System;
using System.Collections.Generic;
using System.IO;
using Dialogs;
using Dialogs.BugReport;
using Dialogs.LicenseDialog;
using FeatureGraph;
using Features;
using Files;
using Gizmos;
using IO;
using Menu;
using MeshUtil;
using PropertySheet;
using StageBar;
using Stages;
using UIComponents;
using Undo;
using UnityEngine;
using UnityEngine.EventSystems;
using Visuals;

public class Main : MonoBehaviour, WorldInputManager.IWorldInputHandler, IStageContext
{
	[SerializeField] private LicenseSpringSDK _licenseManager;
	[SerializeField] private StageBarManager _stageManager;
	[SerializeField] private PropertySheetPanel _propertySheet;
	[SerializeField] private LayerMask _layerFeatures;
	[SerializeField] private FeatureManager _features;
	[SerializeField] private GizmoManager _gizmos;
	[SerializeField] private MarkerManager _markers;
	[SerializeField] private Transition _sideBar;
	[SerializeField] private MenuBar _menuBar;
	[SerializeField] private VisualManager _visuals;
	[SerializeField] private PrintBox _printBox;

	private ProjectFile _project;
	private Visual _previousVisual;

	private void OnApplicationQuit()
	{
		ComputeShaderExtension.DisposeAllBuffers();
	}

	private void Awake()
	{
		Application.wantsToQuit += WantsToQuit;
			
		_menuBar.BeginUpdate(RectTransform.Axis.Horizontal);
		_menuBar.EndUpdate();

		_licenseManager.InitializeSDK();

		//IfHasLicense(() =>
		//{
		PreferencesFile.Load();

		// Add World Input handlers in order of importance - CameraManager should always be last!
		WorldInputManager.instance.RegisterInputHandler( _gizmos );
		WorldInputManager.instance.RegisterInputHandler( this );
		WorldInputManager.instance.RegisterInputHandler( CameraManager.instance );

		_menuBar.BeginUpdate(RectTransform.Axis.Horizontal);
		_menuBar.AddSubMenu("File", filemenu =>
		{
			filemenu.AddItem("New", ShowNewProjectDialog);
			filemenu.AddItem("Load", ShowOpenProjectDialog);
			filemenu.AddSpacing();
			filemenu.AddItem("Reload", ReloadProject);
			filemenu.AddSpacing();
			filemenu.AddItem("Save", true,false, KeyCode.S, ()=>SaveProject(false));
			filemenu.AddItem("Save As...", ()=>SaveProject(true));
			filemenu.AddSpacing();
			filemenu.AddItem("Quit", Quit);
		});
		_menuBar.AddSubMenu("Edit", editmenu =>
		{
			editmenu.AddItem("Undo", true, false, KeyCode.Z, UndoManager.Undo );
			editmenu.AddItem("Redo", true, true, KeyCode.Z, UndoManager.Redo);
		});
		_menuBar.AddSubMenu("View", viewmenu =>
		{
			viewmenu.AddItem("Home", true, false, KeyCode.Keypad5, CameraManager.instance.OnResetFocus );
			viewmenu.AddSpacing();
			viewmenu.AddItem("Perspective", false,false,KeyCode.KeypadPlus, CameraManager.instance.OnSetCameraPerspective );
			viewmenu.AddItem("Orthographic", false,false,KeyCode.KeypadMinus, CameraManager.instance.OnSetCameraOrth );
			viewmenu.AddSpacing();
			viewmenu.AddItem("Top", false,false,KeyCode.Keypad8, CameraManager.instance.SetViewTop );
			viewmenu.AddItem("Bottom", false,false,KeyCode.Keypad2, CameraManager.instance.SetViewBottom );
			viewmenu.AddItem("Left", false,false,KeyCode.Keypad4,CameraManager.instance.SetViewLeft );
			viewmenu.AddItem("Right",false,false,KeyCode.Keypad6, CameraManager.instance.SetViewRight );
			viewmenu.AddItem("Front", false,false,KeyCode.Keypad3, CameraManager.instance.SetViewFront );
			viewmenu.AddItem("Back",false,false,KeyCode.Keypad9, CameraManager.instance.SetViewBack );
		});
		_menuBar.AddSubMenu("Settings", settingsmenu =>
		{
//					settingsmenu.AddItem("Mold Library...", () => { });
//					settingsmenu.AddItem("Printers...", () => { });
//					settingsmenu.AddItem("Injection Machines...", () => { });
//					settingsmenu.AddSpacing();
			settingsmenu.AddItem("Preferences...", ShowPreferences );
		});
		_menuBar.AddSubMenu("About", aboutmenu =>
		{
			aboutmenu.AddItem( $"Mold Designer V{Application.version}", () => {{ }});
			aboutmenu.AddItem( $"{(_licenseManager.IsTrial?"Trial ":"")}License Key: {_licenseManager.LicenseKey}", () => { });
			if(_licenseManager.ExpiresInDays!=int.MaxValue)
				aboutmenu.AddItem( $"Expires in {_licenseManager.ExpiresInDays} days", () => {{ }});
			aboutmenu.AddItem( "Submit Bug Report", () => { FindObjectOfType<UserReportingScript>().CreateUserReport( _features ); });
		});
		_menuBar.EndUpdate();

		_stageManager.SetContext( this );

		_stageManager.AddStage(new PartStage());
		_stageManager.AddStage(new MoldStage());
		_stageManager.AddStage(new CutPlaneStage());
		_stageManager.AddStage(new InletStage());
		_stageManager.AddStage(new OutletStage());
		_stageManager.AddStage(new GuidesStage());
		_stageManager.AddStage(new ExportStage());
		_stageManager.RefreshStages();

		PropertyUIAction.context = this; // TODO: This is a bit hacky. Considered passing context to propertysheet but that also felt icky - at least this way it's limited to the action button.

//				DialogManager.Show<MessageBox>().WithMessage($"Mold Designer V{Application.version} BETA", "This software is currently in beta testing.\n\nBeta software, while functionally complete and generally able to perform its primary function, may be subject to change and will likely contain bugs, inconsistencies and may require workarounds for some features to work as intended.\n\nPlease use the bug reporting feature found in the top menubar to report any issue you encounter.\n\nThank You!",
//					() =>
//					{
		string message = PreferencesFile.current.Validate();
		if (message!=null)
			DialogManager.Show<MessageBox>().WithMessage("Invalid Preferences!", message, ShowPreferences);
		else
			ShowOpenProjectDialog();
//					});
//			}, true);
	}

	private void Update()
	{
		if (_features.Rebuild() )
		{
			Debug.Log("Updating UI Post Build!");
			SelectFeature( _selectedFeature );
		}

		_menuBar.ProcessShortcuts();
			
		ComputeShaderExtension.EmptyTrash();
	}

	private void ClearAllFeatures( Action whenReady )
	{
		_project = null;
		_features.Clear(() =>
		{
			UndoManager.Clear();
			_stageManager.SelectStage(null);
			_visuals.Clear();
			whenReady();
		});
	}

	private void ShowPreferences()
	{
		_sideBar.SetVisible(true);
		_propertySheet.ShowProperties("Preferences",PreferencesFile.current,null);
	}

	private bool WantsToQuit()
	{
		Quit();
		return _okToQuit;
	}
		
	private void Quit()
	{
		IfIsOkToClearProject( () =>
		{
			_okToQuit = true;
			Application.Quit(0);
		});
	}

	public ProjectFile GetProject()
	{
		return _project;
	}

	public void SaveProject()
	{
		SaveProject(false);
	}
		
	private void SaveProject(bool saveAs)
	{
		if (_project != null)
		{
			if (string.IsNullOrWhiteSpace(_project.path))
				saveAs = true;
				
			if(saveAs)
				DialogManager.Show<SaveProjectDialog>().WithProject(_project, DoSave);
			else
				DoSave( Path.GetDirectoryName(_project.path));

			void DoSave(string path)
			{
				if (string.IsNullOrWhiteSpace(path))
				{
					DialogManager.Show<MessageBox>().WithMessage("No Folder Selected", "Please specify a valid folder to save the project to.", () => { });
				}
				else if (saveAs && !path.IsEmptyFolder())
				{
					DialogManager.Show<MessageBox>().WithQuery("Folder Already Exist", "Would you like to replace the contents of this folder?", yes =>
					{
						if(yes)
							DoSaveForReal(path, true);
					});
				}
				else
					DoSaveForReal(path, false);

				void DoSaveForReal(string path, bool overwrite)
				{
					if(!overwrite)
						Directory.CreateDirectory(path);
					_features.GetFeature<ImportedPart>().RefreshLocalPath(path);
					_features.SerializeFeatureData(_project);
					string file = path.AppendPath("Project.afp");
					if (overwrite && File.Exists(file))
						File.Delete(file);
					string name = path.LastPathElement();
					if(!string.IsNullOrEmpty(name))
						_project.name = name;
					_project.Save(file);
					UndoManager.MarkSaved();
					RecentFile.AddRecentFile(_project);
				}
			}
		}
	}

	private void ReloadProject()
	{
		IfIsOkToClearProject(() =>
		{
			LoadProject(_project);
		});
	}

	private void ShowNewProjectDialog()
	{
		IfIsOkToClearProject(() =>
		{
			DialogManager.Show<NewProjectDialog>().Setup(LoadProject);
		});
	}

	private void ShowOpenProjectDialog()
	{
		IfIsOkToClearProject(()=>
		{
			DialogManager.Show<ProjectDialog>().Setup(LoadProject);
		});
	}

	private void IfIsOkToClearProject(Action then)
	{
		if (_project != null && UndoManager.HasChanges())
		{
			DialogManager.Show<MessageBox>().WithQuery("Close Project?","Do you want to close the current project and lose changes?", yes =>
			{
				if (yes) 
					then();
			});
			return;
		}
		then();
	}
/*
		private void IfHasLicense(Action callback, bool showTrialMessage = false)
		{
			_licenseManager.CheckLicense();
			
			if (!_licenseManager.HasLicense || _licenseManager.IsExpired)
			{
				_menuBar.gameObject.SetActive(false);
				DialogManager.Show<LicenseDialog>().WithManager(_licenseManager, licenseOk =>
				{
					if (licenseOk)
						callback();
					else
						Application.Quit();
				});
				return;
			}
			
			if (_licenseManager.HasLicense && _licenseManager.IsTrial && showTrialMessage)
			{
				DialogManager.Show<MessageBox>().WithMessage("Trial License!", $"Your trial license expires in {_licenseManager.DaysRemaining} days", () =>
				{
					callback();
				});
			}
			else
				callback();
		}
*/		
	private void LoadProject(ProjectFile project)
	{
		if (project != null)
		{
			//IfHasLicense(() =>
			//{
			ClearAllFeatures(() =>
			{
				try
				{
					project = _features.VersionCheck(project, out string message);

					if (message != null)
					{
						DialogManager.Show<MessageBox>().WithMessage("Project Version Error!", message, () =>
						{
							ContinueLoading(project);
						});
					}
					else
					{
						ContinueLoading(project);
					}

					void ContinueLoading(ProjectFile project)
					{
						_project = project;
						if (_project != null)
						{
							_features.LoadFeatures(_project);
							UndoManager.MarkSaved();

							ImportedPart part = _features.GetFeature<ImportedPart>("Part");
							if (part == null)
							{
								part = _features.AddFeature<ImportedPart>( "Part");
								part.originalPath.Set(project.partPath);
								part.transform.Set( new XForm(new Vector3(0,0, 0.5f * _project.printer.volume.z), Quaternion.identity));
							}
							part.RefreshLocalPath(PathHelper.RemoveLastPathElement(project.path));

							_printBox.Configure(_project.printer);
								
							SelectFeature(_features.GetFeature<ImportedPart>());
						}
						if(_project==null)
							ShowOpenProjectDialog();
						else
							CameraManager.instance.FocusOnBoundingBox( new Bounds( new Vector3(0,0,_project.printer.volume.z/2.0f) ,_project.printer.volume ) );
					}
				}
				catch (Exception e)
				{
					DialogManager.Show<MessageBox>().WithMessage("Failed to Load Project", $"Opening '{project.name}' failed with\n{e.Message}", () => {});
					Debug.LogError(e);
				}
			});
			//});
		}
	}
		
	private Func<RaycastHit, bool> _onPicked;
	private string[] _pickPriorities;
	private string[] _selectPriorities;
	private Toast _toast;
	private Dictionary<string, SelectionPriority> _selectableObjectPriorities = new Dictionary<string, SelectionPriority>();
	private int _lastRefreshCount;
	private Feature _selectedFeature;
	private bool _okToQuit;

	public LayerMask GetSelectableLayerMask()
	{
		return _layerFeatures;
	}

	public string[] GetTagPriorities()
	{
		return _onPicked != null ? _pickPriorities : _selectPriorities;
	}

	public enum SelectionPriority { Primary, Secondary, Ignore }
	public void SetSelectionPriorityForVisualName(string tag, SelectionPriority prio)
	{
		_selectableObjectPriorities[tag] = prio;
		_selectPriorities = new string[_selectableObjectPriorities.Count];
		int i = 0;
		foreach (KeyValuePair<string, SelectionPriority> pair in _selectableObjectPriorities)
		{
			switch (pair.Value)
			{
				case SelectionPriority.Ignore:
					_selectPriorities[i++] = $"!{pair.Key}";
					break;
				case SelectionPriority.Secondary:
					_selectPriorities[i++] = $"~{pair.Key}";
					break;
				default:
					_selectPriorities[i++] = pair.Key;
					break;
			}
		}
	}

	public bool GetNeedsHoverEvent() { return _onPicked!=null;}

	public bool OnHover(RaycastHit hit)
	{
		if (_onPicked != null)
			return _markers.UpdateCurrent(hit);
		return false;
	}
	public void OnDrag(RaycastHit obj, Vector2 deltaScreen) { }
	public bool OnRelease(RaycastHit source, RaycastHit target) { return true; }
	public bool OnSelect(RaycastHit hit, PointerEventData evt)
	{
		if (_onPicked != null)
		{
			Func<RaycastHit,bool> func = _onPicked;
			Toast toast = _toast;
			// Clear refs in case the callback opens another picker
			_toast = null; 
			_onPicked = null;
			if (func(hit))
			{
				if(_toast==null)
					ClearMarkers();
				toast.Hide();
			}
			else
			{
				// If the picker didn't like the pick. restore the refs and try again
				_toast = toast;
				_onPicked = func;
			}
		}
			
		GameObject obj = hit.collider?.gameObject;
		if (obj != null)
		{
			VisualChild visual = obj.GetComponent<VisualChild>();
			if (visual)
			{
				if(visual.OnSelect( evt ))
					return true;
			}
		}
		return false;
	}

	DataRef<T> IStageContext.CreateData<T>(Feature creator, string name)
	{
		return _features.CreateData<T>(creator, name);
	}

	T IStageContext.AddFeature<T>(string featureName)
	{
		return _features.AddFeature<T>(featureName);
	}

	void IStageContext.AddFeature(string featureName, Feature feature)
	{
		_features.AddFeature(featureName,feature);
	}

	T IStageContext.GetOrCreateFeature<T>(string featureName)
	{
		return _features.GetOrCreateFeature<T>(featureName);
	}
		
	T IStageContext.GetFeature<T>(string featureName)
	{
		return _features.GetFeature<T>(featureName);
	}

	List<T> IStageContext.GetFeatures<T>()
	{
		return _features.GetFeatures<T>();
	}

	T IStageContext.SetEnabled<T>(bool enabled, string name)
	{
		T f = _features.GetFeature<T>(name);
		if (f != null)
			f.enabled = enabled;
		return f;
	}
		
	List<T> IStageContext.SetEnabledAll<T>(bool enabled)
	{
		List<T> fs = _features.GetFeatures<T>();
		foreach (T f in fs)
			f.enabled = enabled;
		return fs;
	}

	Feature IStageContext.GetSelectedFeature()
	{
		return _selectedFeature;
	}

	public void RemoveFeature<T>(T f) where T:Feature
	{
		if (f == null)
			return;
		_features.RemoveFeature(f);
		SelectFeature(null);
	}

	void IStageContext.DisableAllFeatures()
	{
		_features.DisableAll();
	}

	void IStageContext.HideAllVisuals()
	{
		_visuals.HideAll();
	}

	Visual IStageContext.SetVisual(bool visible,DataRef<MeshBuilder> mesh, DataRef<XForm> xform, Visual.Mode mode, Color color, bool selected, SelectionPriority selectionPriority, Action onSelect, Action onDebugDraw)
	{
		if (mesh == null)
			return null;
		SetSelectionPriorityForVisualName(mesh.blockname,selectionPriority);
		return _visuals.SetVisible(visible, mesh, xform, mode, color, selected, onSelect, onDebugDraw);
	}
		
	public void SelectFeature(Feature feature)
	{
		if (_onPicked != null)
			return;
			
		if(feature!=null)
			feature.enabled = true;

//			if(_selectedFeature!=feature && !_gizmos.hasActiveHandle)
//				CameraManager.instance.FocusOnBoundingBox( feature!=null && feature.bounds.extents.sqrMagnitude>0 ? feature.bounds : _features.bounds);

		_selectedFeature = feature;
			
		_sideBar.SetVisible(true);
		_stageManager.RefreshStages();
	}

	Gizmo IStageContext.SetGizmo(bool visible, DataRef<XForm> xformref, GizmoHandleFlags handles, GizmoSpace space, Action<XForm, bool> feedback)
	{
		return _gizmos.SetGizmo(visible, xformref, handles,space, feedback);
	}

	void IStageContext.HideAllGizmos()
	{
		_gizmos.HideAllGizmos();
	}

	void IStageContext.ShowPropertiesOf<T>(string title, T obj, Action<T> onDelete)
	{
		if(onDelete==null)
			_propertySheet.ShowProperties(title, obj, null);
		else
			_propertySheet.ShowProperties(title, obj, del => { onDelete((T)del); });
	}

	void IStageContext.SelectOneOf(Feature[] features, bool[] showDeleteButtons)
	{
		int i = Array.IndexOf(features, _selectedFeature);
		if (i < 0)
		{
			if( features.Length==0 && _selectedFeature!=null)
				SelectFeature(null);
			if ( features.Length>0 )
			{
				i = 0;
				SelectFeature( features[i] );
			}
		}

		if(showDeleteButtons==null || i>=showDeleteButtons.Length || showDeleteButtons[i])
			_propertySheet.ShowProperties($"Selected {_selectedFeature?.name}",_selectedFeature, o =>
			{
				Feature f = (Feature) o;
				DialogManager.Show<MessageBox>().WithQuery("Delete Feature", $"Are you sure you want to delete this '{f.name}'?", yes =>
				{
					if (yes)
					{
						UndoManager.Append(() => { RemoveFeature(f); }, () => { _features.AddFeature(f.name,f); });
					}
				});
			});
		else
			_propertySheet.ShowProperties($"Selected {_selectedFeature?.name}",_selectedFeature, null);
	}
		
	public void PickFromScene(string header, string[] tagPriorities, Func<RaycastHit, bool> action)
	{
		_gizmos.HideAllGizmos();
		PushMarker();
		_sideBar.SetVisible(false);
		_propertySheet.SetVisible(false);
		_toast = DialogManager.Show<Toast>(false);
		_toast.WithMessage(header, () =>
		{
			Func<RaycastHit,bool> func = _onPicked;
			// Clear refs in case the callback opens another picker
			_toast = null; 
			_onPicked = null;
			_sideBar.SetVisible(true);
			_propertySheet.SetVisible(true);
			ClearMarkers();
			func(default);
		});
		_pickPriorities = tagPriorities;
		_onPicked = action;
	}

	public void MeasureWithTags(string[] tags )
	{
		PickFromScene("Pick a point to measure from", tags, hit =>
		{
			if (hit.collider != null)
			{
				PickFromScene("Move to measure distance. Click to close.", tags, hit2 => { return hit2.collider!=null; });
				return true;
			}
			return false;
		});
	}

	public Marker PushMarker(Marker.Flags flags=0)
	{
		return _markers.Push(flags);
	}

	public Marker PopMarker()
	{
		return _markers.Pop();
	}

	public void ClearMarkers()
	{
		_markers.Clear();
	}

	void IStageContext.ConfigurePrintBox(PrintBox.PrintBoxMode mode)
	{
		_printBox.SetMode(mode);
	}
}