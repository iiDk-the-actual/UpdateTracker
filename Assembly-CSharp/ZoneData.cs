using System;
using UnityEngine;

[Serializable]
public class ZoneData
{
	public GTZone zone;

	public string sceneName;

	public float CameraFarClipPlane = 500f;

	public GameObject[] rootGameObjects;

	[NonSerialized]
	public bool active;
}
