using System;
using System.Collections.Generic;
using CjLib;
using Unity.Collections;
using UnityEngine;

public class GRNoiseEventManager : MonoBehaviourTick
{
	public void Awake()
	{
		GRNoiseEventManager.instance = this;
	}

	private void Start()
	{
	}

	public override void Tick()
	{
		this.RemoveExpiredEvents();
		if (GhostReactorManager.noiseDebugEnabled)
		{
			this.RenderDebug();
		}
	}

	private int FindUnusedEventEntry()
	{
		return -1;
	}

	public void AddNoiseEvent(Vector3 position, float magnitude = 1f, float duration = 1f)
	{
		GameNoiseEvent gameNoiseEvent = new GameNoiseEvent
		{
			position = position,
			eventTime = Time.timeAsDouble,
			duration = duration,
			magnitude = magnitude
		};
		int num = this.FindUnusedEventEntry();
		if (num == -1)
		{
			this.noiseEvents.Add(gameNoiseEvent);
			return;
		}
		this.noiseEvents[num] = gameNoiseEvent;
	}

	public List<GameNoiseEvent> GetNoiseEventsInRadius(Vector3 origin, float radius)
	{
		List<GameNoiseEvent> list = new List<GameNoiseEvent>();
		float num = radius * radius;
		foreach (GameNoiseEvent gameNoiseEvent in this.noiseEvents)
		{
			if (gameNoiseEvent.IsValid())
			{
				float sqrMagnitude = (gameNoiseEvent.position - origin).sqrMagnitude;
				float num2 = gameNoiseEvent.magnitude * gameNoiseEvent.magnitude;
				if (sqrMagnitude < num * num2)
				{
					list.Add(gameNoiseEvent);
				}
			}
		}
		return list;
	}

	public bool GetMostRecentNoiseEventInRadius(Vector3 origin, float radius, out GameNoiseEvent outEvent)
	{
		double timeAsDouble = Time.timeAsDouble;
		float num = radius * radius;
		double num2 = -1.0;
		int num3 = -1;
		for (int i = 0; i < this.noiseEvents.Count; i++)
		{
			GameNoiseEvent gameNoiseEvent = this.noiseEvents[i];
			if (gameNoiseEvent.IsValid())
			{
				float sqrMagnitude = (gameNoiseEvent.position - origin).sqrMagnitude;
				float num4 = gameNoiseEvent.magnitude * gameNoiseEvent.magnitude;
				if (sqrMagnitude < num * num4)
				{
					double num5 = timeAsDouble - gameNoiseEvent.eventTime;
					if (num3 < 0 || num5 < num2)
					{
						num3 = i;
						num2 = num5;
					}
				}
			}
		}
		if (num3 < 0)
		{
			outEvent = default(GameNoiseEvent);
			return false;
		}
		outEvent = this.noiseEvents[num3];
		return true;
	}

	public void RenderDebug()
	{
		int num = 0;
		float num2 = 5f;
		for (int i = 0; i < this.noiseEvents.Count; i++)
		{
			GameNoiseEvent gameNoiseEvent = this.noiseEvents[i];
			if (gameNoiseEvent.IsValid())
			{
				float num3 = this.debugMeshScale * gameNoiseEvent.magnitude * num2;
				DebugUtil.DrawSphere(gameNoiseEvent.position, num3, 8, 6, Color.green, true, DebugUtil.Style.Wireframe);
				num++;
			}
		}
	}

	private void RemoveExpiredEvents()
	{
		for (int i = 0; i < this.noiseEvents.Count; i++)
		{
			if (!this.noiseEvents[i].IsValid())
			{
				this.noiseEvents.RemoveAtSwapBack(i);
				i--;
			}
		}
	}

	private List<GameNoiseEvent> noiseEvents = new List<GameNoiseEvent>();

	public static GRNoiseEventManager instance;

	public float debugMeshScale = 1f;
}
