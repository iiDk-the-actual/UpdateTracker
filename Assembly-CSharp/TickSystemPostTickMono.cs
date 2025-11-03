using System;
using UnityEngine;

internal abstract class TickSystemPostTickMono : MonoBehaviour, ITickSystemPost
{
	public bool PostTickRunning { get; set; }

	public virtual void OnEnable()
	{
		TickSystem<object>.AddPostTickCallback(this);
	}

	public virtual void OnDisable()
	{
		TickSystem<object>.RemovePostTickCallback(this);
	}

	public virtual void PostTick()
	{
	}
}
