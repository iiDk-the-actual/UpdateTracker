using System;
using UnityEngine;

public class SpinRotation : MonoBehaviour, ITickSystemTick
{
	public bool TickRunning { get; set; }

	public void Tick()
	{
		base.transform.localRotation = Quaternion.Euler(this.rotationPerSecondEuler * (Time.time - this.baseTime)) * this.baseRotation;
	}

	private void Awake()
	{
		this.baseRotation = base.transform.localRotation;
	}

	private void OnEnable()
	{
		TickSystem<object>.AddTickCallback(this);
		this.baseTime = Time.time;
	}

	private void OnDisable()
	{
		TickSystem<object>.RemoveTickCallback(this);
	}

	[SerializeField]
	private Vector3 rotationPerSecondEuler;

	private Quaternion baseRotation;

	private float baseTime;
}
