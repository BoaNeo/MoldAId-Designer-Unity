using System;
using UIComponents;
using UnityEngine;

namespace StageBar
{
	public class StageBarUIEnum : GridCell
	{
		[SerializeField] private GridBuilder _grid;
		[SerializeField] private StageBarUIEnumItem _itemPrefab;

		public void Setup( string[] options, int option, Action<int> onSelect)
		{
			_grid.BeginUpdate();
			for (int i = 0; i < options.Length; i++)
			{
				int idx = i;
				_grid.AddRow(_itemPrefab, item =>
				{
					item.Setup(options[i], i == option, () =>
					{
						onSelect(idx);
						Setup(options,idx, onSelect);
					});
				});
			}
			_grid.EndUpdate();
			height = _grid.layoutHeight + 12;
		}
	}
}