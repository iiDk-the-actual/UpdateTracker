using System;
using GorillaTag;
using UnityEngine;

[GTStripGameObjectFromBuild("!GT_AUTOMATED_PERF_TEST")]
public class PerfTestFPSCaptureController : MonoBehaviour
{
	[SerializeField]
	private SerializablePerformanceReport<ScenePerformanceData> performanceSummary;
}
