using System;
using System.Collections.Generic;
using UnityEngine;

public interface IFXEffectContextObject
{
	List<int> PrefabPoolIds { get; }

	Vector3 Position { get; }

	Quaternion Rotation { get; }

	AudioSource SoundSource { get; }

	AudioClip Sound { get; }

	float Volume { get; }

	float Pitch { get; }

	void OnTriggerActions();

	void OnPlayVisualFX(int effectID, GameObject effect);

	void OnPlaySoundFX(AudioSource audioSource);
}
