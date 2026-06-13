using System;
using System.Reflection;
using StageBar;
using UnityEngine;
using UnityEngine.UI;

namespace PropertySheet
{
	public class PropertyUIAction : PropertyUI<string>
	{
		public static IStageContext context { get; set; }

		[SerializeField] private Button _button;

		private Action _onRefresh;

		public override Selectable lastSelectable => _button;
		public override Selectable firstSelectable => _button;
		
		protected override void Configure() { }
		protected override void ShowValueInUI(string value) { }
		protected override string GetValueFromUI() { return ""; }

		public override void Setup(PropertyData prop, Action onExplicitRefresh)
		{
			base.Setup(prop,onExplicitRefresh);
			_label.text = prop.name;
		}

		public void OnClick()
		{
			ParameterInfo[] parameterInfos = info.method.GetParameters();
			object[] parameters = new object[parameterInfos.Length];
			for (int i=0;i<parameterInfos.Length;i++)
			{
				if (typeof(IStageContext).IsAssignableFrom(parameterInfos[i].ParameterType))
					parameters[i] = context;
			}

			info.method.Invoke(info.target, parameters);
			if(_onRefresh!=null)
				_onRefresh();
		}
	}
}