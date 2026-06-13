using System.Collections.Generic;
using Menu;
using UIComponents;
using UnityEngine;
using UnityEngine.UI;
using Visuals;

namespace StageBar
{
	public class StageBarManager : MonoBehaviour, ISideBar
	{
		[SerializeField] private StageBarUI _groupPrefab;
		[SerializeField] private GridBuilder _grid;
		[SerializeField] private ScrollRect _scrollRect;
		[SerializeField] private Image _bottomFade;

		private Stage _selected;
		private List<Stage> _stages = new List<Stage>();
		private IStageContext _context;


		public void SetContext(IStageContext context)
		{
			_context = context;
		}

		public void AddStage(Stage stage)
		{
			_stages.Add(stage);
		}

		public void SelectStage(Stage stage)
		{
			if (_selected != null && _selected!=stage)
			{
				_selected.DeActivate();
			}
			_selected = stage;
			if (_selected != null)
			{
				_selected.context = _context;
			}
			RefreshStages();
		}

		public void RefreshStages()
		{
			if (_selected == null)
			{
				SelectStage(_stages[0]);
				return;
			}

			_context.DisableAllFeatures();
			_context.HideAllGizmos();
			_context.HideAllVisuals();
			_context.ConfigurePrintBox(PrintBox.PrintBoxMode.Default);
			_selected.Activate();

			_grid.BeginUpdate();
			for (int i=0;i<_stages.Count;i++)
			{
				Stage stage = _stages[i];
				StageBarUI bar = _grid.AddRow(_groupPrefab, item => item.Setup(stage.name, _selected==stage, () =>
				{
					SelectStage(stage);
				}));
				stage.context = _context;
				stage.BuildUI(bar);
			}
			_grid.EndUpdate();
		}

		private void Update()
		{
			_bottomFade.gameObject.SetActive( _scrollRect.content.rect.height>_scrollRect.viewport.rect.height && _scrollRect.verticalNormalizedPosition>0.0f );
		}

		public float preferredHeight => _grid.layoutHeight;
		public float height { get => ((RectTransform) transform).rect.height; set => ((RectTransform)transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value); }
	}
}