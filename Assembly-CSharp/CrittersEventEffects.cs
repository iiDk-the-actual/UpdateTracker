using System;
using System.Collections.Generic;
using GorillaExtensions;
using UnityEngine;

public class CrittersEventEffects : MonoBehaviour
{
	private void Awake()
	{
		if (this.manager == null)
		{
			GTDev.LogError<string>("CrittersEventEffects missing reference to CrittersManager", null);
			return;
		}
		this.effectResponse = new Dictionary<CrittersManager.CritterEvent, GameObject>();
		for (int i = 0; i < this.eventEffects.Length; i++)
		{
			if (this.eventEffects[i].effect != null)
			{
				this.effectResponse.Add(this.eventEffects[i].eventType, this.eventEffects[i].effect);
			}
		}
		this.manager.OnCritterEventReceived += this.HandleReceivedEvent;
	}

	private void HandleReceivedEvent(CrittersManager.CritterEvent eventType, int sourceActor, Vector3 position, Quaternion rotation)
	{
		GameObject gameObject;
		if (this.effectResponse.TryGetValue(eventType, out gameObject))
		{
			GameObject pooled = CrittersPool.GetPooled(gameObject);
			if (pooled.IsNotNull())
			{
				pooled.transform.position = position;
				pooled.transform.rotation = rotation;
			}
		}
	}

	public CrittersManager manager;

	public CrittersEventEffects.CrittersEventResponse[] eventEffects;

	private Dictionary<CrittersManager.CritterEvent, GameObject> effectResponse;

	[Serializable]
	public class CrittersEventResponse
	{
		public CrittersManager.CritterEvent eventType;

		public GameObject effect;
	}
}
