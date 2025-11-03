using System;
using System.Collections.Generic;
using GorillaTagScripts;
using UnityEngine;

[Serializable]
internal class HandEffectContext : IFXEffectContextObject
{
	public List<int> PrefabPoolIds
	{
		get
		{
			return this.prefabHashes;
		}
	}

	public Vector3 Position
	{
		get
		{
			return this.position;
		}
	}

	public Quaternion Rotation
	{
		get
		{
			return this.rotation;
		}
	}

	public float Speed
	{
		get
		{
			return this.speed;
		}
	}

	public Color Color
	{
		get
		{
			return this.color;
		}
	}

	public AudioSource SoundSource
	{
		get
		{
			return this.handSoundSource;
		}
	}

	public AudioClip Sound
	{
		get
		{
			return this.soundFX;
		}
	}

	public float Volume
	{
		get
		{
			return this.soundVolume;
		}
	}

	public float Pitch
	{
		get
		{
			return this.soundPitch;
		}
	}

	public void AddFXPrefab(int hash)
	{
		this.prefabHashes.Add(hash);
	}

	public void RemoveFXPrefab(int hash)
	{
		int num = this.prefabHashes.IndexOf(hash, 2);
		if (num >= 2)
		{
			this.prefabHashes.RemoveAt(num);
		}
	}

	public bool SeparateUpTapCooldown
	{
		get
		{
			return this.separateUpTapCooldownCount > 0;
		}
		set
		{
			this.separateUpTapCooldownCount = Mathf.Max(this.separateUpTapCooldownCount + (value ? 1 : (-1)), 0);
		}
	}

	public HandTapOverrides DownTapOverrides
	{
		get
		{
			return this.downTapOverrides ?? this.defaultDownTapOverrides;
		}
		set
		{
			this.downTapOverrides = value;
		}
	}

	public HandTapOverrides UpTapOverrides
	{
		get
		{
			return this.upTapOverrides ?? this.defaultUpTapOverrides;
		}
		set
		{
			this.upTapOverrides = value;
		}
	}

	public event Action<HandEffectContext> handTapDown;

	public event Action<HandEffectContext> handTapUp;

	public void OnTriggerActions()
	{
		if (this.isDownTap)
		{
			Action<HandEffectContext> action = this.handTapDown;
			if (action == null)
			{
				return;
			}
			action(this);
			return;
		}
		else
		{
			Action<HandEffectContext> action2 = this.handTapUp;
			if (action2 == null)
			{
				return;
			}
			action2(this);
			return;
		}
	}

	public void OnPlayVisualFX(int fxID, GameObject fx)
	{
		FXModifier fxmodifier;
		if (fx.TryGetComponent<FXModifier>(out fxmodifier))
		{
			fxmodifier.UpdateScale(this.soundVolume * ((fxID == GorillaAmbushManager.HandEffectHash) ? GorillaAmbushManager.HandFXScaleModifier : 1f), this.color);
		}
	}

	public void OnPlaySoundFX(AudioSource audioSource)
	{
	}

	internal List<int> prefabHashes = new List<int> { -1, -1 };

	internal Vector3 position;

	internal Quaternion rotation;

	internal float speed;

	internal Color color = Color.white;

	[SerializeField]
	internal AudioSource handSoundSource;

	internal AudioClip soundFX;

	internal float soundVolume;

	internal float soundPitch;

	internal int separateUpTapCooldownCount;

	[SerializeField]
	internal HandTapOverrides defaultDownTapOverrides;

	internal HandTapOverrides downTapOverrides;

	[SerializeField]
	internal HandTapOverrides defaultUpTapOverrides;

	internal HandTapOverrides upTapOverrides;

	internal bool isDownTap;

	internal bool isLeftHand;
}
