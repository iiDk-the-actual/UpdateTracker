using System;
using GorillaTag;
using UnityEngine;
using UnityEngine.Events;

public class ThermalReceiver : MonoBehaviour, IDynamicFloat, IResettableItem
{
	public float Farenheit
	{
		get
		{
			return this.celsius * 1.8f + 32f;
		}
	}

	public float floatValue
	{
		get
		{
			return this.celsius;
		}
	}

	protected void Awake()
	{
		this.defaultCelsius = this.celsius;
		this.wasAboveThreshold = false;
	}

	protected void OnEnable()
	{
		ThermalManager.Register(this);
	}

	protected void OnDisable()
	{
		this.wasAboveThreshold = false;
		ThermalManager.Unregister(this);
	}

	public void ResetToDefaultState()
	{
		this.celsius = this.defaultCelsius;
	}

	public float radius = 0.2f;

	[Tooltip("How fast the temperature should change overtime. 1.0 would be instantly.")]
	public float conductivity = 0.3f;

	[Tooltip("Optional: Fire events if temperature goes below or above this threshold - Celsius")]
	public float temperatureThreshold;

	[Space]
	public UnityEvent OnAboveThreshold;

	public UnityEvent OnBelowThreshold;

	[DebugOption]
	public float celsius;

	public bool wasAboveThreshold;

	private float defaultCelsius;
}
