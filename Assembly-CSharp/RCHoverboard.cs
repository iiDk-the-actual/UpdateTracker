using System;
using System.Runtime.CompilerServices;
using GorillaLocomotion;
using GorillaLocomotion.Climbing;
using GorillaTag.Cosmetics;
using Photon.Pun;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.XR;

public class RCHoverboard : RCVehicle
{
	private float _MaxForwardSpeed
	{
		get
		{
			return this.m_maxForwardSpeed;
		}
		set
		{
			this.m_maxForwardSpeed = value;
			this._forwardAccel = value / math.max(0.01f, this.m_forwardAccelTime);
		}
	}

	private float _MaxTurnRate
	{
		get
		{
			return this.m_maxTurnRate;
		}
		set
		{
			this.m_maxTurnRate = value;
			this._turnAccel = value / math.max(1E-06f, this.m_turnAccelTime);
		}
	}

	private float _MaxTiltAngle
	{
		get
		{
			return this.m_maxTiltAngle;
		}
		set
		{
			this.m_maxTiltAngle = value;
			this._tiltAccel = value / math.max(1E-06f, this.m_tiltTime);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		this._hasAudioSource = this.m_audioSource != null;
		this._hasHoverSound = this.m_hoverSound != null;
		this._MaxForwardSpeed = this.m_maxForwardSpeed;
		this._MaxTurnRate = this.m_maxTurnRate;
		this._MaxTiltAngle = this.m_maxTiltAngle;
	}

	protected override void AuthorityBeginDocked()
	{
		base.AuthorityBeginDocked();
		this._currentTurnRate = 0f;
		this._currentTiltAngle = 0f;
		float3 @float = this._ProjectOnPlane(base.transform.forward, math.up());
		this._currentTurnAngle = this._SignedAngle(new float3(0f, 0f, 1f), @float, new float3(0f, 1f, 0f));
		this._motorLevel = 0f;
		if (this._hasAudioSource)
		{
			this.m_audioSource.Stop();
			this.m_audioSource.volume = 0f;
		}
		if (this.connectedRemote == null)
		{
			base.gameObject.SetActive(false);
		}
	}

	protected override void AuthorityUpdate(float dt)
	{
		base.AuthorityUpdate(dt);
		if (this.localState == RCVehicle.State.Mobilized)
		{
			float num = math.length(this.activeInput.joystick);
			this._motorLevel = math.saturate(num);
			if (this.hasNetworkSync)
			{
				this.networkSync.syncedState.dataA = (byte)((uint)(this._motorLevel * 255f));
				return;
			}
		}
		else
		{
			this._motorLevel = 0f;
		}
	}

	protected override void RemoteUpdate(float dt)
	{
		base.RemoteUpdate(dt);
		if (this.localState == RCVehicle.State.Mobilized && this.hasNetworkSync)
		{
			this._motorLevel = (float)this.networkSync.syncedState.dataA / 255f;
			return;
		}
		this._motorLevel = 0f;
	}

	protected override void SharedUpdate(float dt)
	{
		base.SharedUpdate(dt);
		switch (this.localState)
		{
		case RCVehicle.State.Disabled:
		case RCVehicle.State.DockedLeft:
		case RCVehicle.State.DockedRight:
		case RCVehicle.State.Crashed:
			break;
		case RCVehicle.State.Mobilized:
			if (this._hasAudioSource && this._hasHoverSound)
			{
				if (this.localStatePrev != RCVehicle.State.Mobilized)
				{
					this.m_audioSource.volume = 0f;
					this.m_audioSource.clip = this.m_hoverSound;
					this.m_audioSource.loop = true;
					this.m_audioSource.GTPlay();
					return;
				}
				float num = math.lerp(this.m_hoverSoundVolumeMinMax.x, this.m_hoverSoundVolumeMinMax.y, this._motorLevel);
				float num2 = this.m_hoverSoundVolumeMinMax.y / this.m_hoverSoundVolumeRampTime * dt;
				this.m_audioSource.volume = this._MoveTowards(this.m_audioSource.volume, num, num2);
			}
			break;
		default:
			return;
		}
	}

	protected void FixedUpdate()
	{
		if (!base.HasLocalAuthority || this.localState != RCVehicle.State.Mobilized)
		{
			return;
		}
		float fixedDeltaTime = Time.fixedDeltaTime;
		float num = this.m_inputThrustForward.Get(this.activeInput) - this.m_inputThrustBack.Get(this.activeInput);
		float num2 = this.m_inputTurn.Get(this.activeInput);
		float num3 = this.m_inputJump.Get(this.activeInput);
		RaycastHit raycastHit;
		bool flag = Physics.Raycast(base.transform.position, Vector3.down, out raycastHit, 10f, this.raycastLayers, QueryTriggerInteraction.Collide);
		bool flag2 = flag && raycastHit.distance <= this.m_hoverHeight + 0.1f;
		if (this.enableJumpInput && num3 > 0.001f && flag2 && !this._hasJumped)
		{
			this.rb.AddForce(Vector3.up * this.m_jumpForce, ForceMode.Impulse);
			this._hasJumped = true;
		}
		else if (num3 <= 0.001f)
		{
			this._hasJumped = false;
		}
		float num4 = num2 * this._MaxTurnRate;
		this._currentTurnRate = this._MoveTowards(this._currentTurnRate, num4, this._turnAccel * fixedDeltaTime);
		this._currentTurnAngle += this._currentTurnRate * fixedDeltaTime;
		float num5 = math.lerp(-this.m_maxTiltAngle, this.m_maxTiltAngle, math.unlerp(-1f, 1f, num));
		this._currentTiltAngle = this._MoveTowards(this._currentTiltAngle, num5, this._tiltAccel * fixedDeltaTime);
		base.transform.rotation = quaternion.EulerXYZ(math.radians(new float3(this._currentTiltAngle, this._currentTurnAngle, 0f)));
		float3 @float = base.transform.forward;
		float num6 = math.dot(@float, this.rb.linearVelocity);
		float num7 = num * this.m_maxForwardSpeed;
		float num8 = ((math.abs(num7) > 0.001f && ((num7 > 0f && num6 < num7) || (num7 < 0f && num6 > num7))) ? math.sign(num7) : 0f);
		this.rb.AddForce(@float * this._forwardAccel * num8 * this.rb.mass, ForceMode.Force);
		if (flag)
		{
			float num9 = math.saturate(this.m_hoverHeight - raycastHit.distance);
			float num10 = math.dot(this.rb.linearVelocity, Vector3.up);
			float num11 = num9 * this.m_hoverForce - num10 * this.m_hoverDamp;
			this.rb.AddForce(math.up() * num11, ForceMode.Force);
		}
	}

	protected void OnCollisionEnter(Collision collision)
	{
		GameObject gameObject = collision.collider.gameObject;
		bool flag = gameObject.IsOnLayer(UnityLayer.GorillaThrowable);
		bool flag2 = gameObject.IsOnLayer(UnityLayer.GorillaHand);
		if ((flag || flag2) && this.localState == RCVehicle.State.Mobilized)
		{
			Vector3 vector = Vector3.zero;
			if (flag2)
			{
				GorillaHandClimber component = gameObject.GetComponent<GorillaHandClimber>();
				if (component != null)
				{
					vector = GTPlayer.Instance.GetHandVelocityTracker(component.xrNode == XRNode.LeftHand).GetAverageVelocity(true, 0.15f, false);
				}
			}
			else if (collision.rigidbody != null)
			{
				vector = collision.rigidbody.linearVelocity;
			}
			if ((flag || vector.sqrMagnitude > 0.01f) && base.HasLocalAuthority)
			{
				this.AuthorityApplyImpact(vector, flag);
				if (this.networkSync != null)
				{
					this.networkSync.photonView.RPC("HitRCVehicleRPC", RpcTarget.Others, new object[] { vector, flag });
				}
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private float _MoveTowards(float current, float target, float maxDelta)
	{
		if (math.abs(target - current) > maxDelta)
		{
			return current + math.sign(target - current) * maxDelta;
		}
		return target;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private float _SignedAngle(float3 from, float3 to, float3 axis)
	{
		float3 @float = math.normalize(from);
		float3 float2 = math.normalize(to);
		float num = math.acos(math.dot(@float, float2));
		float num2 = math.sign(math.dot(math.cross(@float, float2), axis));
		return math.degrees(num) * num2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private float3 _ProjectOnPlane(float3 vector, float3 planeNormal)
	{
		return vector - math.dot(vector, planeNormal) * planeNormal;
	}

	[SerializeField]
	private RCHoverboard._SingleInputOption m_inputTurn = new RCHoverboard._SingleInputOption(RCHoverboard._EInputSource.StickX, new AnimationCurve(new Keyframe[]
	{
		new Keyframe(0f, 0f, 0f, 0f, 0f, 0f),
		new Keyframe(0.1f, 0f, 0f, 1.25f, 0f, 0f),
		new Keyframe(0.9f, 1f, 1.25f, 0f, 0f, 0f),
		new Keyframe(1f, 1f, 0f, 0f, 0f, 0f)
	}));

	[SerializeField]
	private RCHoverboard._SingleInputOption m_inputThrustForward = new RCHoverboard._SingleInputOption(RCHoverboard._EInputSource.Trigger, AnimationCurves.EaseInCirc);

	[SerializeField]
	private RCHoverboard._SingleInputOption m_inputThrustBack = new RCHoverboard._SingleInputOption(RCHoverboard._EInputSource.StickBack, new AnimationCurve(new Keyframe[]
	{
		new Keyframe(0f, 0f, 0f, 0f, 0f, 0f),
		new Keyframe(0.9f, 0f, 0f, 9.9999f, 0.5825f, 0.3767f),
		new Keyframe(1f, 1f, 9.9999f, 1f, 0f, 0f)
	}));

	[SerializeField]
	private RCHoverboard._SingleInputOption m_inputJump = new RCHoverboard._SingleInputOption(RCHoverboard._EInputSource.PrimaryFaceButton, AnimationCurves.Linear);

	[Tooltip("Desired hover height above ground from this transform's position.")]
	[SerializeField]
	private float m_hoverHeight = 0.2f;

	[Tooltip("Upward force to maintain hover when below hoverHeight.")]
	[SerializeField]
	private float m_hoverForce = 200f;

	[Tooltip("Damping factor to smooth out vertical movement.")]
	[SerializeField]
	private float m_hoverDamp = 5f;

	[SerializeField]
	private LayerMask raycastLayers = -1;

	[SerializeField]
	private bool enableJumpInput = true;

	[Tooltip("Upward impulse force for jump.")]
	[SerializeField]
	private float m_jumpForce = 3.5f;

	private bool _hasJumped;

	[SerializeField]
	[HideInInspector]
	private float m_maxForwardSpeed = 6f;

	[SerializeField]
	[Tooltip("Time (seconds) to reach max forward speed from zero.")]
	private float m_forwardAccelTime = 2f;

	[SerializeField]
	[HideInInspector]
	private float m_maxTurnRate = 720f;

	[Tooltip("Time (seconds) to reach max turning rate.")]
	[SerializeField]
	private float m_turnAccelTime = 0.75f;

	[SerializeField]
	[HideInInspector]
	private float m_maxTiltAngle = 30f;

	[Tooltip("Time (seconds) to reach max tilt angle.")]
	[SerializeField]
	private float m_tiltTime = 0.1f;

	[Tooltip("Audio source for any motor or hover sound.")]
	[SerializeField]
	private AudioSource m_audioSource;

	[Tooltip("Looping motor/hover sound clip.")]
	[SerializeField]
	private AudioClip m_hoverSound;

	[Tooltip("Volume range for the hover sound (x = min, y = max).")]
	[SerializeField]
	private float2 m_hoverSoundVolumeMinMax = new float2(0.1f, 0.5f);

	[Tooltip("Time it takes for the volume to reach max value.")]
	[SerializeField]
	private float m_hoverSoundVolumeRampTime = 1f;

	private bool _hasAudioSource;

	private bool _hasHoverSound;

	private float _forwardAccel;

	private float _turnAccel;

	private float _tiltAccel;

	private float _currentTurnRate;

	private float _currentTurnAngle;

	private float _currentTiltAngle;

	private float _motorLevel;

	private enum _EInputSource
	{
		None,
		StickX,
		StickForward,
		StickBack,
		Trigger,
		PrimaryFaceButton
	}

	[Serializable]
	private struct _SingleInputOption
	{
		public _SingleInputOption(RCHoverboard._EInputSource source, AnimationCurve remapCurve)
		{
			this.source = new GTOption<StringEnum<RCHoverboard._EInputSource>>(source);
			this.remapCurve = new GTOption<AnimationCurve>(remapCurve);
		}

		public float Get(RCRemoteHoldable.RCInput input)
		{
			float num;
			switch (this.source.ResolvedValue.Value)
			{
			case RCHoverboard._EInputSource.None:
				num = 0f;
				break;
			case RCHoverboard._EInputSource.StickX:
				num = input.joystick.x;
				break;
			case RCHoverboard._EInputSource.StickForward:
				num = math.saturate(input.joystick.y);
				break;
			case RCHoverboard._EInputSource.StickBack:
				num = math.saturate(-input.joystick.y);
				break;
			case RCHoverboard._EInputSource.Trigger:
				num = input.trigger;
				break;
			case RCHoverboard._EInputSource.PrimaryFaceButton:
				num = (float)input.buttons;
				break;
			default:
				num = 0f;
				break;
			}
			float num2 = num;
			return this.remapCurve.ResolvedValue.Evaluate(math.abs(num2)) * math.sign(num2);
		}

		public GTOption<StringEnum<RCHoverboard._EInputSource>> source;

		public GTOption<AnimationCurve> remapCurve;
	}
}
