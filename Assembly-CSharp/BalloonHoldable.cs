using System;
using GorillaExtensions;
using GorillaLocomotion.Swimming;
using GorillaTag;
using Photon.Pun;
using UnityEngine;

public class BalloonHoldable : TransferrableObject, IFXContext
{
	public override void OnSpawn(VRRig rig)
	{
		base.OnSpawn(rig);
		this.balloonDynamics = base.GetComponent<ITetheredObjectBehavior>();
		if (this.mesh == null)
		{
			this.mesh = base.GetComponent<Renderer>();
		}
		this.lineRenderer = base.GetComponent<LineRenderer>();
		this.itemState = (TransferrableObject.ItemStates)0;
		this.rb = base.GetComponent<Rigidbody>();
	}

	protected override void Start()
	{
		base.Start();
		this.EnableDynamics(false, false, false);
	}

	internal override void OnEnable()
	{
		base.OnEnable();
		this.EnableDynamics(false, false, false);
		this.mesh.enabled = true;
		this.lineRenderer.enabled = false;
		if (NetworkSystem.Instance.InRoom)
		{
			if (this.worldShareableInstance != null)
			{
				return;
			}
			base.SpawnTransferableObjectViews();
		}
		if (base.InHand())
		{
			this.Grab();
			return;
		}
		if (base.Dropped())
		{
			this.Release();
		}
	}

	public override void OnJoinedRoom()
	{
		base.OnJoinedRoom();
		if (this.worldShareableInstance != null)
		{
			return;
		}
		base.SpawnTransferableObjectViews();
	}

	private bool ShouldSimulate()
	{
		return !base.Attached() && this.balloonState == BalloonHoldable.BalloonStates.Normal;
	}

	internal override void OnDisable()
	{
		base.OnDisable();
		this.lineRenderer.enabled = false;
		this.EnableDynamics(false, false, false);
	}

	public override void PreDisable()
	{
		this.originalOwner = null;
		base.PreDisable();
	}

	public override void ResetToDefaultState()
	{
		base.ResetToDefaultState();
		this.balloonState = BalloonHoldable.BalloonStates.Normal;
		base.transform.localScale = Vector3.one;
	}

	protected override void OnWorldShareableItemSpawn()
	{
		WorldShareableItem worldShareableInstance = this.worldShareableInstance;
		if (worldShareableInstance != null)
		{
			worldShareableInstance.rpcCallBack = new Action(this.PopBalloonRemote);
			worldShareableInstance.onOwnerChangeCb = new WorldShareableItem.OnOwnerChangeDelegate(this.OnOwnerChangeCb);
			worldShareableInstance.EnableRemoteSync = this.ShouldSimulate();
		}
		this.originalOwner = worldShareableInstance.target.owner;
	}

	public override void ResetToHome()
	{
		if (base.IsLocalObject() && this.worldShareableInstance != null && !this.worldShareableInstance.guard.isTrulyMine)
		{
			PhotonView photonView = PhotonView.Get(this.worldShareableInstance);
			if (photonView != null)
			{
				photonView.RPC("RPCWorldShareable", RpcTarget.Others, Array.Empty<object>());
			}
			this.worldShareableInstance.guard.RequestOwnershipImmediatelyWithGuaranteedAuthority();
		}
		this.PopBalloon();
		this.balloonState = BalloonHoldable.BalloonStates.WaitForReDock;
		base.ResetToHome();
	}

	protected override void PlayDestroyedOrDisabledEffect()
	{
		base.PlayDestroyedOrDisabledEffect();
		this.PlayPopBalloonFX();
	}

	protected override void OnItemDestroyedOrDisabled()
	{
		this.PlayPopBalloonFX();
		if (this.balloonDynamics != null)
		{
			this.balloonDynamics.ReParent();
		}
		base.transform.parent = base.DefaultAnchor();
		base.OnItemDestroyedOrDisabled();
	}

	private void PlayPopBalloonFX()
	{
		FXSystem.PlayFXForRig(FXType.BalloonPop, this, default(PhotonMessageInfoWrapped));
	}

	private void EnableDynamics(bool enable, bool collider, bool forceKinematicOn = false)
	{
		bool flag = false;
		if (forceKinematicOn)
		{
			flag = true;
		}
		else if (NetworkSystem.Instance.InRoom && this.worldShareableInstance != null)
		{
			PhotonView photonView = PhotonView.Get(this.worldShareableInstance.gameObject);
			if (photonView != null && !photonView.IsMine)
			{
				flag = true;
			}
		}
		if (this.balloonDynamics != null)
		{
			this.balloonDynamics.EnableDynamics(enable, collider, flag);
		}
	}

	private void PopBalloon()
	{
		this.PlayPopBalloonFX();
		this.EnableDynamics(false, false, false);
		this.mesh.enabled = false;
		this.lineRenderer.enabled = false;
		if (this.gripInteractor != null)
		{
			this.gripInteractor.gameObject.SetActive(false);
		}
		if ((object.Equals(this.originalOwner, PhotonNetwork.LocalPlayer) || !NetworkSystem.Instance.InRoom) && NetworkSystem.Instance.InRoom && this.worldShareableInstance != null && !this.worldShareableInstance.guard.isTrulyMine)
		{
			this.worldShareableInstance.guard.RequestOwnershipImmediatelyWithGuaranteedAuthority();
		}
		if (this.balloonDynamics != null)
		{
			this.balloonDynamics.ReParent();
			this.EnableDynamics(false, false, false);
		}
		if (this.IsMyItem())
		{
			if (base.InLeftHand())
			{
				EquipmentInteractor.instance.ReleaseLeftHand();
			}
			if (base.InRightHand())
			{
				EquipmentInteractor.instance.ReleaseRightHand();
			}
		}
	}

	public void PopBalloonRemote()
	{
		if (this.ShouldSimulate())
		{
			this.balloonState = BalloonHoldable.BalloonStates.Pop;
		}
	}

	public void OnOwnerChangeCb(NetPlayer newOwner, NetPlayer prevOwner)
	{
	}

	public override void OnOwnershipTransferred(NetPlayer newOwner, NetPlayer prevOwner)
	{
		base.OnOwnershipTransferred(newOwner, prevOwner);
		if (object.Equals(prevOwner, NetworkSystem.Instance.LocalPlayer) && newOwner == null)
		{
			return;
		}
		if (!object.Equals(newOwner, NetworkSystem.Instance.LocalPlayer))
		{
			this.EnableDynamics(false, true, true);
			return;
		}
		if (this.ShouldSimulate() && this.balloonDynamics != null)
		{
			this.balloonDynamics.EnableDynamics(true, true, false);
		}
		if (!this.rb)
		{
			return;
		}
		if (!this.rb.isKinematic)
		{
			this.rb.AddForceAtPosition(this.forceAppliedAsRemote * this.rb.mass, this.collisionPtAsRemote, ForceMode.Impulse);
		}
		this.forceAppliedAsRemote = Vector3.zero;
		this.collisionPtAsRemote = Vector3.zero;
	}

	private void OwnerPopBalloon()
	{
		if (this.worldShareableInstance != null)
		{
			PhotonView photonView = PhotonView.Get(this.worldShareableInstance);
			if (photonView != null)
			{
				photonView.RPC("RPCWorldShareable", RpcTarget.Others, Array.Empty<object>());
			}
		}
		this.balloonState = BalloonHoldable.BalloonStates.Pop;
	}

	private void RunLocalPopSM()
	{
		switch (this.balloonState)
		{
		case BalloonHoldable.BalloonStates.Normal:
			break;
		case BalloonHoldable.BalloonStates.Pop:
			this.timer = Time.time;
			this.PopBalloon();
			this.balloonState = BalloonHoldable.BalloonStates.WaitForOwnershipTransfer;
			this.lastOwnershipRequest = Time.time;
			return;
		case BalloonHoldable.BalloonStates.Waiting:
			if (Time.time - this.timer >= this.poppedTimerLength)
			{
				this.timer = Time.time;
				this.mesh.enabled = true;
				this.balloonInflatSource.GTPlay();
				this.balloonState = BalloonHoldable.BalloonStates.Refilling;
				return;
			}
			base.transform.localScale = new Vector3(this.beginScale, this.beginScale, this.beginScale);
			return;
		case BalloonHoldable.BalloonStates.WaitForOwnershipTransfer:
			if (!NetworkSystem.Instance.InRoom)
			{
				this.balloonState = BalloonHoldable.BalloonStates.WaitForReDock;
				base.ReDock();
				return;
			}
			if (this.worldShareableInstance != null)
			{
				WorldShareableItem worldShareableInstance = this.worldShareableInstance;
				NetPlayer owner = worldShareableInstance.Owner;
				if (worldShareableInstance != null && owner == this.originalOwner)
				{
					this.balloonState = BalloonHoldable.BalloonStates.WaitForReDock;
					base.ReDock();
				}
				if (base.IsLocalObject() && this.lastOwnershipRequest + 5f < Time.time)
				{
					this.worldShareableInstance.guard.RequestOwnershipImmediatelyWithGuaranteedAuthority();
					this.lastOwnershipRequest = Time.time;
					return;
				}
			}
			break;
		case BalloonHoldable.BalloonStates.WaitForReDock:
			if (base.Attached())
			{
				this.fullyInflatedScale = base.transform.localScale;
				base.ReDock();
				this.balloonState = BalloonHoldable.BalloonStates.Waiting;
				return;
			}
			break;
		case BalloonHoldable.BalloonStates.Refilling:
		{
			float num = Time.time - this.timer;
			if (num >= this.scaleTimerLength)
			{
				base.transform.localScale = this.fullyInflatedScale;
				this.balloonState = BalloonHoldable.BalloonStates.Normal;
				if (this.gripInteractor != null)
				{
					this.gripInteractor.gameObject.SetActive(true);
				}
			}
			num = Mathf.Clamp01(num / this.scaleTimerLength);
			float num2 = Mathf.Lerp(this.beginScale, 1f, num);
			base.transform.localScale = this.fullyInflatedScale * num2;
			return;
		}
		case BalloonHoldable.BalloonStates.Returning:
			if (this.balloonDynamics.ReturnStep())
			{
				this.balloonState = BalloonHoldable.BalloonStates.Normal;
				base.ReDock();
			}
			break;
		default:
			return;
		}
	}

	protected override void OnStateChanged()
	{
		if (base.InHand())
		{
			this.Grab();
			return;
		}
		if (base.Dropped())
		{
			this.Release();
			return;
		}
		if (base.OnShoulder())
		{
			if (this.balloonDynamics != null && this.balloonDynamics.IsEnabled())
			{
				this.EnableDynamics(false, false, false);
			}
			this.lineRenderer.enabled = false;
		}
	}

	protected override void LateUpdateShared()
	{
		base.LateUpdateShared();
		if (Time.frameCount == this.enabledOnFrame)
		{
			this.OnStateChanged();
		}
		if (base.InHand() && this.detatchOnGrab)
		{
			float num = ((this.targetRig != null) ? this.targetRig.transform.localScale.x : 1f);
			base.transform.localScale = Vector3.one * num;
		}
		if (base.Dropped() && this.balloonState == BalloonHoldable.BalloonStates.Normal && this.maxDistanceFromOwner > 0f && (!NetworkSystem.Instance.InRoom || this.originalOwner.IsLocal) && (VRRig.LocalRig.transform.position - base.transform.position).IsLongerThan(this.maxDistanceFromOwner * base.transform.localScale.x))
		{
			this.OwnerPopBalloon();
		}
		if (this.worldShareableInstance != null && !this.worldShareableInstance.guard.isMine)
		{
			this.worldShareableInstance.EnableRemoteSync = this.ShouldSimulate();
		}
		if (this.balloonState != BalloonHoldable.BalloonStates.Normal)
		{
			this.RunLocalPopSM();
		}
	}

	protected override void LateUpdateReplicated()
	{
		base.LateUpdateReplicated();
	}

	private void Grab()
	{
		if (this.balloonDynamics == null)
		{
			return;
		}
		if (this.detatchOnGrab)
		{
			float num = ((this.targetRig != null) ? this.targetRig.transform.localScale.x : 1f);
			base.transform.localScale = Vector3.one * num;
			this.EnableDynamics(true, true, false);
			this.balloonDynamics.EnableDistanceConstraints(true, num);
			this.lineRenderer.enabled = true;
			return;
		}
		base.transform.localScale = Vector3.one;
	}

	private void Release()
	{
		if (this.disableRelease)
		{
			this.balloonState = BalloonHoldable.BalloonStates.Returning;
			return;
		}
		if (this.balloonDynamics == null)
		{
			return;
		}
		float num = ((this.targetRig != null) ? this.targetRig.transform.localScale.x : 1f);
		base.transform.localScale = Vector3.one * num;
		this.EnableDynamics(true, true, false);
		this.balloonDynamics.EnableDistanceConstraints(false, num);
		this.lineRenderer.enabled = false;
	}

	public void OnTriggerEnter(Collider other)
	{
		if (!this.ShouldSimulate())
		{
			return;
		}
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		bool flag = false;
		if (this.balloonDynamics != null)
		{
			this.balloonDynamics.TriggerEnter(other, ref zero, ref zero2, ref flag);
		}
		if (!NetworkSystem.Instance.InRoom)
		{
			return;
		}
		if (this.worldShareableInstance == null)
		{
			return;
		}
		if (flag)
		{
			RequestableOwnershipGuard component = PhotonView.Get(this.worldShareableInstance.gameObject).GetComponent<RequestableOwnershipGuard>();
			if (!component.isTrulyMine)
			{
				if (zero.magnitude > this.forceAppliedAsRemote.magnitude)
				{
					this.forceAppliedAsRemote = zero;
					this.collisionPtAsRemote = zero2;
				}
				component.RequestOwnershipImmediately(delegate
				{
				});
			}
		}
	}

	public void OnCollisionEnter(Collision collision)
	{
		if (!this.ShouldSimulate() || this.disableCollisionHandling)
		{
			return;
		}
		this.balloonBopSource.GTPlay();
		if (!collision.gameObject.IsOnLayer(UnityLayer.GorillaThrowable))
		{
			return;
		}
		if (!NetworkSystem.Instance.InRoom)
		{
			this.OwnerPopBalloon();
			return;
		}
		if (this.worldShareableInstance == null)
		{
			return;
		}
		if (PhotonView.Get(this.worldShareableInstance.gameObject).IsMine)
		{
			this.OwnerPopBalloon();
		}
	}

	FXSystemSettings IFXContext.settings
	{
		get
		{
			return this.ownerRig.fxSettings;
		}
	}

	void IFXContext.OnPlayFX()
	{
		GameObject gameObject = ObjectPools.instance.Instantiate(this.balloonPopFXPrefab, true);
		gameObject.transform.SetPositionAndRotation(base.transform.position, base.transform.rotation);
		GorillaColorizableBase componentInChildren = gameObject.GetComponentInChildren<GorillaColorizableBase>();
		if (componentInChildren != null)
		{
			componentInChildren.SetColor(this.balloonPopFXColor);
		}
	}

	private ITetheredObjectBehavior balloonDynamics;

	[SerializeField]
	private Renderer mesh;

	private LineRenderer lineRenderer;

	private Rigidbody rb;

	private NetPlayer originalOwner;

	public GameObject balloonPopFXPrefab;

	public Color balloonPopFXColor;

	private float timer;

	public float scaleTimerLength = 2f;

	public float poppedTimerLength = 2.5f;

	public float beginScale = 0.1f;

	public float bopSpeed = 1f;

	private Vector3 fullyInflatedScale;

	public AudioSource balloonBopSource;

	public AudioSource balloonInflatSource;

	private Vector3 forceAppliedAsRemote;

	private Vector3 collisionPtAsRemote;

	private WaterVolume waterVolume;

	[DebugReadout]
	private BalloonHoldable.BalloonStates balloonState;

	private float returnTimer;

	[SerializeField]
	private float maxDistanceFromOwner;

	public float lastOwnershipRequest;

	[SerializeField]
	private bool disableCollisionHandling;

	[SerializeField]
	private bool disableRelease;

	private enum BalloonStates
	{
		Normal,
		Pop,
		Waiting,
		WaitForOwnershipTransfer,
		WaitForReDock,
		Refilling,
		Returning
	}
}
