using UIComponents;
using UnityEngine;
using Utility;

namespace Dialogs
{
	public class DialogManager : MonoBehaviour
	{
		[SerializeField] private Transition _curtainPrefab;
		[SerializeField] private PooledObject[] _dialogPrefabs;

		public static T Show<T>(bool modal=true) where T:Dialog
		{
			return Singleton<DialogManager>.Instance.FindAndInstantiatePrefab<T>(modal);
		}

		private T FindAndInstantiatePrefab<T>(bool modal) where T:Dialog
		{
			Transition curtain = null;
			if (modal)
			{
				curtain = Instantiate(_curtainPrefab, ((RectTransform)transform).position, Quaternion.identity, transform );
				curtain.SetVisible(true);
			}

			foreach (PooledObject prefab in _dialogPrefabs)
			{
				if (prefab is T)
				{
					Dialog dialog =  ObjectPool.Instantiate((T)prefab, ((RectTransform)transform).position, Quaternion.identity, transform );
					dialog.curtain = curtain;
					Transition transition = dialog.GetComponent<Transition>();
					if (transition == null)
					{
						transition = dialog.gameObject.AddComponent<Transition>();
						transition.HideAt = Transition.HideLocation.ScreenLowerEdge;
						dialog.transition = transition;
					}
					transition.SetVisible(true);
					return (T)dialog;
				}
			}
			return null;
		}
	}
}