using System;
using UnityEngine;

public class GorillaColorSlider : MonoBehaviour
{
	private void Start()
	{
		if (!this.setRandomly)
		{
			this.startingLocation = base.transform.position;
		}
	}

	public void SetPosition(float speed)
	{
		float num = this.startingLocation.x - this.zRange / 2f;
		float num2 = this.startingLocation.x + this.zRange / 2f;
		float num3 = (speed - this.minValue) * (num2 - num) / (this.maxValue - this.minValue) + num;
		base.transform.position = new Vector3(num3, this.startingLocation.y, this.startingLocation.z);
		this.valueImReporting = this.InterpolateValue(base.transform.position.x);
	}

	public float InterpolateValue(float value)
	{
		float num = this.startingLocation.x - this.zRange / 2f;
		float num2 = this.startingLocation.x + this.zRange / 2f;
		return (value - num) / (num2 - num) * (this.maxValue - this.minValue) + this.minValue;
	}

	public void OnSliderRelease()
	{
		if (this.zRange != 0f && (base.transform.position - this.startingLocation).magnitude > this.zRange / 2f)
		{
			if (base.transform.position.x > this.startingLocation.x)
			{
				base.transform.position = new Vector3(this.startingLocation.x + this.zRange / 2f, this.startingLocation.y, this.startingLocation.z);
			}
			else
			{
				base.transform.position = new Vector3(this.startingLocation.x - this.zRange / 2f, this.startingLocation.y, this.startingLocation.z);
			}
		}
		this.valueImReporting = this.InterpolateValue(base.transform.position.x);
	}

	public bool setRandomly;

	public float zRange;

	public float maxValue;

	public float minValue;

	public Vector3 startingLocation;

	public int valueIndex;

	public float valueImReporting;

	public GorillaTriggerBox gorilla;

	private float startingZ;
}
