using System;
using UnityEngine;
using UnityEngine.UI;

namespace UIComponents {
	public class SlideToggle : MonoBehaviour {
		[SerializeField] private Image _backActive;
		[SerializeField] private Image _backInactive;
		[SerializeField] private Image _knobInactive;
		[SerializeField] private Image _knobActive;

		private bool _state;
		private Action<bool> _onChange;
		private Vector2 _offPos;
		private Vector2 _onPos;

		private void Awake() {
			_offPos = _knobInactive.rectTransform.anchoredPosition;
			_onPos = _knobActive.rectTransform.anchoredPosition;
		}

		public void Setup(bool state, Action<bool> onChange) {
			_state = state;
			_onChange = onChange;
		}

		public void OnTap() {
			_state = !_state;
			_onChange(_state);
		}

		private void Update() {
			LerpAlpha(_backActive, _state ? 1.0f : 0.0f);
			LerpAlpha(_backInactive, _state ? 0.0f : 1.0f);
			LerpAlpha(_knobActive, _state ? 1.0f : 0.0f);
			LerpAlpha(_knobInactive, _state ? 0.0f : 1.0f);

			LerpPos(_knobActive.rectTransform, _state ? _onPos : _offPos);
			LerpPos(_knobInactive.rectTransform, _state ? _onPos : _offPos);
		}

		private void LerpPos(RectTransform xform, Vector2 target) {
			xform.anchoredPosition = Vector2.Lerp(xform.anchoredPosition, target, 8 * Time.deltaTime);
		}

		private void LerpAlpha(Image img, float a) {
			Color c = img.color;
			c.a = Mathf.Lerp(c.a, a, 8 * Time.deltaTime);
			img.color = c;
		}
	}
}