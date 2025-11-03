using System;
using GorillaLocomotion;
using GorillaLocomotion.Climbing;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(GameGrabbable))]
[RequireComponent(typeof(GameSnappable))]
[RequireComponent(typeof(GameButtonActivatable))]
public class SIGadgetDashYoyo : SIGadget
{
	private int _HandIndex
	{
		get
		{
			if ((this.m_snappable.snappedToJoint != null && this.m_snappable.snappedToJoint.jointType == SnapJointType.ArmL) || this.gameEntity.heldByHandIndex == 0)
			{
				return 0;
			}
			if ((this.m_snappable.snappedToJoint != null && this.m_snappable.snappedToJoint.jointType == SnapJointType.ArmR) || this.gameEntity.heldByHandIndex == 1)
			{
				return 1;
			}
			return -1;
		}
	}

	private void Start()
	{
		this._stateMaterials = this.m_baseStateMats;
		GameEntity gameEntity = this.gameEntity;
		gameEntity.OnGrabbed = (Action)Delegate.Combine(gameEntity.OnGrabbed, new Action(this._HandleStartInteraction));
		GameEntity gameEntity2 = this.gameEntity;
		gameEntity2.OnSnapped = (Action)Delegate.Combine(gameEntity2.OnSnapped, new Action(this._HandleStartInteraction));
		GameEntity gameEntity3 = this.gameEntity;
		gameEntity3.OnReleased = (Action)Delegate.Combine(gameEntity3.OnReleased, new Action(this._HandleStopInteraction));
		GameEntity gameEntity4 = this.gameEntity;
		gameEntity4.OnUnsnapped = (Action)Delegate.Combine(gameEntity4.OnUnsnapped, new Action(this._HandleStopInteraction));
	}

	private void OnDestroy()
	{
		if (ApplicationQuittingState.IsQuitting)
		{
			return;
		}
		GameEntity gameEntity = this.gameEntity;
		gameEntity.OnGrabbed = (Action)Delegate.Remove(gameEntity.OnGrabbed, new Action(this._HandleStartInteraction));
		GameEntity gameEntity2 = this.gameEntity;
		gameEntity2.OnSnapped = (Action)Delegate.Remove(gameEntity2.OnSnapped, new Action(this._HandleStartInteraction));
		GameEntity gameEntity3 = this.gameEntity;
		gameEntity3.OnReleased = (Action)Delegate.Remove(gameEntity3.OnReleased, new Action(this._HandleStopInteraction));
		GameEntity gameEntity4 = this.gameEntity;
		gameEntity4.OnUnsnapped = (Action)Delegate.Remove(gameEntity4.OnUnsnapped, new Action(this._HandleStopInteraction));
		if (this._attachedVRRig != null)
		{
			VRRig attachedVRRig = this._attachedVRRig;
			attachedVRRig.OnMaterialIndexChanged = (Action<int, int>)Delegate.Remove(attachedVRRig.OnMaterialIndexChanged, new Action<int, int>(this._HandleVRRigMaterialIndexChanged));
		}
		this._ResetYoYo();
	}

	private void LateUpdate()
	{
		if (ApplicationQuittingState.IsQuitting)
		{
			return;
		}
		SIGadgetDashYoyo.EState state = this._state;
		if (state - SIGadgetDashYoyo.EState.Thrown <= 2)
		{
			this.m_tetherLineRenderer.SetPosition(1, this.m_tetherLineRenderer.transform.InverseTransformPoint(this.m_yoyoTarget.position));
		}
	}

	private void _HandleStartInteraction()
	{
		if (ApplicationQuittingState.IsQuitting)
		{
			return;
		}
		this._attachedPlayerActorNr = base.GetAttachedPlayerActorNumber();
		this._attachedNetPlayer = NetworkSystem.Instance.GetPlayer(this._attachedPlayerActorNr);
		GamePlayer gamePlayer;
		if (!GamePlayer.TryGetGamePlayer(this._attachedPlayerActorNr, out gamePlayer))
		{
			return;
		}
		if (this._attachedVRRig != null)
		{
			VRRig attachedVRRig = this._attachedVRRig;
			attachedVRRig.OnMaterialIndexChanged = (Action<int, int>)Delegate.Remove(attachedVRRig.OnMaterialIndexChanged, new Action<int, int>(this._HandleVRRigMaterialIndexChanged));
		}
		this._attachedVRRig = gamePlayer.rig;
		VRRig attachedVRRig2 = this._attachedVRRig;
		attachedVRRig2.OnMaterialIndexChanged = (Action<int, int>)Delegate.Combine(attachedVRRig2.OnMaterialIndexChanged, new Action<int, int>(this._HandleVRRigMaterialIndexChanged));
		int num = (this._isTagged ? 2 : 0);
		if (num != this._attachedVRRig.setMatIndex)
		{
			this._HandleVRRigMaterialIndexChanged(num, this._attachedVRRig.setMatIndex);
		}
	}

	private void _HandleStopInteraction()
	{
		this._attachedPlayerActorNr = -1;
		this._attachedNetPlayer = null;
		if (this._attachedVRRig != null)
		{
			VRRig attachedVRRig = this._attachedVRRig;
			attachedVRRig.OnMaterialIndexChanged = (Action<int, int>)Delegate.Remove(attachedVRRig.OnMaterialIndexChanged, new Action<int, int>(this._HandleVRRigMaterialIndexChanged));
		}
		this._attachedVRRig = null;
		if (this._isTagged)
		{
			this._HandleVRRigMaterialIndexChanged(2, 0);
		}
		if (!this.gameEntity.IsAuthority())
		{
			return;
		}
		if (this._state == SIGadgetDashYoyo.EState.DashUsed || this._state == SIGadgetDashYoyo.EState.OnCooldown)
		{
			this.SetStateAuthority(SIGadgetDashYoyo.EState.OnCooldown);
		}
		else
		{
			this.SetStateAuthority(SIGadgetDashYoyo.EState.Idle);
		}
		GTPlayer.Instance.ResetRigidbodyInterpolation();
	}

	private void _HandleVRRigMaterialIndexChanged(int oldMatIndex, int newMatIndex)
	{
		if (this._attachedPlayerActorNr != -1 && (newMatIndex == 2 || newMatIndex == 1) && this._hasTagUpgrade)
		{
			SuperInfectionGame superInfectionGame = GorillaGameManager.instance as SuperInfectionGame;
			if (superInfectionGame != null)
			{
				this._isTagged = this._attachedNetPlayer != null && superInfectionGame.IsInfected(this._attachedNetPlayer);
				this._OnTagStateOrUpgradesChanged();
				return;
			}
		}
		this._isTagged = false;
		this._OnTagStateOrUpgradesChanged();
	}

	protected override void OnUpdateAuthority(float dt)
	{
		base.OnUpdateAuthority(dt);
		this._wasActivated = this._isActivated;
		this._isActivated = this._CheckInput();
		if (Time.unscaledTime < this._successfulYankTime + this.m_slipperySurfacesTime)
		{
			GTPlayer.Instance.SetMaximumSlipThisFrame();
		}
		switch (this._state)
		{
		case SIGadgetDashYoyo.EState.Idle:
			if (this._isActivated)
			{
				this._PlayHaptic(0.1f);
				this.SetStateAuthority(SIGadgetDashYoyo.EState.PreparedToThrow);
				return;
			}
			break;
		case SIGadgetDashYoyo.EState.OnCooldown:
			if (Time.unscaledTime > this._successfulYankTime + this._cooldownDuration)
			{
				this._PlayHaptic(0.5f);
				this.SetStateAuthority(SIGadgetDashYoyo.EState.Idle);
				return;
			}
			break;
		case SIGadgetDashYoyo.EState.PreparedToThrow:
			if (!this._isActivated)
			{
				if (this._ThrowYoYoTarget())
				{
					this._PlayHaptic(0.5f);
					GTPlayer.Instance.RigidbodyInterpolation = RigidbodyInterpolation.None;
					this.SetStateAuthority(SIGadgetDashYoyo.EState.Thrown);
					return;
				}
				this.SetStateAuthority(SIGadgetDashYoyo.EState.Idle);
				return;
			}
			break;
		case SIGadgetDashYoyo.EState.Thrown:
			if (Time.unscaledTime > this._timeLastThrown + this.m_waitBeforeAutoReturn)
			{
				this._PlayHaptic(0.75f);
				this.SetStateAuthority(SIGadgetDashYoyo.EState.Idle);
				GTPlayer.Instance.ResetRigidbodyInterpolation();
				return;
			}
			if (GTPlayer.Instance.RigidbodyInterpolation != RigidbodyInterpolation.None)
			{
				GTPlayer.Instance.RigidbodyInterpolation = RigidbodyInterpolation.None;
			}
			if (this._isActivated)
			{
				this.SetStateAuthority(SIGadgetDashYoyo.EState.PreparedToDash);
				return;
			}
			break;
		case SIGadgetDashYoyo.EState.PreparedToDash:
			if (Time.unscaledTime > this._timeLastThrown + this.m_waitBeforeAutoReturn)
			{
				this._PlayHaptic(0.75f);
				this.SetStateAuthority(SIGadgetDashYoyo.EState.Idle);
				return;
			}
			if (!this._isActivated)
			{
				this.SetStateAuthority(SIGadgetDashYoyo.EState.Thrown);
				return;
			}
			this._CheckYankProgression();
			return;
		case SIGadgetDashYoyo.EState.DashUsed:
			if (Time.unscaledTime > this._successfulYankTime + this.m_postYankCooldown)
			{
				this._PlayHaptic(0.1f);
				this.SetStateAuthority(SIGadgetDashYoyo.EState.OnCooldown);
			}
			break;
		default:
			return;
		}
	}

	protected override void OnUpdateRemote(float dt)
	{
		base.OnUpdateRemote(dt);
		SIGadgetDashYoyo.EState estate = (SIGadgetDashYoyo.EState)this.gameEntity.GetState();
		if (estate != this._state)
		{
			this._SetStateShared(estate);
		}
	}

	private static bool _CanChangeState(long newStateIndex)
	{
		return newStateIndex >= 0L && newStateIndex < 6L;
	}

	private void SetStateAuthority(SIGadgetDashYoyo.EState newState)
	{
		this._SetStateShared(newState);
		this.gameEntity.RequestState(this.gameEntity.id, (long)newState);
	}

	private void _SetStateShared(SIGadgetDashYoyo.EState newState)
	{
		if (newState == this._state || !SIGadgetDashYoyo._CanChangeState((long)newState))
		{
			return;
		}
		SIGadgetDashYoyo.EState state = this._state;
		this._state = newState;
		switch (this._state)
		{
		case SIGadgetDashYoyo.EState.Idle:
			if (state == SIGadgetDashYoyo.EState.OnCooldown)
			{
				this._PlayAudio(4);
			}
			else if (state == SIGadgetDashYoyo.EState.PreparedToThrow)
			{
				this._PlayAudio(5);
			}
			this._ResetYoYo();
			this._SetMaterials(this._stateMaterials.idle);
			return;
		case SIGadgetDashYoyo.EState.OnCooldown:
			this._PlayAudio(3);
			this._ResetYoYo();
			this._SetMaterials(this._stateMaterials.cooldown);
			return;
		case SIGadgetDashYoyo.EState.PreparedToThrow:
			this._PlayAudio(0);
			this._SetMaterials(this._stateMaterials.ready);
			return;
		case SIGadgetDashYoyo.EState.Thrown:
			if (state != SIGadgetDashYoyo.EState.PreparedToDash)
			{
				this._PlayAudio(1);
			}
			this._SetMaterials(this._stateMaterials.ready);
			return;
		case SIGadgetDashYoyo.EState.PreparedToDash:
			this._yankBeginPos = this.m_yoyoDefaultPosXform.position;
			this._SetMaterials(this._stateMaterials.ready);
			return;
		case SIGadgetDashYoyo.EState.DashUsed:
			this._PlayAudio(2);
			this._FreezeYoYo();
			this._SetMaterials(this._stateMaterials.cooldown);
			return;
		default:
			return;
		}
	}

	private bool _CheckInput()
	{
		float num = (this._wasActivated ? this.m_inputDeactivateThreshold : this.m_inputActivateThreshold);
		return this.m_buttonActivatable.CheckInput(true, true, num, true);
	}

	private bool _ThrowYoYoTarget()
	{
		Vector3 vector = GamePlayerLocal.instance.GetHandVelocity(this._HandIndex);
		if (vector.magnitude < this.m_minThrowSpeed)
		{
			return false;
		}
		Vector3 handAngularVelocity = GamePlayerLocal.instance.GetHandAngularVelocity(this._HandIndex);
		GorillaVelocityTracker bodyVelocityTracker = GTPlayer.Instance.bodyVelocityTracker;
		vector *= this._throwMultiplier;
		vector += bodyVelocityTracker.GetAverageVelocity(true, 0.05f, false);
		this._LaunchYoYoShared(vector, handAngularVelocity, this.m_yoyoTargetRB.transform.position, this.m_yoyoTargetRB.transform.rotation);
		this._timeLastThrown = Time.unscaledTime;
		if (!NetworkSystem.Instance.InRoom)
		{
			return true;
		}
		SuperInfectionManager simanagerForZone = SuperInfectionManager.GetSIManagerForZone(this.gameEntity.manager.zone);
		if (simanagerForZone == null)
		{
			return true;
		}
		this._launchYoyoRPCArgs[0] = this.gameEntity.GetNetId();
		this._launchYoyoRPCArgs[1] = vector;
		this._launchYoyoRPCArgs[2] = handAngularVelocity;
		this._launchYoyoRPCArgs[3] = this.m_yoyoTargetRB.transform.position;
		this._launchYoyoRPCArgs[4] = this.m_yoyoTargetRB.transform.rotation;
		simanagerForZone.CallRPC(SuperInfectionManager.ClientToClientRPC.LaunchDashYoyo, this._launchYoyoRPCArgs);
		return true;
	}

	internal void RemoteThrowYoYoTarget(Vector3 velocity, Vector3 angVelocity, Vector3 targetPosition, Quaternion targetRotation)
	{
		this._LaunchYoYoShared(velocity, angVelocity, targetPosition, targetRotation);
	}

	private void _LaunchYoYoShared(Vector3 velocity, Vector3 angVelocity, Vector3 targetPosition, Quaternion targetRotation)
	{
		this.m_yoyoTargetRB.transform.parent = null;
		this.m_yoyoTargetRB.transform.position = targetPosition;
		this.m_yoyoTargetRB.transform.rotation = targetRotation;
		this.m_yoyoTargetRB.gameObject.SetActive(true);
		this.m_yoyoTarget.parent = this.m_yoyoTargetRB.transform;
		this.m_yoyoTargetRB.isKinematic = false;
		this.m_yoyoTargetRB.linearVelocity = velocity;
		this.m_yoyoTargetRB.angularVelocity = angVelocity;
		this.m_tetherLineRenderer.gameObject.SetActive(true);
	}

	private void _FreezeYoYo()
	{
		this.m_yoyoTargetRB.gameObject.SetActive(false);
		this.m_yoyoTarget.parent = null;
	}

	internal void OnHitPlayer_Authority(SuperInfectionGame siTagGameManager, NetPlayer victimNetPlayer)
	{
		bool flag = siTagGameManager.IsInfected(this._attachedNetPlayer);
		bool flag2 = siTagGameManager.IsInfected(victimNetPlayer);
		if (flag == flag2)
		{
			return;
		}
		if (this._hasTagUpgrade && !flag2)
		{
			siTagGameManager.ReportTag(victimNetPlayer, this._attachedNetPlayer);
			return;
		}
		RoomSystem.SendStatusEffectToPlayer(RoomSystem.StatusEffects.SetSlowedTime, victimNetPlayer);
		RoomSystem.SendSoundEffectOnOther(5, 0.125f, victimNetPlayer, false);
	}

	private void _ResetYoYo()
	{
		this.m_tetherLineRenderer.gameObject.SetActive(false);
		this.m_yoyoTargetRB.gameObject.SetActive(false);
		this.m_yoyoTarget.SetParent(this.m_yoyoDefaultPosXform, false);
		this.m_yoyoTarget.transform.localPosition = Vector3.zero;
		this.m_yoyoTarget.transform.localRotation = Quaternion.identity;
		this.m_yoyoTargetRB.transform.SetParent(this.m_yoyoDefaultPosXform, false);
		this.m_yoyoTargetRB.transform.localPosition = Vector3.zero;
		this.m_yoyoTargetRB.transform.localRotation = Quaternion.identity;
	}

	private void _SetMaterials(Material mat)
	{
		this.m_yoyoRenderer.sharedMaterial = mat;
		this.m_tetherLineRenderer.sharedMaterial = mat;
	}

	private void _CheckYankProgression()
	{
		Vector3 handVelocity = GamePlayerLocal.instance.GetHandVelocity(this._HandIndex);
		this._maxEncounteredYankSpeed = Mathf.Max(this._maxEncounteredYankSpeed, handVelocity.magnitude);
		Vector3 vector = this._yankBeginPos - this.m_yoyoDefaultPosXform.position;
		Vector3 normalized = (-handVelocity.normalized + vector.normalized).normalized;
		Vector3 vector2 = this.m_yoyoTarget.position - this.m_yoyoDefaultPosXform.position;
		if (vector.magnitude < this.m_yankMinDistance || this._maxEncounteredYankSpeed < this.m_yankMinSpeed || Vector3.Angle(vector2, normalized) > this.m_yankMaxAngle)
		{
			return;
		}
		this._successfulYankTime = Time.unscaledTime;
		float num = this._CalculateDashSpeed(handVelocity.magnitude);
		GTPlayer instance = GTPlayer.Instance;
		instance.SetMaximumSlipThisFrame();
		instance.SetVelocity(Vector3.RotateTowards(vector2.normalized, normalized, this._maxInfluenceAngle * 0.017453292f, 0f) * num);
		this._PlayHaptic(2f);
		this.SetStateAuthority(SIGadgetDashYoyo.EState.DashUsed);
	}

	private float _CalculateDashSpeed(float currentYankSpeed)
	{
		float num = Mathf.InverseLerp(this.m_yankMinSpeed, this.m_yankMaxSpeed, currentYankSpeed);
		float num2 = this.m_speedMappingCurve.Evaluate(num);
		return Mathf.Lerp(this.m_minDashSpeed, this._maxDashSpeed, num2);
	}

	private void _PlayHaptic(float strengthMultiplier)
	{
		bool flag;
		if (base.FindAttachedHand(out flag, true, true))
		{
			GorillaTagger.Instance.StartVibration(flag, GorillaTagger.Instance.tapHapticStrength * strengthMultiplier, GorillaTagger.Instance.tapHapticDuration);
		}
	}

	private void _PlayAudio(int index)
	{
		this.m_audioSource.clip = this.m_clips[index];
		this.m_audioSource.volume = this.m_clipVolumes[index];
		this.m_audioSource.GTPlay();
	}

	private void _OnTagStateOrUpgradesChanged()
	{
		this._stateMaterials = (this._hasTagUpgrade ? (this._isTagged ? this.m_tagUpgradeStateMatsWhileTagged : this.m_tagUpgradeStateMatsWhileUntagged) : this.m_baseStateMats);
		switch (this._state)
		{
		case SIGadgetDashYoyo.EState.Idle:
			this._SetMaterials(this._stateMaterials.idle);
			return;
		case SIGadgetDashYoyo.EState.OnCooldown:
			this._SetMaterials(this._stateMaterials.cooldown);
			return;
		case SIGadgetDashYoyo.EState.PreparedToThrow:
			this._SetMaterials(this._stateMaterials.ready);
			return;
		case SIGadgetDashYoyo.EState.Thrown:
			this._SetMaterials(this._stateMaterials.ready);
			return;
		case SIGadgetDashYoyo.EState.PreparedToDash:
			this._SetMaterials(this._stateMaterials.ready);
			return;
		case SIGadgetDashYoyo.EState.DashUsed:
			this._SetMaterials(this._stateMaterials.cooldown);
			return;
		default:
			return;
		}
	}

	public override void ApplyUpgradeNodes(SIUpgradeSet withUpgrades)
	{
		this._cooldownDuration = (withUpgrades.Contains(SIUpgradeType.Dash_Yoyo_Cooldown) ? this.m_cooldownDurationUpgrade : this.m_cooldownDurationDefault);
		this._throwMultiplier = (withUpgrades.Contains(SIUpgradeType.Dash_Yoyo_Range) ? this.m_throwMultiplierUpgrade : this.m_throwMultiplierDefault);
		this._maxDashSpeed = (withUpgrades.Contains(SIUpgradeType.Dash_Yoyo_Speed) ? this.m_maxDashSpeedUpgraded : this.m_maxDashSpeedDefault);
		this._maxInfluenceAngle = (withUpgrades.Contains(SIUpgradeType.Dash_Yoyo_Dynamic) ? this.m_maxInfluenceAngleUpgrade : this.m_maxInfluenceAngleDefault);
		this._hasStunUpgrade = withUpgrades.Contains(SIUpgradeType.Dash_Yoyo_Stun);
		this._hasTagUpgrade = withUpgrades.Contains(SIUpgradeType.Dash_Yoyo_Tag);
		this._OnTagStateOrUpgradesChanged();
	}

	private const string preLog = "[SIGadgetDashYoyo]  ";

	private const string preErr = "[SIGadgetDashYoyo]  ERROR!!!  ";

	[SerializeField]
	private GameSnappable m_snappable;

	[SerializeField]
	private Transform m_yoyoDefaultPosXform;

	[SerializeField]
	private Transform m_yoyoTarget;

	[SerializeField]
	private Rigidbody m_yoyoTargetRB;

	[SerializeField]
	private GameButtonActivatable m_buttonActivatable;

	[SerializeField]
	private float m_inputActivateThreshold = 0.35f;

	[SerializeField]
	private float m_inputDeactivateThreshold = 0.25f;

	private SIGadgetDashYoyo.StateMaterialsInfo _stateMaterials;

	[SerializeField]
	private SIGadgetDashYoyo.StateMaterialsInfo m_baseStateMats;

	[SerializeField]
	private SIGadgetDashYoyo.StateMaterialsInfo m_tagUpgradeStateMatsWhileTagged;

	[SerializeField]
	private SIGadgetDashYoyo.StateMaterialsInfo m_tagUpgradeStateMatsWhileUntagged;

	[SerializeField]
	private MeshRenderer m_yoyoRenderer;

	[SerializeField]
	private AudioSource m_audioSource;

	[SerializeField]
	public AudioClip[] m_clips;

	[SerializeField]
	public float[] m_clipVolumes;

	private float _throwMultiplier;

	[SerializeField]
	private float m_throwMultiplierDefault = 1.5f;

	[SerializeField]
	private float m_throwMultiplierUpgrade = 2f;

	[FormerlySerializedAs("m_tether")]
	[SerializeField]
	private LineRenderer m_tetherLineRenderer;

	[SerializeField]
	private float m_minThrowSpeed = 2f;

	[SerializeField]
	private float m_waitBeforeAutoReturn = 3f;

	[SerializeField]
	private float m_postYankCooldown = 2f;

	[SerializeField]
	private float m_maxYankRecheckTime = 0.2f;

	[SerializeField]
	private float m_yankMinDistance = 0.5f;

	[SerializeField]
	private float m_yankMaxAngle = 60f;

	[Tooltip("Yank min/max: How fast you have to be moving your hand for the yank to register and result in a dash.")]
	[SerializeField]
	private float m_yankMinSpeed = 2f;

	[Tooltip("Yank min/max: How fast you have to be moving your hand for the yank to register and result in a dash.")]
	[SerializeField]
	private float m_yankMaxSpeed = 8f;

	[Tooltip("Dash min/max speed: The fastest speed the player will move")]
	[SerializeField]
	private float m_minDashSpeed = 4f;

	private float _maxDashSpeed;

	[SerializeField]
	private float m_maxDashSpeedDefault = 11f;

	[SerializeField]
	private float m_maxDashSpeedUpgraded = 13f;

	[Tooltip("Maps yank speed to dash speed.\nX = Yank Speed (min to max)\nY = Dash Speed (min to max).")]
	[SerializeField]
	private AnimationCurve m_speedMappingCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	private float m_slipperySurfacesTime = 0.25f;

	private float _maxInfluenceAngle;

	[SerializeField]
	private float m_maxInfluenceAngleDefault = 10f;

	[SerializeField]
	private float m_maxInfluenceAngleUpgrade = 15f;

	private float _cooldownDuration;

	[SerializeField]
	private float m_cooldownDurationDefault = 6f;

	[SerializeField]
	private float m_cooldownDurationUpgrade = 5f;

	private bool _hasStunUpgrade;

	private bool _hasTagUpgrade;

	private bool _isActivated;

	private bool _wasActivated;

	private float _timeLastThrown;

	private float _successfulYankTime;

	private float _maxEncounteredYankSpeed;

	private Vector3 _yankBeginPos;

	private bool _isRecheckingYank;

	private VRRig _attachedVRRig;

	private int _lastAttachedPlayerActorNr;

	private int _attachedPlayerActorNr = int.MinValue;

	private NetPlayer _attachedNetPlayer;

	private bool _isTagged;

	private readonly object[] _launchYoyoRPCArgs = new object[5];

	private SIGadgetDashYoyo.EState _state;

	[Serializable]
	public struct StateMaterialsInfo
	{
		public Material idle;

		public Material ready;

		public Material cooldown;
	}

	private enum EState
	{
		Idle,
		OnCooldown,
		PreparedToThrow,
		Thrown,
		PreparedToDash,
		DashUsed,
		Count
	}
}
