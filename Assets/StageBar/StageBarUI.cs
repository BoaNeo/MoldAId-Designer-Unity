using System;
using TMPro;
using UIComponents;
using UnityEngine;
using UnityEngine.UI;

namespace StageBar
{
	public class StageBarUI : GridCell
	{
		[SerializeField] private TMP_Text _label;
		[SerializeField] private Image _expanded;
		[SerializeField] private StageBarUIAction _actionPrefab;
		[SerializeField] private StageBarUIEnum _enumPrefab;
		[SerializeField] private StageBarUIList _listPrefab;
		[SerializeField] private StageBarUILabel _labelPrefab;
		[SerializeField] private GridBuilder _subItems;

		private Action _onSelect;
		private bool _selected;

		public void Setup(string title, bool selected, Action onSelect)
		{
			_label.text = title;
			_selected = selected;
			_onSelect = onSelect;
		}

		private void Update()
		{
			float h = _label.rectTransform.rect.height+5;
			float r = 90;
			if (_selected)
			{
				h += _subItems.layoutHeight;
				r = 0;
			}

			_expanded.transform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(_expanded.transform.rotation.eulerAngles.z, r, 10*Time.deltaTime));
			if( Mathf.Abs(height-h)>2)
				height = Mathf.Lerp(height, h, 10 * Time.deltaTime);
			else
				height = h;
		}

		public void OnSelect()
		{
			_onSelect();
		}

		public void AddAction(string label, Action onAction)
		{
			_subItems.AddRow(_actionPrefab, item => item.Setup(label, onAction));
		}

		public StageBarUIList AddList(string title, int count, Action<int, StageBarUIListItem> configure, Action<int,bool> onSelect)
		{
			AddText(title);
			return _subItems.AddRow(_listPrefab, list => list.Setup( count, configure, onSelect));
		}

		public void EndUpdate()
		{
			_subItems.EndUpdate();
		}

		public void BeginUpdate()
		{
			_subItems.BeginUpdate();
		}

		public void AddSpacing()
		{
			_subItems.AddRowSpacing(5);
		}

		public void AddText(string title)
		{
			_subItems.AddRow(_labelPrefab, item => item.Setup(title));
		}

		public void AddEnum(string title, string[] options, int option, Action<int> onSelect)
		{
			AddText(title);
			_subItems.AddRow(_enumPrefab, e => e.Setup( options, option, onSelect));
		}
	}
}