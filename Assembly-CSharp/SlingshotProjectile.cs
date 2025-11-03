using System;
using GorillaExtensions;
using GorillaGameModes;
using GorillaLocomotion;
using GorillaLocomotion.Swimming;
using GorillaTag.Reactions;
using UnityEngine;
using UnityEngine.Events;

public class SlingshotProjectile : MonoBehaviour
{
	public Vector3 launchPosition { get; private set; }

	public event SlingshotProjectile.ProjectileImpactEvent OnImpact;

	public void Launch(Vector3 position, Vector3 velocity, NetPlayer player, bool blueTeam, bool orangeTeam, int projectileCount, float scale, bool shouldOverrideColor = false, Color overrideColor = default(Color))
	{
		if (this.launchSoundBankPlayer != null)
		{
			this.launchSoundBankPlayer.Play();
		}
		this.particleLaunched = true;
		this.timeCreated = Time.time;
		this.launchPosition = position;
		Transform transform = base.transform;
		transform.position = position;
		transform.localScale = Vector3.one * scale;
		base.GetComponent<Collider>().contactOffset = 0.01f * scale;
		RigidbodyWaterInteraction component = base.GetComponent<RigidbodyWaterInteraction>();
		if (component != null)
		{
			component.objectRadiusForWaterCollision = 0.02f * scale;
		}
		this.projectileRigidbody.isKinematic = false;
		this.projectileRigidbody.useGravity = false;
		this.forceComponent.enabled = true;
		this.forceComponent.force = Physics.gravity * this.projectileRigidbody.mass * this.gravityMultiplier * ((scale < 1f) ? scale : 1f);
		this.projectileRigidbody.linearVelocity = velocity;
		this.projectileOwner = player;
		this.myProjectileCount = projectileCount;
		this.projectileRigidbody.position = position;
		this.ApplyTeamModelAndColor(blueTeam, orangeTeam, shouldOverrideColor, overrideColor);
		this.remainingLifeTime = this.lifeTime;
		if (this.forceComponent)
		{
			this.forceComponent.enabled = true;
			this.forceComponent.force = Physics.gravity * this.projectileRigidbody.mass * this.gravityMultiplier * ((scale < 1f) ? scale : 1f);
			if (this.useForwardForce)
			{
				this.forceComponent.force += this.projectileRigidbody.linearVelocity.normalized * this.forwardForceMultiplier;
			}
		}
		this.isSettled = false;
		UnityEvent<NetPlayer> onLaunch = this.OnLaunch;
		if (onLaunch == null)
		{
			return;
		}
		onLaunch.Invoke(this.projectileOwner);
	}

	protected void Awake()
	{
		if (this.playerImpactEffectPrefab == null)
		{
			this.playerImpactEffectPrefab = this.surfaceImpactEffectPrefab;
		}
		this.projectileRigidbody = base.GetComponent<Rigidbody>();
		this.forceComponent = base.GetComponent<ConstantForce>();
		this.initialScale = base.transform.localScale.x;
		this.matPropBlock = new MaterialPropertyBlock();
		this.spawnWorldEffects = base.GetComponent<SpawnWorldEffects>();
		this.remainingLifeTime = this.lifeTime;
	}

	public void Deactivate()
	{
		base.transform.localScale = Vector3.one * this.initialScale;
		this.projectileRigidbody.useGravity = true;
		if (this.forceComponent)
		{
			this.forceComponent.force = Vector3.zero;
		}
		this.OnImpact = null;
		this.aoeKnockbackConfig = null;
		this.impactSoundVolumeOverride = null;
		this.impactSoundPitchOverride = null;
		this.impactEffectScaleMultiplier = 1f;
		this.projectileRigidbody.isKinematic = false;
		ObjectPools.instance.Destroy(base.gameObject);
	}

	private void SpawnImpactEffect(GameObject prefab, Vector3 position, Vector3 normal)
	{
		if (prefab == null)
		{
			return;
		}
		Vector3 vector = position + normal * this.impactEffectOffset;
		GameObject gameObject = ObjectPools.instance.Instantiate(prefab, vector, true);
		Vector3 localScale = base.transform.localScale;
		gameObject.transform.localScale = localScale * this.impactEffectScaleMultiplier;
		gameObject.transform.up = normal;
		GorillaColorizableBase component = gameObject.GetComponent<GorillaColorizableBase>();
		if (component != null)
		{
			component.SetColor(this.teamColor);
		}
		SurfaceImpactFX component2 = gameObject.GetComponent<SurfaceImpactFX>();
		if (component2 != null)
		{
			component2.SetScale(localScale.x * this.impactEffectScaleMultiplier);
		}
		SoundBankPlayer component3 = gameObject.GetComponent<SoundBankPlayer>();
		if (component3 != null && !component3.playOnEnable)
		{
			component3.Play(this.impactSoundVolumeOverride, this.impactSoundPitchOverride);
		}
		if (this.spawnWorldEffects != null)
		{
			this.spawnWorldEffects.RequestSpawn(position, normal);
		}
		UnityEvent<Vector3> onImapctEvent = this.OnImapctEvent;
		if (onImapctEvent == null)
		{
			return;
		}
		onImapctEvent.Invoke(position);
	}

	public void CheckForAOEKnockback(Vector3 impactPosition, float impactSpeed)
	{
		if (this.aoeKnockbackConfig != null && this.aoeKnockbackConfig.Value.applyAOEKnockback)
		{
			Vector3 vector = GTPlayer.Instance.HeadCenterPosition - impactPosition;
			if (vector.sqrMagnitude < this.aoeKnockbackConfig.Value.aeoOuterRadius * this.aoeKnockbackConfig.Value.aeoOuterRadius)
			{
				float magnitude = vector.magnitude;
				Vector3 vector2 = ((magnitude > 0.001f) ? (vector / magnitude) : Vector3.up);
				float num = Mathf.InverseLerp(this.aoeKnockbackConfig.Value.aeoOuterRadius, this.aoeKnockbackConfig.Value.aeoInnerRadius, magnitude);
				float num2 = Mathf.InverseLerp(0f, this.aoeKnockbackConfig.Value.impactVelocityThreshold, impactSpeed);
				GTPlayer.Instance.ApplyKnockback(vector2, this.aoeKnockbackConfig.Value.knockbackVelocity * num * num2, false);
				this.impactEffectScaleMultiplier = Mathf.Lerp(1f, this.impactEffectScaleMultiplier, num2);
				if (this.impactSoundVolumeOverride != null)
				{
					this.impactSoundVolumeOverride = new float?(Mathf.Lerp(this.impactSoundVolumeOverride.Value * 0.5f, this.impactSoundVolumeOverride.Value, num2));
				}
				float num3 = Mathf.Lerp(this.aoeKnockbackConfig.Value.aeoInnerRadius, this.aoeKnockbackConfig.Value.aeoOuterRadius, 0.25f);
				if (this.aoeKnockbackConfig.Value.playerProximityEffect != PlayerEffect.NONE && vector.sqrMagnitude < num3 * num3)
				{
					RoomSystem.SendPlayerEffect(PlayerEffect.SNOWBALL_IMPACT, NetworkSystem.Instance.LocalPlayer);
				}
			}
		}
	}

	public void ApplyTeamModelAndColor(bool blueTeam, bool orangeTeam, bool shouldOverrideColor = false, Color overrideColor = default(Color))
	{
		if (shouldOverrideColor)
		{
			this.teamColor = overrideColor;
		}
		else
		{
			this.teamColor = (blueTeam ? this.blueColor : (orangeTeam ? this.orangeColor : this.defaultColor));
		}
		this.blueBall.enabled = blueTeam;
		this.orangeBall.enabled = orangeTeam;
		this.defaultBall.enabled = !blueTeam && !orangeTeam;
		this.teamRenderer = (blueTeam ? this.blueBall : (orangeTeam ? this.orangeBall : this.defaultBall));
		this.ApplyColor(this.teamRenderer, (this.colorizeBalls || shouldOverrideColor) ? this.teamColor : Color.white);
	}

	protected void OnEnable()
	{
		this.timeCreated = 0f;
		this.particleLaunched = false;
		SlingshotProjectileManager.RegisterSP(this);
	}

	protected void OnDisable()
	{
		this.particleLaunched = false;
		SlingshotProjectileManager.UnregisterSP(this);
	}

	public void InvokeUpdate()
	{
		if (this.particleLaunched || this.dontDestroyOnHit)
		{
			if (Time.time > this.timeCreated + this.GetRemainingLifeTime())
			{
				this.DestroyAfterRelease();
			}
			if (this.faceDirectionOfTravel)
			{
				Transform transform = base.transform;
				Vector3 position = transform.position;
				Vector3 vector = position - this.previousPosition;
				transform.rotation = ((vector.sqrMagnitude > 0f) ? Quaternion.LookRotation(vector) : transform.rotation);
				this.previousPosition = position;
			}
		}
		if (this.dontDestroyOnHit)
		{
			this.SettleProjectile();
		}
	}

	public void DestroyAfterRelease()
	{
		this.SpawnImpactEffect(this.surfaceImpactEffectPrefab, base.transform.position, Vector3.up);
		this.Deactivate();
	}

	public float GetRemainingLifeTime()
	{
		return this.remainingLifeTime;
	}

	public void UpdateRemainingLifeTime(float newLifeTime)
	{
		this.remainingLifeTime = newLifeTime;
	}

	public float GetDistanceTraveled()
	{
		return (base.transform.position - this.launchPosition).magnitude;
	}

	private void SettleProjectile()
	{
		if (!this.isSettled)
		{
			int value = this.floorLayerMask.value;
			RaycastHit raycastHit;
			if (Physics.Raycast(base.transform.position, Vector3.down, out raycastHit, 0.1f, value, QueryTriggerInteraction.Ignore) && Vector3.Angle(raycastHit.normal, Vector3.up) < 40f)
			{
				if (this.forceComponent)
				{
					this.forceComponent.force = Vector3.zero;
				}
				this.projectileRigidbody.angularVelocity = Vector3.zero;
				this.projectileRigidbody.linearVelocity = Vector3.zero;
				this.projectileRigidbody.isKinematic = true;
				base.transform.position = raycastHit.point + Vector3.up * this.placementOffset;
				this.isSettled = true;
				return;
			}
		}
		else if (this.keepRotationUpright)
		{
			Quaternion quaternion = Quaternion.LookRotation(Vector3.ProjectOnPlane(base.transform.up, Vector3.up).normalized, Vector3.up);
			base.transform.rotation = quaternion;
		}
	}

	protected void OnCollisionEnter(Collision collision)
	{
		if (!this.particleLaunched)
		{
			return;
		}
		if (this.dontDestroyOnHit)
		{
			return;
		}
		SlingshotProjectileHitNotifier slingshotProjectileHitNotifier;
		if (collision.collider.gameObject.TryGetComponent<SlingshotProjectileHitNotifier>(out slingshotProjectileHitNotifier))
		{
			slingshotProjectileHitNotifier.InvokeHit(this, collision);
		}
		ContactPoint contact = collision.GetContact(0);
		this.CheckForAOEKnockback(contact.point, collision.relativeVelocity.magnitude);
		this.SpawnImpactEffect(this.surfaceImpactEffectPrefab, contact.point, contact.normal);
		SlingshotProjectile.ProjectileImpactEvent onImpact = this.OnImpact;
		if (onImpact != null)
		{
			onImpact(this, contact.point, null);
		}
		this.Deactivate();
	}

	protected void OnCollisionStay(Collision collision)
	{
		if (!this.particleLaunched)
		{
			return;
		}
		if (this.dontDestroyOnHit)
		{
			return;
		}
		SlingshotProjectileHitNotifier slingshotProjectileHitNotifier;
		if (collision.gameObject.TryGetComponent<SlingshotProjectileHitNotifier>(out slingshotProjectileHitNotifier))
		{
			slingshotProjectileHitNotifier.InvokeCollisionStay(this, collision);
		}
		ContactPoint contact = collision.GetContact(0);
		this.CheckForAOEKnockback(contact.point, collision.relativeVelocity.magnitude);
		this.SpawnImpactEffect(this.surfaceImpactEffectPrefab, contact.point, contact.normal);
		SlingshotProjectile.ProjectileImpactEvent onImpact = this.OnImpact;
		if (onImpact != null)
		{
			onImpact(this, contact.point, null);
		}
		this.Deactivate();
	}

	protected void OnTriggerExit(Collider other)
	{
		if (!this.particleLaunched)
		{
			return;
		}
		SlingshotProjectileHitNotifier slingshotProjectileHitNotifier;
		if (other.gameObject.TryGetComponent<SlingshotProjectileHitNotifier>(out slingshotProjectileHitNotifier))
		{
			slingshotProjectileHitNotifier.InvokeTriggerExit(this, other);
		}
	}

	protected void OnTriggerEnter(Collider other)
	{
		if (!this.particleLaunched)
		{
			return;
		}
		SlingshotProjectileHitNotifier slingshotProjectileHitNotifier;
		if (other.gameObject.TryGetComponent<SlingshotProjectileHitNotifier>(out slingshotProjectileHitNotifier))
		{
			slingshotProjectileHitNotifier.InvokeTriggerEnter(this, other);
		}
		if (this.projectileOwner == NetworkSystem.Instance.LocalPlayer)
		{
			if (!NetworkSystem.Instance.InRoom || GorillaGameManager.instance == null)
			{
				return;
			}
			GorillaPaintbrawlManager component = GorillaGameManager.instance.gameObject.GetComponent<GorillaPaintbrawlManager>();
			if (!other.gameObject.IsOnLayer(UnityLayer.GorillaTagCollider) && !other.gameObject.IsOnLayer(UnityLayer.GorillaSlingshotCollider))
			{
				return;
			}
			VRRig componentInParent = other.GetComponentInParent<VRRig>();
			NetPlayer netPlayer = ((componentInParent != null) ? componentInParent.creator : null);
			if (netPlayer == null)
			{
				return;
			}
			SlingshotProjectile.ProjectileImpactEvent onImpact = this.OnImpact;
			if (onImpact != null)
			{
				onImpact(this, base.transform.position, netPlayer);
			}
			if (NetworkSystem.Instance.LocalPlayer == netPlayer)
			{
				return;
			}
			if (component && !component.LocalCanHit(NetworkSystem.Instance.LocalPlayer, netPlayer))
			{
				return;
			}
			if (component && GameMode.ActiveNetworkHandler)
			{
				GameMode.ActiveNetworkHandler.SendRPC("RPC_ReportSlingshotHit", false, new object[]
				{
					(netPlayer as PunNetPlayer).PlayerRef,
					base.transform.position,
					this.myProjectileCount
				});
				PlayerGameEvents.GameModeObjectiveTriggered();
			}
			if (this.m_sendNetworkedImpact)
			{
				RoomSystem.SendImpactEffect(base.transform.position, this.teamColor.r, this.teamColor.g, this.teamColor.b, this.teamColor.a, this.myProjectileCount);
			}
			this.Deactivate();
		}
		Rigidbody attachedRigidbody = other.attachedRigidbody;
		VRRig vrrig;
		if (attachedRigidbody.IsNotNull() && attachedRigidbody.gameObject.TryGetComponent<VRRig>(out vrrig))
		{
			UnityEvent<VRRig> onHitPlayer = this.OnHitPlayer;
			if (onHitPlayer == null)
			{
				return;
			}
			onHitPlayer.Invoke(vrrig);
		}
	}

	private void ApplyColor(Renderer rend, Color color)
	{
		if (!rend)
		{
			return;
		}
		this.matPropBlock.SetColor(ShaderProps._BaseColor, color);
		this.matPropBlock.SetColor(ShaderProps._Color, color);
		rend.SetPropertyBlock(this.matPropBlock);
	}

	public NetPlayer projectileOwner;

	[Tooltip("Rotates to point along the Y axis after spawn.")]
	public GameObject surfaceImpactEffectPrefab;

	[Tooltip("if left empty, the default player impact that is set in Room System Setting will be played")]
	public GameObject playerImpactEffectPrefab;

	[Tooltip("Distance from the surface that the particle should spawn.")]
	[SerializeField]
	private float impactEffectOffset;

	[SerializeField]
	private SoundBankPlayer launchSoundBankPlayer;

	[SerializeField]
	private bool dontDestroyOnHit;

	[SerializeField]
	private LayerMask floorLayerMask;

	[SerializeField]
	private float placementOffset = 0.01f;

	[SerializeField]
	private bool keepRotationUpright = true;

	public float lifeTime = 20f;

	public float gravityMultiplier = 1f;

	public bool useForwardForce;

	public float forwardForceMultiplier = 0.1f;

	public Color defaultColor = Color.white;

	public Color orangeColor = new Color(1f, 0.5f, 0f, 1f);

	public Color blueColor = new Color(0f, 0.72f, 1f, 1f);

	[Tooltip("Renderers with team specific meshes, materials, effects, etc.")]
	public Renderer defaultBall;

	[Tooltip("Renderers with team specific meshes, materials, effects, etc.")]
	public Renderer orangeBall;

	[Tooltip("Renderers with team specific meshes, materials, effects, etc.")]
	public Renderer blueBall;

	public bool colorizeBalls;

	public bool faceDirectionOfTravel = true;

	private bool particleLaunched;

	private float timeCreated;

	private Rigidbody projectileRigidbody;

	private Color teamColor = Color.white;

	private Renderer teamRenderer;

	public int myProjectileCount;

	private float initialScale;

	private Vector3 previousPosition;

	[HideInInspector]
	public SlingshotProjectile.AOEKnockbackConfig? aoeKnockbackConfig;

	[HideInInspector]
	public float? impactSoundVolumeOverride;

	[HideInInspector]
	public float? impactSoundPitchOverride;

	[HideInInspector]
	public float impactEffectScaleMultiplier = 1f;

	private ConstantForce forceComponent;

	public bool m_sendNetworkedImpact = true;

	public UnityEvent<NetPlayer> OnLaunch;

	public UnityEvent<Vector3> OnImapctEvent;

	private MaterialPropertyBlock matPropBlock;

	private SpawnWorldEffects spawnWorldEffects;

	public UnityEvent<VRRig> OnHitPlayer;

	private float remainingLifeTime;

	private bool isSettled;

	private float distanceTraveled;

	[Serializable]
	public struct AOEKnockbackConfig
	{
		public bool applyAOEKnockback;

		[Tooltip("Full knockback velocity is imparted within the inner radius")]
		public float aeoInnerRadius;

		[Tooltip("Partial knockback velocity is imparted between the inner and outer radius")]
		public float aeoOuterRadius;

		public float knockbackVelocity;

		[Tooltip("The required impact velocity to achieve full knockback velocity")]
		public float impactVelocityThreshold;

		[SerializeField]
		public PlayerEffect playerProximityEffect;
	}

	public delegate void ProjectileImpactEvent(SlingshotProjectile projectile, Vector3 impactPos, NetPlayer hitPlayer);
}
