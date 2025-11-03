using System;
using UnityEngine;

public class ThermalSourceVolume : MonoBehaviour
{
	protected void OnEnable()
	{
		ThermalManager.Register(this);
	}

	protected void OnDisable()
	{
		ThermalManager.Unregister(this);
	}

	[Tooltip("Temperature in celsius. Default is 20 which is room temperature.")]
	public float celsius = 20f;

	public float innerRadius = 0.1f;

	public float outerRadius = 1f;
}
