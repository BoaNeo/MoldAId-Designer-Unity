using UnityEngine;
using UnityEngine.UI;

namespace PropertySheet
{
	public class PropertyUIBool : PropertyUI<bool>
	{
		[SerializeField] private Button _selectable;
		[SerializeField] private GameObject _check;

		public override Selectable firstSelectable => _selectable;
		public override Selectable lastSelectable => _selectable;

		protected override void Configure()
		{
		}

		protected override void ShowValueInUI(bool value)
		{
			_check.gameObject.SetActive(value);
		}

		protected override bool GetValueFromUI()
		{
			return _check.gameObject.activeSelf;
		}

		public void OnCheck()
		{
			ShowValueInUI(!GetValueFromUI());
			OnChange();
		}
	}
}