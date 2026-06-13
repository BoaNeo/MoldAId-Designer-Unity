using System;
using System.Collections.Generic;
using TMPro;
using UIComponents;
using UnityEngine;

namespace Menu
{
	public class MenuBar : GridCell
	{
		[SerializeField] private TMP_Text _label;
		[SerializeField] private TMP_Text _shortcutLabel;
		[SerializeField] private GridBuilder _items;
		[SerializeField] private MenuBar _itemPrefab;
		[SerializeField] private Transition _subPanel;
		[SerializeField] private GameObject _blocker;

		private RectTransform.Axis _axis;
		private Action _onClick;
		private MenuBar _parent;

		private static MenuBar _visibleRoot;
		private KeyCode _shortcutKey;
		private bool _shortcutCmd;
		private bool _shortcutShift;
		private float _maxWidth;
		private List<MenuBar> _subItems = new();

		public void BeginUpdate(RectTransform.Axis axis)
		{
			_axis=axis;
			_maxWidth = 0;
			_subItems.Clear();
			_items.BeginUpdate();
		}

		public void AddSubMenu(string name, Action<MenuBar> setup)
		{
			_items.AddCell(_itemPrefab, item =>
			{
				item.rectTransform.anchorMin = Vector2.up;
				item.rectTransform.anchorMax = Vector2.up;
				item.width = item.Setup(this, name, false, false, KeyCode.None);
				item.BeginUpdate(RectTransform.Axis.Vertical);
				setup(item);
				item.EndUpdate();
				_subItems.Add(item);
			}, _axis==RectTransform.Axis.Vertical);
		}

		private float Setup(MenuBar parent, string s, bool cmd, bool shift, KeyCode shortcut)
		{
			_parent = parent;
			_label.text = s;
			_shortcutLabel.text = $"{ToString(cmd, shift, shortcut)}";
			_shortcutShift = shift;
			_shortcutCmd = cmd;
			_shortcutKey = shortcut;
			_onClick = ToggleList;
			return _label.preferredWidth + (_shortcutKey == 0 ? 0:30)+_shortcutLabel.preferredWidth+10;
		}

		private string ToString(bool cmd, bool shift, KeyCode code)
		{
			string c="";

			if (code == KeyCode.None)
				return c;
			if (code >= KeyCode.Keypad0 && code <= KeyCode.Keypad9)
				c = $"Num {(char)('0' + (code - KeyCode.Keypad0))}";
			else if (code == KeyCode.KeypadPlus)
				c = "Num +";
			else if (code == KeyCode.KeypadMinus)
				c = "Num -";
			else
				c = $"{char.ToUpper((char)code)}";

			return $"{(cmd?(IsWindows ? "CTRL+" : "CMD+"):"")}{(shift ? "SHIFT+" : "")}{c}";
		}

		private void ToggleList()
		{
			SetItemsVisible( !_subPanel.isActiveAndEnabled );
		}

		private void SetItemsVisible( bool vis)
		{
			if (_subPanel == null)
				return;

			if (vis)
				HideAll();

			RectTransform w = ((RectTransform)_subPanel.transform);
			w.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left,rectTransform.rect.xMin,_maxWidth+10);
			w.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom,-_items.layoutHeight-10,_items.layoutHeight+10);
			_subPanel.SetVisible(vis);

			_visibleRoot = this;

			if(_parent._blocker!=null)
				_parent._blocker.SetActive(vis);
		}

		public static void HideAll()
		{
			if(_visibleRoot!=null)
				_visibleRoot.SetItemsVisible(false);
			_visibleRoot = null;
		}

		private void HideFrom(MenuBar parent)
		{
			if(parent!=null)
				parent.SetItemsVisible(false);
			else
				HideAll();
		}

		public void AddItem(string name, Action onClick)
		{
			AddItem(name, false, false, KeyCode.None, onClick);
		}

		public void AddItem(string name, bool cmd, bool shift, KeyCode shortcut, Action onClick)
		{
			_items.AddCell(_itemPrefab, item =>
			{
				_maxWidth = Mathf.Max(_maxWidth, item.Setup(this,name, cmd,shift,shortcut));
				item.rectTransform.anchorMin = Vector2.up;
				item.rectTransform.anchorMax = Vector2.one;
				item.rectTransform.offsetMax = Vector2.zero;
				item.rectTransform.offsetMin = new Vector2(0, -item.height+_items.layoutHeight);
				item._onClick = onClick;
				_subItems.Add(item);
			}, _axis==RectTransform.Axis.Vertical);
		}

		public void OnClick()
		{
			if (_onClick!=null)
				_onClick();
			if(_onClick!=ToggleList)
				HideAll();
		}

		public float EndUpdate()
		{
			_items.EndUpdate();
			return _maxWidth;
		}

		public void AddSpacing()
		{
			if(_axis==RectTransform.Axis.Vertical)
				_items.AddRowSpacing(5);
			else
				_items.AddCellSpacing(5);
		}

		public void ProcessShortcuts()
		{
			if (_shortcutKey != KeyCode.None)
			{
				KeyCode shortcutKey1 = IsWindows ? KeyCode.LeftControl : KeyCode.LeftCommand;
				KeyCode shortcutKey2 = IsWindows ? KeyCode.RightControl : KeyCode.RightCommand;

				if (_shortcutCmd == (Input.GetKey(shortcutKey1) || Input.GetKey(shortcutKey2)) && 
					  _shortcutShift == (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ) &&
						Input.GetKeyDown(_shortcutKey) )
				{
					OnClick();
				}
			}

			foreach (MenuBar menu in _subItems)
			{
				menu.ProcessShortcuts();
			}
		}

		public bool IsWindows => Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsServer;
	}
}