using System;
using UIComponents;
using UnityEngine;

namespace StageBar
{
	public class StageBarUIList : GridCell
	{
		[SerializeField] private GridOnDemand _grid;
		[SerializeField] private StageBarUIListItem _itemPrefab;

		public int selectedRow { get; set; }
		private float _selectTime;

		public void Setup(int count, Action<int,StageBarUIListItem> configure, Action<int, bool> onSelect)
		{
			_grid.Setup(_itemPrefab, 1, count, (col, row, cell) =>
			{
				configure(row, cell);
				cell.onSelect = () =>
				{
					bool doubleclick = false;
					float t = Time.time;
					if (selectedRow == row && (t - _selectTime) < 0.5f)
						doubleclick = true;
					selectedRow = row;
					_selectTime = t;
					onSelect(row,doubleclick);
				};
			} );
		}

		public void UpdateRowCount(int rows)
		{
			_grid.SetDataSize(1,rows);
		}
	}
}