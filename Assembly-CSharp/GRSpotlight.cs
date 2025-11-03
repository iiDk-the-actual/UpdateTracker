using System;
using UnityEngine;

public class GRSpotlight : MonoBehaviourTick
{
	private void Awake()
	{
		this.yStart = base.transform.rotation.eulerAngles.y;
		this.xStart = base.transform.rotation.eulerAngles.x;
		this.timeOffset = Random.value * 360f;
		this.yFrequency += Random.value / 100f;
		this.xFrequency += Random.value / 100f;
	}

	public override void Tick()
	{
		base.transform.eulerAngles = new Vector3(this.xStart + this.xAmplitude * Mathf.Sin(Time.time * this.xFrequency), this.yStart + this.yAmplitude * Mathf.Cos(Time.time * this.yFrequency), 0f);
	}

	public float yAmplitude = 75f;

	public float xAmplitude = 40f;

	public float yFrequency = 0.2f;

	public float xFrequency = 0.3f;

	private float yStart;

	private float xStart;

	private float timeOffset;
}
