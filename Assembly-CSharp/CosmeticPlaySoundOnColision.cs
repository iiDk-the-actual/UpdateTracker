using System;
using System.Collections;
using System.Collections.Generic;
using GorillaLocomotion;
using UnityEngine;
using UnityEngine.Events;

public class CosmeticPlaySoundOnColision : MonoBehaviour
{
	private void Awake()
	{
		this.transferrableObject = base.GetComponentInParent<TransferrableObject>();
		this.soundLookup = new Dictionary<int, int>();
		this.audioSource = base.GetComponent<AudioSource>();
		for (int i = 0; i < this.soundIdRemappings.Length; i++)
		{
			this.soundLookup.Add(this.soundIdRemappings[i].SoundIn, this.soundIdRemappings[i].SoundOut);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		GorillaSurfaceOverride gorillaSurfaceOverride;
		if (this.speed >= this.minSpeed && other.TryGetComponent<GorillaSurfaceOverride>(out gorillaSurfaceOverride))
		{
			int num;
			if (this.soundLookup.TryGetValue(gorillaSurfaceOverride.overrideIndex, out num))
			{
				this.playSound(num, this.invokeEventOnOverideSound);
				return;
			}
			this.playSound(this.defaultSound, this.invokeEventOnDefaultSound);
		}
	}

	private void playSound(int soundIndex, bool invokeEvent)
	{
		if (soundIndex > -1 && soundIndex < GTPlayer.Instance.materialData.Count)
		{
			if (this.audioSource.isPlaying)
			{
				this.audioSource.GTStop();
				if (this.invokeEventsOnAllClients || this.transferrableObject.IsMyItem())
				{
					this.OnStopPlayback.Invoke();
				}
				if (this.crWaitForStopPlayback != null)
				{
					base.StopCoroutine(this.crWaitForStopPlayback);
					this.crWaitForStopPlayback = null;
				}
			}
			this.audioSource.clip = GTPlayer.Instance.materialData[soundIndex].audio;
			this.audioSource.GTPlay();
			if (invokeEvent && (this.invokeEventsOnAllClients || this.transferrableObject.IsMyItem()))
			{
				this.OnStartPlayback.Invoke();
				this.crWaitForStopPlayback = base.StartCoroutine(this.waitForStopPlayback());
			}
		}
	}

	private IEnumerator waitForStopPlayback()
	{
		while (this.audioSource.isPlaying)
		{
			yield return null;
		}
		if (this.invokeEventsOnAllClients || this.transferrableObject.IsMyItem())
		{
			this.OnStopPlayback.Invoke();
		}
		this.crWaitForStopPlayback = null;
		yield break;
	}

	private void FixedUpdate()
	{
		this.speed = Vector3.Distance(base.transform.position, this.previousFramePosition) * Time.fixedDeltaTime * 100f;
		this.previousFramePosition = base.transform.position;
	}

	[GorillaSoundLookup]
	[SerializeField]
	private int defaultSound = 1;

	[SerializeField]
	private SoundIdRemapping[] soundIdRemappings;

	[SerializeField]
	private UnityEvent OnStartPlayback;

	[SerializeField]
	private UnityEvent OnStopPlayback;

	[SerializeField]
	private float minSpeed = 0.1f;

	private TransferrableObject transferrableObject;

	private Dictionary<int, int> soundLookup;

	private AudioSource audioSource;

	private Coroutine crWaitForStopPlayback;

	private float speed;

	private Vector3 previousFramePosition;

	[SerializeField]
	private bool invokeEventsOnAllClients;

	[SerializeField]
	private bool invokeEventOnOverideSound = true;

	[SerializeField]
	private bool invokeEventOnDefaultSound;
}
