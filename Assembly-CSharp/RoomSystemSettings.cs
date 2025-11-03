using System;
using System.Collections.Generic;
using GorillaTag;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/RoomSystemSettings", order = 2)]
internal class RoomSystemSettings : ScriptableObject
{
	public ExpectedUsersDecayTimer ExpectedUsersTimer
	{
		get
		{
			return this.expectedUsersTimer;
		}
	}

	public TickSystemTimer ResyncNetworkTimeTimer
	{
		get
		{
			return this.resyncNetworkTimeTimer;
		}
	}

	public CallLimiterWithCooldown StatusEffectLimiter
	{
		get
		{
			return this.statusEffectLimiter;
		}
	}

	public CallLimiterWithCooldown SoundEffectLimiter
	{
		get
		{
			return this.soundEffectLimiter;
		}
	}

	public CallLimiterWithCooldown SoundEffectOtherLimiter
	{
		get
		{
			return this.soundEffectOtherLimiter;
		}
	}

	public CallLimiterWithCooldown PlayerEffectLimiter
	{
		get
		{
			return this.playerEffectLimiter;
		}
	}

	public GameObject PlayerImpactEffect
	{
		get
		{
			return this.playerImpactEffect;
		}
	}

	public List<RoomSystem.PlayerEffectConfig> PlayerEffects
	{
		get
		{
			return this.playerEffects;
		}
	}

	public int PausedDCTimer
	{
		get
		{
			return this.pausedDCTimer;
		}
	}

	[SerializeField]
	private ExpectedUsersDecayTimer expectedUsersTimer;

	[SerializeField]
	private TickSystemTimer resyncNetworkTimeTimer;

	[SerializeField]
	private CallLimiterWithCooldown statusEffectLimiter;

	[SerializeField]
	private CallLimiterWithCooldown soundEffectLimiter;

	[SerializeField]
	private CallLimiterWithCooldown soundEffectOtherLimiter;

	[SerializeField]
	private CallLimiterWithCooldown playerEffectLimiter;

	[SerializeField]
	private GameObject playerImpactEffect;

	[SerializeField]
	private List<RoomSystem.PlayerEffectConfig> playerEffects = new List<RoomSystem.PlayerEffectConfig>();

	[SerializeField]
	private int pausedDCTimer;
}
