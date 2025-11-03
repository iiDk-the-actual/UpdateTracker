using System;
using UnityEngine;

public class Oscillator : MonoBehaviour
{
	public void Init(Vector3 center, Vector3 radius, Vector3 frequency, Vector3 startPhase)
	{
		this.Center = center;
		this.Radius = radius;
		this.Frequency = frequency;
		this.Phase = startPhase;
	}

	private float SampleWave(float phase)
	{
		switch (this.WaveType)
		{
		case Oscillator.WaveTypeEnum.Sine:
			return Mathf.Sin(phase);
		case Oscillator.WaveTypeEnum.Square:
			phase = Mathf.Repeat(phase, 6.2831855f);
			if (phase >= 3.1415927f)
			{
				return -1f;
			}
			return 1f;
		case Oscillator.WaveTypeEnum.Triangle:
			phase = Mathf.Repeat(phase, 6.2831855f);
			if (phase < 1.5707964f)
			{
				return phase / 1.5707964f;
			}
			if (phase < 3.1415927f)
			{
				return 1f - (phase - 1.5707964f) / 1.5707964f;
			}
			if (phase < 4.712389f)
			{
				return (3.1415927f - phase) / 1.5707964f;
			}
			return (phase - 4.712389f) / 1.5707964f - 1f;
		default:
			return 0f;
		}
	}

	public void OnEnable()
	{
		this.m_initCenter = base.transform.position;
	}

	public void Update()
	{
		this.Phase += this.Frequency * 2f * 3.1415927f * Time.deltaTime;
		Vector3 vector = (this.UseCenter ? this.Center : this.m_initCenter);
		vector.x += this.Radius.x * this.SampleWave(this.Phase.x);
		vector.y += this.Radius.y * this.SampleWave(this.Phase.y);
		vector.z += this.Radius.z * this.SampleWave(this.Phase.z);
		base.transform.position = vector;
	}

	public Oscillator.WaveTypeEnum WaveType;

	private Vector3 m_initCenter;

	public bool UseCenter;

	public Vector3 Center;

	public Vector3 Radius;

	public Vector3 Frequency;

	public Vector3 Phase;

	public enum WaveTypeEnum
	{
		Sine,
		Square,
		Triangle
	}
}
