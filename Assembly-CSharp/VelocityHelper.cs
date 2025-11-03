using System;
using UnityEngine;

[Serializable]
public class VelocityHelper
{
	public VelocityHelper(int historySize = 12)
	{
		this._size = historySize;
		this._samples = new float[historySize * 4];
	}

	public void SamplePosition(Transform target, float dt)
	{
		Vector3 position = target.position;
		if (!this._initialized)
		{
			this._InitSamples(position, dt);
		}
		this._SetSample(this._latest, position, dt);
		this._latest = (this._latest + 1) % this._size;
	}

	private void _InitSamples(Vector3 position, float dt)
	{
		for (int i = 0; i < this._size; i++)
		{
			this._SetSample(i, position, dt);
		}
		this._initialized = true;
	}

	private void _SetSample(int i, Vector3 position, float dt)
	{
		this._samples[i] = position.x;
		this._samples[i + 1] = position.y;
		this._samples[i + 2] = position.z;
		this._samples[i + 3] = dt;
	}

	private float[] _samples;

	private int _latest;

	private int _size;

	private bool _initialized;
}
