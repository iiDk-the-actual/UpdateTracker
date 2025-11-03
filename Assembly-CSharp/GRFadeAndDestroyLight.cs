using System;
using UnityEngine;

public class GRFadeAndDestroyLight : MonoBehaviour
{
	private void Start()
	{
		if (this.gameLight != null)
		{
			this.fadeRate = this.gameLight.light.intensity / this.TimeToFade;
		}
		this.timeSinceLastUpdate = Time.time;
	}

	public void OnEnable()
	{
	}

	public void OnDisable()
	{
	}

	public void Update()
	{
		if (Time.time < this.timeSinceLastUpdate || Time.time > this.timeSinceLastUpdate + this.timeSlice)
		{
			this.timeSinceLastUpdate = Time.time;
			float num = this.gameLight.light.intensity;
			num -= this.timeSlice * this.fadeRate;
			if (num <= 0f)
			{
				base.gameObject.Destroy();
				return;
			}
			this.gameLight.light.intensity = num;
		}
	}

	public float TimeToFade = 10f;

	private float fadeRate;

	public GameLight gameLight;

	public float timeSlice = 0.1f;

	public float timeSinceLastUpdate;
}
