using System;
using UnityEngine;

namespace UIComponents {
	public class SafeRect : MonoBehaviour {
		[SerializeField] private bool _leftEdge = true;
		[SerializeField] private bool _rightEdge = true;
		[SerializeField] private bool _topEdge = true;
		[SerializeField] private bool _bottomEdge = true;

		private RectTransform _rect;
		private Canvas _canvas;
		private Action _reset;

		private void OnEnable() {
			Update();
		}

		private void Update() {
			if (!_canvas) {
				_canvas = GetComponentInParent<Canvas>();
				_rect = GetComponent<RectTransform>();

//				_rect.ForceUpdateRectTransforms();
				Vector2 anchoredPos = _rect.anchoredPosition;
				Vector2 anchorMin = _rect.anchorMin;
				Vector2 anchorMax = _rect.anchorMax;
				Vector2 offsetMin = _rect.offsetMin;
				Vector2 offsetMax = _rect.offsetMax;
				Vector2 sizeDelta = _rect.sizeDelta;
				Vector2 pivot = _rect.pivot;
				_reset = () => {
					_rect.anchoredPosition = anchoredPos;
					_rect.anchorMin = anchorMin;
					_rect.anchorMax = anchorMax;
					_rect.offsetMin = offsetMin;
					_rect.offsetMax = offsetMax;
					_rect.pivot = pivot;
					_rect.sizeDelta = sizeDelta;
//					_rect.ForceUpdateRectTransforms();
				};
			}

			if (!_canvas)
				return;

			float scale = _canvas.transform.localScale.x;

			_reset();

			Rect rect = _rect.rect;
			if (_leftEdge || _rightEdge) {
				float xmin = _leftEdge ? Screen.safeArea.xMin / scale : rect.xMin - rect.x;
				float xmax = _rightEdge ? Screen.safeArea.xMax / scale : rect.xMax - rect.x;
				_rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, xmin, xmax - xmin);
			}

			if (_topEdge || _bottomEdge) {
				float ymin = _bottomEdge ? Screen.safeArea.yMin / scale : rect.yMin - rect.y;
				float ymax = _topEdge ? Screen.safeArea.yMax / scale : rect.yMax - rect.y;
				_rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, ymin, ymax - ymin);
			}
		}
	}
}