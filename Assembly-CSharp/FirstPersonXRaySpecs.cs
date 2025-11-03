using System;
using UnityEngine;

public class FirstPersonXRaySpecs : MonoBehaviour
{
	private void OnEnable()
	{
		GorillaBodyRenderer.SetAllSkeletons(true);
	}

	private void OnDisable()
	{
		GorillaBodyRenderer.SetAllSkeletons(false);
	}
}
