using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR;

public class GRToolCollector : MonoBehaviour, IGameEntityDebugComponent, IGameEntityComponent
{
	private void Awake()
	{
		this.state = GRToolCollector.State.Idle;
		this.stateTimeRemaining = -1f;
	}

	private void OnEnable()
	{
		this.SetState(GRToolCollector.State.Idle);
	}

	public void OnEntityInit()
	{
		if (this.tool != null)
		{
			this.tool.onToolUpgraded += this.OnToolUpgraded;
			this.OnToolUpgraded(this.tool);
		}
		this.lastRechargeTime = (double)Time.time;
	}

	public void OnEntityDestroy()
	{
	}

	public void OnEntityStateChange(long prevState, long nextState)
	{
	}

	private void OnToolUpgraded(GRTool tool)
	{
		this.rechargeRate = this.attributes.CalculateFinalFloatValueForAttribute(GRAttributeType.RechargeRate);
		if (tool.HasUpgradeInstalled(GRToolProgressionManager.ToolParts.CollectorBonus1))
		{
			this.vacuumSound = this.upgrade1vacuumSound;
			this.vacuumParticleEffect = this.upgrade1VacuumParticleEffect;
			return;
		}
		if (tool.HasUpgradeInstalled(GRToolProgressionManager.ToolParts.CollectorBonus2))
		{
			this.vacuumSound = this.upgrade2vacuumSound;
			this.vacuumParticleEffect = this.upgrade2VacuumParticleEffect;
			return;
		}
		if (tool.HasUpgradeInstalled(GRToolProgressionManager.ToolParts.CollectorBonus3))
		{
			this.vacuumSound = this.upgrade3vacuumSound;
			this.vacuumParticleEffect = this.upgrade3VacuumParticleEffect;
		}
	}

	private bool IsHeldLocal()
	{
		return this.gameEntity.heldByActorNumber == PhotonNetwork.LocalPlayer.ActorNumber;
	}

	public void OnUpdate(float dt)
	{
		if (this.IsHeldLocal() || this.activatedLocally)
		{
			this.OnUpdateAuthority(dt);
			return;
		}
		this.OnUpdateRemote(dt);
	}

	public void Update()
	{
		float deltaTime = Time.deltaTime;
		if (this.IsHeldLocal() || this.activatedLocally)
		{
			this.OnUpdateAuthority(deltaTime);
			return;
		}
		this.OnUpdateRemote(deltaTime);
	}

	private void OnUpdateAuthority(float dt)
	{
		switch (this.state)
		{
		case GRToolCollector.State.Idle:
		{
			bool flag = this.IsButtonHeld();
			this.waitingForButtonRelease = this.waitingForButtonRelease && flag;
			if (flag && !this.waitingForButtonRelease)
			{
				this.SetStateAuthority(GRToolCollector.State.Vacuuming);
				this.activatedLocally = true;
			}
			if (this.rechargeRate > 0f && Time.timeAsDouble > this.lastRechargeTime + (double)this.rechargeInterval)
			{
				this.gameEntity.manager.ghostReactorManager.RequestChargeTool(this.gameEntity.id, this.gameEntity.id, (int)(this.rechargeRate * this.rechargeInterval), false);
				this.lastRechargeTime = Time.timeAsDouble;
				if (this.passiveChargeParticleEffect != null)
				{
					this.passiveChargeParticleEffect.Play();
					return;
				}
			}
			break;
		}
		case GRToolCollector.State.Vacuuming:
		{
			bool flag2 = this.IsButtonHeld();
			this.stateTimeRemaining -= dt;
			if (this.stateTimeRemaining <= 0f)
			{
				this.SetStateAuthority(GRToolCollector.State.Collect);
				return;
			}
			if (!flag2)
			{
				this.SetStateAuthority(GRToolCollector.State.Idle);
				this.activatedLocally = false;
				return;
			}
			break;
		}
		case GRToolCollector.State.Collect:
			this.stateTimeRemaining -= dt;
			if (this.stateTimeRemaining <= 0f)
			{
				this.SetStateAuthority(GRToolCollector.State.Cooldown);
				return;
			}
			break;
		case GRToolCollector.State.Cooldown:
			this.stateTimeRemaining -= dt;
			if (this.stateTimeRemaining <= 0f)
			{
				this.activatedLocally = false;
				this.waitingForButtonRelease = true;
				this.SetStateAuthority(GRToolCollector.State.Idle);
			}
			break;
		default:
			return;
		}
	}

	private void OnUpdateRemote(float dt)
	{
		GRToolCollector.State state = (GRToolCollector.State)this.gameEntity.GetState();
		if (state != this.state)
		{
			this.SetState(state);
		}
	}

	private void SetStateAuthority(GRToolCollector.State newState)
	{
		this.SetState(newState);
		this.gameEntity.RequestState(this.gameEntity.id, (long)newState);
	}

	private void SetState(GRToolCollector.State newState)
	{
		this.state = newState;
		switch (this.state)
		{
		case GRToolCollector.State.Idle:
			this.StopVacuum();
			this.stateTimeRemaining = -1f;
			this.lastRechargeTime = (double)Time.time;
			return;
		case GRToolCollector.State.Vacuuming:
			this.StartVacuum();
			this.stateTimeRemaining = this.chargeDuration;
			return;
		case GRToolCollector.State.Collect:
			this.TryCollect();
			this.stateTimeRemaining = this.collectDuration;
			return;
		case GRToolCollector.State.Cooldown:
			this.stateTimeRemaining = this.cooldownDuration;
			return;
		default:
			return;
		}
	}

	private void StartVacuum()
	{
		this.vacuumAudioSource.clip = this.vacuumSound;
		this.vacuumAudioSource.volume = this.vacuumSoundVolume;
		this.vacuumAudioSource.loop = true;
		this.vacuumAudioSource.Play();
		this.vacuumParticleEffect.Play();
		if (this.IsHeldLocal())
		{
			this.PlayVibration(GorillaTagger.Instance.tapHapticStrength, this.chargeDuration);
		}
	}

	private void StopVacuum()
	{
		this.vacuumAudioSource.loop = false;
		this.vacuumAudioSource.Stop();
		this.vacuumParticleEffect.Stop();
	}

	private void TryCollect()
	{
		if (this.IsHeldLocal())
		{
			int num = Physics.SphereCastNonAlloc(this.shootFrom.position, 0.2f, this.shootFrom.rotation * Vector3.forward, this.tempHitResults, 1f, this.collectibleLayerMask);
			for (int i = 0; i < num; i++)
			{
				RaycastHit raycastHit = this.tempHitResults[i];
				GameObject gameObject = null;
				Rigidbody attachedRigidbody = raycastHit.collider.attachedRigidbody;
				if (attachedRigidbody != null)
				{
					gameObject = attachedRigidbody.gameObject;
				}
				else
				{
					GameEntity gameEntity = GameEntity.Get(raycastHit.collider);
					if (gameEntity != null)
					{
						gameObject = gameEntity.gameObject;
					}
				}
				if (gameObject != null)
				{
					GRCollectible component = gameObject.GetComponent<GRCollectible>();
					if (component != null && component.type != ProgressionManager.CoreType.ChaosSeed && this.tool.energy < this.tool.GetEnergyMax())
					{
						GhostReactorManager.Get(this.gameEntity).RequestCollectItem(component.entity.id, this.gameEntity.id);
						return;
					}
				}
			}
			for (int j = 0; j < num; j++)
			{
				RaycastHit raycastHit2 = this.tempHitResults[j];
				GameObject gameObject2 = null;
				Rigidbody attachedRigidbody2 = raycastHit2.collider.attachedRigidbody;
				if (attachedRigidbody2 != null)
				{
					gameObject2 = attachedRigidbody2.gameObject;
				}
				else
				{
					GameEntity gameEntity2 = GameEntity.Get(raycastHit2.collider);
					if (gameEntity2 != null)
					{
						gameObject2 = gameEntity2.gameObject;
					}
				}
				if (gameObject2 != null)
				{
					if (gameObject2.GetComponent<GRCurrencyDepositor>() != null)
					{
						if (this.tool.energy > 0)
						{
							GhostReactorManager.Get(this.gameEntity).RequestDepositCurrency(this.gameEntity.id);
						}
						return;
					}
					GRTool component2 = gameObject2.GetComponent<GRTool>();
					if (!(component2 == null) && !(component2 == this.tool))
					{
						GameEntity component3 = gameObject2.GetComponent<GameEntity>();
						if (component2 != null && component3 != null)
						{
							GhostReactorManager.Get(this.gameEntity).RequestChargeTool(this.gameEntity.id, component3.id, 0, true);
							if (this.tool.HasUpgradeInstalled(GRToolProgressionManager.ToolParts.CollectorBonus3) && this.tool.energy > 50)
							{
								List<GRTool> list = new List<GRTool>();
								this.gameEntity.manager.GetEntitiesWithComponentInRadius<GRTool>(base.transform.position, this.level3ChargeRadius, true, list);
								for (int k = 0; k < list.Count; k++)
								{
									GRTool grtool = list[k];
									if (!(grtool.GetComponent<GRToolCollector>() != null) && !(grtool.gameEntity == this.gameEntity) && !(grtool.gameEntity == component3))
									{
										GhostReactorManager.Get(this.gameEntity).RequestChargeTool(this.gameEntity.id, grtool.gameEntity.id, 0, false);
									}
								}
							}
							return;
						}
					}
				}
			}
		}
	}

	public void PerformCollection(GRCollectible collectible)
	{
		this.tool.RefillEnergy(collectible.energyValue + this.attributes.CalculateFinalValueForAttribute(GRAttributeType.HarvestGain), collectible.entity.id);
		this.collectAudioSource.volume = this.collectSoundVolume;
		this.collectAudioSource.PlayOneShot(this.collectSound);
	}

	public void PlayChargeEffect(GRTool targetTool)
	{
		if (targetTool == null)
		{
			return;
		}
		if (targetTool == this.tool)
		{
			return;
		}
		this.collectAudioSource.volume = this.chargeBeamVolume;
		this.collectAudioSource.PlayOneShot(this.chargeBeamSound);
		for (int i = 0; i < targetTool.energyMeters.Count; i++)
		{
			if (targetTool.energyMeters[i].chargePoint != null)
			{
				this.lightningDispatcher.DispatchLightning(this.lightningDispatcher.transform.position, targetTool.energyMeters[i].chargePoint.position);
			}
			else
			{
				this.lightningDispatcher.DispatchLightning(this.lightningDispatcher.transform.position, targetTool.energyMeters[i].transform.position);
			}
		}
	}

	public void PlayChargeEffect(GRCurrencyDepositor targetDepositor)
	{
		if (targetDepositor == null)
		{
			return;
		}
		this.collectAudioSource.volume = this.chargeBeamVolume;
		this.collectAudioSource.PlayOneShot(this.chargeBeamSound);
		this.lightningDispatcher.DispatchLightning(this.lightningDispatcher.transform.position, targetDepositor.depositingChargePoint.position);
	}

	private bool IsButtonHeld()
	{
		if (!this.IsHeldLocal())
		{
			return false;
		}
		GamePlayer gamePlayer;
		if (!GamePlayer.TryGetGamePlayer(this.gameEntity.heldByActorNumber, out gamePlayer))
		{
			return false;
		}
		int num = gamePlayer.FindHandIndex(this.gameEntity.id);
		return num != -1 && ControllerInputPoller.TriggerFloat(GamePlayer.IsLeftHand(num) ? XRNode.LeftHand : XRNode.RightHand) > 0.25f;
	}

	private void PlayVibration(float strength, float duration)
	{
		if (!this.IsHeldLocal())
		{
			return;
		}
		GamePlayer gamePlayer;
		if (!GamePlayer.TryGetGamePlayer(this.gameEntity.heldByActorNumber, out gamePlayer))
		{
			return;
		}
		int num = gamePlayer.FindHandIndex(this.gameEntity.id);
		if (num == -1)
		{
			return;
		}
		GorillaTagger.Instance.StartVibration(GamePlayer.IsLeftHand(num), strength, duration);
	}

	public void GetDebugTextLines(out List<string> strings)
	{
		strings = new List<string>();
		strings.Add(string.Format("Recharge Rate: <color=\"yellow\">{0}<color=\"white\">", this.rechargeRate));
	}

	public GameEntity gameEntity;

	public GRTool tool;

	public GRAttributes attributes;

	public int energyDepositPerUse = 100;

	public Transform shootFrom;

	public LayerMask collectibleLayerMask;

	public ParticleSystem vacuumParticleEffect;

	public ParticleSystem upgrade1VacuumParticleEffect;

	public ParticleSystem upgrade2VacuumParticleEffect;

	public ParticleSystem upgrade3VacuumParticleEffect;

	public ParticleSystem passiveChargeParticleEffect;

	public AudioSource vacuumAudioSource;

	public AudioClip vacuumSound;

	public AudioClip upgrade1vacuumSound;

	public AudioClip upgrade2vacuumSound;

	public AudioClip upgrade3vacuumSound;

	public float vacuumSoundVolume = 0.2f;

	public AudioSource collectAudioSource;

	[FormerlySerializedAs("flashSound")]
	public AudioClip collectSound;

	[FormerlySerializedAs("flashSoundVolume")]
	public float collectSoundVolume = 1f;

	public AudioClip chargeBeamSound;

	public float chargeBeamVolume = 0.2f;

	public LightningDispatcher lightningDispatcher;

	public float chargeDuration = 0.75f;

	[FormerlySerializedAs("flashDuration")]
	public float collectDuration = 0.1f;

	public float cooldownDuration;

	public AbilityHaptic collectHaptic;

	[NonSerialized]
	public GhostReactorManager grManager;

	private float rechargeRate;

	public float rechargeInterval = 1f;

	private double lastRechargeTime;

	public float level3ChargeRadius = 4f;

	private GRToolCollector.State state;

	private float stateTimeRemaining;

	private bool activatedLocally;

	private bool waitingForButtonRelease;

	private RaycastHit[] tempHitResults = new RaycastHit[128];

	private enum State
	{
		Idle,
		Vacuuming,
		Collect,
		Cooldown
	}
}
