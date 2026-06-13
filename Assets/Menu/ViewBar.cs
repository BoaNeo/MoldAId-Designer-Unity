using UIComponents;
using UnityEngine;

namespace Menu
{
	[RequireComponent(typeof(Transition))]
	public class ViewBar : MonoBehaviour
	{
		private static ViewBar _active;
		private Transition _transition;

		private void Awake()
		{
			_transition = GetComponent<Transition>();
		}

		private void Start()
		{
			//_transition.HiddenSize = ((RectTransform) transform.GetChild(0).transform).rect.size;
			_transition.Hide();
		}

		public void OnShow()
		{
			if(_active!=null)
				_active._transition.SetVisible(false);
			if (_active == this)
				_active = null;
			else
				_active = this;
			if(_active!=null)
				_active._transition.SetVisible(true);
		}
	}
}