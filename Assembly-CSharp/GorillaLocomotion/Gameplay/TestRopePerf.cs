using System;
using System.Collections;
using UnityEngine;

namespace GorillaLocomotion.Gameplay
{
	public class TestRopePerf : MonoBehaviour
	{
		private IEnumerator Start()
		{
			yield break;
		}

		[SerializeField]
		private GameObject ropesOld;

		[SerializeField]
		private GameObject ropesCustom;

		[SerializeField]
		private GameObject ropesCustomVectorized;
	}
}
