using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Utility;

namespace UIComponents {
	public class GridOnDemand : GridCell {
		[SerializeField] private GridBuilder _content;
		[SerializeField] private bool _vertical;
		[SerializeField] private bool _horizontal;
		[SerializeField] private bool _disableScroll;

		private GridCell _prefabItem;

		private Vector2 _dragPos;
		private Vector2 _lastDragDelta;
		private Vector2 _currentOffset;
		private Vector2 _targetOffset;

		private bool _drag;
		private float _visible = 1.0f;

		private int _columns;
		private int _rows;

		private int _visRows;
		private int _visCols;

		private float _cellW;
		private float _cellH;

		private Action<int, int, GridCell> _setup;
		private Action<int, int> _click;

		private GridCell[] _currentItems;
		private Canvas _canvas;

		void Awake() {
			if (!_disableScroll) {
				DragTrigger trigger = gameObject.GetComponent<DragTrigger>();
				if (trigger == null)
					trigger = gameObject.AddComponent<DragTrigger>();
				trigger._target = this;
			}
		}

		public void Setup<T>(T cellPrefab, int columns, int rows, Action<int, int, T> onSetup) where T : GridCell {
			RectTransform rect = (RectTransform) cellPrefab.transform;
			_cellW = (rect.anchorMax.x * width + rect.offsetMax.x) - (rect.anchorMin.x * width + rect.offsetMin.x);
			_cellH = (rect.anchorMax.y * height + rect.offsetMax.y) - (rect.anchorMin.y * height + rect.offsetMin.y);

			_visRows = (int) (Mathf.Ceil(height / _cellH) + 1);
//    if (_visRows > rows)
//      _visRows = rows;

			_visCols = (int) (Mathf.Ceil(width / _cellW) + 1);
//    if (_visCols > columns)
//      _visCols = columns;

			_setup = (int column, int row, GridCell cell) => { onSetup(column, row, (T) cell); };

			_rows = rows;
			_columns = columns;

			if (_prefabItem == cellPrefab && _currentItems != null && _visRows * _visCols == _currentItems.Length)
				return;

			_prefabItem = cellPrefab;
			_currentItems = new T[_visRows * _visCols];
			_content.BeginUpdate();
			for (int y = 0; y < _visRows; y++) {
				for (int x = 0; x < _visCols; x++)
					_currentItems[x + y * _visCols] = _content.AddCell(_prefabItem);
				_content.EndRow();
			}

			_content.EndUpdate();
		}

		public void SetDataSize(int columns,int rows)
		{
			_rows = rows;
			_columns = columns;
		}

		void Update() {
			if (_canvas == null)
				_canvas = GetComponentInParent<Canvas>();

			if (_setup == null)
				return;

			if (_visible > 0) {
				FadeTo(_visible);
				if (!_drag) {
					Vector2 delta = _targetOffset - _currentOffset;
					Vector2 speed = 5 * delta;
					delta.x = Mathf.Abs(delta.x);
					delta.y = Mathf.Abs(delta.y);
					if (delta.x < 0.001f)
						delta.x = 0;
					if (delta.y < 0.001f)
						delta.y = 0;

					if (Mathf.Abs(speed.x) < Mathf.Abs(_lastDragDelta.x))
						speed.x = _lastDragDelta.x;
					if (Mathf.Abs(speed.y) < Mathf.Abs(_lastDragDelta.y))
						speed.y = _lastDragDelta.y;

					delta.x = Mathf.Clamp(speed.x * Time.deltaTime, -delta.x, delta.x);
					delta.y = Mathf.Clamp(speed.y * Time.deltaTime, -delta.y, delta.y);

					_currentOffset += delta;
					_lastDragDelta = _lastDragDelta * Mathf.Clamp(.9f - Time.deltaTime, 0.0f, 1.0f);
				}
			}
			else {
				FadeTo(0.0f);
			}

			_content.rectTransform.anchoredPosition = new Vector2(_horizontal ? _currentOffset.x % _cellW : 0, _vertical ? _currentOffset.y % _cellH : 0);

			for (int i = 0; i < _currentItems.Length; i++) {
				int offset_y = (int) (_currentOffset.y / _cellH) + i / _visCols;
				int offset_x = (int) (-_currentOffset.x / _cellW) + i % _visCols;

				if (offset_x >= 0 && offset_x < _columns && offset_y >= 0 && offset_y < _rows) {
					_currentItems[i].gameObject.SetActive(true);
					_setup(offset_x, offset_y, _currentItems[i]);
				}
				else
					_currentItems[i].gameObject.SetActive(false);
			}
		}

		void FadeTo(float s) {
			s = Tween.easeOutExpo(transform.localScale.x, s, Time.deltaTime);
			transform.localScale = new Vector3(s, s, 1);
		}

		public float visible {
			get { return _visible; }
			set { _visible = Mathf.Clamp(value, 0.0f, 1.0f); }
		}

		private class DragTrigger : EventTrigger {
			public GridOnDemand _target;

			public override void OnBeginDrag(PointerEventData evt) {
				_target.OnBeginDrag(evt);
			}

			public override void OnDrag(PointerEventData evt) {
				_target.OnDrag(evt);
			}

			public override void OnEndDrag(PointerEventData evt) {
				_target.OnEndDrag(evt);
			}
		}

		public void OnBeginDrag(PointerEventData evt) {
			_drag = true;
			_dragPos = evt.position / _canvas.transform.localScale.x; //AngleFromPoint(evt.position);
		}

		public void OnDrag(PointerEventData evt) {
			Vector2 a = evt.position / _canvas.transform.localScale.x; // AngleFromPoint(evt.position);

			Vector2 da = a - _dragPos;
			_dragPos = a;

			_currentOffset += da;

			_lastDragDelta = da;
		}

		public void OnEndDrag(PointerEventData evt) {
			_drag = false;
			float viswidth = Mathf.Min(((RectTransform) transform).rect.width, _columns * _prefabItem.width);
			_targetOffset.x = (float) Math.Round(_currentOffset.x + 10 * _lastDragDelta.x, MidpointRounding.AwayFromZero);
			_targetOffset.x = Mathf.Clamp(_targetOffset.x, -_cellW * _columns + viswidth, 0);

			float visheight = Mathf.Min(((RectTransform) transform).rect.height, _rows * _prefabItem.height);

			_targetOffset.y = (float) Math.Round(_currentOffset.y + 10 * _lastDragDelta.y, MidpointRounding.AwayFromZero);
			_targetOffset.y = Mathf.Clamp(_targetOffset.y, 0, _cellH * _rows - visheight);
		}

		public void ScrollToEnd() {
			_targetOffset = new Vector2(-Mathf.Clamp(_columns * _prefabItem.width - width, 0, float.MaxValue), Mathf.Clamp(_rows * _prefabItem.height - height, 0, float.MaxValue));
		}

		public void ScrollToTop() {
			_targetOffset = new Vector2(0, 0);
		}

		public void ScrollTo(int column, int row) {
			_targetOffset = new Vector2(-_prefabItem.width * column, -_prefabItem.height * row);
		}
	}
}