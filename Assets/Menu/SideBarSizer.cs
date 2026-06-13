using UnityEngine;

// TODO: This should be a generic UI component that distributes space between its children.
namespace Menu
{
	public interface ISideBar
	{
		public float preferredHeight { get; }
		public float height { get; set; }
	}
	
	public class SideBarSizer : MonoBehaviour
	{
		private ISideBar _upper;
		private ISideBar _lower;

		private void Update()
		{
			if (_upper == null || _lower == null)
			{
				ISideBar[] sidebars = GetComponentsInChildren<ISideBar>();
				if (sidebars==null || sidebars.Length < 2)
					return;
				_upper = sidebars[0];
				_lower = sidebars[1];
			}
			float total = ((RectTransform)transform).rect.height;

			float upper = _upper.preferredHeight;
			float lower = _lower.preferredHeight;

			if (upper + lower > total)
			{
				if (upper > total / 2)
					upper = Mathf.Max(total / 2, total - lower);
				if(lower>total/2)
					lower = Mathf.Max(total / 2, total - upper);
			}

			if (upper + lower < total)
			{
				lower = total - upper;
			}

			_upper.height = Mathf.Lerp(_upper.height, upper, 5 * Time.deltaTime);
			_lower.height = Mathf.Lerp(_lower.height , lower, 5*Time.deltaTime);

/*			
			private void Resize()
			{
				RectTransform rect = (RectTransform) transform;
				float desiredH = _grid.layoutHeight + _header.rectTransform.rect.height;
				float maxH = Mathf.Max(Screen.height/2,_stageBarContent.rect.yMin);
				float h = Mathf.Max(maxH, desiredH);
				if (Mathf.Abs(rect.rect.height - h) > 1)
				{
					rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
					_transition.SetShownState( new Vector2(0, h), rect.rect.size, true);
				}
			}
*/
			
		}
	}
}