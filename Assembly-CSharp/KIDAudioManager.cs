using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[DefaultExecutionOrder(0)]
public class KIDAudioManager : MonoBehaviour
{
	public static KIDAudioManager Instance
	{
		get
		{
			if (!KIDAudioManager._instance)
			{
				if (!ApplicationQuittingState.IsQuitting)
				{
					Debug.LogError("No KIDAudioManager instance found in scene!");
				}
				return null;
			}
			return KIDAudioManager._instance;
		}
	}

	private void Awake()
	{
		if (KIDAudioManager._instance == null)
		{
			KIDAudioManager._instance = this;
			base.transform.parent = null;
			Object.DontDestroyOnLoad(base.gameObject);
			this.ConfigureAudioSource();
			this.InitializeSoundClips();
			this.mainMixer.GetFloat("Game_Volume", out this.cachedGameVolume);
			return;
		}
		if (KIDAudioManager._instance != this)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void ConfigureAudioSource()
	{
		if (this.audioSource != null)
		{
			this.audioSource.outputAudioMixerGroup = this.kidUIGroup;
			this.audioSource.playOnAwake = false;
			this.audioSource.spatialBlend = 0f;
			this.audioSource.volume = 1f;
			this.audioSource.enabled = true;
		}
		if (this.loopingAudioSource != null)
		{
			this.loopingAudioSource.outputAudioMixerGroup = this.kidUIGroup;
			this.loopingAudioSource.playOnAwake = false;
			this.loopingAudioSource.spatialBlend = 0f;
			this.loopingAudioSource.volume = 1f;
			this.loopingAudioSource.loop = true;
			this.loopingAudioSource.enabled = true;
		}
	}

	private void InitializeSoundClips()
	{
		this.soundClips = new Dictionary<KIDAudioManager.KIDSoundType, AudioClip>
		{
			{
				KIDAudioManager.KIDSoundType.ButtonClick,
				this.buttonClickSound
			},
			{
				KIDAudioManager.KIDSoundType.Denied,
				this.deniedSound
			},
			{
				KIDAudioManager.KIDSoundType.Success,
				this.successSound
			},
			{
				KIDAudioManager.KIDSoundType.Hover,
				this.buttonHoverSound
			},
			{
				KIDAudioManager.KIDSoundType.ButtonHeld,
				this.buttonHeldSound
			},
			{
				KIDAudioManager.KIDSoundType.PageTransition,
				this.pageTransitionSound
			},
			{
				KIDAudioManager.KIDSoundType.InputBack,
				this.inputBackSound
			},
			{
				KIDAudioManager.KIDSoundType.TurnOffPermission,
				this.turnOffPermissionSound
			}
		};
	}

	public void SetKIDUIAudioActive(bool active)
	{
		if (!this.IsInstanceValid() || this.isKIDUIActive == active)
		{
			return;
		}
		this.isKIDUIActive = active;
		if (!active)
		{
			this.StopButtonHeldSound();
		}
		if (active)
		{
			this.KIDSnapshot.TransitionTo(0f);
			return;
		}
		this.normalSnapshot.TransitionTo(0f);
	}

	public void PlaySound(KIDAudioManager.KIDSoundType soundType)
	{
		if (!this.IsInstanceValid())
		{
			return;
		}
		if (soundType == KIDAudioManager.KIDSoundType.ButtonHeld)
		{
			Debug.LogWarning("[KIDAudioManager] Button held sound is already playing, skipping delayed sound.");
			return;
		}
		AudioClip audioClip;
		if (this.soundClips.TryGetValue(soundType, out audioClip) && audioClip != null)
		{
			this.audioSource.PlayOneShot(audioClip);
			return;
		}
		Debug.LogWarning(string.Format("[KIDAudioManager] Sound clip for {0} is null or not found!", soundType));
	}

	public void StartButtonHeldSound()
	{
		if (!this.IsInstanceValid() || this.buttonHeldSound == null || this.isHoldSoundPlaying)
		{
			return;
		}
		this.loopingAudioSource.clip = this.buttonHeldSound;
		this.loopingAudioSource.Play();
		this.isHoldSoundPlaying = true;
	}

	public void StopButtonHeldSound()
	{
		if (!this.IsInstanceValid() || !this.isHoldSoundPlaying)
		{
			return;
		}
		if (this.loopingAudioSource.clip == this.buttonHeldSound)
		{
			this.loopingAudioSource.Stop();
		}
		this.isHoldSoundPlaying = false;
	}

	private bool IsInstanceValid()
	{
		return !(KIDAudioManager._instance == null) && !(KIDAudioManager._instance != this) && !(this.audioSource == null) && !(this.loopingAudioSource == null);
	}

	public bool IsKIDUIActive()
	{
		return this.isKIDUIActive;
	}

	public void PlaySoundWithDelay(KIDAudioManager.KIDSoundType soundType)
	{
		base.StartCoroutine(this.PlayDelayedSound(soundType, 0.05f));
	}

	private IEnumerator PlayDelayedSound(KIDAudioManager.KIDSoundType soundType, float delay)
	{
		yield return new WaitForSeconds(delay);
		this.PlaySound(soundType);
		yield break;
	}

	private static KIDAudioManager _instance;

	[SerializeField]
	private AudioSource audioSource;

	[SerializeField]
	private AudioSource loopingAudioSource;

	[SerializeField]
	private AudioMixer mainMixer;

	[SerializeField]
	private AudioMixerSnapshot KIDSnapshot;

	[SerializeField]
	private AudioMixerSnapshot normalSnapshot;

	[SerializeField]
	private AudioMixerGroup kidUIGroup;

	[SerializeField]
	private AudioClip buttonClickSound;

	[SerializeField]
	private AudioClip deniedSound;

	[SerializeField]
	private AudioClip successSound;

	[SerializeField]
	private AudioClip buttonHoverSound;

	[SerializeField]
	private AudioClip buttonHeldSound;

	[SerializeField]
	private AudioClip pageTransitionSound;

	[SerializeField]
	private AudioClip inputBackSound;

	[SerializeField]
	private AudioClip turnOffPermissionSound;

	private const string GAME_VOLUME = "Game_Volume";

	private const string KID_VOLUME = "KID_UI_Volume";

	private const float MUTED_VALUE = -80f;

	private const float UNMUTED_VALUE = 0f;

	private bool isKIDUIActive;

	private float cachedGameVolume;

	private bool isHoldSoundPlaying;

	private Dictionary<KIDAudioManager.KIDSoundType, AudioClip> soundClips;

	public enum KIDSoundType
	{
		ButtonClick,
		Hover,
		Success,
		Denied,
		InputBack,
		TurnOffPermission,
		PageTransition,
		ButtonHeld
	}
}
