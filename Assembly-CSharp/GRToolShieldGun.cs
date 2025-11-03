using System;
using System.Collections.Generic;
using GorillaExtensions;
using GorillaLocomotion;
using Photon.Pun;
using UnityEngine;
using UnityEngine.XR;

public class GRToolShieldGun : MonoBehaviour
{
	private void Awake()
	{
		if (this.tool != null)
		{
			this.tool.onToolUpgraded += this.OnToolUpgraded;
			this.OnToolUpgraded(this.tool);
		}
	}

	private void OnToolUpgraded(GRTool tool)
	{
		if (tool.HasUpgradeInstalled(GRToolProgressionManager.ToolParts.ShieldGunStrength1))
		{
			this.firingSound = this.upgrade1FiringSound;
			return;
		}
		if (tool.HasUpgradeInstalled(GRToolProgressionManager.ToolParts.ShieldGunStrength2))
		{
			this.firingSound = this.upgrade2FiringSound;
			return;
		}
		if (tool.HasUpgradeInstalled(GRToolProgressionManager.ToolParts.ShieldGunStrength3))
		{
			this.firingSound = this.upgrade3FiringSound;
		}
	}

	private bool IsHeldLocal()
	{
		return this.gameEntity.heldByActorNumber == PhotonNetwork.LocalPlayer.ActorNumber;
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
		case GRToolShieldGun.State.Idle:
			if (this.tool.HasEnoughEnergy() && this.IsButtonHeld())
			{
				this.SetStateAuthority(GRToolShieldGun.State.Charging);
				this.activatedLocally = true;
				return;
			}
			break;
		case GRToolShieldGun.State.Charging:
		{
			bool flag = this.IsButtonHeld();
			this.stateTimeRemaining -= dt;
			if (this.stateTimeRemaining <= 0f)
			{
				this.SetStateAuthority(GRToolShieldGun.State.Firing);
				return;
			}
			if (!flag)
			{
				this.SetStateAuthority(GRToolShieldGun.State.Idle);
				this.activatedLocally = false;
				return;
			}
			break;
		}
		case GRToolShieldGun.State.Firing:
			this.stateTimeRemaining -= dt;
			if (this.stateTimeRemaining <= 0f)
			{
				this.SetStateAuthority(GRToolShieldGun.State.Cooldown);
				return;
			}
			break;
		case GRToolShieldGun.State.Cooldown:
			this.stateTimeRemaining -= dt;
			if (this.stateTimeRemaining <= 0f && !this.IsButtonHeld())
			{
				this.SetStateAuthority(GRToolShieldGun.State.Idle);
				this.activatedLocally = false;
			}
			break;
		default:
			return;
		}
	}

	private void OnUpdateRemote(float dt)
	{
		GRToolShieldGun.State state = (GRToolShieldGun.State)this.gameEntity.GetState();
		if (state != this.state)
		{
			this.SetStateAuthority(state);
		}
	}

	private void SetStateAuthority(GRToolShieldGun.State newState)
	{
		this.SetState(newState);
		this.gameEntity.RequestState(this.gameEntity.id, (long)newState);
	}

	private void SetState(GRToolShieldGun.State newState)
	{
		if (newState == this.state || !this.CanChangeState((long)newState))
		{
			return;
		}
		this.state = newState;
		switch (this.state)
		{
		case GRToolShieldGun.State.Idle:
			this.stateTimeRemaining = -1f;
			return;
		case GRToolShieldGun.State.Charging:
			this.StartCharge();
			this.stateTimeRemaining = this.chargeDuration;
			return;
		case GRToolShieldGun.State.Firing:
			this.StartFiring();
			this.stateTimeRemaining = this.flashDuration;
			return;
		case GRToolShieldGun.State.Cooldown:
			this.stateTimeRemaining = this.cooldownDuration;
			return;
		default:
			return;
		}
	}

	private void StartCharge()
	{
		if (this.chargeSound != null)
		{
			this.audioSource.PlayOneShot(this.chargeSound, this.chargeSoundVolume);
		}
		if (this.IsHeldLocal())
		{
			this.PlayVibration(GorillaTagger.Instance.tapHapticStrength, this.chargeDuration);
		}
	}

	private void StartFiring()
	{
		if (this.firingSound != null)
		{
			this.audioSource.PlayOneShot(this.firingSound, this.firingSoundVolume);
		}
		this.timeLastFired = Time.time;
		this.tool.UseEnergy();
		Vector3 position = this.firingTransform.position;
		Vector3 vector = this.firingTransform.forward * this.projectileSpeed;
		float scale = GTPlayer.Instance.scale;
		int num = PoolUtils.GameObjHashCode(this.projectilePrefab);
		this.firedProjectile = ObjectPools.instance.Instantiate(num, true).GetComponent<SlingshotProjectile>();
		this.firedProjectile.transform.localScale = Vector3.one * scale;
		if (this.projectileTrailPrefab != null)
		{
			int num2 = PoolUtils.GameObjHashCode(this.projectileTrailPrefab);
			this.AttachTrail(num2, this.firedProjectile.gameObject, position, false, false);
		}
		Collider component = this.firedProjectile.gameObject.GetComponent<Collider>();
		if (component != null)
		{
			for (int i = 0; i < this.colliders.Count; i++)
			{
				Physics.IgnoreCollision(this.colliders[i], component);
			}
		}
		if (this.IsHeldLocal())
		{
			this.firedProjectile.OnImpact += this.OnProjectileImpact;
		}
		this.onHaptic.PlayIfHeldLocal(this.gameEntity);
		this.firedProjectile.Launch(position, vector, NetworkSystem.Instance.LocalPlayer, false, false, 1, scale, true, this.projectileColor);
	}

	private void AttachTrail(int trailHash, GameObject newProjectile, Vector3 location, bool blueTeam, bool orangeTeam)
	{
		GameObject gameObject = ObjectPools.instance.Instantiate(trailHash, true);
		SlingshotProjectileTrail component = gameObject.GetComponent<SlingshotProjectileTrail>();
		if (component.IsNull())
		{
			ObjectPools.instance.Destroy(gameObject);
		}
		newProjectile.transform.position = location;
		component.AttachTrail(newProjectile, blueTeam, orangeTeam, false, default(Color));
	}

	private void OnProjectileImpact(SlingshotProjectile projectile, Vector3 impactPos, NetPlayer hitPlayer)
	{
		projectile.OnImpact -= this.OnProjectileImpact;
		GRPlayer grplayer = null;
		RigContainer rigContainer;
		if (hitPlayer != null && VRRigCache.Instance.TryGetVrrig(hitPlayer, out rigContainer) && rigContainer.Rig != null)
		{
			grplayer = rigContainer.Rig.GetComponent<GRPlayer>();
		}
		else if (this.allowAoeHits)
		{
			GRToolShieldGun.vrRigs.Clear();
			GRToolShieldGun.vrRigs.Add(VRRig.LocalRig);
			VRRigCache.Instance.GetAllUsedRigs(GRToolShieldGun.vrRigs);
			VRRig vrrig = null;
			float num = float.MaxValue;
			for (int i = 0; i < GRToolShieldGun.vrRigs.Count; i++)
			{
				float sqrMagnitude = (GRToolShieldGun.vrRigs[i].bodyTransform.position - impactPos).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					vrrig = GRToolShieldGun.vrRigs[i];
				}
			}
			if (vrrig != null)
			{
				grplayer = vrrig.GetComponent<GRPlayer>();
			}
		}
		if (grplayer != null)
		{
			int num2 = 0;
			if (this.tool.HasUpgradeInstalled(GRToolProgressionManager.ToolParts.ShieldGunStrength1))
			{
				num2 |= 1;
			}
			if (this.tool.HasUpgradeInstalled(GRToolProgressionManager.ToolParts.ShieldGunStrength2))
			{
				num2 |= 2;
			}
			if (this.tool.HasUpgradeInstalled(GRToolProgressionManager.ToolParts.ShieldGunStrength3))
			{
				num2 |= 4;
			}
			this.gameEntity.manager.ghostReactorManager.RequestGrantPlayerShield(grplayer, this.attributes.CalculateFinalValueForAttribute(GRAttributeType.ShieldSize), num2);
		}
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

	public bool CanChangeState(long newStateIndex)
	{
		return newStateIndex >= 0L && newStateIndex < 4L && ((int)newStateIndex != 2 || Time.time > this.timeLastFired + this.cooldownMinimum);
	}

	public GameEntity gameEntity;

	public GRTool tool;

	public GRAttributes attributes;

	public GameObject projectilePrefab;

	public GameObject projectileTrailPrefab;

	public Transform firingTransform;

	public List<Collider> colliders;

	public float projectileSpeed = 25f;

	public Color projectileColor = new Color(0.25f, 0.25f, 1f);

	public bool allowAoeHits;

	public float aeoHitRadius = 0.5f;

	public float chargeDuration = 0.75f;

	public float flashDuration = 0.1f;

	public float cooldownDuration;

	public AudioSource audioSource;

	public AudioClip chargeSound;

	public float chargeSoundVolume = 0.5f;

	public AudioClip firingSound;

	public float firingSoundVolume = 0.5f;

	public AudioClip upgrade1FiringSound;

	public AudioClip upgrade2FiringSound;

	public AudioClip upgrade3FiringSound;

	[Header("Haptic")]
	public AbilityHaptic onHaptic;

	private GRToolShieldGun.State state;

	private float stateTimeRemaining;

	private bool activatedLocally;

	private bool waitingForButtonRelease;

	private float timeLastFired;

	private float cooldownMinimum = 0.35f;

	private SlingshotProjectile firedProjectile;

	private static List<VRRig> vrRigs = new List<VRRig>(10);

	private enum State
	{
		Idle,
		Charging,
		Firing,
		Cooldown,
		Count
	}
}
