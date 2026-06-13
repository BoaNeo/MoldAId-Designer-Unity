using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Utility;

namespace Gizmos
{
	public class MarkerManager : MonoBehaviour
	{
		[SerializeField] private Marker _markerPrefab;
		[SerializeField] private LineRenderer _line;
		[SerializeField] private TMP_Text _infoText;
		[SerializeField] private Canvas _infoUI;

		private Marker _currentMarker;
		private List<Marker> _markers = new();
		private Vector3[] _pts;

		private void Awake()
		{
			UpdateLine();
		}

		public bool UpdateCurrent(RaycastHit hit)
		{
			bool result = false;
			if (_currentMarker != null)
			{
				result = _currentMarker.UpdateHit(hit);
				UpdateLine();
			}
			return result;
		}

		private void UpdateLine()
		{
			_line.enabled = _markers.Count >= 2;
			if (!_line.enabled)
			{
				_infoUI.gameObject.SetActive(false);
				return;
			}

			float l=0;
			Vector3 c = Vector3.zero;
			if (_pts == null || _pts.Length != _markers.Count)
				_pts = new Vector3[_markers.Count];
			for (int i = 0; i < _markers.Count; i++)
			{
				// TODO: Skip disconnected and start new line segment
				_pts[i] = _markers[i].transform.position;
				if (i > 0)
					l += (_pts[i] - _pts[i - 1]).magnitude;
				c += _pts[i];
			}
			_line.positionCount = _pts.Length;
			_line.SetPositions( _pts );
			c /= _pts.Length;

			if (l > 0)
			{
				_infoUI.gameObject.SetActive(true);
				_infoUI.transform.position = c;
				_infoText.text = $"{l:F2}mm";
			}
			else
				_infoUI.gameObject.SetActive(false);
		}

		public Marker Push(Marker.Flags flags=0)
		{
			Marker marker = ObjectPool.Instantiate(_markerPrefab);
			marker.flags = flags;
			if ( (flags & Marker.Flags.KeepCurrent)==0 )
				_currentMarker = marker;
			_markers.Add(marker);

			UpdateLine();

			return marker;
		}

		public Marker Pop()
		{
			if (_markers.Count > 0)
			{
				Marker last = _markers[_markers.Count-1];
				_markers.RemoveAt(_markers.Count-1);
				if (last == _currentMarker)
					_currentMarker = null;
				ObjectPool.Recycle(last);

				UpdateLine();
				
				return last;
			}

			return null;
		}

		public void Clear()
		{
			while (_markers.Count > 0)
				Pop();
		}
	}
}