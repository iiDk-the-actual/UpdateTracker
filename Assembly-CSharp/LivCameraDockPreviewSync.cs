using System;
using Docking;
using UnityEngine;

[ExecuteAlways]
public class LivCameraDockPreviewSync : MonoBehaviour
{
	private LivCameraDock dock;

	private Camera parentCamera;

	private float _lastCameraFOV = -1f;

	private float _lastDockFOV = -1f;
}
