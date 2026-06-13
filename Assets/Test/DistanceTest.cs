using TMPro;
using UnityEngine;
using Utility;

namespace Test
{
	public class DistanceTest : MonoBehaviour
	{
		[SerializeField] private Transform plane;
		[SerializeField] private Transform vertex;
		[SerializeField] private TMP_Text distance;

		private void Update()
		{
			distance.text = vertex.position.DistanceToPlane(plane.position, plane.forward).ToString();
		}
	}
}