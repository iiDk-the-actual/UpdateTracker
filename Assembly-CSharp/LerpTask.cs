using System;
using UnityEngine;

public class LerpTask<T>
{
	public void Reset()
	{
		this.onLerp(this.lerpFrom, this.lerpTo, 0f);
		this.active = false;
		this.elapsed = 0f;
	}

	public void Start(T from, T to, float duration)
	{
		this.lerpFrom = from;
		this.lerpTo = to;
		this.duration = duration;
		this.elapsed = 0f;
		this.active = true;
	}

	public void Finish()
	{
		this.onLerp(this.lerpFrom, this.lerpTo, 1f);
		Action action = this.onLerpEnd;
		if (action != null)
		{
			action();
		}
		this.active = false;
		this.elapsed = 0f;
	}

	public void Update()
	{
		if (!this.active)
		{
			return;
		}
		float deltaTime = Time.deltaTime;
		if (this.elapsed < this.duration)
		{
			float num = ((this.elapsed + deltaTime >= this.duration) ? 1f : (this.elapsed / this.duration));
			this.onLerp(this.lerpFrom, this.lerpTo, num);
			this.elapsed += deltaTime;
			return;
		}
		this.Finish();
	}

	public float elapsed;

	public float duration;

	public T lerpFrom;

	public T lerpTo;

	public Action<T, T, float> onLerp;

	public Action onLerpEnd;

	public bool active;
}
