using System;
using TMPro;
using UIComponents;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Dialogs.CrossSection
{
	public class InputLineUI : MonoBehaviour
	{
		[SerializeField] private TMP_Text _message;
		[SerializeField] private Transition _uicontainer;
		[SerializeField] private RectTransform _startPt;
		[SerializeField] private RectTransform _endPt;
		[SerializeField] private Image _line;
		
		private Action<Vector2,Vector3> _callback;
		
		private void Update()
		{
			if(_line.gameObject.activeSelf)
				Draw();
		}

		private void Draw()
		{
			Vector3 p0 = _startPt.anchoredPosition;
			Vector3 p1 = _endPt.anchoredPosition;
			
			float l = (p1 - p0).magnitude;
			if (l > 1)
			{
				_line.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,l);
				_line.rectTransform.anchoredPosition = (p1 + p0) / 2;
				Quaternion q = Quaternion.LookRotation(p1 - p0, Vector3.forward);
				_line.rectTransform.rotation = Quaternion.Euler(0,p1.x<p0.x ? 0:180,q.eulerAngles.x);
			}
		}

		public void GetLine(string message, Action<Vector2, Vector3> action)
		{
			gameObject.SetActive(true);
			_callback = action;
			_message.text = message;
			_uicontainer.Show();
			SetLineEnabled(false);
		}

		private void SetLineEnabled(bool enabled)
		{
			_startPt.gameObject.SetActive(enabled);
			_endPt.gameObject.SetActive(enabled);
			_line.gameObject.SetActive(enabled);
		}

		public void OnDragStart(BaseEventData e)
		{
			SetLineEnabled(true);
			PointerEventData pe = (PointerEventData) e;
			_startPt.anchoredPosition = pe.position;
			_endPt.anchoredPosition = _startPt.anchoredPosition;
		}

		public void OnDrag(BaseEventData e)
		{
			PointerEventData pe = (PointerEventData) e;
			_endPt.anchoredPosition = pe.position;
		}

		public void OnDragEnd(BaseEventData e)
		{
			SetLineEnabled(false);
			PointerEventData pe = (PointerEventData) e;
			_callback(_startPt.position, _endPt.position);
			_uicontainer.SetVisible(false, 0, -1, () =>
			{
				gameObject.SetActive(false);
			} );
			
		}
	}
}