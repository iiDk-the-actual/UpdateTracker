using System;
using UnityEngine;

public class AmbientSoundRandomizer : MonoBehaviour
{
	private void Button_Cache()
	{
		this.audioSources = base.GetComponentsInChildren<AudioSource>();
	}

	private void Awake()
	{
		this.SetTarget();
	}

	private void Update()
	{
		if (this.timer >= this.timerTarget)
		{
			int num = Random.Range(0, this.audioSources.Length);
			int num2 = Random.Range(0, this.audioClips.Length);
			this.audioSources[num].clip = this.audioClips[num2];
			this.audioSources[num].GTPlay();
			this.SetTarget();
			return;
		}
		this.timer += Time.deltaTime;
	}

	private void SetTarget()
	{
		this.timerTarget = this.baseTime + Random.Range(0f, this.randomModifier);
		this.timer = 0f;
	}

	[SerializeField]
	private AudioSource[] audioSources;

	[SerializeField]
	private AudioClip[] audioClips;

	[SerializeField]
	private float baseTime = 15f;

	[SerializeField]
	private float randomModifier = 5f;

	private float timer;

	private float timerTarget;
}
