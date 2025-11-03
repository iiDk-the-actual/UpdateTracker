using System;
using UnityEngine;

namespace Critters.Scripts
{
	public class CrittersSpawnPoint : MonoBehaviour
	{
		private void OnDrawGizmos()
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawSphere(base.transform.position, 0.1f);
		}
	}
}
