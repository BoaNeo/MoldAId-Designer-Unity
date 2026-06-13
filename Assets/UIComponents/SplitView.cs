using System;
using UnityEngine;

namespace UIComponents {
	/// <summary>
	/// Split parent rect along the largest axis, and reshape the specified child rects along this axis.
	/// The primary use of this is to split a screen horizontally in landscape and vertically in portrait leaving you with smaller areas with a more usable aspect ratio
	/// Horizontal and Vertical sets can, but need not, be the same views and you may mix as needed.
	/// </summary>
	[ExecuteInEditMode]
	public class SplitView : MonoBehaviour {
		[Serializable]
		struct Split {
			public RectTransform rect;
			public float size;
			public bool relativeSize;
			public bool rotateInLandscape;

			public float GetSize(float availsize, float relsum) {
				return relativeSize ? availsize * size / relsum : size;
			}
		}

		[SerializeField] private Split[] _horizontalViews;
		[SerializeField] private Split[] _verticalViews;

		private RectTransform _rt;
		private Rect _lastRect;

		private void Awake() {
			_rt = (RectTransform) transform;
		}

		private void Update() {
			Reshape();
		}

		public void Reshape() {
			Rect r = _rt.rect;
			if (_rt.rect != _lastRect) {
				_lastRect = r;
				float w = r.width;
				float h = r.height;
				float sx = r.width > r.height ? 1.0f : 0.0f;
				float sy = r.width < r.height ? 1.0f : 0.0f;
				SetInactive(r.width > r.height ? _verticalViews : _horizontalViews);
				SetActive(r.width > r.height ? _horizontalViews : _verticalViews, w, h, sx, sy);
				_rt.ForceUpdateRectTransforms();
			}
		}

		private void SetInactive(Split[] views) {
			for (int i = 0; i < views.Length; i++) {
				if (views[i].rect)
					views[i].rect.gameObject.SetActive(false);
			}
		}

		private void SetActive(Split[] views, float w, float h, float sx, float sy) {
			bool isLandscape = w > h;
			// First subtract all fixed sizes from the width we'll distribute and calculate total relative size
			float relsum = 0;
			for (int i = 0; i < views.Length; i++) {
				if (!views[i].relativeSize) {
					w -= sx * views[i].size;
					h -= sy * views[i].size;
				}
				else {
					relsum += views[i].size;
				}
			}

			float x = 0, y = 0;
			for (int i = 0; i < views.Length; i++) {
				RectTransform r = views[i].rect;
				float vw = views[i].GetSize(w, relsum);
				float vh = views[i].GetSize(h, relsum);
				if (r) {
					r.gameObject.SetActive(true);
					SetRect(r, x, y, sy * w + sx * vw, sx * h + sy * vh, views[i].rotateInLandscape && isLandscape);
				}

				x += sx * vw;
				y += sy * vh;
			}
		}

		private void SetRect(RectTransform r, float x, float y, float w, float h, bool rotate) {
			if (rotate) {
				r.pivot = Vector2.zero;
				// Move to compensate for the fact that we're rotating around the upper left corner rather than the origin
				y += h - w;
				x += w;
				// Swap w and h because the rect is now rotated
				float t = w;
				w = h;
				h = t;
			}

			r.rotation = Quaternion.Euler(0, 0, rotate ? 90 : 0);

			r.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, x, w);
			r.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, y, h);
		}
	}
}