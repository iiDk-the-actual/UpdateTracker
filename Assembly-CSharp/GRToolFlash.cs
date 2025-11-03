using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.XR;

public class GRToolFlash : MonoBehaviour, IGameEntityDebugComponent, IGameEntityComponent
{
	private void Awake()
	{
		this.state = GRToolFlash.State.Idle;
		this.stateTimeRemaining = -1f;
		this.gameHitter = base.GetComponent<GameHitter>();
	}

	private void OnEnable()
	{
		this.StopFlash();
		this.SetState(GRToolFlash.State.Idle);
	}

	public void OnEntityInit()
	{
		if (this.tool != null)
		{
			this.tool.onToolUpgraded += this.OnToolUpgraded;
			this.OnToolUpgraded(this.tool);
		}
	}

	public void OnEntityDestroy()
	{
	}

	public void OnEntityStateChange(long prevState, long nextState)
	{
	}

	private void OnToolUpgraded(GRTool tool)
	{
		this.stunDuration = this.attributes.CalculateFinalFloatValueForAttribute(GRAttributeType.FlashStunDuration);
		if (tool.HasUpgradeInstalled(GRToolProgressionManager.ToolParts.FlashDamage1))
		{
			this.flashSound = this.upgrade1FlashSound;
			this.flash = this.upgrade1FlashCone;
			return;
		}
		if (tool.HasUpgradeInstalled(GRToolProgressionManager.ToolParts.FlashDamage2))
		{
			this.flashSound = this.upgrade2FlashSound;
			this.flash = this.upgrade2FlashCone;
			return;
		}
		if (tool.HasUpgradeInstalled(GRToolProgressionManager.ToolParts.FlashDamage3))
		{
			this.flashSound = this.upgrade3FlashSound;
			this.flash = this.upgrade3FlashCone;
		}
	}

	private bool IsHeldLocal()
	{
		return this.item.heldByActorNumber == PhotonNetwork.LocalPlayer.ActorNumber;
	}

	public void OnUpdate(float dt)
	{
		if (this.IsHeldLocal())
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
		case GRToolFlash.State.Idle:
			if (this.tool.HasEnoughEnergy() && this.IsButtonHeld())
			{
				this.SetStateAuthority(GRToolFlash.State.Charging);
				this.activatedLocally = true;
				return;
			}
			break;
		case GRToolFlash.State.Charging:
		{
			bool flag = this.IsButtonHeld();
			this.stateTimeRemaining -= dt;
			if (this.stateTimeRemaining <= 0f)
			{
				this.SetStateAuthority(GRToolFlash.State.Flash);
				return;
			}
			if (!flag)
			{
				this.SetStateAuthority(GRToolFlash.State.Idle);
				this.activatedLocally = false;
				return;
			}
			break;
		}
		case GRToolFlash.State.Flash:
			this.stateTimeRemaining -= dt;
			if (this.stateTimeRemaining <= 0f)
			{
				this.SetStateAuthority(GRToolFlash.State.Cooldown);
				return;
			}
			break;
		case GRToolFlash.State.Cooldown:
			this.stateTimeRemaining -= dt;
			if (this.stateTimeRemaining <= 0f && !this.IsButtonHeld())
			{
				this.SetStateAuthority(GRToolFlash.State.Idle);
				this.activatedLocally = false;
			}
			break;
		default:
			return;
		}
	}

	private void OnUpdateRemote(float dt)
	{
		GRToolFlash.State state = (GRToolFlash.State)this.gameEntity.GetState();
		if (state != this.state)
		{
			if (this.state == GRToolFlash.State.Charging && state == GRToolFlash.State.Cooldown)
			{
				this.SetState(GRToolFlash.State.Flash);
				return;
			}
			if (this.state == GRToolFlash.State.Flash && state == GRToolFlash.State.Cooldown)
			{
				if (Time.time > this.timeLastFlashed + this.flashDuration)
				{
					this.SetState(GRToolFlash.State.Cooldown);
					return;
				}
			}
			else
			{
				this.SetState(state);
			}
		}
	}

	private void SetStateAuthority(GRToolFlash.State newState)
	{
		this.SetState(newState);
		this.gameEntity.RequestState(this.gameEntity.id, (long)newState);
	}

	private void SetState(GRToolFlash.State newState)
	{
		if (!this.CanChangeState((long)newState))
		{
			return;
		}
		this.state = newState;
		switch (this.state)
		{
		case GRToolFlash.State.Idle:
			this.stateTimeRemaining = -1f;
			return;
		case GRToolFlash.State.Charging:
			this.StartCharge();
			this.stateTimeRemaining = this.chargeDuration;
			return;
		case GRToolFlash.State.Flash:
			this.StartFlash();
			this.stateTimeRemaining = this.flashDuration;
			return;
		case GRToolFlash.State.Cooldown:
			this.StopFlash();
			this.stateTimeRemaining = this.cooldownDuration;
			return;
		default:
			return;
		}
	}

	private void StartCharge()
	{
		this.audioSource.volume = this.chargeSoundVolume;
		this.audioSource.clip = this.chargeSound;
		this.audioSource.Play();
		if (this.IsHeldLocal())
		{
			this.PlayVibration(GorillaTagger.Instance.tapHapticStrength, this.chargeDuration);
		}
	}

	private void StartFlash()
	{
		this.flash.SetActive(true);
		this.audioSource.volume = this.flashSoundVolume;
		this.audioSource.clip = this.flashSound;
		this.audioSource.Play();
		this.tool.UseEnergy();
		this.timeLastFlashed = Time.time;
		if (this.IsHeldLocal())
		{
			int num = Physics.SphereCastNonAlloc(this.shootFrom.position, 1f, this.shootFrom.rotation * Vector3.forward, this.tempHitResults, 5f, this.enemyLayerMask);
			for (int i = 0; i < num; i++)
			{
				RaycastHit raycastHit = this.tempHitResults[i];
				Rigidbody attachedRigidbody = raycastHit.collider.attachedRigidbody;
				if (attachedRigidbody != null)
				{
					GameHittable component = attachedRigidbody.GetComponent<GameHittable>();
					if (component != null && this.gameHitter != null)
					{
						GameHitData gameHitData = new GameHitData
						{
							hitTypeId = 1,
							hitEntityId = component.gameEntity.id,
							hitByEntityId = this.gameEntity.id,
							hitEntityPosition = component.gameEntity.transform.position,
							hitPosition = ((raycastHit.distance == 0f) ? this.shootFrom.position : raycastHit.point),
							hitImpulse = Vector3.zero,
							hitAmount = this.gameHitter.CalcHitAmount(GameHitType.Flash, component, this.gameEntity)
						};
						component.RequestHit(gameHitData);
					}
				}
			}
		}
	}

	private void StopFlash()
	{
		this.flash.SetActive(false);
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
		int num = gamePlayer.FindHandIndex(this.item.id);
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
		int num = gamePlayer.FindHandIndex(this.item.id);
		if (num == -1)
		{
			return;
		}
		GorillaTagger.Instance.StartVibration(GamePlayer.IsLeftHand(num), strength, duration);
	}

	public bool CanChangeState(long newStateIndex)
	{
		return newStateIndex >= 0L && newStateIndex < 4L && ((int)newStateIndex != 2 || Time.time > this.timeLastFlashed + this.cooldownMinimum);
	}

	public void GetDebugTextLines(out List<string> strings)
	{
		strings = new List<string>();
		strings.Add(string.Format("Stun Duration: <color=\"yellow\">{0}<color=\"white\">", this.stunDuration));
	}

	public GameEntity gameEntity;

	public GRTool tool;

	public GRAttributes attributes;

	public GameObject flash;

	public Transform shootFrom;

	public LayerMask enemyLayerMask;

	public AudioSource audioSource;

	public AudioClip chargeSound;

	public float chargeSoundVolume = 0.2f;

	public AudioClip flashSound;

	public AudioClip upgrade1FlashSound;

	public AudioClip upgrade2FlashSound;

	public AudioClip upgrade3FlashSound;

	public GameObject upgrade1FlashCone;

	public GameObject upgrade2FlashCone;

	public GameObject upgrade3FlashCone;

	public float flashSoundVolume = 1f;

	public float stunDuration;

	public GRToolFlash.UpgradeTypes upgradesApplied;

	public float chargeDuration = 0.75f;

	public float flashDuration = 0.1f;

	public float cooldownDuration;

	private float timeLastFlashed;

	private float cooldownMinimum = 0.35f;

	private bool activatedLocally;

	public GameEntity item;

	private GameHitter gameHitter;

	private GRToolFlash.State state;

	private float stateTimeRemaining;

	private RaycastHit[] tempHitResults = new RaycastHit[128];

	[Flags]
	public enum UpgradeTypes
	{
		None = 1,
		UpagredA = 2,
		UpagredB = 4,
		UpagredC = 8
	}

	private enum State
	{
		Idle,
		Charging,
		Flash,
		Cooldown,
		Count
	}
}
