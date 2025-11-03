using System;
using System.Collections.Generic;
using UnityEngine;

public class AverageVector3
{
	public AverageVector3(float averagingWindow = 0.1f)
	{
		this.timeWindow = averagingWindow;
	}

	public void AddSample(Vector3 sample, float time)
	{
		this.samples.Add(new AverageVector3.Sample
		{
			timeStamp = time,
			value = sample
		});
		this.RefreshSamples();
	}

	public Vector3 GetAverage()
	{
		this.RefreshSamples();
		Vector3 vector = Vector3.zero;
		for (int i = 0; i < this.samples.Count; i++)
		{
			vector += this.samples[i].value;
		}
		return vector / (float)this.samples.Count;
	}

	public void Clear()
	{
		this.samples.Clear();
	}

	private void RefreshSamples()
	{
		float num = Time.time - this.timeWindow;
		for (int i = this.samples.Count - 1; i >= 0; i--)
		{
			if (this.samples[i].timeStamp < num)
			{
				this.samples.RemoveAt(i);
			}
		}
	}

	private List<AverageVector3.Sample> samples = new List<AverageVector3.Sample>();

	private float timeWindow = 0.1f;

	public struct Sample
	{
		public float timeStamp;

		public Vector3 value;
	}
}
