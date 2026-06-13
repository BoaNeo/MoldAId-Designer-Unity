using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utility;

namespace UIComponents {
	public class ViewAnimator : MonoBehaviour {
		public struct State {
			public float alpha;
			public Vector2 scale;
			public Vector2 position;
			public bool disabled;
		}

		private Action _then;
		private float _time;
		private float _delay;
		private float _speed;
		private State _from;
		private State _to;
		private State _current;
		private Tween.EasingFunction _tween;
		private Dictionary<int, float> _originalAlphas;
		private Graphic[] _childGraphic;
		private bool _hasAlpha;

		public void SetState(State state) {
			_from = state;
			_to = state;
			_delay = 0;
			_time = 1.0f;
			_speed = 1.0f;
			_tween = Tween.easeInBack;
			UpdateCurrentState();
		}

		public void StartTransition(State to, Tween.EasingFunction tween, float speed = 1.0f, float delay = 0, Action then = null) {
			Then(then);
			_from = _current;
			_to = to;
			_delay = delay;
			_time = 0;
			_speed = speed;
			_tween = tween;
			gameObject.SetActive(true);
		}

		private void Then(Action then) {
			Action prev = _then;
			_then = then;
			if (prev != null) {
				prev();
			}
		}

		private void Update() {
			InternalUpdate();
		}

		protected void InternalUpdate() {
			if (_delay > 0) {
				_delay -= Time.deltaTime;
				return;
			}

			if (_tween == null)
				return;

			if (_time < 1.0f) {
				if (Step.Forward(ref _time, _speed * Time.deltaTime)) {
					UpdateCurrentState();
				}
			}
		}

		private float GetOriginalAlpha(Graphic gfx) {
			if (_originalAlphas == null)
				_originalAlphas = new Dictionary<int, float>();
			if (_originalAlphas.TryGetValue(gfx.GetInstanceID(), out float alpha))
				return alpha;
			_originalAlphas[gfx.GetInstanceID()] = gfx.color.a;
			return gfx.color.a;
		}

		private void UpdateCurrentState() {
			float x = _tween(_from.position.x, _to.position.x, _time);
			float y = _tween(_from.position.y, _to.position.y, _time);
			_current.position = new Vector2(x, y);
			float sx = _tween(_from.scale.x, _to.scale.x, _time);
			float sy = _tween(_from.scale.y, _to.scale.y, _time);
			_current.scale = new Vector3(sx, sy, 1);
			_current.alpha = _tween(_from.alpha, _to.alpha, _time);
			_current.disabled = _time >= 1.0f ? _to.disabled : _from.disabled;

			ApplyCurrentState();

			if (_time >= 1.0f) {
				Then(null);
				// Force get of children for alpha updates during next transition (in case the hierarchy changed)
				_childGraphic = null; 
			}
		}

		private void ApplyCurrentState() {
			if (_current.disabled)
				gameObject.SetActive(false);

			RectTransform _xform = (RectTransform) transform;
			_xform.anchoredPosition = _current.position;
			_xform.localScale = _current.scale;

			if ((_to.alpha < 1 || _from.alpha < 1 || _hasAlpha) && (_childGraphic == null || _childGraphic.Length == 0)) {
				_childGraphic = GetComponentsInChildren<Graphic>(true);
				_hasAlpha = true;
			}

			if (_childGraphic != null) {
				for (int g = 0; g < _childGraphic.Length; g++) {
					Graphic gfx = _childGraphic[g];
					if (gfx) // Just in case someone destroyed this while we were animating it!
					{
						float alpha_org = GetOriginalAlpha(gfx);
						Color c = gfx.color;
						c.a = _current.alpha * alpha_org;
						gfx.color = c;
					}
				}
			}
		}
	}
}