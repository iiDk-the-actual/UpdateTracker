using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.XR;

public class LegacyTransferrableObject : HoldableObject
{
	protected void Awake()
	{
		this.latched = false;
		this.initOffset = base.transform.localPosition;
		this.initRotation = base.transform.localRotation;
	}

	protected virtual void Start()
	{
		RoomSystem.JoinedRoomEvent += new Action(this.OnJoinedRoom);
		RoomSystem.LeftRoomEvent += new Action(this.OnLeftRoom);
		RoomSystem.PlayerJoinedEvent += new Action<NetPlayer>(this.OnPlayerLeftRoom);
	}

	public void OnEnable()
	{
		if (this.myRig == null && this.myOnlineRig != null && this.myOnlineRig.netView != null && this.myOnlineRig.netView.IsMine)
		{
			base.gameObject.SetActive(false);
			return;
		}
		if (this.myRig == null && this.myOnlineRig == null)
		{
			base.gameObject.SetActive(false);
			return;
		}
		this.objectIndex = this.targetDock.ReturnTransferrableItemIndex(this.myIndex);
		if (this.myRig != null && this.myRig.isOfflineVRRig)
		{
			if (this.currentState == TransferrableObject.PositionState.OnLeftArm)
			{
				this.storedZone = BodyDockPositions.DropPositions.LeftArm;
			}
			else if (this.currentState == TransferrableObject.PositionState.OnRightArm)
			{
				this.storedZone = BodyDockPositions.DropPositions.RightArm;
			}
			else if (this.currentState == TransferrableObject.PositionState.OnLeftShoulder)
			{
				this.storedZone = BodyDockPositions.DropPositions.LeftBack;
			}
			else if (this.currentState == TransferrableObject.PositionState.OnRightShoulder)
			{
				this.storedZone = BodyDockPositions.DropPositions.RightBack;
			}
			else
			{
				this.storedZone = BodyDockPositions.DropPositions.Chest;
			}
		}
		if (this.objectIndex == -1)
		{
			base.gameObject.SetActive(false);
			return;
		}
		if (this.currentState == TransferrableObject.PositionState.OnLeftArm && this.flipOnXForLeftArm)
		{
			Transform transform = this.GetAnchor(this.currentState);
			transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
		}
		this.initState = this.currentState;
		this.enabledOnFrame = Time.frameCount;
		this.SpawnShareableObject();
	}

	public void OnDisable()
	{
		this.enabledOnFrame = -1;
	}

	private void SpawnShareableObject()
	{
		if (!PhotonNetwork.InRoom)
		{
			return;
		}
		if (!this.canDrop && !this.shareable)
		{
			return;
		}
		if (this.worldShareableInstance != null)
		{
			return;
		}
		object[] array = new object[]
		{
			this.myIndex,
			PhotonNetwork.LocalPlayer
		};
		this.worldShareableInstance = PhotonNetwork.Instantiate("Objects/equipment/WorldShareableItem", base.transform.position, base.transform.rotation, 0, array);
		if (this.myRig != null && this.worldShareableInstance != null)
		{
			this.OnWorldShareableItemSpawn();
		}
	}

	public void OnJoinedRoom()
	{
		Debug.Log("Here");
		this.SpawnShareableObject();
	}

	public void OnLeftRoom()
	{
		if (this.worldShareableInstance != null)
		{
			PhotonNetwork.Destroy(this.worldShareableInstance);
		}
		this.OnWorldShareableItemDeallocated(NetworkSystem.Instance.LocalPlayer);
	}

	public void OnPlayerLeftRoom(NetPlayer otherPlayer)
	{
		this.OnWorldShareableItemDeallocated(otherPlayer);
	}

	public void SetWorldShareableItem(GameObject item)
	{
		this.worldShareableInstance = item;
		this.OnWorldShareableItemSpawn();
	}

	protected virtual void OnWorldShareableItemSpawn()
	{
	}

	protected virtual void OnWorldShareableItemDeallocated(NetPlayer player)
	{
	}

	public virtual void LateUpdate()
	{
		if (this.interactor == null)
		{
			this.interactor = EquipmentInteractor.instance;
		}
		if (this.IsMyItem())
		{
			this.LateUpdateLocal();
		}
		else
		{
			this.LateUpdateReplicated();
		}
		this.LateUpdateShared();
		this.previousState = this.currentState;
	}

	protected Transform DefaultAnchor()
	{
		if (!(this.anchor == null))
		{
			return this.anchor;
		}
		return base.transform;
	}

	private Transform GetAnchor(TransferrableObject.PositionState pos)
	{
		if (this.grabAnchor == null)
		{
			return this.DefaultAnchor();
		}
		if (this.InHand())
		{
			return this.grabAnchor;
		}
		return this.DefaultAnchor();
	}

	protected bool Attached()
	{
		bool flag = this.InHand() && this.detatchOnGrab;
		return !this.Dropped() && !flag;
	}

	private void UpdateFollowXform()
	{
		if (this.targetRig == null)
		{
			return;
		}
		if (this.targetDock == null)
		{
			this.targetDock = this.targetRig.GetComponent<BodyDockPositions>();
		}
		if (this.anchorOverrides == null)
		{
			this.anchorOverrides = this.targetRig.GetComponent<VRRigAnchorOverrides>();
		}
		Transform transform = this.GetAnchor(this.currentState);
		Transform transform2 = transform;
		TransferrableObject.PositionState positionState = this.currentState;
		if (positionState <= TransferrableObject.PositionState.InRightHand)
		{
			switch (positionState)
			{
			case TransferrableObject.PositionState.OnLeftArm:
				transform2 = this.anchorOverrides.AnchorOverride(this.currentState, this.targetDock.leftArmTransform);
				break;
			case TransferrableObject.PositionState.OnRightArm:
				transform2 = this.anchorOverrides.AnchorOverride(this.currentState, this.targetDock.rightArmTransform);
				break;
			case TransferrableObject.PositionState.OnLeftArm | TransferrableObject.PositionState.OnRightArm:
				break;
			case TransferrableObject.PositionState.InLeftHand:
				transform2 = this.anchorOverrides.AnchorOverride(this.currentState, this.targetDock.leftHandTransform);
				break;
			default:
				if (positionState == TransferrableObject.PositionState.InRightHand)
				{
					transform2 = this.anchorOverrides.AnchorOverride(this.currentState, this.targetDock.rightHandTransform);
				}
				break;
			}
		}
		else if (positionState != TransferrableObject.PositionState.OnChest)
		{
			if (positionState != TransferrableObject.PositionState.OnLeftShoulder)
			{
				if (positionState == TransferrableObject.PositionState.OnRightShoulder)
				{
					transform2 = this.anchorOverrides.AnchorOverride(this.currentState, this.targetDock.rightBackTransform);
				}
			}
			else
			{
				transform2 = this.anchorOverrides.AnchorOverride(this.currentState, this.targetDock.leftBackTransform);
			}
		}
		else
		{
			transform2 = this.anchorOverrides.AnchorOverride(this.currentState, this.targetDock.chestTransform);
		}
		LegacyTransferrableObject.InterpolateState interpolateState = this.interpState;
		if (interpolateState != LegacyTransferrableObject.InterpolateState.None)
		{
			if (interpolateState != LegacyTransferrableObject.InterpolateState.Interpolating)
			{
				return;
			}
			float num = Mathf.Clamp((this.interpTime - this.interpDt) / this.interpTime, 0f, 1f);
			transform.transform.position = Vector3.Lerp(this.interpStartPos, transform2.transform.position, num);
			transform.transform.rotation = Quaternion.Slerp(this.interpStartRot, transform2.transform.rotation, num);
			this.interpDt -= Time.deltaTime;
			if (this.interpDt <= 0f)
			{
				transform.parent = transform2;
				this.interpState = LegacyTransferrableObject.InterpolateState.None;
				transform.localPosition = Vector3.zero;
				transform.localRotation = Quaternion.identity;
				transform.localScale = Vector3.one;
				if (this.flipOnXForLeftHand && this.currentState == TransferrableObject.PositionState.InLeftHand)
				{
					transform.localScale = new Vector3(-1f, 1f, 1f);
				}
				if (this.flipOnYForLeftHand && this.currentState == TransferrableObject.PositionState.InLeftHand)
				{
					transform.localScale = new Vector3(1f, -1f, 1f);
				}
			}
		}
		else if (transform2 != transform.parent)
		{
			if (Time.frameCount == this.enabledOnFrame)
			{
				transform.parent = transform2;
				transform.localPosition = Vector3.zero;
				transform.localRotation = Quaternion.identity;
				return;
			}
			this.interpState = LegacyTransferrableObject.InterpolateState.Interpolating;
			this.interpDt = this.interpTime;
			this.interpStartPos = transform.transform.position;
			this.interpStartRot = transform.transform.rotation;
			return;
		}
	}

	public void DropItem()
	{
		base.transform.parent = null;
	}

	protected virtual void LateUpdateShared()
	{
		this.disableItem = true;
		for (int i = 0; i < this.targetRig.ActiveTransferrableObjectIndexLength(); i++)
		{
			if (this.targetRig.ActiveTransferrableObjectIndex(i) == this.myIndex)
			{
				this.disableItem = false;
				break;
			}
		}
		if (this.disableItem)
		{
			base.gameObject.SetActive(false);
			return;
		}
		if (this.previousState != this.currentState && this.detatchOnGrab && this.InHand())
		{
			base.transform.parent = null;
		}
		if (this.currentState != TransferrableObject.PositionState.Dropped)
		{
			this.UpdateFollowXform();
			return;
		}
		if (this.canDrop)
		{
			this.DropItem();
		}
	}

	protected void ResetXf()
	{
		if (this.canDrop)
		{
			Transform transform = this.DefaultAnchor();
			if (base.transform != transform && base.transform.parent != transform)
			{
				base.transform.parent = transform;
			}
			base.transform.localPosition = this.initOffset;
			base.transform.localRotation = this.initRotation;
		}
	}

	protected void ReDock()
	{
		if (this.IsMyItem())
		{
			this.currentState = this.initState;
		}
		this.ResetXf();
	}

	private void HandleLocalInput()
	{
		GameObject[] array;
		if (!this.InHand())
		{
			array = this.gameObjectsActiveOnlyWhileHeld;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(false);
			}
			return;
		}
		array = this.gameObjectsActiveOnlyWhileHeld;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(true);
		}
		XRNode xrnode = ((this.currentState == TransferrableObject.PositionState.InLeftHand) ? XRNode.LeftHand : XRNode.RightHand);
		this.indexTrigger = ControllerInputPoller.TriggerFloat(xrnode);
		bool flag = !this.latched && this.indexTrigger >= this.myThreshold;
		bool flag2 = this.latched && this.indexTrigger < this.myThreshold - this.hysterisis;
		if (flag || this.testActivate)
		{
			this.testActivate = false;
			if (this.CanActivate())
			{
				this.OnActivate();
				return;
			}
		}
		else if (flag2 || this.testDeactivate)
		{
			this.testDeactivate = false;
			if (this.CanDeactivate())
			{
				this.OnDeactivate();
			}
		}
	}

	protected virtual void LateUpdateLocal()
	{
		this.wasHover = this.isHover;
		this.isHover = false;
		if (PhotonNetwork.InRoom)
		{
			this.myRig.SetTransferrablePosStates(this.objectIndex, this.currentState);
			this.myRig.SetTransferrableItemStates(this.objectIndex, this.itemState);
		}
		this.targetRig = this.myRig;
		this.HandleLocalInput();
	}

	protected virtual void LateUpdateReplicated()
	{
		this.currentState = this.myOnlineRig.TransferrablePosStates(this.objectIndex);
		if (this.currentState == TransferrableObject.PositionState.Dropped && !this.canDrop && !this.shareable)
		{
			if (this.previousState == TransferrableObject.PositionState.None)
			{
				base.gameObject.SetActive(false);
			}
			this.currentState = this.previousState;
		}
		this.itemState = this.myOnlineRig.TransferrableItemStates(this.objectIndex);
		this.targetRig = this.myOnlineRig;
		if (this.myOnlineRig != null)
		{
			bool flag = true;
			for (int i = 0; i < this.myOnlineRig.ActiveTransferrableObjectIndexLength(); i++)
			{
				if (this.myOnlineRig.ActiveTransferrableObjectIndex(i) == this.myIndex)
				{
					flag = false;
					GameObject[] array = this.gameObjectsActiveOnlyWhileHeld;
					for (int j = 0; j < array.Length; j++)
					{
						array[j].SetActive(this.InHand());
					}
				}
			}
			if (flag)
			{
				base.gameObject.SetActive(false);
			}
		}
	}

	public virtual void ResetToDefaultState()
	{
		this.canAutoGrabLeft = true;
		this.canAutoGrabRight = true;
		this.wasHover = false;
		this.isHover = false;
		this.ResetXf();
	}

	public override void OnGrab(InteractionPoint pointGrabbed, GameObject grabbingHand)
	{
		if (!this.IsMyItem())
		{
			return;
		}
		if (!(grabbingHand == this.interactor.leftHand) || this.currentState == TransferrableObject.PositionState.OnLeftArm)
		{
			if (grabbingHand == this.interactor.rightHand && this.currentState != TransferrableObject.PositionState.OnRightArm)
			{
				if (this.currentState == TransferrableObject.PositionState.InLeftHand && this.disableStealing)
				{
					return;
				}
				this.canAutoGrabRight = false;
				this.currentState = TransferrableObject.PositionState.InRightHand;
				EquipmentInteractor.instance.UpdateHandEquipment(this, false);
				GorillaTagger.Instance.StartVibration(false, GorillaTagger.Instance.tapHapticStrength / 8f, GorillaTagger.Instance.tapHapticDuration * 0.5f);
			}
			return;
		}
		if (this.currentState == TransferrableObject.PositionState.InRightHand && this.disableStealing)
		{
			return;
		}
		this.canAutoGrabLeft = false;
		this.currentState = TransferrableObject.PositionState.InLeftHand;
		EquipmentInteractor.instance.UpdateHandEquipment(this, true);
		GorillaTagger.Instance.StartVibration(true, GorillaTagger.Instance.tapHapticStrength / 8f, GorillaTagger.Instance.tapHapticDuration * 0.5f);
	}

	public override bool OnRelease(DropZone zoneReleased, GameObject releasingHand)
	{
		if (!this.IsMyItem())
		{
			return false;
		}
		if (!this.CanDeactivate())
		{
			return false;
		}
		if (this.IsHeld() && ((releasingHand == EquipmentInteractor.instance.rightHand && EquipmentInteractor.instance.rightHandHeldEquipment != null && this == (LegacyTransferrableObject)EquipmentInteractor.instance.rightHandHeldEquipment) || (releasingHand == EquipmentInteractor.instance.leftHand && EquipmentInteractor.instance.leftHandHeldEquipment != null && this == (LegacyTransferrableObject)EquipmentInteractor.instance.leftHandHeldEquipment)))
		{
			if (releasingHand == EquipmentInteractor.instance.leftHand)
			{
				this.canAutoGrabLeft = true;
			}
			else
			{
				this.canAutoGrabRight = true;
			}
			if (zoneReleased != null)
			{
				bool flag = this.currentState == TransferrableObject.PositionState.InLeftHand && zoneReleased.dropPosition == BodyDockPositions.DropPositions.LeftArm;
				bool flag2 = this.currentState == TransferrableObject.PositionState.InRightHand && zoneReleased.dropPosition == BodyDockPositions.DropPositions.RightArm;
				if (flag || flag2)
				{
					return false;
				}
				if (this.targetDock.DropZoneStorageUsed(zoneReleased.dropPosition) == -1 && zoneReleased.forBodyDock == this.targetDock && (zoneReleased.dropPosition & this.dockPositions) != BodyDockPositions.DropPositions.None)
				{
					this.storedZone = zoneReleased.dropPosition;
				}
			}
			this.DropItemCleanup();
			EquipmentInteractor.instance.UpdateHandEquipment(null, releasingHand == EquipmentInteractor.instance.leftHand);
			return true;
		}
		return false;
	}

	public override void DropItemCleanup()
	{
		if (this.canDrop)
		{
			this.currentState = TransferrableObject.PositionState.Dropped;
			return;
		}
		BodyDockPositions.DropPositions dropPositions = this.storedZone;
		switch (dropPositions)
		{
		case BodyDockPositions.DropPositions.LeftArm:
			this.currentState = TransferrableObject.PositionState.OnLeftArm;
			return;
		case BodyDockPositions.DropPositions.RightArm:
			this.currentState = TransferrableObject.PositionState.OnRightArm;
			return;
		case BodyDockPositions.DropPositions.LeftArm | BodyDockPositions.DropPositions.RightArm:
			break;
		case BodyDockPositions.DropPositions.Chest:
			this.currentState = TransferrableObject.PositionState.OnChest;
			return;
		default:
			if (dropPositions == BodyDockPositions.DropPositions.LeftBack)
			{
				this.currentState = TransferrableObject.PositionState.OnLeftShoulder;
				return;
			}
			if (dropPositions != BodyDockPositions.DropPositions.RightBack)
			{
				return;
			}
			this.currentState = TransferrableObject.PositionState.OnRightShoulder;
			break;
		}
	}

	public override void OnHover(InteractionPoint pointHovered, GameObject hoveringHand)
	{
		if (!this.IsMyItem())
		{
			return;
		}
		if (!this.wasHover)
		{
			GorillaTagger.Instance.StartVibration(hoveringHand == EquipmentInteractor.instance.leftHand, GorillaTagger.Instance.tapHapticStrength / 8f, GorillaTagger.Instance.tapHapticDuration * 0.5f);
		}
		this.isHover = true;
	}

	protected void ActivateItemFX(float hapticStrength, float hapticDuration, int soundIndex, float soundVolume)
	{
		bool flag = this.currentState == TransferrableObject.PositionState.InLeftHand;
		VRRig vrrig = this.targetRig;
		if ((vrrig != null) ? vrrig.netView : null)
		{
			this.targetRig.rigSerializer.RPC_PlayHandTap(soundIndex, flag, 0.1f, default(PhotonMessageInfo));
		}
		this.myRig.PlayHandTapLocal(soundIndex, flag, soundVolume);
		GorillaTagger.Instance.StartVibration(flag, hapticStrength, hapticDuration);
	}

	public virtual void PlayNote(int note, float volume)
	{
	}

	public virtual bool AutoGrabTrue(bool leftGrabbingHand)
	{
		if (!leftGrabbingHand)
		{
			return this.canAutoGrabRight;
		}
		return this.canAutoGrabLeft;
	}

	public virtual bool CanActivate()
	{
		return true;
	}

	public virtual bool CanDeactivate()
	{
		return true;
	}

	public virtual void OnActivate()
	{
		this.latched = true;
	}

	public virtual void OnDeactivate()
	{
		this.latched = false;
	}

	public virtual bool IsMyItem()
	{
		return this.myRig != null && this.myRig.isOfflineVRRig;
	}

	protected virtual bool IsHeld()
	{
		return (EquipmentInteractor.instance.leftHandHeldEquipment != null && (LegacyTransferrableObject)EquipmentInteractor.instance.leftHandHeldEquipment == this) || (EquipmentInteractor.instance.rightHandHeldEquipment != null && (LegacyTransferrableObject)EquipmentInteractor.instance.rightHandHeldEquipment == this);
	}

	public bool InHand()
	{
		return this.currentState == TransferrableObject.PositionState.InLeftHand || this.currentState == TransferrableObject.PositionState.InRightHand;
	}

	public bool Dropped()
	{
		return this.currentState == TransferrableObject.PositionState.Dropped;
	}

	public bool InLeftHand()
	{
		return this.currentState == TransferrableObject.PositionState.InLeftHand;
	}

	public bool InRightHand()
	{
		return this.currentState == TransferrableObject.PositionState.InRightHand;
	}

	public bool OnChest()
	{
		return this.currentState == TransferrableObject.PositionState.OnChest;
	}

	public bool OnShoulder()
	{
		return this.currentState == TransferrableObject.PositionState.OnLeftShoulder || this.currentState == TransferrableObject.PositionState.OnRightShoulder;
	}

	protected NetPlayer OwningPlayer()
	{
		if (this.myRig == null)
		{
			return this.myOnlineRig.netView.Owner;
		}
		return NetworkSystem.Instance.LocalPlayer;
	}

	protected EquipmentInteractor interactor;

	public VRRig myRig;

	public VRRig myOnlineRig;

	public bool latched;

	private float indexTrigger;

	public bool testActivate;

	public bool testDeactivate;

	public float myThreshold = 0.8f;

	public float hysterisis = 0.05f;

	public bool flipOnXForLeftHand;

	public bool flipOnYForLeftHand;

	public bool flipOnXForLeftArm;

	public bool disableStealing;

	private TransferrableObject.PositionState initState;

	public TransferrableObject.ItemStates itemState;

	public BodyDockPositions.DropPositions storedZone;

	protected TransferrableObject.PositionState previousState;

	public TransferrableObject.PositionState currentState;

	public BodyDockPositions.DropPositions dockPositions;

	public VRRig targetRig;

	public BodyDockPositions targetDock;

	private VRRigAnchorOverrides anchorOverrides;

	public bool canAutoGrabLeft;

	public bool canAutoGrabRight;

	public int objectIndex;

	[Tooltip("In Holdables.prefab, assign to the parent of this transform.\nExample: 'Holdables/YellowHandBootsRight' is the anchor of 'Holdables/YellowHandBootsRight/YELLOW HAND BOOTS'")]
	public Transform anchor;

	[Tooltip("In Holdables.prefab, assign to the Collider to grab this object")]
	public InteractionPoint gripInteractor;

	[Tooltip("(Optional) Use this to override the transform used when the object is in the hand.\nExample: 'GHOST BALLOON' uses child 'grabPtAnchor' which is the end of the balloon's string.")]
	public Transform grabAnchor;

	public int myIndex;

	[Tooltip("(Optional)")]
	public GameObject[] gameObjectsActiveOnlyWhileHeld;

	protected GameObject worldShareableInstance;

	private float interpTime = 0.1f;

	private float interpDt;

	private Vector3 interpStartPos;

	private Quaternion interpStartRot;

	protected int enabledOnFrame = -1;

	private Vector3 initOffset;

	private Quaternion initRotation;

	public bool canDrop;

	public bool shareable;

	public bool detatchOnGrab;

	private bool wasHover;

	private bool isHover;

	private bool disableItem;

	public const int kPositionStateCount = 8;

	public LegacyTransferrableObject.InterpolateState interpState;

	public enum InterpolateState
	{
		None,
		Interpolating
	}
}
