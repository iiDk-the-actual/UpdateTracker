using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameAbilityEvents
{
	public void Reset()
	{
		for (int i = 0; i < this.events.Count; i++)
		{
			this.events[i].Reset();
		}
	}

	public void TryPlay(float abilityTime, AudioSource audioSource)
	{
		for (int i = 0; i < this.events.Count; i++)
		{
			this.events[i].TryPlay(abilityTime, audioSource);
		}
	}

	public List<GameAbilityEvent> events;
}
