using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.XR;

public class GRToolDirectionalShield : MonoBehaviour, IGameHitter
{
	private void Awake()
	{
		this.hitter = base.GetComponent<GameHitter>();
		this.attributes = base.GetComponent<GRAttributes>();
		if (this.tool != null)
		{
			this.tool.onToolUpgraded += this.OnToolUpgraded;
			this.OnToolUpgraded(this.tool);
		}
	}

	private void OnToolUpgraded(GRTool tool)
	{
		if (tool.HasUpgradeInstalled(GRToolProgressionManager.ToolParts.DirectionalShieldSize1))
		{
			this.deflectAudio = this.upgrade1DeflectAudio;
			this.shieldDeflectVFX = this.upgrade1ShieldDeflectVFX;
			this.reflectsProjectiles = true;
			return;
		}
		if (tool.HasUpgradeInstalled(GRToolProgressionManager.ToolParts.DirectionalShieldSize2))
		{
			this.deflectAudio = this.upgrade2DeflectAudio;
			this.shieldDeflectVFX = this.upgrade2ShieldDeflectVFX;
			this.reflectsProjectiles = false;
			return;
		}
		if (tool.HasUpgradeInstalled(GRToolProgressionManager.ToolParts.DirectionalShieldSize3))
		{
			this.deflectAudio = this.upgrade3DeflectAudio;
			this.shieldDeflectVFX = this.upgrade3ShieldDeflectVFX;
			this.reflectsProjectiles = true;
			return;
		}
		this.reflectsProjectiles = false;
	}

	public void OnEnable()
	{
		this.SetState(GRToolDirectionalShield.State.Closed);
	}

	private bool IsHeldLocal()
	{
		return this.gameEntity.heldByActorNumber == PhotonNetwork.LocalPlayer.ActorNumber;
	}

	private bool IsHeld()
	{
		return this.gameEntity.heldByActorNumber != -1;
	}

	public void BlockHittable(Vector3 enemyPosition, Vector3 enemyAttackDirection, GameHittable hittable, GRShieldCollider shieldCollider)
	{
		if (this.IsHeldLocal())
		{
			float num = 1f;
			if (this.attributes != null && this.attributes.HasValueForAttribute(GRAttributeType.KnockbackMultiplier))
			{
				num = this.attributes.CalculateFinalFloatValueForAttribute(GRAttributeType.KnockbackMultiplier);
			}
			Vector3 vector = -enemyAttackDirection * shieldCollider.KnockbackVelocity * num;
			if (this.reflectsProjectiles)
			{
				GRRangedEnemyProjectile component = hittable.GetComponent<GRRangedEnemyProjectile>();
				Vector3 vector2;
				if (component != null && component.owningEntity != null && GREnemyRanged.CalculateLaunchDirection(enemyPosition, component.owningEntity.transform.position + new Vector3(0f, 0.5f, 0f), component.projectileSpeed, out vector2))
				{
					vector = vector2 * component.projectileSpeed;
				}
			}
			GameHitData gameHitData = new GameHitData
			{
				hitTypeId = 2,
				hitEntityId = hittable.gameEntity.id,
				hitByEntityId = this.gameEntity.id,
				hitEntityPosition = enemyPosition,
				hitImpulse = vector,
				hitPosition = enemyPosition,
				hitAmount = this.hitter.CalcHitAmount(GameHitType.Shield, hittable, this.gameEntity)
			};
			if (hittable.IsHitValid(gameHitData))
			{
				hittable.RequestHit(gameHitData);
			}
		}
	}

	public void OnEnemyBlocked(Vector3 enemyPosition)
	{
		this.tool.UseEnergy();
		this.PlayBlockEffects(enemyPosition);
	}

	private void PlayBlockEffects(Vector3 enemyPosition)
	{
		this.audioSource.PlayOneShot(this.deflectAudio, this.deflectVolume);
		this.shieldDeflectVFX.Play();
		Vector3 vector = Vector3.ClampMagnitude(enemyPosition - this.shieldArcCenterReferencePoint.position, this.shieldArcCenterRadius);
		Vector3 vector2 = this.shieldArcCenterReferencePoint.position + vector;
		this.shieldDeflectImpactPointVFX.transform.position = vector2;
		this.shieldDeflectImpactPointVFX.Play();
	}

	public void OnSuccessfulHit(GameHitData hitData)
	{
		this.tool.UseEnergy();
		this.PlayBlockEffects(hitData.hitEntityPosition);
	}

	public void Update()
	{
		float deltaTime = Time.deltaTime;
		if (!this.IsHeld())
		{
			this.SetState(GRToolDirectionalShield.State.Closed);
			return;
		}
		if (this.IsHeldLocal())
		{
			this.OnUpdateAuthority(deltaTime);
			return;
		}
		this.OnUpdateRemote(deltaTime);
	}

	private void OnUpdateAuthority(float dt)
	{
		GRToolDirectionalShield.State state = this.state;
		if (state != GRToolDirectionalShield.State.Closed)
		{
			if (state != GRToolDirectionalShield.State.Open)
			{
				return;
			}
			if (!this.IsButtonHeld() || !this.tool.HasEnoughEnergy())
			{
				this.SetStateAuthority(GRToolDirectionalShield.State.Closed);
			}
		}
		else if (this.IsButtonHeld() && this.tool.HasEnoughEnergy())
		{
			this.SetStateAuthority(GRToolDirectionalShield.State.Open);
			return;
		}
	}

	private void OnUpdateRemote(float dt)
	{
		GRToolDirectionalShield.State state = (GRToolDirectionalShield.State)this.gameEntity.GetState();
		if (state != this.state)
		{
			this.SetState(state);
		}
	}

	private void SetStateAuthority(GRToolDirectionalShield.State newState)
	{
		this.SetState(newState);
		this.gameEntity.RequestState(this.gameEntity.id, (long)newState);
	}

	private void SetState(GRToolDirectionalShield.State newState)
	{
		if (this.state == newState)
		{
			return;
		}
		GRToolDirectionalShield.State state = this.state;
		if (state != GRToolDirectionalShield.State.Closed)
		{
		}
		this.state = newState;
		state = this.state;
		if (state == GRToolDirectionalShield.State.Closed)
		{
			this.openCollidersParent.gameObject.SetActive(false);
			for (int i = 0; i < this.shieldAnimators.Count; i++)
			{
				this.shieldAnimators[i].SetBool("Activated", false);
			}
			this.audioSource.PlayOneShot(this.closeAudio, this.closeVolume);
			this.closeHaptic.PlayIfHeldLocal(this.gameEntity);
			this.hitter != null;
			return;
		}
		if (state != GRToolDirectionalShield.State.Open)
		{
			return;
		}
		this.openCollidersParent.gameObject.SetActive(true);
		for (int j = 0; j < this.shieldAnimators.Count; j++)
		{
			this.shieldAnimators[j].SetBool("Activated", true);
		}
		this.audioSource.PlayOneShot(this.openAudio, this.openVolume);
		this.openHaptic.PlayIfHeldLocal(this.gameEntity);
		this.hitter != null;
	}

	private bool IsButtonHeld()
	{
		if (!this.IsHeldLocal())
		{
			return false;
		}
		GamePlayer gamePlayer = GamePlayer.GetGamePlayer(this.gameEntity.heldByActorNumber);
		if (gamePlayer == null)
		{
			return false;
		}
		int num = gamePlayer.FindHandIndex(this.gameEntity.id);
		return num != -1 && ControllerInputPoller.TriggerFloat(GamePlayer.IsLeftHand(num) ? XRNode.LeftHand : XRNode.RightHand) > 0.25f;
	}

	[Header("References")]
	public GameEntity gameEntity;

	public GRTool tool;

	public Rigidbody rigidBody;

	public AudioSource audioSource;

	public List<Animator> shieldAnimators;

	public Transform openCollidersParent;

	private GameHitter hitter;

	private GRAttributes attributes;

	[Header("Audio")]
	public AudioClip openAudio;

	public float openVolume = 0.5f;

	public AudioClip closeAudio;

	public float closeVolume = 0.5f;

	public AudioClip deflectAudio;

	public AudioClip upgrade1DeflectAudio;

	public AudioClip upgrade2DeflectAudio;

	public AudioClip upgrade3DeflectAudio;

	public float deflectVolume = 0.5f;

	[Header("VFX")]
	public ParticleSystem shieldDeflectVFX;

	public ParticleSystem upgrade1ShieldDeflectVFX;

	public ParticleSystem upgrade2ShieldDeflectVFX;

	public ParticleSystem upgrade3ShieldDeflectVFX;

	public ParticleSystem shieldDeflectImpactPointVFX;

	public Transform shieldArcCenterReferencePoint;

	public float shieldArcCenterRadius = 1f;

	[Header("Haptic")]
	public AbilityHaptic openHaptic;

	public AbilityHaptic closeHaptic;

	public bool reflectsProjectiles;

	private GRToolDirectionalShield.State state;

	private enum State
	{
		Closed,
		Open
	}
}
