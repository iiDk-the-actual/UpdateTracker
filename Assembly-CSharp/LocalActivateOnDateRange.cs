using System;
using GorillaTag;
using UnityEngine;

public class LocalActivateOnDateRange : MonoBehaviour
{
	private void Awake()
	{
		GameObject[] array = this.gameObjectsToActivate;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(false);
		}
	}

	private void OnEnable()
	{
		this.InitActiveTimes();
	}

	private void InitActiveTimes()
	{
		this.activationTime = new DateTime(this.activationYear, this.activationMonth, this.activationDay, this.activationHour, this.activationMinute, this.activationSecond, DateTimeKind.Utc);
		this.deactivationTime = new DateTime(this.deactivationYear, this.deactivationMonth, this.deactivationDay, this.deactivationHour, this.deactivationMinute, this.deactivationSecond, DateTimeKind.Utc);
	}

	private void LateUpdate()
	{
		DateTime utcNow = DateTime.UtcNow;
		this.dbgTimeUntilActivation = (this.activationTime - utcNow).TotalSeconds;
		this.dbgTimeUntilDeactivation = (this.deactivationTime - utcNow).TotalSeconds;
		bool flag = utcNow >= this.activationTime && utcNow <= this.deactivationTime;
		if (flag != this.isActive)
		{
			GameObject[] array = this.gameObjectsToActivate;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(flag);
			}
			this.isActive = flag;
		}
	}

	[Header("Activation Date and Time (UTC)")]
	public int activationYear = 2023;

	public int activationMonth = 4;

	public int activationDay = 1;

	public int activationHour = 7;

	public int activationMinute;

	public int activationSecond;

	[Header("Deactivation Date and Time (UTC)")]
	public int deactivationYear = 2023;

	public int deactivationMonth = 4;

	public int deactivationDay = 2;

	public int deactivationHour = 7;

	public int deactivationMinute;

	public int deactivationSecond;

	public GameObject[] gameObjectsToActivate;

	private bool isActive;

	private DateTime activationTime;

	private DateTime deactivationTime;

	[DebugReadout]
	public double dbgTimeUntilActivation;

	[DebugReadout]
	public double dbgTimeUntilDeactivation;
}
