using System;
using UnityEngine;

[ExecuteInEditMode]
public class SkyboxSettings : MonoBehaviour
{
	private void OnEnable()
	{
		if (this._skyMaterial)
		{
			RenderSettings.skybox = this._skyMaterial;
		}
	}

	[SerializeField]
	private Material _skyMaterial;
}
