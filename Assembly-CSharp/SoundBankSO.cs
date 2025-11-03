using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Gorilla Tag/SoundBankSO")]
public class SoundBankSO : ScriptableObject
{
	public AudioClip[] sounds;

	public Vector2 volumeRange = new Vector2(0.5f, 0.5f);

	public Vector2 pitchRange = new Vector2(1f, 1f);
}
