using Files;
using UnityEngine;

namespace Visuals
{
	public class PrintBox : MonoBehaviour
	{
		public enum PrintBoxMode{Box,Floor,Wireframe, Default}

		[SerializeField] private Transform _minX;
		[SerializeField] private Transform _maxX;
		[SerializeField] private Transform _minZ;
		[SerializeField] private Transform _maxZ;
		[SerializeField] private Transform _minY;
		[SerializeField] private Transform _maxY;
		[SerializeField] private Transform _light;

		[SerializeField] private Material _wireMaterial;
		[SerializeField] private Material _dotMaterial;

		private PrinterFile _printer;
		private PrintBoxMode _mode;

		private void Awake()
		{
			SetActive(false);
		}

		private void SetActive(bool b, PrintBoxMode mode = PrintBoxMode.Default)
		{
			bool fullBox = mode == PrintBoxMode.Box || mode == PrintBoxMode.Wireframe;
			_minX.gameObject.SetActive(b && fullBox);
			_maxX.gameObject.SetActive(b && fullBox);
			_minY.gameObject.SetActive(b && fullBox);
			_maxY.gameObject.SetActive(b && fullBox);
			_minZ.gameObject.SetActive(b);
			_maxZ.gameObject.SetActive(b && fullBox);
		}

		public void Configure(PrinterFile p, PrintBoxMode mode = PrintBoxMode.Default)
		{
			if (p == null)
			{
				SetActive(false);
				return;
			}
			
			_printer = p;
			_mode = mode;

			if (mode == PrintBoxMode.Default)
				mode = (PrintBoxMode) PreferencesFile.current.printBoxMode;
			
			SetActive(true, mode);
			// The quad we use is 2x2 (from -1 to 1 in all directions)
			float sx = p.volume.x;
			float sy = p.volume.y;
			float sz = p.volume.z;
			
			float x = sx / 2;
			float y = sy / 2;
			float z = sz / 2;

			_minX.position = new Vector3(-x, 0, z);
			_maxX.position = new Vector3(x, 0, z);
			_minY.position = new Vector3(0, -y, z);
			_maxY.position = new Vector3(0, y, z);
			_minZ.position = new Vector3(0, 0, -.005f);
			_maxZ.position = new Vector3(0, 0, 2*z);
			_minX.localScale = new Vector3(sz, sy, 1);
			_maxX.localScale = new Vector3(sz, sy, 1);
			_minY.localScale = new Vector3(sx, sz, 1);
			_maxY.localScale = new Vector3(sx, sz, 1);
			_minZ.localScale = new Vector3(sx, sy, 1);
			_maxZ.localScale = new Vector3(sx, sy, 1);

			_light.position = new Vector3(0, 0, z);
			
			sx /= 50;
			sy /= 50;
			sz /= 50;

			bool wire = mode == PrintBoxMode.Wireframe;
			
			SetUV(_minX, new Vector2(sz,sy), new Vector2(0*sz,-.5f*sy), wire);
			SetUV(_maxX, new Vector2(sz,sy), new Vector2(0*sz,-.5f*sy), wire);
			SetUV(_minY, new Vector2(sx,sz), new Vector2(-.5f*sx,0*sz), wire);
			SetUV(_maxY, new Vector2(sx,sz), new Vector2(-.5f*sx,0*sz), wire);
			SetUV(_minZ, new Vector2(sx,sy), new Vector2(-.5f*sx,-.5f*sy), false);
			SetUV(_maxZ, new Vector2(sx,sy), new Vector2(-.5f*sx,-.5f*sy), wire);

			PreferencesFile.current.OnPreferencesApplied -= OnPreferencesApplied;
			PreferencesFile.current.OnPreferencesApplied += OnPreferencesApplied;
		}

		public void SetMode(PrintBoxMode mode)
		{
			Configure(_printer, mode);
		}

		public void OnPreferencesApplied()
		{
			Configure(_printer, _mode);
		}

		private void SetUV(Transform t, Vector2 s, Vector2 o, bool wire)
		{
			MeshRenderer mr = t.GetComponent<MeshRenderer>();
			if (wire)
			{
				mr.material = _wireMaterial;
				mr.material.mainTextureScale = Vector2.one;
				mr.material.mainTextureOffset = Vector2.zero;
			}
			else
			{
				mr.material = _dotMaterial;
				mr.material.mainTextureScale = s;
				mr.material.mainTextureOffset = o;
			}
		}
	}
}