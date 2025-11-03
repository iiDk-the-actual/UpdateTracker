using System;
using GorillaExtensions;
using GorillaGameModes;
using GorillaLocomotion;
using GorillaNetworking;
using GorillaTagScripts.VirtualStumpCustomMaps;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;

public class HandLink : HoldableObject, IGorillaSliceableSimple
{
	public bool IsLocal { get; private set; }

	private void Start()
	{
		this.myOtherHandLink = (this.isLeftHand ? this.myRig.rightHandLink : this.myRig.leftHandLink);
		if (this.myRig.isOfflineVRRig)
		{
			base.gameObject.SetActive(false);
			this.IsLocal = true;
		}
		if (this.interactionPoint == null)
		{
			this.interactionPoint = base.GetComponent<InteractionPoint>();
		}
	}

	public void OnEnable()
	{
		GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.LateUpdate);
	}

	public void OnDisable()
	{
		GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.LateUpdate);
	}

	public void SliceUpdate()
	{
		this.interactionPoint.enabled = this.canBeGrabbed && (this.myRig.transform.position - VRRig.LocalRig.transform.position).sqrMagnitude < 9f;
	}

	public override void OnGrab(InteractionPoint pointGrabbed, GameObject grabbingHand)
	{
		if (!this.CanBeGrabbed())
		{
			return;
		}
		GorillaGuardianManager gorillaGuardianManager = GameMode.ActiveGameMode as GorillaGuardianManager;
		if (gorillaGuardianManager != null && gorillaGuardianManager.IsPlayerGuardian(NetworkSystem.Instance.LocalPlayer))
		{
			(this.isLeftHand ? this.myRig.leftHolds : this.myRig.rightHolds).OnGrab(pointGrabbed, grabbingHand);
			return;
		}
		HandLink handLink = ((grabbingHand == EquipmentInteractor.instance.leftHand) ? VRRig.LocalRig.leftHandLink : VRRig.LocalRig.rightHandLink);
		if (handLink.canBeGrabbed && Time.time - handLink.gripPressedAtTimestamp < 0.1f)
		{
			handLink.CreateLink(this);
		}
	}

	public override bool OnRelease(DropZone zoneReleased, GameObject releasingHand)
	{
		if (!base.OnRelease(zoneReleased, releasingHand))
		{
			return false;
		}
		if (!this.myRig.isOfflineVRRig)
		{
			HandLink handLink = ((releasingHand == EquipmentInteractor.instance.leftHand) ? VRRig.LocalRig.leftHandLink : VRRig.LocalRig.rightHandLink);
			bool flag = false;
			HandLinkAuthorityStatus selfHandLinkAuthority = GTPlayer.Instance.GetSelfHandLinkAuthority();
			int num;
			HandLinkAuthorityStatus chainAuthority = handLink.GetChainAuthority(out num);
			if (selfHandLinkAuthority.type >= HandLinkAuthorityType.ButtGrounded && chainAuthority.type < selfHandLinkAuthority.type)
			{
				flag = true;
			}
			else if (handLink.myOtherHandLink.grabbedLink != null)
			{
				int num2;
				HandLinkAuthorityStatus chainAuthority2 = handLink.myOtherHandLink.GetChainAuthority(out num2);
				if (chainAuthority2.type >= HandLinkAuthorityType.ButtGrounded && chainAuthority.type < chainAuthority2.type)
				{
					flag = true;
				}
			}
			if (flag)
			{
				Vector3 averageVelocity = GTPlayer.Instance.GetHandVelocityTracker(handLink.isLeftHand).GetAverageVelocity(true, 0.15f, false);
				this.myRig.netView.SendRPC("DroppedByPlayer", this.myRig.OwningNetPlayer, new object[] { averageVelocity });
				this.myRig.ApplyLocalTrajectoryOverride(averageVelocity);
			}
			handLink.BreakLink();
		}
		return true;
	}

	public override void OnHover(InteractionPoint pointHovered, GameObject hoveringHand)
	{
	}

	public override void DropItemCleanup()
	{
		if (this.grabbedLink != null)
		{
			this.grabbedLink.BreakLink();
		}
	}

	public bool CanBeGrabbed()
	{
		return (!GorillaComputer.instance.IsPlayerInVirtualStump() || !CustomMapManager.WantsHoldingHandsDisabled()) && Time.time >= this.rejectGrabsUntilTimestamp && this.canBeGrabbed && this.grabbedPlayer == null;
	}

	public bool IsLinkActive()
	{
		return this.grabbedLink != null;
	}

	private void CreateLink(HandLink remoteLink)
	{
		if (this.grabbedPlayer != null || !this.myRig.isLocal)
		{
			return;
		}
		GRPlayer grplayer = GRPlayer.Get(remoteLink.myRig);
		GRPlayer grplayer2 = GRPlayer.Get(NetworkSystem.Instance.LocalPlayer);
		if (grplayer2 != null && grplayer != null && grplayer2.State == GRPlayer.GRPlayerState.Ghost != (grplayer.State == GRPlayer.GRPlayerState.Ghost))
		{
			return;
		}
		EquipmentInteractor.instance.UpdateHandEquipment(remoteLink, this.isLeftHand);
		this.grabbedLink = remoteLink;
		this.grabbedPlayer = remoteLink.myRig.OwningNetPlayer;
		this.grabbedHandIsLeft = remoteLink.isLeftHand;
		GorillaTagger.Instance.StartVibration(this.isLeftHand, this.hapticStrengthOnGrab, this.hapticDurationOnGrab);
		(this.isLeftHand ? VRRig.LocalRig.leftHandPlayer : VRRig.LocalRig.rightHandPlayer).GTPlayOneShot(this.audioOnGrab, 1f);
		Action onHandLinkChanged = HandLink.OnHandLinkChanged;
		if (onHandLinkChanged == null)
		{
			return;
		}
		onHandLinkChanged();
	}

	public void BreakLinkTo(HandLink targetLink)
	{
		if (this.grabbedLink == targetLink)
		{
			this.BreakLink();
		}
	}

	public void BreakLink()
	{
		if (this.grabbedPlayer == null || this.grabbedLink == null)
		{
			return;
		}
		Vector3 vector = this.myRig.LatestVelocity();
		GTPlayer.Instance.SetVelocity(vector);
		this.grabbedLink = null;
		this.grabbedPlayer = null;
		this.grabbedHandIsLeft = false;
		EquipmentInteractor.instance.UpdateHandEquipment(null, this.isLeftHand);
		Action onHandLinkChanged = HandLink.OnHandLinkChanged;
		if (onHandLinkChanged == null)
		{
			return;
		}
		onHandLinkChanged();
	}

	public static bool IsHandInChainWithOtherPlayer(HandLink startingLink, int targetPlayer)
	{
		HandLink handLink = startingLink;
		int num = 0;
		int roomPlayerCount = NetworkSystem.Instance.RoomPlayerCount;
		while (handLink != null && num < roomPlayerCount)
		{
			if (handLink.myRig == null || handLink.myRig.creator == null)
			{
				return false;
			}
			if (handLink.myRig.creator.ActorNumber == targetPlayer)
			{
				return true;
			}
			HandLink handLink2 = null;
			RigContainer rigContainer;
			if (handLink.grabbedLink != null && handLink.grabbedLink.myOtherHandLink != null)
			{
				handLink2 = handLink.grabbedLink.myOtherHandLink;
			}
			else if (handLink.grabbedPlayer != null && VRRigCache.Instance.TryGetVrrig(handLink.grabbedPlayer, out rigContainer))
			{
				HandLink handLink3 = (handLink.grabbedHandIsLeft ? rigContainer.Rig.leftHandLink : rigContainer.Rig.rightHandLink);
				if (handLink3 != null && handLink3.myOtherHandLink != null)
				{
					handLink2 = handLink3.myOtherHandLink;
				}
			}
			handLink = handLink2;
			num++;
		}
		return false;
	}

	public void LocalUpdate(bool isGroundedHand, bool isGroundedButt, bool isGripPressed, bool canBeGrabbed)
	{
		if (isGripPressed && !this.wasGripPressed)
		{
			this.gripPressedAtTimestamp = Time.time;
		}
		this.wasGripPressed = isGripPressed;
		this.canBeGrabbed = canBeGrabbed;
		this.isGroundedHand = isGroundedHand;
		this.isGroundedButt = isGroundedButt;
		if (this.grabbedLink != null)
		{
			if (!this.grabbedLink.canBeGrabbed && this.grabbedLink.grabbedPlayer != NetworkSystem.Instance.LocalPlayer)
			{
				this.BreakLink();
				return;
			}
			if (!isGripPressed || !this.grabbedLink.myRig.gameObject.activeSelf)
			{
				this.BreakLink();
				return;
			}
			GorillaGuardianManager gorillaGuardianManager = GameMode.ActiveGameMode as GorillaGuardianManager;
			if (gorillaGuardianManager != null && gorillaGuardianManager.IsPlayerGuardian(this.grabbedPlayer))
			{
				this.BreakLink();
				return;
			}
			GRPlayer grplayer = GRPlayer.Get(this.grabbedLink.myRig);
			GRPlayer grplayer2 = GRPlayer.Get(NetworkSystem.Instance.LocalPlayer);
			if (grplayer2 != null && grplayer != null && grplayer2.State == GRPlayer.GRPlayerState.Ghost != (grplayer.State == GRPlayer.GRPlayerState.Ghost))
			{
				this.BreakLink();
				return;
			}
			if (GorillaComputer.instance.IsPlayerInVirtualStump() && CustomMapManager.WantsHoldingHandsDisabled())
			{
				this.BreakLink();
				return;
			}
		}
	}

	public void RejectGrabsFor(float duration)
	{
		this.rejectGrabsUntilTimestamp = Mathf.Max(this.rejectGrabsUntilTimestamp, Time.time + duration);
	}

	public void Write(out bool isGroundedHand, out bool isGroundedButt, out int grabbedPlayerActorNumber, out bool grabbedHandIsLeft)
	{
		isGroundedHand = this.isGroundedHand;
		isGroundedButt = this.isGroundedButt;
		if (this.grabbedPlayer != null)
		{
			grabbedPlayerActorNumber = this.grabbedPlayer.ActorNumber;
			grabbedHandIsLeft = this.grabbedHandIsLeft;
			return;
		}
		grabbedPlayerActorNumber = 0;
		grabbedHandIsLeft = false;
	}

	public void Read(Vector3 remoteHandLocalPos, Quaternion remoteBodyWorldRot, Vector3 remoteBodyWorldPos, bool isGroundedHand, bool isGroundedButt, bool isGripReady, int grabbedPlayerActorNumber, bool grabbedHandIsLeft)
	{
		this.isGroundedHand = isGroundedHand;
		this.isGroundedButt = isGroundedButt;
		this.canBeGrabbed = isGripReady;
		if (grabbedPlayerActorNumber == 0)
		{
			if (this.grabbedPlayer != null && this.grabbedPlayer.IsLocal)
			{
				(grabbedHandIsLeft ? VRRig.LocalRig.leftHandLink : VRRig.LocalRig.rightHandLink).BreakLink();
			}
			bool flag = this.grabbedPlayer != null;
			this.grabbedPlayer = null;
			this.grabbedLink = null;
			if (flag)
			{
				Action onHandLinkChanged = HandLink.OnHandLinkChanged;
				if (onHandLinkChanged != null)
				{
					onHandLinkChanged();
				}
			}
		}
		else if (this.lastReadGrabbedPlayerActorNumber == grabbedPlayerActorNumber)
		{
			if (this.grabbedPlayer != null && this.grabbedPlayer.IsValid && this.grabbedPlayer.ActorNumber == grabbedPlayerActorNumber && this.grabbedPlayer.IsLocal && !this.IsLocalGrabInRange(grabbedHandIsLeft, remoteHandLocalPos, remoteBodyWorldRot, remoteBodyWorldPos, 7f))
			{
				if (this.grabbedHandIsLeft)
				{
					VRRig.LocalRig.leftHandLink.BreakLink();
				}
				else
				{
					VRRig.LocalRig.rightHandLink.BreakLink();
				}
			}
		}
		else
		{
			if (this.grabbedPlayer != null && this.grabbedPlayer.IsLocal)
			{
				VRRig.LocalRig.leftHandLink.BreakLinkTo(this);
				VRRig.LocalRig.rightHandLink.BreakLinkTo(this);
			}
			NetPlayer player = NetworkSystem.Instance.GetPlayer(grabbedPlayerActorNumber);
			if (player != null)
			{
				if (player.IsLocal && !this.IsLocalGrabInRange(grabbedHandIsLeft, remoteHandLocalPos, remoteBodyWorldRot, remoteBodyWorldPos, 0.25f))
				{
					bool flag2 = this.grabbedPlayer != null;
					this.grabbedPlayer = null;
					this.grabbedLink = null;
					if (flag2)
					{
						Action onHandLinkChanged2 = HandLink.OnHandLinkChanged;
						if (onHandLinkChanged2 != null)
						{
							onHandLinkChanged2();
						}
					}
				}
				else if (player == this.myRig.OwningNetPlayer)
				{
					bool flag3 = this.grabbedPlayer != null;
					this.grabbedPlayer = null;
					this.grabbedLink = null;
					if (flag3)
					{
						Action onHandLinkChanged3 = HandLink.OnHandLinkChanged;
						if (onHandLinkChanged3 != null)
						{
							onHandLinkChanged3();
						}
					}
				}
				else
				{
					this.grabbedPlayer = player;
					this.grabbedHandIsLeft = grabbedHandIsLeft;
					this.CheckFormLinkWithRemoteGrab();
					Action onHandLinkChanged4 = HandLink.OnHandLinkChanged;
					if (onHandLinkChanged4 != null)
					{
						onHandLinkChanged4();
					}
				}
			}
			else
			{
				bool flag4 = this.grabbedPlayer != null;
				this.grabbedPlayer = null;
				this.grabbedLink = null;
				if (flag4)
				{
					Action onHandLinkChanged5 = HandLink.OnHandLinkChanged;
					if (onHandLinkChanged5 != null)
					{
						onHandLinkChanged5();
					}
				}
			}
		}
		this.lastReadGrabbedPlayerActorNumber = grabbedPlayerActorNumber;
	}

	private bool IsLocalGrabInRange(bool grabbedLeftHand, Vector3 handLocalPos, Quaternion bodyWorldRot, Vector3 bodyWorldPos, float tolerance)
	{
		return ((grabbedLeftHand ? VRRig.LocalRig.leftHandLink : VRRig.LocalRig.rightHandLink).transform.position - (bodyWorldPos + bodyWorldRot * handLocalPos)).IsShorterThan(tolerance);
	}

	private void CheckFormLinkWithRemoteGrab()
	{
		RigContainer rigContainer;
		if (this.grabbedPlayer == NetworkSystem.Instance.LocalPlayer)
		{
			HandLink handLink = (this.grabbedHandIsLeft ? VRRig.LocalRig.leftHandLink : VRRig.LocalRig.rightHandLink);
			if (handLink.canBeGrabbed && Time.time > handLink.rejectGrabsUntilTimestamp)
			{
				handLink.CreateLink(this);
				return;
			}
		}
		else if (VRRigCache.Instance.TryGetVrrig(this.grabbedPlayer, out rigContainer))
		{
			HandLink handLink2 = (this.grabbedHandIsLeft ? rigContainer.Rig.leftHandLink : rigContainer.Rig.rightHandLink);
			if (handLink2.grabbedPlayer == this.myRig.creator)
			{
				this.grabbedLink = handLink2;
				this.grabbedLink.grabbedLink = this;
			}
		}
	}

	public HandLinkAuthorityStatus GetChainAuthority(out int stepsToAuth)
	{
		HandLink handLink = this.grabbedLink;
		int num = 1;
		HandLinkAuthorityStatus handLinkAuthorityStatus = new HandLinkAuthorityStatus(HandLinkAuthorityType.None, -1f, -1);
		stepsToAuth = -1;
		while (handLink != null && num < 10 && !handLink.IsLocal)
		{
			if (handLink.isGroundedHand)
			{
				stepsToAuth = num;
				return new HandLinkAuthorityStatus(HandLinkAuthorityType.HandGrounded, -1f, -1);
			}
			if (handLinkAuthorityStatus.type < HandLinkAuthorityType.ResidualHandGrounded && (double)(handLink.myRig.LastHandTouchedGroundAtNetworkTime + 1f) > PhotonNetwork.Time)
			{
				stepsToAuth = num;
				handLinkAuthorityStatus = new HandLinkAuthorityStatus(HandLinkAuthorityType.ResidualHandGrounded, handLink.myRig.LastHandTouchedGroundAtNetworkTime, handLink.myRig.OwningNetPlayer.ActorNumber);
			}
			else if (handLinkAuthorityStatus.type < HandLinkAuthorityType.ButtGrounded && handLink.isGroundedButt)
			{
				stepsToAuth = num;
				handLinkAuthorityStatus = new HandLinkAuthorityStatus(HandLinkAuthorityType.ButtGrounded, -1f, -1);
			}
			else if (handLinkAuthorityStatus.type == HandLinkAuthorityType.None)
			{
				HandLinkAuthorityStatus handLinkAuthorityStatus2 = new HandLinkAuthorityStatus(HandLinkAuthorityType.None, handLink.myRig.LastTouchedGroundAtNetworkTime, handLink.myRig.OwningNetPlayer.ActorNumber);
				if (handLinkAuthorityStatus2 > handLinkAuthorityStatus)
				{
					stepsToAuth = num;
					handLinkAuthorityStatus = handLinkAuthorityStatus2;
				}
			}
			num++;
			handLink = handLink.myOtherHandLink.grabbedLink;
		}
		return handLinkAuthorityStatus;
	}

	public void SnapHandsTogether()
	{
		if (this.grabbedLink == null)
		{
			return;
		}
		if (this.grabbedLink.snapPositionCalculatedAtFrame == Time.frameCount)
		{
			this.snapPositionCalculatedAtFrame = Time.frameCount;
			return;
		}
		Vector3 position = base.transform.position;
		Vector3 position2 = this.grabbedLink.transform.position;
		Vector3 vector = (position + position2) / 2f;
		Vector3 vector2 = (this.isLeftHand ? this.myRig.leftHand.rigTarget : this.myRig.rightHand.rigTarget).position - position;
		Vector3 vector3 = (this.grabbedLink.isLeftHand ? this.grabbedLink.myRig.leftHand.rigTarget : this.grabbedLink.myRig.rightHand.rigTarget).position - position2;
		Vector3 vector4 = vector + vector2;
		Vector3 vector5 = vector + vector3;
		this.myIK.OverrideTargetPos(this.isLeftHand, vector4);
		this.grabbedLink.myIK.OverrideTargetPos(this.grabbedLink.isLeftHand, vector5);
	}

	public void PlayVicariousTapHaptic()
	{
		GorillaTagger.Instance.StartVibration(this.isLeftHand, this.hapticStrengthOnVicariousTap, this.hapticDurationOnVicariousTap);
	}

	[FormerlySerializedAs("myPlayer")]
	[SerializeField]
	public VRRig myRig;

	[FormerlySerializedAs("leftHand")]
	[SerializeField]
	private bool isLeftHand;

	[SerializeField]
	public GorillaIK myIK;

	private HandLink myOtherHandLink;

	private bool canBeGrabbed;

	public bool isGroundedHand;

	public bool isGroundedButt;

	private bool wasGripPressed;

	private float gripPressedAtTimestamp;

	private float rejectGrabsUntilTimestamp;

	public HandLink grabbedLink;

	public NetPlayer grabbedPlayer;

	public bool grabbedHandIsLeft;

	private const bool DEBUG_GRAB_ANYONE = false;

	[SerializeField]
	private float hapticStrengthOnGrab;

	[SerializeField]
	private float hapticDurationOnGrab;

	[SerializeField]
	private float hapticStrengthOnVicariousTap;

	[SerializeField]
	private float hapticDurationOnVicariousTap;

	[SerializeField]
	private AudioClip audioOnGrab;

	public InteractionPoint interactionPoint;

	public static Action OnHandLinkChanged;

	private int lastReadGrabbedPlayerActorNumber;

	private int snapPositionCalculatedAtFrame = -1;
}
