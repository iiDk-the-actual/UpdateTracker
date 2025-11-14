using System;
using System.Collections;
using System.Collections.Generic;
using GorillaLocomotion;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(GameGrabbable))]
[RequireComponent(typeof(GameSnappable))]
[RequireComponent(typeof(GameButtonActivatable))]
public class SIGadgetBlaster : SIGadget
{
	protected override void OnEnable()
	{
		base.OnEnable();
		this.currentCharge = 0f;
		this.lastFired = 0f;
		GameEntity gameEntity = this.gameEntity;
		gameEntity.OnGrabbed = (Action)Delegate.Combine(gameEntity.OnGrabbed, new Action(this.StartGrabbing));
		GameEntity gameEntity2 = this.gameEntity;
		gameEntity2.OnSnapped = (Action)Delegate.Combine(gameEntity2.OnSnapped, new Action(this.StartGrabbing));
		GameEntity gameEntity3 = this.gameEntity;
		gameEntity3.OnReleased = (Action)Delegate.Combine(gameEntity3.OnReleased, new Action(this.StopGrabbing));
		GameEntity gameEntity4 = this.gameEntity;
		gameEntity4.OnUnsnapped = (Action)Delegate.Combine(gameEntity4.OnUnsnapped, new Action(this.StopGrabbing));
	}

	protected override void OnUpdateAuthority(float dt)
	{
		base.OnUpdateAuthority(dt);
		switch (this.currentState)
		{
		case SIGadgetBlaster.BlasterState.Idle:
			if (this.CheckInput())
			{
				this.FireProjectile(0f, this.NextFireId(), this.firingPosition.position, this.firingPosition.rotation);
				this.SetStateAuthority(SIGadgetBlaster.BlasterState.Charging);
				return;
			}
			break;
		case SIGadgetBlaster.BlasterState.Charging:
			this.currentCharge += this.chargeRatePerSecond * Time.deltaTime;
			this.UpdateChargingVisuals();
			if (!this.CheckInput())
			{
				if (this.currentCharge >= this.mediumChargeLevel)
				{
					this.FireProjectile(this.currentCharge, this.NextFireId(), this.firingPosition.position, this.firingPosition.rotation);
					this.SetStateAuthority(SIGadgetBlaster.BlasterState.Cooldown);
					return;
				}
				this.SetStateAuthority(SIGadgetBlaster.BlasterState.Idle);
				return;
			}
			break;
		case SIGadgetBlaster.BlasterState.Cooldown:
			if (Time.time >= this.lastFired + this.fireCooldown)
			{
				if (this.CheckInput())
				{
					this.SetStateAuthority(SIGadgetBlaster.BlasterState.Charging);
					return;
				}
				this.SetStateAuthority(SIGadgetBlaster.BlasterState.Idle);
			}
			break;
		default:
			return;
		}
	}

	protected override void OnUpdateRemote(float dt)
	{
		base.OnUpdateRemote(dt);
		SIGadgetBlaster.BlasterState blasterState = (SIGadgetBlaster.BlasterState)this.gameEntity.GetState();
		if (blasterState != this.currentState)
		{
			this.SetStateShared(blasterState);
		}
		switch (this.currentState)
		{
		case SIGadgetBlaster.BlasterState.Idle:
		case SIGadgetBlaster.BlasterState.Cooldown:
			break;
		case SIGadgetBlaster.BlasterState.Charging:
			this.currentCharge += this.chargeRatePerSecond * Time.deltaTime;
			this.UpdateChargingVisuals();
			break;
		default:
			return;
		}
	}

	private void SetStateAuthority(SIGadgetBlaster.BlasterState newState)
	{
		this.SetStateShared(newState);
		this.gameEntity.RequestState(this.gameEntity.id, (long)newState);
	}

	private void SetStateShared(SIGadgetBlaster.BlasterState newState)
	{
		if (newState == this.currentState || !SIGadgetBlaster.CanChangeState((long)newState))
		{
			return;
		}
		SIGadgetBlaster.BlasterState blasterState = this.currentState;
		this.currentState = newState;
		switch (this.currentState)
		{
		case SIGadgetBlaster.BlasterState.Idle:
			this.blasterSource.clip = this.idleClip;
			this.blasterSource.volume = this.idleVolume;
			this.currentCharge = 0f;
			break;
		case SIGadgetBlaster.BlasterState.Charging:
			this.currentCharge = 0f;
			this.blasterSource.clip = this.chargingClip;
			this.blasterSource.volume = this.chargingSmallVolume;
			this.blasterSource.loop = true;
			this.blasterSource.Play();
			break;
		case SIGadgetBlaster.BlasterState.Cooldown:
			this.blasterSource.Stop();
			if (Time.time > this.lastFired + this.fireCooldown)
			{
				this.lastFired = Time.time;
			}
			break;
		}
		this.UpdateChargingVisuals();
	}

	private void UpdateChargingVisuals()
	{
		bool flag = this.currentState == SIGadgetBlaster.BlasterState.Charging;
		bool flag2 = this.currentCharge >= this.largeChargeLevel && flag;
		bool flag3 = this.currentCharge >= this.mediumChargeLevel && !flag2 && flag;
		bool flag4 = !flag2 && !flag3 && flag;
		if (this.largeChargingFX.activeSelf != flag2)
		{
			if (flag2)
			{
				this.blasterSource.clip = this.chargingClip;
				this.blasterSource.volume = this.chargingLargeVolume;
			}
			this.largeChargingFX.SetActive(flag2);
		}
		if (this.mediumChargingFX.activeSelf != flag3)
		{
			if (flag3)
			{
				this.blasterSource.clip = this.chargingClip;
				this.blasterSource.volume = this.chargingMediumVolume;
			}
			this.mediumChargingFX.SetActive(flag3);
		}
		if (this.smallChargingFX.activeSelf != flag4)
		{
			if (flag4)
			{
				this.blasterSource.volume = this.chargingSmallVolume;
				this.blasterSource.clip = this.chargingClip;
			}
			this.smallChargingFX.SetActive(flag4);
		}
		if (!flag)
		{
			this.blasterSource.Stop();
		}
	}

	public override void ApplyUpgradeNodes(SIUpgradeSet withUpgrades)
	{
	}

	private static bool CanChangeState(long newStateIndex)
	{
		return newStateIndex >= 0L && newStateIndex < 3L;
	}

	private bool CheckInput()
	{
		float num = (this.wasActivated ? this.inputActivateThreshold : this.inputDeactivateThreshold);
		return this.buttonActivatable.CheckInput(true, true, num, true);
	}

	private int NextFireId()
	{
		int num = this.projectileId;
		this.projectileId = num + 1;
		return num;
	}

	public void FireProjectile(float firedAtChargeLevel, int fireId, Vector3 position, Quaternion rotation)
	{
		if (this.IsEquippedLocal() || this.activatedLocally)
		{
			if (Time.time < this.lastFired + this.fireCooldown)
			{
				return;
			}
			base.SendClientToClientRPC(0, new object[] { firedAtChargeLevel, fireId, position, rotation });
		}
		if (this.projectileCount > this.maxProjectileCount)
		{
			return;
		}
		if (Mathf.Abs(this.currentCharge - firedAtChargeLevel) <= this.maxChargeDiff)
		{
			this.currentCharge = firedAtChargeLevel;
		}
		GameObject gameObject;
		if (this.currentCharge > this.largeChargeLevel)
		{
			this.firingSource.clip = this.firingLargeClip;
			this.firingSource.volume = this.firingLargeVolume;
			this.largeFireFX.Play();
			gameObject = this.largeChargeProjectile;
		}
		else if (this.currentCharge > this.mediumChargeLevel)
		{
			this.firingSource.clip = this.firingMediumClip;
			this.firingSource.volume = this.firingMediumVolume;
			this.mediumFireFX.Play();
			gameObject = this.mediumChargeProjectile;
		}
		else
		{
			this.firingSource.clip = this.firingSmallClip;
			this.firingSource.volume = this.firingSmallVolume;
			this.smallFireFX.Play();
			gameObject = this.smallProjectile;
		}
		this.firingSource.time = 0f;
		this.firingSource.Play();
		this.firingSource.loop = false;
		this.currentCharge = 0f;
		this.projectileCount++;
		SIGadgetBlasterProjectile component = Object.Instantiate<GameObject>(gameObject, position, rotation).GetComponent<SIGadgetBlasterProjectile>();
		component.parentBlaster = this;
		component.projectileId = fireId;
		component.firedByPlayer = (this.gameEntity.IsHeld() ? SIPlayer.Get(this.gameEntity.heldByActorNumber) : SIPlayer.Get(this.gameEntity.snappedByActorNumber));
		this.activeProjectiles.Add(component);
		this.lastFired = Time.time;
	}

	public override void ProcessClientToClientRPC(PhotonMessageInfo info, int rpcID, object[] data)
	{
		if (rpcID != 0)
		{
			if (rpcID != 1)
			{
				return;
			}
			if (data == null || data.Length != 4)
			{
				return;
			}
			int num;
			if (!GameEntityManager.ValidateDataType<int>(data[0], out num))
			{
				return;
			}
			Vector3 vector;
			if (!GameEntityManager.ValidateDataType<Vector3>(data[1], out vector))
			{
				return;
			}
			Vector3 vector2;
			if (!GameEntityManager.ValidateDataType<Vector3>(data[2], out vector2))
			{
				return;
			}
			int num2;
			if (!GameEntityManager.ValidateDataType<int>(data[3], out num2))
			{
				return;
			}
			SIGadgetBlasterProjectile sigadgetBlasterProjectile = null;
			for (int i = 0; i < this.activeProjectiles.Count; i++)
			{
				if (this.activeProjectiles[i].projectileId == num)
				{
					sigadgetBlasterProjectile = this.activeProjectiles[i];
					break;
				}
			}
			if (sigadgetBlasterProjectile == null)
			{
				return;
			}
			if (sigadgetBlasterProjectile.firedByPlayer != SIPlayer.Get(info.Sender.ActorNumber))
			{
				return;
			}
			if ((sigadgetBlasterProjectile.transform.position - vector).magnitude > this.maxLagDistance)
			{
				return;
			}
			SIGadgetBlasterProjectile.BlasterProjectileSize projectileSize = sigadgetBlasterProjectile.projectileSize;
			this.DespawnProjectile(sigadgetBlasterProjectile);
			SIPlayer siplayer = SIPlayer.Get(num2);
			if (siplayer != null && sigadgetBlasterProjectile.hitEffectPlayer != null)
			{
				Object.Instantiate<GameObject>(sigadgetBlasterProjectile.hitEffect, vector, sigadgetBlasterProjectile.transform.rotation);
			}
			if (siplayer != SIPlayer.LocalPlayer)
			{
				return;
			}
			this.TriggerBlastHitPlayerKnockback(projectileSize, vector2);
			return;
		}
		else
		{
			if (data == null || data.Length != 4)
			{
				return;
			}
			float num3;
			if (!GameEntityManager.ValidateDataType<float>(data[0], out num3))
			{
				return;
			}
			int num4;
			if (!GameEntityManager.ValidateDataType<int>(data[1], out num4))
			{
				return;
			}
			Vector3 vector3;
			if (!GameEntityManager.ValidateDataType<Vector3>(data[2], out vector3))
			{
				return;
			}
			Quaternion quaternion;
			if (!GameEntityManager.ValidateDataType<Quaternion>(data[3], out quaternion))
			{
				return;
			}
			if (!this.gameEntity.IsAttachedToPlayer(NetPlayer.Get(info.Sender)))
			{
				return;
			}
			this.FireProjectile(num3, num4, vector3, quaternion);
			return;
		}
	}

	public void TriggerBlastHitPlayer(SIPlayer playerHit, int projectileId, Vector3 position, Vector3 forwardDirection)
	{
		if (playerHit == SIPlayer.LocalPlayer)
		{
			return;
		}
		float num = Vector3.Angle(forwardDirection, Vector3.up);
		Vector3 vector = Vector3.RotateTowards(forwardDirection.normalized, Vector3.up, Mathf.Clamp(num - this.upwardsAngle, 0f, this.upwardsAngle) * 0.017453292f, 0f);
		base.SendClientToClientRPC(1, new object[] { projectileId, position, vector, playerHit.ActorNr });
	}

	public void TriggerBlastHitPlayerKnockback(SIGadgetBlasterProjectile.BlasterProjectileSize projectileSize, Vector3 direction)
	{
		float num = 0f;
		switch (projectileSize)
		{
		case SIGadgetBlasterProjectile.BlasterProjectileSize.Small:
			num = this.smallProjectileKnockbackSpeed;
			break;
		case SIGadgetBlasterProjectile.BlasterProjectileSize.Medium:
			num = this.mediumProjectileKnockbackSpeed;
			break;
		case SIGadgetBlasterProjectile.BlasterProjectileSize.Large:
			num = this.largeProjectileKnockbackSpeed;
			break;
		}
		GTPlayer.Instance.ApplyKnockback(direction.normalized, num, true);
	}

	public void StartGrabbing()
	{
		if (this.IsEquippedLocal() || this.activatedLocally)
		{
			this.SetStateAuthority(SIGadgetBlaster.BlasterState.Idle);
		}
	}

	public void StopGrabbing()
	{
		this.SetStateShared(SIGadgetBlaster.BlasterState.Idle);
	}

	public void ProjectileHit(SIPlayer hitPlayer, SIGadgetBlasterProjectile projectile)
	{
		if (hitPlayer != null && projectile.hitEffectPlayer != null)
		{
			Object.Instantiate<GameObject>(projectile.hitEffectPlayer, projectile.transform.position, projectile.transform.rotation);
		}
		if (hitPlayer == null && projectile.hitEffect != null)
		{
			Object.Instantiate<GameObject>(projectile.hitEffect, projectile.transform.position, projectile.transform.rotation);
		}
		if (hitPlayer != null)
		{
			this.TriggerBlastHitPlayer(hitPlayer, projectile.projectileId, projectile.transform.position, projectile.transform.forward);
		}
		this.DespawnProjectile(projectile);
	}

	private void DespawnProjectile(SIGadgetBlasterProjectile projectile)
	{
		projectile.gameObject.SetActive(false);
		if (!this.projectilesToDespawn.Contains(projectile))
		{
			base.StartCoroutine(this.DelayedDestroyProjectile(projectile));
		}
	}

	public IEnumerator DelayedDestroyProjectile(SIGadgetBlasterProjectile projectile)
	{
		this.projectilesToDespawn.Add(projectile);
		yield return this.projectileDestroyDelay;
		this.projectileCount--;
		if (this.activeProjectiles.Contains(projectile))
		{
			this.activeProjectiles.Remove(projectile);
		}
		if (projectile == null || projectile.gameObject == null)
		{
			yield return null;
		}
		this.projectilesToDespawn.Remove(projectile);
		Object.Destroy(projectile.gameObject);
		yield break;
	}

	private SIGadgetBlaster.BlasterState currentState;

	public GameObject smallProjectile;

	public GameObject mediumChargeProjectile;

	public GameObject largeChargeProjectile;

	[SerializeField]
	private GameButtonActivatable buttonActivatable;

	[SerializeField]
	private float inputActivateThreshold = 0.35f;

	[SerializeField]
	private float inputDeactivateThreshold = 0.25f;

	[SerializeField]
	private float chargeRatePerSecond = 20f;

	[SerializeField]
	private float mediumChargeLevel = 20f;

	[SerializeField]
	private float largeChargeLevel = 60f;

	[SerializeField]
	private int maxProjectileCount = 10;

	[SerializeField]
	private float fireCooldown = 0.2f;

	public float upwardsAngle = 30f;

	public float maxLagDistance = 5f;

	public float verticalOffset = -0.133f;

	public float largeProjectileKnockbackSpeed = 8f;

	public float mediumProjectileKnockbackSpeed = 5f;

	public float smallProjectileKnockbackSpeed = 2f;

	private bool wasActivated;

	public float maxChargeDiff = 5f;

	public const float PROJECTILE_MAX_LATENCY = 1f;

	private float currentCharge;

	private float lastFired;

	private int projectileCount;

	private int projectileId;

	private WaitForSeconds projectileDestroyDelay = new WaitForSeconds(1f);

	private List<SIGadgetBlasterProjectile> activeProjectiles = new List<SIGadgetBlasterProjectile>();

	private List<SIGadgetBlasterProjectile> projectilesToDespawn = new List<SIGadgetBlasterProjectile>();

	public Transform firingPosition;

	public AudioSource firingSource;

	public AudioClip firingSmallClip;

	public AudioClip firingMediumClip;

	public AudioClip firingLargeClip;

	public float firingSmallVolume;

	public float firingMediumVolume;

	public float firingLargeVolume;

	public AudioSource blasterSource;

	public AudioClip idleClip;

	public AudioClip chargingClip;

	public float idleVolume;

	public float chargingSmallVolume;

	public float chargingMediumVolume;

	public float chargingLargeVolume;

	public ParticleSystem smallFireFX;

	public ParticleSystem mediumFireFX;

	public ParticleSystem largeFireFX;

	public GameObject smallChargingFX;

	public GameObject mediumChargingFX;

	public GameObject largeChargingFX;

	private enum BlasterState
	{
		Idle,
		Charging,
		Cooldown,
		Count
	}

	private enum RPCCalls
	{
		FireProjectile,
		ProjectileHitPlayer
	}
}
