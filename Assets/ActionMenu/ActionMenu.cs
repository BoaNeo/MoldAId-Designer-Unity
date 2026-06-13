using System;
using UIComponents;
using UnityEngine;
using UnityEngine.EventSystems;
using Utility;

namespace ActionMenu
{
	public class ActionMenu : MonoBehaviour
	{
		[SerializeField] private Transition _panel;
		[SerializeField] private GridBuilder _grid;
		[SerializeField] private ActionItem _itemPrefab;

		public static void ShowMenu(PointerEventData evt, params (string,Action)[] content)
		{
			Singleton<ActionMenu>.Instance.Show(evt, content);
		}

		private void Awake()
		{
			Hide();
		}

		private void Show(PointerEventData evt, params (string,Action)[] content)
		{
			gameObject.SetActive(true);
			_panel.SetShownState(evt.position);
			_grid.BeginUpdate();
			for (int i = 0; i < content.Length; i++)
			{
				_grid.AddRow(_itemPrefab, item =>
				{
					(string label, Action action) = content[i];
					item.Setup(label, () =>
					{
						Hide();
						action();
					});
				});
			}
			_grid.EndUpdate();
			_panel.SetVisible(true);
		}

		public void Hide()
		{
			_panel.SetVisible(false, 0, -1f, () =>
			{
				gameObject.SetActive(false);
			});
		}
	}
}