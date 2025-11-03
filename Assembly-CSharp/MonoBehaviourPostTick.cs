using System;
using UnityEngine;

public abstract class MonoBehaviourPostTick : MonoBehaviour, ITickSystemPost
{
	public bool PostTickRunning { get; set; }

	public void OnEnable()
	{
		TickSystem<object>.AddPostTickCallback(this);
	}

	public void OnDisable()
	{
		TickSystem<object>.RemovePostTickCallback(this);
	}

	public abstract void PostTick();
}
