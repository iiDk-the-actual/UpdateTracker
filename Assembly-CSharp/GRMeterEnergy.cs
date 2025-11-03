using System;
using UnityEngine;

public class GRMeterEnergy : MonoBehaviour
{
	public void Awake()
	{
	}

	public void Refresh()
	{
		float num = 0f;
		if (this.tool != null && this.tool.GetEnergyMax() > 0)
		{
			num = (float)this.tool.energy / (float)this.tool.GetEnergyMax();
		}
		num = Mathf.Clamp(num, 0f, 1f);
		GRMeterEnergy.MeterType meterType = this.meterType;
		if (meterType == GRMeterEnergy.MeterType.Linear || meterType != GRMeterEnergy.MeterType.Radial)
		{
			this.meter.localScale = new Vector3(1f, num, 1f);
			return;
		}
		float num2 = Mathf.Lerp(this.angularRange.x, this.angularRange.y, num);
		Vector3 zero = Vector3.zero;
		zero[this.rotationAxis] = num2;
		this.meter.localRotation = Quaternion.Euler(zero);
	}

	public GRTool tool;

	public Transform meter;

	public Transform chargePoint;

	public GRMeterEnergy.MeterType meterType;

	public Vector2 angularRange = new Vector2(-45f, 45f);

	[Range(0f, 2f)]
	public int rotationAxis;

	public enum MeterType
	{
		Linear,
		Radial
	}
}
