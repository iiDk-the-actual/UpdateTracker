using System;
using UnityEngine;

public class MainCamera : MonoBehaviourStatic<MainCamera>
{
	public static implicit operator Camera(MainCamera mc)
	{
		return mc.camera;
	}

	public Camera camera;
}
