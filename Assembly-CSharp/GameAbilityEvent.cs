using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class GameAbilityEvent
{
	public void Reset()
	{
		this.played = false;
	}

	public void TryPlay(float abilityTime, AudioSource audioSource)
	{
		if (abilityTime < this.time || this.played)
		{
			return;
		}
		this.played = true;
		if (this.sound.IsValid())
		{
			this.sound.Play(audioSource);
		}
		for (int i = 0; i < this.triggerEvent.Count; i++)
		{
			this.triggerEvent[i].Invoke();
		}
	}

	public float time;

	public AbilitySound sound;

	public List<UnityEvent> triggerEvent;

	[NonSerialized]
	public bool played;
}
