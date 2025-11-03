using System;
using UnityEngine;
using UnityEngine.UI;

public class BlinkingText : MonoBehaviour
{
	private void Awake()
	{
		this.textComponent = base.GetComponent<Text>();
	}

	private void Update()
	{
		if (this.isOn && Time.time > this.lastTime + this.cycleTime * this.dutyCycle)
		{
			this.isOn = false;
			this.textComponent.enabled = false;
			return;
		}
		if (!this.isOn && Time.time > this.lastTime + this.cycleTime)
		{
			this.lastTime = Time.time;
			this.isOn = true;
			this.textComponent.enabled = true;
		}
	}

	public float cycleTime;

	public float dutyCycle;

	private bool isOn;

	private float lastTime;

	private Text textComponent;
}
