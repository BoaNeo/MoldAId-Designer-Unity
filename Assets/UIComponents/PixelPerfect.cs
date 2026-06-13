
using UnityEngine;

namespace UIComponents
{
	public class PixelPerfect : MonoBehaviour
	{
		[SerializeField] private int _pixelWidth;
		[SerializeField] private int _pixelHeight;

		void Update()
		{
			Canvas canvas = GetComponentInParent<Canvas>();

			RectTransform t = (RectTransform)transform;
			if(_pixelHeight!=0)
			{
				float height = ((RectTransform)canvas.transform).rect.height/canvas.pixelRect.height;
				t.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _pixelHeight*height);
			}
			if(_pixelWidth!=0)
			{
				float width = ((RectTransform)canvas.transform).rect.width/canvas.pixelRect.width;
				t.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _pixelWidth*width);
			}
		}
	}
}
