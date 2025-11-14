using System;
using System.Collections.Generic;
using GorillaTag;
using GorillaTagScripts;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class SnowballThrowable : HoldableObject
{
	public XformOffset SpawnOffset
	{
		get
		{
			return this.spawnOffset;
		}
		set
		{
			this.spawnOffset = value;
		}
	}

	internal int ProjectileHash
	{
		get
		{
			return PoolUtils.GameObjHashCode(this.randomModelSelection ? this.localModels[this.randModelIndex].GetProjectilePrefab() : this.projectilePrefab);
		}
	}

	protected virtual void Awake()
	{
		if (this.awakeHasBeenCalled)
		{
			return;
		}
		this.awakeHasBeenCalled = true;
		this.targetRig = base.GetComponentInParent<VRRig>(true);
		this.isOfflineRig = this.targetRig != null && this.targetRig.isOfflineVRRig;
		this.renderers = base.GetComponentsInChildren<Renderer>();
		this.randModelIndex = -1;
		foreach (RandomProjectileThrowable randomProjectileThrowable in this.localModels)
		{
			if (randomProjectileThrowable != null)
			{
				RandomProjectileThrowable randomProjectileThrowable2 = randomProjectileThrowable;
				randomProjectileThrowable2.OnDestroyRandomProjectile = (UnityAction<bool>)Delegate.Combine(randomProjectileThrowable2.OnDestroyRandomProjectile, new UnityAction<bool>(this.HandleOnDestroyRandomProjectile));
			}
		}
	}

	public bool IsMine()
	{
		return this.targetRig != null && this.targetRig.isOfflineVRRig;
	}

	public virtual void OnEnable()
	{
		if (this.targetRig == null)
		{
			Debug.LogError("SnowballThrowable: targetRig is null! Deactivating.");
			base.gameObject.SetActive(false);
			return;
		}
		if (!this.targetRig.isOfflineVRRig)
		{
			if (this.targetRig.netView != null && this.targetRig.netView.IsMine)
			{
				base.gameObject.SetActive(false);
				return;
			}
			Color32 throwableProjectileColor = this.targetRig.GetThrowableProjectileColor(this.isLeftHanded);
			this.ApplyColor(throwableProjectileColor);
			if (this.randomModelSelection)
			{
				foreach (RandomProjectileThrowable randomProjectileThrowable in this.localModels)
				{
					randomProjectileThrowable.gameObject.SetActive(false);
				}
				this.randModelIndex = this.targetRig.GetRandomThrowableModelIndex();
				this.EnableRandomModel(this.randModelIndex, true);
			}
		}
		this.AnchorToHand();
		this.OnEnableHasBeenCalled = true;
	}

	public virtual void OnDisable()
	{
	}

	protected new virtual void OnDestroy()
	{
	}

	public void SetSnowballActiveLocal(bool enabled)
	{
		if (!this.awakeHasBeenCalled)
		{
			this.Awake();
		}
		if (!this.OnEnableHasBeenCalled)
		{
			this.OnEnable();
		}
		if (this.isLeftHanded)
		{
			this.targetRig.LeftThrowableProjectileIndex = (enabled ? this.throwableMakerIndex : (-1));
		}
		else
		{
			this.targetRig.RightThrowableProjectileIndex = (enabled ? this.throwableMakerIndex : (-1));
		}
		bool flag = !base.gameObject.activeSelf && enabled;
		base.gameObject.SetActive(enabled);
		if (flag && this.pickupSoundBankPlayer != null)
		{
			this.pickupSoundBankPlayer.Play();
		}
		if (this.randomModelSelection)
		{
			if (enabled)
			{
				this.EnableRandomModel(this.GetRandomModelIndex(), true);
			}
			else
			{
				this.EnableRandomModel(this.randModelIndex, false);
			}
			this.targetRig.SetRandomThrowableModelIndex(this.randModelIndex);
		}
		EquipmentInteractor.instance.UpdateHandEquipment(enabled ? this : null, this.isLeftHanded);
		if (this.randomizeColor)
		{
			Color color = (enabled ? GTColor.RandomHSV(this.randomColorHSVRanges) : Color.white);
			this.targetRig.SetThrowableProjectileColor(this.isLeftHanded, color);
			this.ApplyColor(color);
		}
	}

	private int GetRandomModelIndex()
	{
		if (this.localModels.Count == 0)
		{
			return -1;
		}
		this.randModelIndex = Random.Range(0, this.localModels.Count);
		if ((float)Random.Range(1, 100) <= this.localModels[this.randModelIndex].spawnChance * 100f)
		{
			return this.randModelIndex;
		}
		return this.GetRandomModelIndex();
	}

	private void EnableRandomModel(int index, bool enable)
	{
		if (this.randModelIndex >= 0 && this.randModelIndex < this.localModels.Count)
		{
			this.localModels[this.randModelIndex].gameObject.SetActive(enable);
			if (enable && this.localModels[this.randModelIndex].autoDestroyAfterSeconds > 0f)
			{
				this.destroyTimer = 0f;
			}
			return;
		}
	}

	protected virtual void LateUpdateLocal()
	{
		if (this.randomModelSelection && this.randModelIndex > -1 && this.localModels[this.randModelIndex].ForceDestroy)
		{
			this.localModels[this.randModelIndex].ForceDestroy = false;
			if (this.localModels[this.randModelIndex].gameObject.activeSelf)
			{
				this.PerformSnowballThrowAuthority();
			}
		}
		if (this.randomModelSelection && this.randModelIndex > -1 && this.localModels[this.randModelIndex].autoDestroyAfterSeconds > 0f)
		{
			this.destroyTimer += Time.deltaTime;
			if (this.destroyTimer > this.localModels[this.randModelIndex].autoDestroyAfterSeconds)
			{
				if (this.localModels[this.randModelIndex].gameObject.activeSelf)
				{
					this.PerformSnowballThrowAuthority();
				}
				this.destroyTimer = -1f;
			}
		}
	}

	protected void LateUpdateReplicated()
	{
	}

	protected void LateUpdateShared()
	{
	}

	private Transform Anchor()
	{
		return base.transform.parent;
	}

	private void AnchorToHand()
	{
		BodyDockPositions myBodyDockPositions = this.targetRig.myBodyDockPositions;
		Transform transform = this.Anchor();
		if (this.isLeftHanded)
		{
			transform.parent = myBodyDockPositions.leftHandTransform;
		}
		else
		{
			transform.parent = myBodyDockPositions.rightHandTransform;
		}
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		base.transform.localPosition = this.spawnOffset.pos;
		base.transform.localRotation = this.spawnOffset.rot;
	}

	protected void LateUpdate()
	{
		if (this.IsMine())
		{
			this.LateUpdateLocal();
		}
		else
		{
			this.LateUpdateReplicated();
		}
		this.LateUpdateShared();
	}

	public override bool OnRelease(DropZone zoneReleased, GameObject releasingHand)
	{
		if (!base.OnRelease(zoneReleased, releasingHand))
		{
			return false;
		}
		this.OnSnowballRelease();
		return true;
	}

	protected virtual void OnSnowballRelease()
	{
		this.PerformSnowballThrowAuthority();
	}

	protected virtual void PerformSnowballThrowAuthority()
	{
		if (!(this.targetRig != null) || this.targetRig.creator == null || !this.targetRig.creator.IsLocal)
		{
			return;
		}
		Vector3 vector = Vector3.zero;
		Rigidbody component = GorillaTagger.Instance.GetComponent<Rigidbody>();
		if (component != null)
		{
			vector = component.linearVelocity;
		}
		Vector3 vector2 = this.velocityEstimator.linearVelocity - vector;
		float magnitude = vector2.magnitude;
		if (magnitude > 0.001f)
		{
			float num = Mathf.Clamp(magnitude * this.linSpeedMultiplier, 0f, this.maxLinSpeed);
			vector2 *= num / magnitude;
		}
		Vector3 vector3 = vector2 + vector;
		Color32 throwableProjectileColor = this.targetRig.GetThrowableProjectileColor(this.isLeftHanded);
		Transform transform = base.transform;
		Vector3 position = transform.position;
		float x = transform.lossyScale.x;
		SlingshotProjectile slingshotProjectile = this.LaunchSnowballLocal(position, vector3, x, this.randomizeColor, throwableProjectileColor);
		this.SetSnowballActiveLocal(false);
		if (this.randModelIndex > -1 && this.randModelIndex < this.localModels.Count)
		{
			if (this.localModels[this.randModelIndex].ForceDestroy || this.localModels[this.randModelIndex].destroyAfterRelease)
			{
				slingshotProjectile.DestroyAfterRelease();
			}
			else if (this.localModels[this.randModelIndex].moveOverPassedLifeTime)
			{
				float num2 = Time.time - this.localModels[this.randModelIndex].TimeEnabled;
				float remainingLifeTime = slingshotProjectile.GetRemainingLifeTime();
				if (remainingLifeTime > num2)
				{
					float num3 = remainingLifeTime - num2;
					slingshotProjectile.UpdateRemainingLifeTime(num3);
				}
				else
				{
					slingshotProjectile.UpdateRemainingLifeTime(0f);
				}
			}
		}
		if (NetworkSystem.Instance.InRoom)
		{
			RoomSystem.SendLaunchProjectile(position, vector3, this.isLeftHanded ? RoomSystem.ProjectileSource.LeftHand : RoomSystem.ProjectileSource.RightHand, slingshotProjectile.myProjectileCount, this.randomizeColor, throwableProjectileColor.r, throwableProjectileColor.g, throwableProjectileColor.b, throwableProjectileColor.a);
		}
	}

	protected virtual SlingshotProjectile LaunchSnowballLocal(Vector3 location, Vector3 velocity, float scale, bool randomColour, Color colour)
	{
		SlingshotProjectile component = ObjectPools.instance.Instantiate(this.randomModelSelection ? this.localModels[this.randModelIndex].GetProjectilePrefab() : this.projectilePrefab, true).GetComponent<SlingshotProjectile>();
		int num = ProjectileTracker.AddAndIncrementLocalProjectile(component, velocity, location, scale);
		component.Launch(location, velocity, NetworkSystem.Instance.LocalPlayer, false, false, num, scale, randomColour, colour);
		if (string.IsNullOrEmpty(this.throwEventName))
		{
			PlayerGameEvents.LaunchedProjectile(this.projectilePrefab.name);
		}
		else
		{
			PlayerGameEvents.LaunchedProjectile(this.throwEventName);
		}
		component.OnImpact += this.OnProjectileImpact;
		return component;
	}

	protected virtual SlingshotProjectile SpawnProjectile()
	{
		return ObjectPools.instance.Instantiate(this.randomModelSelection ? this.localModels[this.randModelIndex].GetProjectilePrefab() : this.projectilePrefab, true).GetComponent<SlingshotProjectile>();
	}

	protected virtual void OnProjectileImpact(SlingshotProjectile projectile, Vector3 impactPos, NetPlayer hitPlayer)
	{
		if (hitPlayer != null)
		{
			ScienceExperimentManager instance = ScienceExperimentManager.instance;
			if (instance != null && this.projectilePrefab != null && this.projectilePrefab == instance.waterBalloonPrefab)
			{
				instance.OnWaterBalloonHitPlayer(hitPlayer);
			}
		}
	}

	private void ApplyColor(Color newColor)
	{
		foreach (Renderer renderer in this.renderers)
		{
			if (renderer)
			{
				foreach (Material material in renderer.materials)
				{
					if (!(material == null))
					{
						if (material.HasProperty(ShaderProps._BaseColor))
						{
							material.SetColor(ShaderProps._BaseColor, newColor);
						}
						if (material.HasProperty(ShaderProps._Color))
						{
							material.SetColor(ShaderProps._Color, newColor);
						}
					}
				}
			}
		}
	}

	public override void OnHover(InteractionPoint pointHovered, GameObject hoveringHand)
	{
	}

	public override void OnGrab(InteractionPoint pointGrabbed, GameObject grabbingHand)
	{
	}

	public override void DropItemCleanup()
	{
		if (base.gameObject.activeSelf)
		{
			this.OnSnowballRelease();
		}
	}

	private void HandleOnDestroyRandomProjectile(bool enable)
	{
		this.SetSnowballActiveLocal(enable);
	}

	[GorillaSoundLookup]
	public List<int> matDataIndexes = new List<int> { 32 };

	[Tooltip("prefab to spawn from global object pools when thrown")]
	public GameObject projectilePrefab;

	public SoundBankPlayer pickupSoundBankPlayer;

	public bool isLeftHanded;

	[Tooltip("This needs to match the index of the projectilePrefab on the Local Gorilla Player's BodyDockPositions LeftHandThrowables or RightHandThrowables list\nCheck the array in play mode to find the index")]
	public int throwableMakerIndex;

	[Tooltip("Multiplier is applied to hand speed to get launch speed of the projectile")]
	public float linSpeedMultiplier = 1f;

	[Tooltip("Maximum launch speed of the projectile")]
	public float maxLinSpeed = 12f;

	[Space]
	[FormerlySerializedAs("shouldColorize")]
	public bool randomizeColor;

	public GTColor.HSVRanges randomColorHSVRanges = new GTColor.HSVRanges(0f, 1f, 0.7f, 1f, 1f, 1f);

	[Tooltip("Check this part only if we want to randomize the prefab meshes and projectile")]
	public bool randomModelSelection;

	public List<RandomProjectileThrowable> localModels;

	[Tooltip("projectile identifier sent out by the PlayerGameEvents.LaunchedProjectile event. Uses prefab name if empty")]
	public string throwEventName;

	public GorillaVelocityEstimator velocityEstimator;

	protected VRRig targetRig;

	protected bool isOfflineRig;

	private bool awakeHasBeenCalled;

	private bool OnEnableHasBeenCalled;

	private Renderer[] renderers;

	protected int randModelIndex;

	private float destroyTimer = -1f;

	private XformOffset spawnOffset;
}
