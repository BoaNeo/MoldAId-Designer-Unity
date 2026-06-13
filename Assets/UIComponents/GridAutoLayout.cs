using System;
using UnityEngine;

namespace UIComponents {
	public class GridAutoLayout {
		/// <summary>
		/// Layout prefab instances in a grid in either landscape or portrait, taking care to make the most use of available space.
		/// The code will use multiple columns/rows if possible without downscaling the prefab, and will upscale the prefab to exactly
		/// match the row/column size, making the most use of available space.
		/// </summary>
		/// <param name="viewport">Visible rect through which the grid is viewed - this is needed to determine how many columns/rows will fit in the grid. If grid is fixed size this can be the grid itself</param>
		/// <param name="grid">Grid where prefabs are added</param>
		/// <param name="prefab">The prefab to use for each grid cell</param>
		/// <param name="major">Major orientation. This is the direction that is not limited</param>
		/// <param name="count">Total number of cells</param>
		/// <param name="layout">A callback that will format a specific cell</param>
		/// <typeparam name="C">Type of prefab, must be a GridCell</typeparam>
		public static void Layout<C>(Rect viewport, GridBuilder grid, C prefab, RectTransform.Axis major, int count, Action<int, C> layout) where C : GridCell {
			int columns = count;

			if (count > 0) {
				float fitwidth = viewport.width / prefab.width;
				float fitheight = viewport.height / prefab.height;
				int fith = (int) fitwidth;
				int fitv = (int) fitheight;
				float scale = 1.0f;

				if (major == RectTransform.Axis.Horizontal) {
					if (fitv > 1)
						columns = Mathf.CeilToInt(columns / (float) fitv);
					int rows = Mathf.CeilToInt(count / (float) columns);
					float extraspace = viewport.height - rows * prefab.height;
					extraspace /= rows;
					scale = 1.0f + extraspace / prefab.height;
				}
				else {
					columns = Mathf.Max(1, fith);
					float extraspace = viewport.width - columns * prefab.width;
					extraspace /= columns;
					scale = 1.0f + extraspace / prefab.width;
				}

				grid.transform.localScale = scale * Vector3.one;
			}

			grid.BeginUpdate();
			for (int i = 0; i < count; i++) {
				if (i > 0 && i % columns == 0)
					grid.EndRow();
				int local_i = i;
				grid.AddCell(prefab, item => { layout(local_i, item); });
			}

			grid.EndUpdate();
		}
	}
}