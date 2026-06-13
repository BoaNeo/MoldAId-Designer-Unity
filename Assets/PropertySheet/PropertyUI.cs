using System;
using FeatureGraph;
using TMPro;
using UIComponents;
using Undo;
using UnityEngine;
using UnityEngine.UI;

namespace PropertySheet
{
	public abstract class PropertyUI : GridCell
	{
		// ReSharper disable Unity.PerformanceAnalysis
		public abstract void Setup(PropertyData prop, Action onExplicitRefresh);
		public abstract Selectable lastSelectable { get; }
		public abstract Selectable firstSelectable { get; }
	}
	public abstract class PropertyUI<T> : PropertyUI
	{
		[SerializeField] protected TMP_Text _label;

		public PropertyData info { get; set; }

		private DataRef<T> _dataref;

		public override void Setup( PropertyData prop, Action onExplicitRefresh)
		{
			info = prop;

			_label.text = info.name;

			Configure();

			var p = info.value;

			if (p is DataRef<T>)
			{
				_dataref = (DataRef<T>)p;
				ShowValueInUI(_dataref.value);
			}
			else
			{
				ShowValueInUI((T)info.value);
			}
		}

		public void OnChange()
		{
			try
			{
				T v = GetValueFromUI();
				if (info.value is DataRef<T>)
				{
					T oldvalue = _dataref.value;
					if ( !Equals(oldvalue,v))
					{
						UndoManager.Append(() =>
						{
							_dataref.Set(v);
						},() =>
						{
							_dataref.Set(oldvalue);
						});
					}
				}
				else
				{
					object oldvalue = info.value;
					if (!Equals(oldvalue, v))
					{
						UndoManager.Append(() =>
						{
							info.SetValue(v);
						}, () =>
						{
							info.SetValue(oldvalue);
						});
					}
				}
			}
			catch (Exception e)
			{
				Debug.LogError($"Failed to get value from UI: {e}");
			}
		}

		protected float Clamp(float v)
		{
			return Mathf.Clamp(v, info.attribute.min, info.attribute.max);
		}

		protected abstract void Configure();
		protected abstract void ShowValueInUI(T value);
		protected abstract T GetValueFromUI();
	}
}