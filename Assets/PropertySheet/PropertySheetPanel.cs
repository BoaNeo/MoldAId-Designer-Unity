using System;
using System.Collections.Generic;
using System.Reflection;
using FeatureGraph;
using Gizmos;
using Menu;
using TMPro;
using UIComponents;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PropertySheet
{
	public class PropertySheetPanel : MonoBehaviour, ISideBar
	{ 
		[SerializeField] private TMP_Text _header;
		[SerializeField] private GridBuilder _grid;
		[SerializeField] private Image _bottomFade;
		[SerializeField] private ScrollRect _scrollRect;
		[SerializeField] private GameObject _deleteButton;
		[SerializeField] private PropertyUIXForm _xformPrefab;
		[SerializeField] private PropertyUIVector3 _vectorPrefab;
		[SerializeField] private PropertyUIFloat _floatPrefab;
		[SerializeField] private PropertyUIString _stringPrefab;
		[SerializeField] private PropertyUIInt _intPrefab;
		[SerializeField] private PropertyUIPath _pathPrefab;
		[SerializeField] private PropertyUIAction _actionPrefab;
		[SerializeField] private PropertyUIEnum _enumPrefab;
		[SerializeField] private PropertyUIBool _boolPrefab;

		private Transition _transition;
		private object _selected;
		private Action<object> _onDelete;
		private string _title;

		private void Awake()
		{
			_transition = GetComponent<Transition>();
		}
		
		void Update()
		{
			// This whole mess is just to fix Unity's non existing tab navigation
			bool forward = Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.DownArrow);
			bool backward = ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyDown(KeyCode.Tab)) || Input.GetKeyDown(KeyCode.UpArrow);
			if (forward || backward)
			{
				EventSystem system = EventSystem.current;
				GameObject curObj = system.currentSelectedGameObject;
				GameObject nextObj = null;
				if (!curObj)
				{
					nextObj = system.firstSelectedGameObject;
				}
				else
				{
					Selectable curSelect = curObj.GetComponent<Selectable>();
					Selectable nextSelect =backward ? curSelect.FindSelectableOnUp() : curSelect.FindSelectableOnDown();
					if (nextSelect)
					{
						nextObj = nextSelect.gameObject;
					}
				}
				if (nextObj)
				{
					system.SetSelectedGameObject(nextObj, new BaseEventData(system));
				}
			}
			
			if(FeatureManager.transientChange)
				Refresh();

			_bottomFade.gameObject.SetActive( _scrollRect.content.rect.height>_scrollRect.viewport.rect.height && _scrollRect.verticalNormalizedPosition>0.0f );
		}

		public float preferredHeight => _grid.layoutHeight + 30;
		public float height { get => ((RectTransform) transform).rect.height; set => ((RectTransform)transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value); }

		public void SetVisible(bool b)
		{
			_transition.SetVisible(b);
		}

		public void ShowProperties(string title, object obj, Action<object> onDelete)
		{
			if (obj == null)
			{
				SetVisible(false);
				return;
			}
			SetVisible(true);
			bool focusFirstField = obj != _selected;
			_title = title;
			_selected = obj;
			_onDelete = onDelete;
			_deleteButton.SetActive(_onDelete != null);
			Refresh(focusFirstField);
		}

		public void Refresh( bool focusFirstField=false)
		{
			if (_selected == null)
			{
				_transition.SetVisible(false);
				return;
			}

			_header.text = _title;

			List<PropertyData> sortedProperties = new List<PropertyData>();

			int i = 0;
			foreach (PropertyInfo prop in _selected.GetType().GetProperties())
			{
				ShowPropertyAttribute attr = prop.GetCustomAttribute<ShowPropertyAttribute>();
				if (attr != null)
					sortedProperties.Add(new PropertyData { target=_selected, property = prop, attribute = attr, order = attr.order + i-- });
			}

			foreach (FieldInfo field in _selected.GetType().GetFields())
			{
				ShowPropertyAttribute attr = field.GetCustomAttribute<ShowPropertyAttribute>();
				if (attr != null)
					sortedProperties.Add(new PropertyData { target = _selected, field = field, attribute = attr, order = attr.order + i-- });
			}

			foreach (MethodInfo method in _selected.GetType().GetMethods())
			{
				ShowPropertyAttribute attr = method.GetCustomAttribute<ShowPropertyAttribute>();
				if (attr != null)
					sortedProperties.Add(new PropertyData { target=_selected, method = method, attribute = attr, order = attr.order + i-- });
			}

			sortedProperties.Sort((p1, p2) => p2.order - p1.order);

			PropertyUI first = null;
			PropertyUI prev = null;
			PropertyUI prevprev = null;
			_grid.BeginUpdate();
			foreach (PropertyData prop in sortedProperties)
			{
				PropertyUI prefab = prop.method != null ? _actionPrefab : GetPrefab(prop.type, prop.attribute);
				_grid.AddRow( prefab, v =>
				{
					if (first == null)
						first = v;
					if (prev)
					{
						SetNavigateBack(v.firstSelectable, prev.lastSelectable);
						SetNavigateForward(prev.lastSelectable, v.firstSelectable);
					}

					try
					{
						v.Setup(prop, ()=> Refresh() );
						prevprev = prev;
						prev = v;
					}
					catch (Exception e)
					{
						Debug.LogWarning($"Unable to show property value for {_selected}.{prop.name} using {prefab}: {e.Message}");
						throw;
					}
				});
			}
			_grid.AddRowSpacing(4);
			_grid.EndUpdate();

			if (first)
			{
				SetNavigateForward(prev.lastSelectable, first.firstSelectable);
				SetNavigateBack(first.firstSelectable, prev.lastSelectable);
				if(focusFirstField && first.firstSelectable)
					first.firstSelectable.Select();
			}
		}

		public void SetNavigateForward(Selectable current, Selectable next)
		{
			if (!current)
				return;
			current.navigation = new Navigation
			{
				mode = Navigation.Mode.Explicit,
				wrapAround = true,
				selectOnUp = current.navigation.selectOnUp,
				selectOnDown = next,
				selectOnLeft = null,
				selectOnRight = null
			};
		}

		public void SetNavigateBack(Selectable current, Selectable prev)
		{
			if (!current)
				return;
			current.navigation = new Navigation
			{
				mode = Navigation.Mode.Explicit,
				wrapAround = true,
				selectOnUp = prev,
				selectOnDown = current.navigation.selectOnDown,
				selectOnLeft = null,
				selectOnRight = null
			};
		}

		private PropertyUI GetPrefab(Type t, ShowPropertyAttribute attrib)
		{
			if (t == typeof(XForm) || t == typeof(DataRef<XForm>))
				return _xformPrefab;
			if (t == typeof(Vector3) || t == typeof(DataRef<Vector3>))
				return _vectorPrefab;
			if (t == typeof(float) || t == typeof(DataRef<float>))
				return _floatPrefab;
			if (t == typeof(int) || t == typeof(DataRef<int>))
				return attrib.values!=null ? (PropertyUI)_enumPrefab : _intPrefab; // TODO: This is a bit hacky, but enums are a pain in the arse...
			if (t == typeof(bool) || t == typeof(DataRef<bool>))
				return _boolPrefab;
			if (t == typeof(PathProperty) || t == typeof(DataRef<PathProperty>))
				return _pathPrefab;
			if (t == typeof(string) || t == typeof(DataRef<string>))
				return _stringPrefab;
			return _enumPrefab;
		}

		public void OnDelete()
		{
			if(_onDelete!=null)
				_onDelete?.Invoke(_selected);
		}
	}
}