using System;
using UnityEngine;
using UnityEngine.Audio;

public class PlayerAudioManager : MonoBehaviour
{
	public void SetMixerSnapshot(AudioMixerSnapshot snapshot, float transitionTime = 0.1f)
	{
		snapshot.TransitionTo(transitionTime);
	}

	public void UnsetMixerSnapshot(float transitionTime = 0.1f)
	{
		this.defaultSnapshot.TransitionTo(transitionTime);
	}

	public AudioMixerSnapshot defaultSnapshot;

	public AudioMixerSnapshot underwaterSnapshot;
}
