using System;
using UnityEngine;

internal abstract class TickSystemMono : MonoBehaviour, ITickSystem, ITickSystemPre, ITickSystemTick, ITickSystemPost
{
	public bool PreTickRunning { get; set; }

	public bool TickRunning { get; set; }

	public bool PostTickRunning { get; set; }

	public virtual void OnEnable()
	{
		TickSystem<object>.AddTickSystemCallBack(this);
	}

	public virtual void OnDisable()
	{
		TickSystem<object>.RemoveTickSystemCallback(this);
	}

	public virtual void PreTick()
	{
	}

	public virtual void Tick()
	{
	}

	public virtual void PostTick()
	{
	}
}
