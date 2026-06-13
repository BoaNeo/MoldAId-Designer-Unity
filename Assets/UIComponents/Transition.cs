using System;
using UnityEngine;
using UnityEngine.Serialization;
using Utility;

namespace UIComponents {
	public sealed class Transition : ViewAnimator {
		public const float CONFIGURED_TIME = -1;

		public enum HideLocation {
			Stay,
			ParentLeftEdge,
			ParentRightEdge,
			ParentUpperEdge,
			ParentLowerEdge,
			Unused,
			ScreenLeftEdge,
			ScreenRightEdge,
			ScreenUpperEdge,
			ScreenLowerEdge
		}

		[FormerlySerializedAs("showEase")]
		[Tooltip("Easing method used when showing panel")]
		[SerializeField] private Tween.EaseType _showEase = Tween.EaseType.easeOutExpo;

		[FormerlySerializedAs("hideEase")]
		[Tooltip("Easing method used when hiding panel")]
		[SerializeField] private Tween.EaseType _hideEase = Tween.EaseType.easeOutExpo;

		[Tooltip("Easing time (secs)")] 
		[SerializeField] private float _easeTime = .5f;

		[FormerlySerializedAs("hideAt")]
		[Tooltip("Abstract location to move panel to when hiding")]
		[SerializeField] private HideLocation _hideAt = HideLocation.Stay;

		[FormerlySerializedAs("hideInitially")]
		[Tooltip("Hide panel initially")] 
		[SerializeField] private  bool _hideInitially = true;

		[FormerlySerializedAs("disableWhenHidden")]
		[Tooltip("Disable when hidden")] 
		[SerializeField] private  bool _disableWhenHidden = true;

		[FormerlySerializedAs("hideScale")]
		[Tooltip("Scale to apply when hidden")]
		[SerializeField] private Vector3 _hideScale = Vector3.one;

		[FormerlySerializedAs("hideAlpha")]
		[Tooltip("Alpha to apply to child Graphics when hidden")]
		[SerializeField] private  float _hideAlpha = 1.0f;

		private State _shownState;
		private State _hiddenState;
		private bool _transitionVisible;
		private bool _initialized;

		private float _transitionDelay;
		private float _transitionSpeed;
		private Action _transitionThen;
		private bool _transitionStart;

		private void Awake() {
			if (_hideInitially && !_transitionStart)
			{
				if(_disableWhenHidden)
					gameObject.SetActive(false);
				else
				{
					_transitionVisible = true;
					SetVisible(false,0,0,null);
				}
			}
		}

		private void OnEnable() {
			if (!_initialized)
			{
				RectTransform rt = ((RectTransform) transform);
				Init(rt.anchoredPosition);
			}

			_hiddenState.disabled = false; // We don't want SetState to disable us again, we just got enabled
			SetState(_hideInitially ? _hiddenState : _shownState);
			_hiddenState.disabled = _disableWhenHidden;
		}

		private void Init(Vector2 anchoredPosition) {
			_shownState.alpha = 1.0f;
			_shownState.scale = Vector2.one;
			_shownState.disabled = false;
			_shownState.position = anchoredPosition;

			_hiddenState.alpha = _hideAlpha;
			_hiddenState.scale = _hideScale;
			_hiddenState.disabled = _disableWhenHidden;
			_hiddenState.position = ConvertRelativePos(_hideAt, _shownState.position);
			_initialized = true;
		}

		public void SetShownState(Vector2 pos, bool animate=false) 
		{
			if (animate && isVisible)
			{
				_shownState.position = pos;
				SetVisible(true, 0, -1, null, true );
			}
			else
			{
				Init(pos);
				SetState(_transitionVisible ? _shownState : _hiddenState);
			}
//				Debug.Log($"Settings shown position to {value}, hidden position to {_hiddenState.position}");
		}

		public Vector2 HiddenPosition
		{
			get => _hiddenState.position;
			set => _hiddenState.position = value;
		}
		
		public Vector3 HideScale { get => _hiddenState.scale; set => _hiddenState.scale = value; }
		public HideLocation HideAt {
			get => _hideAt;
			set {
				_hideAt = value;
				_hiddenState.position = ConvertRelativePos(value, _shownState.position);
			}
		}

		public bool isVisible => _transitionVisible;

		public void Show(){ SetVisible(true);}
		public void Hide(){SetVisible(false);}
		public void ToggleVisible() {SetVisible(!_transitionVisible);}

		public void SetVisible(bool v, float delay = 0, float time = CONFIGURED_TIME, Action then = null, bool force=false) {
			if (!force && v == _transitionVisible) {
				if (then != null)
					then();
				return;
			}

			// In case someone calls SetVisible multiple times in the same frame, make sure we don't leave callback hanging
			if (_transitionThen != null)
				_transitionThen();

			// We can't start this immediately because we have to wait for the object to become active before we can grab the shown-state the first time
			_transitionDelay = delay;
			_transitionSpeed = 1.0f / (time < 0 ? _easeTime : time);
			_transitionThen = then;
			_transitionVisible = v;
			_transitionStart = true;

			// Activate object, let OnEnable reset if needed, and then let Update start the actual transition
			gameObject.SetActive(true);
		}

		private void Update() {
			if (_transitionStart) {
				if (_transitionVisible) {
					StartTransition(_shownState, Tween.GetEasingFunction(_showEase), _transitionSpeed, _transitionDelay, _transitionThen);
				}
				else {
					StartTransition(_hiddenState, Tween.GetEasingFunction(_hideEase), _transitionSpeed, _transitionDelay, _transitionThen);
				}

				_transitionThen = null;
				_transitionStart = false;
			}

			InternalUpdate();
		}

		private Vector2 ConvertRelativePos(HideLocation hide, Vector2 defaultpos) {
			if (transform.parent == null || !(transform.parent is RectTransform))
				return defaultpos;

			RectTransform rt = (RectTransform) transform;
			RectTransform prttt = (RectTransform) rt.parent;
			Rect parent = prttt.rect;
			parent.x = 0;
			parent.y = 0;

			float s = prttt.lossyScale.x;
			// TODO: This is not correct - it assumes the parent is a child of a fullscreen rect which might not be true
			// Also, it assumes the transform is anchored to the upper left edge
			switch (hide) {
				case HideLocation.ScreenLeftEdge:
					parent = new Rect(-prttt.offsetMin.x, 0, Screen.width / s, Screen.height / s);
					break;
				case HideLocation.ScreenRightEdge:
					parent = new Rect(Screen.width / s - prttt.offsetMax.x, 0, Screen.width / s, Screen.height / s);
					break;
				case HideLocation.ScreenLowerEdge:
					parent = new Rect(0, -prttt.offsetMin.y, Screen.width / s, Screen.height / s);
					break;
				case HideLocation.ScreenUpperEdge:
					parent = new Rect(0, Screen.height / s - prttt.offsetMax.y, Screen.width / s, Screen.height / s);
					break;
			}

			Vector2 hidden = defaultpos;

			switch (hide) {
				case HideLocation.Stay:
					break;
				case HideLocation.ScreenLowerEdge:
				case HideLocation.ParentLowerEdge:
					hidden.y = parent.y + parent.height * (rt.anchorMin.y * (rt.pivot.y - 1.0f) - rt.anchorMax.y * rt.pivot.y) + (rt.pivot.y - 1.0f) * _hideScale.y * rt.rect.height;
					break;
				case HideLocation.ScreenUpperEdge:
				case HideLocation.ParentUpperEdge:
					hidden.y = parent.y + parent.height * (1 + rt.anchorMin.y * (rt.pivot.y - 1.0f) - rt.anchorMax.y * rt.pivot.y) + (rt.pivot.y) * _hideScale.y * rt.rect.height;
					break;
				case HideLocation.ScreenLeftEdge:
				case HideLocation.ParentLeftEdge:
					hidden.x = parent.x + parent.width * (rt.anchorMin.x * (rt.pivot.x - 1.0f) - rt.anchorMax.x * rt.pivot.x) + (rt.pivot.x - 1.0f) * _hideScale.x * rt.rect.width;
					break;
				case HideLocation.ScreenRightEdge:
				case HideLocation.ParentRightEdge:
					hidden.x = parent.x + parent.width * (1 + rt.anchorMin.x * (rt.pivot.x - 1.0f) - rt.anchorMax.x * rt.pivot.x) + (rt.pivot.x) * _hideScale.x * rt.rect.width;
					break;
			}

			return hidden;
		}

		private Vector2 MapAnchoredPosition(RectTransform from, RectTransform to) {
			float wto = ((RectTransform) from.parent).rect.width;
			float hto = ((RectTransform) from.parent).rect.height;
			Vector2 vto = from.anchoredPosition;
			Vector2 cto = new Vector2((from.anchorMin.x + from.anchorMax.x) * wto / 2, (from.anchorMin.y + from.anchorMax.y) * hto / 2);

			float wfrom = ((RectTransform) to.parent).rect.width;
			float hfrom = ((RectTransform) to.parent).rect.height;
			Vector2 cfrom = new Vector2((to.anchorMin.x + to.anchorMax.x) * wfrom / 2, (to.anchorMin.y + to.anchorMax.y) * hfrom / 2);

			return cto + vto - cfrom;
		}
	}
}