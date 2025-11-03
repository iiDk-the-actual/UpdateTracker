using System;
using GorillaLocomotion;
using GorillaLocomotion.Climbing;
using UnityEngine;

public class SIGadgetStilt : SIGadget
{
	public bool TriggerToExtend { get; private set; }

	public bool StickToAdjustLength { get; private set; }

	public bool CanTag { get; private set; }

	public bool CanStun { get; private set; }

	private void Awake()
	{
		this.tipVelocityTracker.enabled = false;
		this.tipDefaultOffset = this.tip.transform.localPosition;
		GameEntity gameEntity = this.gameEntity;
		gameEntity.OnGrabbed = (Action)Delegate.Combine(gameEntity.OnGrabbed, new Action(this.OnGrabbed));
		GameEntity gameEntity2 = this.gameEntity;
		gameEntity2.OnSnapped = (Action)Delegate.Combine(gameEntity2.OnSnapped, new Action(this.OnSnapped));
		GameEntity gameEntity3 = this.gameEntity;
		gameEntity3.OnReleased = (Action)Delegate.Combine(gameEntity3.OnReleased, new Action(this.OnReleased));
		GameEntity gameEntity4 = this.gameEntity;
		gameEntity4.OnUnsnapped = (Action)Delegate.Combine(gameEntity4.OnUnsnapped, new Action(this.OnUnsnapped));
		this.gameEntity.OnStateChanged += this.OnEntityStateChanged;
	}

	private void DisableCurrentStilt()
	{
		if (this.currentStiltID != StiltID.None)
		{
			GTPlayer.Instance.DisableStilt(this.currentStiltID);
			this.currentStiltID = StiltID.None;
			this.tipVelocityTracker.enabled = false;
		}
	}

	private void OnGrabbed()
	{
		this.DisableCurrentStilt();
		this.HandleStartInteraction();
		if (this.IsEquippedLocal())
		{
			this.activatedLocally = true;
			this.currentStiltID = ((this.gameEntity.heldByHandIndex == 0) ? StiltID.Held_Left : StiltID.Held_Right);
			if (this.boostSpeedFactor > 0f)
			{
				this.tipVelocityTracker.enabled = true;
				this.tipVelocityTracker.SetRelativeTo(VRRig.LocalRig.transform);
			}
			GTPlayer.Instance.EnableStilt(this.currentStiltID, this.stiltEnd.position, this.maxArmLength, this.CanTag, this.CanStun, this.boostSpeedFactor, this.tipVelocityTracker);
		}
		else
		{
			this.activatedLocally = false;
		}
		this.wasSnappedByLocalJoint = SnapJointType.None;
	}

	private void OnReleased()
	{
		this.DisableCurrentStilt();
		this.HandleStopInteraction();
		if (this.gameEntity.WasLastHeldByLocalPlayer() && this.TriggerToExtend && !Mathf.Approximately(this.targetLength, this.retractedLength))
		{
			this.targetLength = this.retractedLength;
			this.gameEntity.RequestState(this.gameEntity.id, (long)(this.targetLength * 1000f));
		}
	}

	private void OnSnapped()
	{
		this.DisableCurrentStilt();
		this.HandleStartInteraction();
		if (this.IsEquippedLocal())
		{
			this.wasSnappedByLocalJoint = this.gameEntity.snappedJoint;
			if (this.wasSnappedByLocalJoint == SnapJointType.ArmL)
			{
				this.currentStiltID = StiltID.Snapped_Left;
				if (this.boostSpeedFactor > 0f)
				{
					this.tipVelocityTracker.enabled = true;
					this.tipVelocityTracker.SetRelativeTo(VRRig.LocalRig.transform);
				}
				GTPlayer.Instance.EnableStilt(this.currentStiltID, this.stiltEnd.position, this.maxArmLength, this.CanTag, this.CanStun, this.boostSpeedFactor, this.tipVelocityTracker);
				return;
			}
			if (this.wasSnappedByLocalJoint == SnapJointType.ArmR)
			{
				this.currentStiltID = StiltID.Snapped_Right;
				if (this.boostSpeedFactor > 0f)
				{
					this.tipVelocityTracker.enabled = true;
					this.tipVelocityTracker.SetRelativeTo(VRRig.LocalRig.transform);
				}
				GTPlayer.Instance.EnableStilt(this.currentStiltID, this.stiltEnd.position, this.maxArmLength, this.CanTag, this.CanStun, this.boostSpeedFactor, this.tipVelocityTracker);
				return;
			}
		}
		else
		{
			this.wasSnappedByLocalJoint = SnapJointType.None;
		}
	}

	private void OnUnsnapped()
	{
		this.DisableCurrentStilt();
		this.HandleStopInteraction();
		if (this.wasSnappedByLocalJoint == SnapJointType.ArmL)
		{
			this.wasSnappedByLocalJoint = SnapJointType.None;
			return;
		}
		if (this.wasSnappedByLocalJoint == SnapJointType.ArmR)
		{
			this.wasSnappedByLocalJoint = SnapJointType.None;
		}
	}

	private void OnDestroy()
	{
		if (ApplicationQuittingState.IsQuitting)
		{
			return;
		}
		this.DisableCurrentStilt();
		if (this.attachedVRRig != null)
		{
			VRRig vrrig = this.attachedVRRig;
			vrrig.OnMaterialIndexChanged = (Action<int, int>)Delegate.Remove(vrrig.OnMaterialIndexChanged, new Action<int, int>(this.HandleVRRigMaterialIndexChanged));
		}
	}

	protected override void OnUpdateAuthority(float dt)
	{
		if (this.currentStiltID != StiltID.None)
		{
			bool flag = !this.TriggerToExtend || this.CheckInput();
			bool flag2 = false;
			float num = this.targetLength;
			if (flag)
			{
				if (this.StickToAdjustLength)
				{
					Vector2 joystickInput = base.GetJoystickInput();
					if (Mathf.Abs(joystickInput.y) > 0.75f && Mathf.Abs(joystickInput.x) < 0.5f)
					{
						this.currentExtendedLength = Mathf.Clamp(this.currentExtendedLength + joystickInput.y * this.lengthChangeSpeed * Time.deltaTime, this.retractedLength, this.maxLength);
					}
				}
				if (!Mathf.Approximately(this.targetLength, this.currentExtendedLength))
				{
					this.targetLength = this.currentExtendedLength;
				}
				if (!Mathf.Approximately(this.targetLength, this.lastSentLength) && Time.time > this.nextAdjustmentSendTime)
				{
					this.nextAdjustmentSendTime = Time.time + this.adjustmentSendRate;
					this.lastSentLength = this.targetLength;
					flag2 = true;
				}
			}
			else if (!Mathf.Approximately(this.targetLength, this.retractedLength))
			{
				this.targetLength = this.retractedLength;
				this.lastSentLength = this.targetLength;
				flag2 = true;
			}
			if (flag2)
			{
				this.CheckPlaySounds(num, this.targetLength);
				this.gameEntity.RequestState(this.gameEntity.id, (long)(this.targetLength * 1000f));
			}
		}
		this.UpdateLength();
	}

	protected override void OnUpdateRemote(float dt)
	{
		base.OnUpdateRemote(dt);
		this.UpdateLength();
	}

	private bool CheckInput()
	{
		return this.buttonActivatable.CheckInput(true, true, 0.25f, true);
	}

	public override SIUpgradeSet FilterUpgradeNodes(SIUpgradeSet upgrades)
	{
		if (this.restrictedUpgrades.Length == 0)
		{
			return upgrades;
		}
		SIUpgradeSet siupgradeSet = default(SIUpgradeSet);
		foreach (SIUpgradeType siupgradeType in this.restrictedUpgrades)
		{
			if (upgrades.Contains(siupgradeType))
			{
				siupgradeSet.Add(siupgradeType);
			}
		}
		return siupgradeSet;
	}

	public override void ApplyUpgradeNodes(SIUpgradeSet withUpgrades)
	{
		this.CanTag = withUpgrades.Contains(SIUpgradeType.Stilt_Tag_Tip);
		this.CanStun = withUpgrades.Contains(SIUpgradeType.Stilt_Stun_Tip);
		this.TriggerToExtend = this.buttonActivatable != null && withUpgrades.Contains(SIUpgradeType.Stilt_Retractable);
		this.StickToAdjustLength = this.TriggerToExtend && withUpgrades.Contains(SIUpgradeType.Stilt_Adjustable_Length);
		this.extendSpeed = (withUpgrades.Contains(SIUpgradeType.Stilt_Retract_Speed) ? this.extendSpeedUpgraded : this.extendSpeedNormal);
		this.retractSpeed = (withUpgrades.Contains(SIUpgradeType.Stilt_Retract_Speed) ? this.retractSpeedUpgraded : this.retractSpeedNormal);
		this.maxLength = ((this.TriggerToExtend && withUpgrades.Contains(SIUpgradeType.Stilt_Max_Length)) ? this.maxLengthUpgraded : this.maxLengthNormal);
		this.currentExtendedLength = this.maxLength;
		this.targetLength = (this.TriggerToExtend ? this.retractedLength : this.currentExtendedLength);
		this.currentLength = this.targetLength;
		this.ApplyCurrentLength();
	}

	private void UpdateLength()
	{
		if (Mathf.Approximately(this.currentLength, this.targetLength))
		{
			return;
		}
		float num = ((this.targetLength > this.currentLength) ? this.extendSpeed : this.retractSpeed);
		this.currentLength = Mathf.MoveTowards(this.currentLength, this.targetLength, num * Time.deltaTime);
		this.ApplyCurrentLength();
		if (this.currentStiltID != StiltID.None)
		{
			GTPlayer.Instance.UpdateStiltOffset(this.currentStiltID, this.stiltEnd.position);
		}
	}

	private void ApplyCurrentLength()
	{
		this.tip.transform.localPosition = this.offsetDir * this.currentLength + this.tipDefaultOffset;
		Vector3 localScale = this.midpoint.transform.localScale;
		localScale.z = this.currentLength;
		this.midpoint.transform.localScale = localScale;
	}

	private void OnEntityStateChanged(long oldState, long newState)
	{
		float num = this.targetLength;
		this.targetLength = Mathf.Clamp((float)newState * 0.001f, this.retractedLength, this.maxLength);
		if (this.IsEquippedLocal())
		{
			return;
		}
		this.CheckPlaySounds(num, this.targetLength);
	}

	private void CheckPlaySounds(float oldLength, float newLength)
	{
		if (Mathf.Approximately(oldLength, newLength))
		{
			return;
		}
		if (Mathf.Approximately(newLength, this.retractedLength))
		{
			this.retractSoundBank.Play();
			return;
		}
		if (Mathf.Approximately(oldLength, this.retractedLength))
		{
			this.extendSoundBank.Play();
		}
	}

	private void HandleStartInteraction()
	{
		if (ApplicationQuittingState.IsQuitting)
		{
			return;
		}
		this.attachedPlayerActorNr = base.GetAttachedPlayerActorNumber();
		this.attachedNetPlayer = NetworkSystem.Instance.GetPlayer(this.attachedPlayerActorNr);
		GamePlayer gamePlayer;
		if (!GamePlayer.TryGetGamePlayer(this.attachedPlayerActorNr, out gamePlayer))
		{
			return;
		}
		if (this.attachedVRRig != null)
		{
			VRRig vrrig = this.attachedVRRig;
			vrrig.OnMaterialIndexChanged = (Action<int, int>)Delegate.Remove(vrrig.OnMaterialIndexChanged, new Action<int, int>(this.HandleVRRigMaterialIndexChanged));
		}
		this.attachedVRRig = gamePlayer.rig;
		VRRig vrrig2 = this.attachedVRRig;
		vrrig2.OnMaterialIndexChanged = (Action<int, int>)Delegate.Combine(vrrig2.OnMaterialIndexChanged, new Action<int, int>(this.HandleVRRigMaterialIndexChanged));
		int num = (this.isTagged ? 2 : 0);
		if (num != this.attachedVRRig.setMatIndex)
		{
			this.HandleVRRigMaterialIndexChanged(num, this.attachedVRRig.setMatIndex);
		}
	}

	private void HandleStopInteraction()
	{
		this.attachedPlayerActorNr = -1;
		this.attachedNetPlayer = null;
		if (this.attachedVRRig != null)
		{
			VRRig vrrig = this.attachedVRRig;
			vrrig.OnMaterialIndexChanged = (Action<int, int>)Delegate.Remove(vrrig.OnMaterialIndexChanged, new Action<int, int>(this.HandleVRRigMaterialIndexChanged));
		}
		this.attachedVRRig = null;
		if (this.isTagged)
		{
			this.HandleVRRigMaterialIndexChanged(2, 0);
		}
	}

	private void HandleVRRigMaterialIndexChanged(int oldMatIndex, int newMatIndex)
	{
		if (this.attachedPlayerActorNr != -1 && (newMatIndex == 2 || newMatIndex == 1) && this.CanTag)
		{
			SuperInfectionGame superInfectionGame = GorillaGameManager.instance as SuperInfectionGame;
			if (superInfectionGame != null)
			{
				this.isTagged = this.attachedNetPlayer != null && superInfectionGame.IsInfected(this.attachedNetPlayer);
				if (this.matDest)
				{
					this.matDest.sharedMaterial = this.tagActivatedMat;
				}
				if (this.skinnedMatDest)
				{
					this.skinnedMatDest.sharedMaterial = this.tagActivatedMat;
					return;
				}
				return;
			}
		}
		this.isTagged = false;
		if (this.matDest)
		{
			this.matDest.sharedMaterial = this.defaultMat;
		}
		if (this.skinnedMatDest)
		{
			this.skinnedMatDest.sharedMaterial = this.defaultMat;
		}
	}

	[SerializeField]
	private GameButtonActivatable buttonActivatable;

	public GameObject tip;

	[SerializeField]
	private Vector3 offsetDir = Vector3.forward;

	private Vector3 tipDefaultOffset;

	public GameObject midpoint;

	public Transform stiltEnd;

	[SerializeField]
	private SIUpgradeType[] restrictedUpgrades;

	[SerializeField]
	private float maxLengthNormal;

	[SerializeField]
	private float maxLengthUpgraded;

	[SerializeField]
	private float retractedLength;

	[SerializeField]
	private float lengthChangeSpeed;

	[SerializeField]
	private float maxArmLength;

	[SerializeField]
	private float extendSpeedNormal;

	[SerializeField]
	private float extendSpeedUpgraded;

	[SerializeField]
	private float retractSpeedNormal;

	[SerializeField]
	private float retractSpeedUpgraded;

	[SerializeField]
	private float boostSpeedFactor;

	[SerializeField]
	private GorillaVelocityTracker tipVelocityTracker;

	[SerializeField]
	private SoundBankPlayer retractSoundBank;

	[SerializeField]
	private SoundBankPlayer extendSoundBank;

	[SerializeField]
	private Material defaultMat;

	[SerializeField]
	private Material tagActivatedMat;

	[SerializeField]
	private MeshRenderer matDest;

	[SerializeField]
	private SkinnedMeshRenderer skinnedMatDest;

	private float currentExtendedLength;

	private float targetLength;

	private float currentLength;

	private float maxLength;

	private float extendSpeed;

	private float retractSpeed;

	private float adjustmentSendRate = 0.25f;

	private float lastSentLength;

	private float nextAdjustmentSendTime = -1f;

	private StiltID currentStiltID = StiltID.None;

	private SnapJointType wasSnappedByLocalJoint;

	private int attachedPlayerActorNr = int.MinValue;

	private NetPlayer attachedNetPlayer;

	private VRRig attachedVRRig;

	private bool isTagged;
}
