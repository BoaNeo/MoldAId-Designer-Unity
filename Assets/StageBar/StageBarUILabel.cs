using TMPro;
using UIComponents;
using UnityEngine;

namespace StageBar
{
	public class StageBarUILabel : GridCell
	{
		[SerializeField] private TMP_Text _label;

		public void Setup(string title)
		{
			_label.text = title;
		}
	}
}