using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Utility;

public class WorldInputManager : MonoBehaviour
{
	public interface IWorldInputHandler
	{
		bool OnHover(RaycastHit obj);
		bool OnSelect(RaycastHit obj, PointerEventData evt);
		void OnDrag(RaycastHit obj, Vector2 deltaScreen);
		bool OnRelease(RaycastHit source, RaycastHit target);
		LayerMask GetSelectableLayerMask();
		string[] GetTagPriorities();
		bool GetNeedsHoverEvent();
	}

	public static WorldInputManager instance => Singleton<WorldInputManager>.Instance;

	private List<IWorldInputHandler> _handlers = new List<IWorldInputHandler>();
	private IWorldInputHandler _currentHandler;
	private RaycastHit _source;
	private Vector2 _screenPos;

	public static bool panModifier => Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) || Input.GetMouseButton(2);
	public static bool rotateModifier => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
	public bool hasFocus { get; private set; }

	public void RegisterInputHandler(IWorldInputHandler handler)
	{
		_handlers.Add(handler);
	}

	private void Update()
	{
		if (panModifier || rotateModifier)
			return;
		foreach (IWorldInputHandler handler in _handlers)
		{
			if (handler.GetNeedsHoverEvent())
			{
				RayCast(Input.mousePosition, out RaycastHit hit, handler.GetSelectableLayerMask(), handler.GetTagPriorities());
				handler.OnHover(hit);
			}
		}
	}

	public void OnPointerEnter(BaseEventData evt)
	{
		hasFocus = true;
	}

	public void OnPointerExit(BaseEventData evt)
	{
		hasFocus = false;
	}

	public void OnPointerDown(BaseEventData evt)
	{
		if (panModifier || rotateModifier)
			return;
		OnPointerUp(evt);

		foreach (IWorldInputHandler handler in _handlers)
		{
			RayCast(evt, out RaycastHit hit, handler.GetSelectableLayerMask(), handler.GetTagPriorities());

			if (handler.OnSelect(hit, ((PointerEventData)evt) ))
			{
				_currentHandler = handler;
				_source = hit;
				_screenPos = ((PointerEventData)evt).pointerCurrentRaycast.screenPosition;
				return;
			}
		}
	}

	public void OnPointerUp(BaseEventData evt)
	{
		if (panModifier || rotateModifier)
			return;
		IWorldInputHandler currentHandler = _currentHandler;
		_currentHandler = null;

		// If there's a current handler, give it a chance to handle the release event
		if (currentHandler != null)
		{
			RayCast(evt, out RaycastHit hit, currentHandler.GetSelectableLayerMask(), currentHandler.GetTagPriorities());
			currentHandler.OnRelease(_source, hit);
		}
					
		// See if there are other handlers who wish to deal with a release
		foreach (IWorldInputHandler handler in _handlers)
		{
			RayCast(evt, out RaycastHit hit, handler.GetSelectableLayerMask(), handler.GetTagPriorities());
			if (handler.OnRelease(_source,hit))
				return;
		}
	}

	public void OnDrag(BaseEventData evt)
	{
		if (panModifier || rotateModifier)
			return;
		if (_currentHandler!=null)
		{
			Vector2 sp = ((PointerEventData)evt).pointerCurrentRaycast.screenPosition;
			Vector2 sd = (sp - _screenPos);
			_screenPos = sp;
			_currentHandler.OnDrag(_source, sd);
		}
	}

	private bool RayCast(BaseEventData evt, out RaycastHit hit, LayerMask layers, string[] tags)
	{
		Vector2 screenpos = ((PointerEventData)evt).pointerCurrentRaycast.screenPosition;
		return RayCast(screenpos, out hit, layers, tags);
	}

	private bool RayCast(Vector2 mp, out RaycastHit hit, LayerMask layers, string[]tags)
	{
		Ray ray = Camera.main.ScreenPointToRay(mp);

		bool foundbackup = false;
		RaycastHit backup = default;
		while (Physics.Raycast(ray, out hit, 10000, layers))
		{
			string tag = hit.collider.gameObject.name;
			if (tags==null || tags.Length == 0 || tags.Length==1 && tags[0]==tag)
				return true;
			bool implicitlyincluded = true;
			foreach (string t in tags)
			{
				if (t == tag)
					return true;
				if (t.StartsWith("!"))
				{
					if (t.Substring(1) == tag)
					{
						implicitlyincluded = false;
						break;
					}
				}
				else if (t.StartsWith("~"))
				{
					if(t.Substring(1) == tag)
					{
						if (!foundbackup)
						{
							foundbackup = true;
							backup = hit;
						}
						implicitlyincluded = false;
						break;
					}
				}
				else
					implicitlyincluded = false;
			}
			if (implicitlyincluded) // We were given a list that didn't contain an exact match, but also no low prio or explicitly ignored tags, so assume that what we found is ok
				return true;
			// Try again from inside the object we found to see if there's something else hiding there
			ray.origin = hit.point + 0.001f * ray.direction; 
		}

		if (foundbackup)
		{
			hit = backup;
			return true;
		}
		return false;
	}
}