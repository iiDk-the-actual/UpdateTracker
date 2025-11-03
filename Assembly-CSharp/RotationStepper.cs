using System;
using BoingKit;
using UnityEngine;

public class RotationStepper : MonoBehaviour
{
	public void OnEnable()
	{
		this.m_phase = 0f;
		Random.InitState(0);
	}

	public void Update()
	{
		this.m_phase += this.Frequency * Time.deltaTime;
		RotationStepper.ModeEnum mode = this.Mode;
		if (mode == RotationStepper.ModeEnum.Fixed)
		{
			base.transform.rotation = Quaternion.Euler(0f, 0f, (Mathf.Repeat(this.m_phase, 2f) < 1f) ? (-25f) : 25f);
			return;
		}
		if (mode != RotationStepper.ModeEnum.Random)
		{
			return;
		}
		while (this.m_phase >= 1f)
		{
			Random.InitState(Time.frameCount);
			base.transform.rotation = Random.rotationUniform;
			this.m_phase -= 1f;
		}
	}

	public RotationStepper.ModeEnum Mode;

	[ConditionalField("Mode", RotationStepper.ModeEnum.Fixed, null, null, null, null, null)]
	public float Angle = 25f;

	public float Frequency;

	private float m_phase;

	public enum ModeEnum
	{
		Fixed,
		Random
	}
}
