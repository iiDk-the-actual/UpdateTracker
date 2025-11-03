using System;
using GorillaExtensions;
using GorillaLocomotion;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(GameGrabbable))]
[RequireComponent(typeof(GameSnappable))]
[RequireComponent(typeof(GameButtonActivatable))]
public class SIGadgetWristJet : SIGadget, I_SIDisruptable
{
	private bool CanRecharge
	{
		get
		{
			return (!this.rechargeRequiresFloorTouch || this._floorTouched) && this.state != SIGadgetWristJet.State.Active;
		}
	}

	private void Awake()
	{
		this._maxSqrHorizontalSpeed = this.maxHorizontalSpeed * this.maxHorizontalSpeed;
		this._hasThrustLoopAudioSource = this.m_thrustLoopAudioSource != null;
		this.m_warnFuelLowThreshold = ((this.m_warnFuelLowSound != null) ? this.m_warnFuelLowThreshold : (-1f));
		this._hasInactiveStateVisual = this.inactiveStateVisual != null;
		this._hasActiveStateVisual = this.activeStateVisual != null;
		this._gaugeMatPropBlock = new MaterialPropertyBlock();
		if (this.m_gaugeMatSlots == null)
		{
			this.m_gaugeMatSlots = Array.Empty<GTRendererMatSlot>();
		}
		int num = 0;
		for (int i = 0; i < this.m_gaugeMatSlots.Length; i++)
		{
			if (this.m_gaugeMatSlots[i].TryInitialize())
			{
				this.m_gaugeMatSlots[num] = this.m_gaugeMatSlots[i];
				num++;
			}
		}
		if (num != this.m_gaugeMatSlots.Length)
		{
			Array.Resize<GTRendererMatSlot>(ref this.m_gaugeMatSlots, num);
		}
		this.throttleFlapInitialRots = ((this.m_throttleFlapXforms != null) ? new Quaternion[this.m_throttleFlapXforms.Length] : Array.Empty<Quaternion>());
		for (int j = 0; j < this.throttleFlapInitialRots.Length; j++)
		{
			if (this.m_throttleFlapXforms[j] == null)
			{
				this.throttleFlapInitialRots = Array.Empty<Quaternion>();
				Debug.LogError("[SIGadgetWristJet]  ERROR!!!  Awake: Throttle indicator flaps will not animate because entry is null in " + string.Format("array at `{0}[{1}]`. Path={2}", "m_throttleFlapXforms", j, base.transform.GetPathQ()), this);
				return;
			}
			this.throttleFlapInitialRots[j] = this.m_throttleFlapXforms[j].localRotation;
		}
	}

	private void Start()
	{
		this.gtPlayer = GTPlayer.Instance;
		this.gameEntity.OnStateChanged += this.OnEntityStateChanged;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (this.m_warnFuelLowThreshold > 0f)
		{
			this.m_warnFuelLowSound.LoadAudioData();
		}
	}

	protected override void OnDisable()
	{
		if (this.m_warnFuelLowThreshold > 0f && this.m_warnFuelLowSound.loadState != AudioDataLoadState.Unloaded)
		{
			this.m_warnFuelLowSound.UnloadAudioData();
		}
	}

	protected override void Update()
	{
		base.Update();
		if (this._hasThrustLoopAudioSource)
		{
			float num = ((this.state == SIGadgetWristJet.State.Active) ? this.m_thrustLoopSoundVolume : 0f);
			float num2 = ((this.state == SIGadgetWristJet.State.Active) ? this.m_thrustLoopAudioFadeInTime : this.m_thrustLoopAudioFadeOutTime);
			this.m_thrustLoopAudioSource.volume = Mathf.MoveTowards(this.m_thrustLoopAudioSource.volume, num, 1f / num2 * Time.unscaledDeltaTime);
		}
	}

	private void FixedUpdate()
	{
		if (!this.IsEquippedLocal() && !this.activatedLocally)
		{
			return;
		}
		if (this.state == SIGadgetWristJet.State.Active && this.currentFuel > 0f && this.buttonActivatable.CheckInput(true, true, 0.25f, true))
		{
			this.gtPlayer.AddForce(-Physics.gravity * (this.gtPlayer.scale * this.gravityNegationPercent), ForceMode.Acceleration);
			this._ApplyClampedThrust();
		}
	}

	protected override void OnUpdateAuthority(float dt)
	{
		base.OnUpdateAuthority(dt);
		bool flag = this.buttonActivatable.CheckInput(true, true, 0.25f, true);
		if (!this._floorTouched)
		{
			this._floorTouched = this.gtPlayer.IsGroundedButt || this.gtPlayer.IsGroundedHand;
		}
		if (this._throttleControl)
		{
			Vector2 joystickInput = base.GetJoystickInput();
			if (Mathf.Abs(joystickInput.y) > 0.75f && Mathf.Abs(joystickInput.x) < 0.5f)
			{
				this._throttle = Mathf.Clamp01(this._throttle + joystickInput.y * this.throttleChangeSpeed * Time.deltaTime);
				this._currentBurnRate = Mathf.Lerp(this.minimumBurnRate, 1f, this._throttle);
				this.UpdateThrottleIndicator();
			}
		}
		switch (this.state)
		{
		case SIGadgetWristJet.State.Unactive:
			if (this.CanRecharge)
			{
				this.currentFuel = Mathf.Clamp(this.currentFuel + dt * this.fuelGainRate, 0f, this.fuelSize);
			}
			if (flag)
			{
				this.SetStateAuthority(SIGadgetWristJet.State.Active);
			}
			break;
		case SIGadgetWristJet.State.Active:
			this.currentFuel = Mathf.Clamp(this.currentFuel - dt * this.fuelSpendRate * this._currentBurnRate, 0f, this.fuelSize);
			this._floorTouched = false;
			this.gtPlayer.ThrusterActiveAtFrame = Time.frameCount;
			if (flag && this.m_warnFuelLowThreshold > 0f)
			{
				float num = this.currentFuel / this.fuelSize;
				if (this._warnFuelLowSoundWasPlayed && num > this.m_warnFuelLowThreshold)
				{
					this._warnFuelLowSoundWasPlayed = false;
				}
				else if (!this._warnFuelLowSoundWasPlayed && num <= this.m_warnFuelLowThreshold)
				{
					this._warnFuelLowSoundWasPlayed = true;
					this.gameEntity.audioSource.GTPlayOneShot(this.m_warnFuelLowSound, this.m_warnFuelLowSoundVolume);
				}
			}
			if (!flag || this.currentFuel <= 0f)
			{
				this.SetStateAuthority(SIGadgetWristJet.State.OutOfFuel);
			}
			break;
		case SIGadgetWristJet.State.OutOfFuel:
			if (!flag)
			{
				this.emptiedCooldownResetProgress += dt;
			}
			else if (this.currentFuel > 0f)
			{
				this.SetStateAuthority(SIGadgetWristJet.State.Active);
			}
			if (this.emptiedCooldownResetProgress > this.emptiedCooldown)
			{
				this.emptiedCooldownResetProgress = 0f;
				this.SetStateAuthority(SIGadgetWristJet.State.Unactive);
			}
			break;
		}
		float num2 = this.currentFuel / this.fuelSize;
		for (int i = 0; i < this.m_gaugeMatSlots.Length; i++)
		{
			this._gaugeMatPropBlock.SetFloat(ShaderProps._EmissionDissolveProgress, num2);
			this.m_gaugeMatSlots[i].renderer.SetPropertyBlock(this._gaugeMatPropBlock, this.m_gaugeMatSlots[i].slot);
		}
	}

	private void UpdateThrottleIndicator()
	{
		for (int i = 0; i < this.throttleFlapInitialRots.Length; i++)
		{
			Quaternion quaternion = this.throttleFlapInitialRots[i] * this.m_throttleFlapMaxRotOffset;
			this.m_throttleFlapXforms[i].localRotation = Quaternion.Lerp(this.throttleFlapInitialRots[i], quaternion, this._throttle);
		}
	}

	private void _ApplyClampedThrust()
	{
		Vector3 rigidbodyVelocity = this.gtPlayer.RigidbodyVelocity;
		float num = this.jetForce * this._currentBurnRate;
		Vector3 vector = rigidbodyVelocity + base.transform.forward * (num * Time.fixedDeltaTime);
		Vector3 vector2 = new Vector3(vector.x, 0f, vector.z);
		if (vector2.sqrMagnitude > this._maxSqrHorizontalSpeed)
		{
			float magnitude = new Vector3(rigidbodyVelocity.x, 0f, rigidbodyVelocity.z).magnitude;
			vector2 = Vector3.ClampMagnitude(vector2, Mathf.Max(this.maxHorizontalSpeed, magnitude));
		}
		Vector3 vector3 = vector2;
		vector3.y = ((vector.y > this.maxVerticalSpeed) ? Mathf.Max(this.maxVerticalSpeed, rigidbodyVelocity.y) : vector.y);
		this.gtPlayer.AddForce(vector3 - rigidbodyVelocity, ForceMode.VelocityChange);
	}

	private void OnEntityStateChanged(long oldState, long newState)
	{
		SIGadgetWristJet.State state = (SIGadgetWristJet.State)oldState;
		SIGadgetWristJet.State state2 = (SIGadgetWristJet.State)newState;
		if (state != state2)
		{
			this.SetState(state2);
		}
	}

	private void SetStateAuthority(SIGadgetWristJet.State newState)
	{
		this.SetState(newState);
		this.gameEntity.RequestState(this.gameEntity.id, (long)newState);
	}

	private void SetState(SIGadgetWristJet.State newState)
	{
		if (this.state == newState)
		{
			return;
		}
		this.state = newState;
		switch (this.state)
		{
		case SIGadgetWristJet.State.Unactive:
			if (this._hasInactiveStateVisual)
			{
				this.inactiveStateVisual.SetActive(true);
			}
			if (this._hasActiveStateVisual)
			{
				this.activeStateVisual.SetActive(false);
				return;
			}
			break;
		case SIGadgetWristJet.State.Active:
			if (this._hasInactiveStateVisual)
			{
				this.inactiveStateVisual.SetActive(false);
			}
			if (this._hasActiveStateVisual)
			{
				this.activeStateVisual.SetActive(true);
				return;
			}
			break;
		case SIGadgetWristJet.State.OutOfFuel:
			if (this._hasInactiveStateVisual)
			{
				this.inactiveStateVisual.SetActive(true);
			}
			if (this._hasActiveStateVisual)
			{
				this.activeStateVisual.SetActive(false);
			}
			break;
		default:
			return;
		}
	}

	public override void ApplyUpgradeNodes(SIUpgradeSet withUpgrades)
	{
		this._throttleControl = withUpgrades.Contains(SIUpgradeType.Thruster_Throttle_Control);
		if (this._throttleControl)
		{
			this.UpdateThrottleIndicator();
		}
		switch (this.jetType)
		{
		case SIGadgetWristJet.WristJetType.Jet:
			this.fuelSpendRate *= (withUpgrades.Contains(SIUpgradeType.Thruster_Jet_Duration) ? 0.8f : 1f);
			this.jetForce *= (withUpgrades.Contains(SIUpgradeType.Thruster_Jet_Accel) ? 1.2f : 1f);
			break;
		case SIGadgetWristJet.WristJetType.Propellor:
			this.fuelSpendRate *= (withUpgrades.Contains(SIUpgradeType.Thruster_Prop_Duration) ? 0.8f : 1f);
			this.maxVerticalSpeed *= (withUpgrades.Contains(SIUpgradeType.Thruster_Prop_Speed) ? 1.2f : 1f);
			this.maxHorizontalSpeed *= (withUpgrades.Contains(SIUpgradeType.Thruster_Prop_Speed) ? 1.2f : 1f);
			break;
		}
		AudioClip audioClip;
		if (this._hasThrustLoopAudioSource && this.m_thrustLoopSoundByUpgrade.TryGetActiveValue(withUpgrades, out audioClip))
		{
			this.m_thrustLoopAudioSource.clip = audioClip;
			this.m_thrustLoopAudioSource.Play();
		}
	}

	public void Disrupt(float disruptTime)
	{
		this.emptiedCooldownResetProgress = -disruptTime;
		this.SetState(SIGadgetWristJet.State.OutOfFuel);
	}

	public override void OnEntityInit()
	{
		this.emptiedCooldownResetProgress = 0f;
		if (this._hasInactiveStateVisual)
		{
			this.inactiveStateVisual.SetActive(true);
		}
		if (this._hasActiveStateVisual)
		{
			this.activeStateVisual.SetActive(false);
		}
		this.currentFuel = (this.fuelSize = 10f);
		this._throttle = (this._currentBurnRate = 1f);
	}

	private const string preLog = "[SIGadgetWristJet]  ";

	private const string preErr = "[SIGadgetWristJet]  ERROR!!!  ";

	private const string preErrBeta = "[SIGadgetWristJet]  ERROR!!!  (beta only log)  ";

	[SerializeField]
	private AudioSource m_thrustLoopAudioSource;

	private bool _hasThrustLoopAudioSource;

	[SerializeField]
	private SIUpgradeBasedGeneric<AudioClip> m_thrustLoopSoundByUpgrade;

	[SerializeField]
	private float m_thrustLoopAudioFadeInTime = 0.1f;

	[SerializeField]
	private float m_thrustLoopAudioFadeOutTime = 0.5f;

	[SerializeField]
	private float m_thrustLoopSoundVolume = 0.33f;

	[SerializeField]
	private AudioClip m_warnFuelLowSound;

	[SerializeField]
	private float m_warnFuelLowThreshold = 0.5f;

	[SerializeField]
	private float m_warnFuelLowSoundVolume = 0.05f;

	private bool _warnFuelLowSoundWasPlayed;

	[Tooltip("This renderer's material will have the `_EmissionDissolveProgress` property changed to visually communicate current fuel amount.")]
	[SerializeField]
	private GTRendererMatSlot[] m_gaugeMatSlots;

	public SIGadgetWristJet.WristJetType jetType;

	public GameButtonActivatable buttonActivatable;

	public GameObject inactiveStateVisual;

	private bool _hasInactiveStateVisual;

	[FormerlySerializedAs("jetFlame")]
	public GameObject activeStateVisual;

	private bool _hasActiveStateVisual;

	public float jetForce;

	public float fuelGainRate;

	public float fuelSpendRate;

	public float emptiedCooldown;

	public float gravityNegationPercent;

	public float maxVerticalSpeed;

	public float maxHorizontalSpeed;

	[SerializeField]
	private bool rechargeRequiresFloorTouch;

	[SerializeField]
	private float throttleChangeSpeed = 2f;

	[SerializeField]
	[Tooltip("Minimum proportion of thrust allowed with throttle control.")]
	[Range(0f, 1f)]
	private float minimumBurnRate = 0.33f;

	[SerializeField]
	private Transform[] m_throttleFlapXforms;

	private Quaternion[] throttleFlapInitialRots;

	[SerializeField]
	private Quaternion m_throttleFlapMaxRotOffset = Quaternion.Euler(45f, 0f, 0f);

	private float fuelSize;

	private float currentFuel;

	private SIGadgetWristJet.State state;

	private GTPlayer gtPlayer;

	private float emptiedCooldownResetProgress;

	private bool _floorTouched;

	private float _maxSqrHorizontalSpeed;

	private const float kFUEL_CAPACITY = 10f;

	private MaterialPropertyBlock _gaugeMatPropBlock;

	private bool _throttleControl;

	private float _throttle;

	private float _currentBurnRate;

	private enum State
	{
		Unactive,
		Active,
		OutOfFuel
	}

	public enum WristJetType
	{
		Basic,
		Jet,
		Propellor
	}
}
