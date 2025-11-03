using System;
using GorillaTag;
using UnityEngine;

[GTStripGameObjectFromBuild("!GT_AUTOMATED_PERF_TEST")]
public class PerfTestObjectDestroyer : MonoBehaviour
{
	private void Start()
	{
		Object.DestroyImmediate(base.gameObject, true);
	}
}
