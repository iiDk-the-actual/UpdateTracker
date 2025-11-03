using System;
using ExitGames.Client.Photon;
using GorillaExtensions;
using GorillaLocomotion;
using GorillaLocomotion.Climbing;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PaperPlaneThrowable : TransferrableObject
{
	private void OnLaunchRPC(int sender, int receiver, object[] args, PhotonMessageInfoWrapped info)
	{
		if (info.senderID != this.ownerRig.creator.ActorNumber)
		{
			return;
		}
		GorillaNot.IncrementRPCCall(info, "OnLaunchRPC");
		if (sender != receiver)
		{
			return;
		}
		if (!this)
		{
			return;
		}
		int num = PaperPlaneThrowable.FetchViewID(this);
		int num2 = (int)args[0];
		if (num == -1)
		{
			return;
		}
		if (num2 == -1)
		{
			return;
		}
		if (num != num2)
		{
			return;
		}
		int num3 = (int)args[1];
		int throwableId = this.GetThrowableId();
		if (num3 != throwableId)
		{
			return;
		}
		Vector3 vector = (Vector3)args[2];
		Quaternion quaternion = (Quaternion)args[3];
		Vector3 vector2 = (Vector3)args[4];
		float num4 = 10000f;
		if ((in vector).IsValid(in num4) && (in quaternion).IsValid())
		{
			float num5 = 10000f;
			if ((in vector2).IsValid(in num5) && !this._renderer.forceRenderingOff)
			{
				this.LaunchProjectileLocal(vector, quaternion, vector2);
				return;
			}
		}
	}

	internal override void OnEnable()
	{
		PhotonNetwork.NetworkingClient.EventReceived += this.OnPhotonEvent;
		this._lastWorldPos = base.transform.position;
		this._renderer.forceRenderingOff = false;
		base.OnEnable();
	}

	internal override void OnDisable()
	{
		PhotonNetwork.NetworkingClient.EventReceived -= this.OnPhotonEvent;
		base.OnDisable();
	}

	private void OnPhotonEvent(EventData evData)
	{
		if (evData.Code != 176)
		{
			return;
		}
		object[] array = (object[])evData.CustomData;
		object obj = array[0];
		if (!(obj is int))
		{
			return;
		}
		int num = (int)obj;
		if (num != PaperPlaneThrowable.kProjectileEvent)
		{
			return;
		}
		NetPlayer player = NetworkSystem.Instance.GetPlayer(evData.Sender);
		NetPlayer netPlayer = base.OwningPlayer();
		if (player != netPlayer)
		{
			return;
		}
		GorillaNot.IncrementRPCCall(new PhotonMessageInfo(netPlayer.GetPlayerRef(), PhotonNetwork.ServerTimestamp, null), "OnPhotonEvent");
		if (!this.m_spamCheck.CheckCallTime(Time.unscaledTime))
		{
			return;
		}
		TransferrableObject.PositionState positionState = (TransferrableObject.PositionState)array[1];
		Vector3 vector = (Vector3)array[2];
		Quaternion quaternion = (Quaternion)array[3];
		Vector3 vector2 = (Vector3)array[4];
		TransferrableObject.PositionState positionState2 = positionState;
		if (positionState2 != TransferrableObject.PositionState.InLeftHand)
		{
			if (positionState2 != TransferrableObject.PositionState.InRightHand)
			{
				goto IL_00CE;
			}
			if (base.InRightHand())
			{
				goto IL_00CE;
			}
		}
		else if (base.InLeftHand())
		{
			goto IL_00CE;
		}
		return;
		IL_00CE:
		float num2 = 10000f;
		if ((in vector).IsValid(in num2) && (in quaternion).IsValid())
		{
			float num3 = 10000f;
			if ((in vector2).IsValid(in num3) && !this._renderer.forceRenderingOff && !base.myOnlineRig.IsNull() && base.myOnlineRig.IsPositionInRange(vector, 4f))
			{
				this.LaunchProjectileLocal(vector, quaternion, vector2);
				return;
			}
		}
	}

	protected override void Start()
	{
		base.Start();
		if (PaperPlaneThrowable._playerView == null)
		{
			PaperPlaneThrowable._playerView = Camera.main;
		}
	}

	public override void OnGrab(InteractionPoint pointGrabbed, GameObject grabbingHand)
	{
		if (this._renderer.forceRenderingOff)
		{
			return;
		}
		base.OnGrab(pointGrabbed, grabbingHand);
	}

	private static int FetchViewID(PaperPlaneThrowable ppt)
	{
		NetPlayer netPlayer = ((ppt.myOnlineRig != null) ? ppt.myOnlineRig.creator : ((ppt.myRig != null) ? ((ppt.myRig.creator != null) ? ppt.myRig.creator : NetworkSystem.Instance.LocalPlayer) : null));
		if (netPlayer == null)
		{
			return -1;
		}
		RigContainer rigContainer;
		if (!VRRigCache.Instance.TryGetVrrig(netPlayer, out rigContainer))
		{
			return -1;
		}
		if (rigContainer.Rig.netView == null)
		{
			return -1;
		}
		return rigContainer.Rig.netView.ViewID;
	}

	public override bool OnRelease(DropZone zoneReleased, GameObject releasingHand)
	{
		TransferrableObject.PositionState currentState = this.currentState;
		if (!base.OnRelease(zoneReleased, releasingHand))
		{
			return false;
		}
		if (VRRigCache.Instance.localRig.Rig != this.ownerRig)
		{
			return false;
		}
		if (this._renderer.forceRenderingOff)
		{
			return false;
		}
		bool flag = releasingHand == EquipmentInteractor.instance.leftHand;
		GorillaVelocityTracker interactPointVelocityTracker = GTPlayer.Instance.GetInteractPointVelocityTracker(flag);
		Vector3 vector = base.transform.TransformPoint(Vector3.zero);
		Quaternion rotation = base.transform.rotation;
		Vector3 averageVelocity = interactPointVelocityTracker.GetAverageVelocity(true, 0.15f, false);
		PaperPlaneThrowable.FetchViewID(this);
		this.GetThrowableId();
		this.LaunchProjectileLocal(vector, rotation, averageVelocity);
		if (PaperPlaneThrowable.gRaiseOpts == null)
		{
			PaperPlaneThrowable.gRaiseOpts = RaiseEventOptions.Default;
			PaperPlaneThrowable.gRaiseOpts.Receivers = ReceiverGroup.Others;
		}
		PaperPlaneThrowable.gEventArgs[0] = PaperPlaneThrowable.kProjectileEvent;
		PaperPlaneThrowable.gEventArgs[1] = currentState;
		PaperPlaneThrowable.gEventArgs[2] = vector;
		PaperPlaneThrowable.gEventArgs[3] = rotation;
		PaperPlaneThrowable.gEventArgs[4] = averageVelocity;
		PhotonNetwork.RaiseEvent(176, PaperPlaneThrowable.gEventArgs, PaperPlaneThrowable.gRaiseOpts, SendOptions.SendReliable);
		return true;
	}

	private int GetThrowableId()
	{
		int num = this._throwableIdHash.GetValueOrDefault();
		if (this._throwableIdHash == null)
		{
			num = StaticHash.Compute(this._throwableID);
			this._throwableIdHash = new int?(num);
			return num;
		}
		return num;
	}

	private void LaunchProjectileLocal(Vector3 launchPos, Quaternion launchRot, Vector3 releaseVel)
	{
		if (releaseVel.sqrMagnitude <= this.minThrowSpeed * base.transform.lossyScale.z * base.transform.lossyScale.z)
		{
			return;
		}
		GameObject gameObject = ObjectPools.instance.Instantiate(this._projectilePrefab.gameObject, launchPos, true);
		gameObject.transform.localScale = base.transform.lossyScale;
		PaperPlaneProjectile component = gameObject.GetComponent<PaperPlaneProjectile>();
		component.OnHit += this.OnProjectileHit;
		if (this.networkedStateEvents != TransferrableObject.SyncOptions.None)
		{
			int num = (int)(this.itemState & (TransferrableObject.ItemStates)(-65));
			component.SetTransferrableState(this.networkedStateEvents, num);
		}
		component.ResetProjectile();
		component.SetVRRig(base.myRig);
		component.Launch(launchPos, launchRot, releaseVel);
		this._renderer.forceRenderingOff = true;
	}

	private void OnProjectileHit(Vector3 endPoint)
	{
		this._renderer.forceRenderingOff = false;
		if (base.IsLocalObject() && this.networkedStateEvents != TransferrableObject.SyncOptions.None && this.resetOnDocked)
		{
			TransferrableObject.SyncOptions networkedStateEvents = this.networkedStateEvents;
			if (networkedStateEvents == TransferrableObject.SyncOptions.Bool)
			{
				base.ResetStateBools();
				return;
			}
			if (networkedStateEvents != TransferrableObject.SyncOptions.Int)
			{
				return;
			}
			base.SetItemStateInt(0);
		}
	}

	protected override void LateUpdateLocal()
	{
		base.LateUpdateLocal();
		Transform transform = base.transform;
		Vector3 position = transform.position;
		this._itemWorldVel = (position - this._lastWorldPos) / Time.deltaTime;
		Quaternion localRotation = transform.localRotation;
		this._itemWorldAngVel = PaperPlaneThrowable.CalcAngularVelocity(this._lastWorldRot, localRotation, Time.deltaTime);
		this._lastWorldRot = localRotation;
		this._lastWorldPos = position;
	}

	private static Vector3 CalcAngularVelocity(Quaternion from, Quaternion to, float dt)
	{
		Vector3 vector = (to * Quaternion.Inverse(from)).eulerAngles;
		if (vector.x > 180f)
		{
			vector.x -= 360f;
		}
		if (vector.y > 180f)
		{
			vector.y -= 360f;
		}
		if (vector.z > 180f)
		{
			vector.z -= 360f;
		}
		vector *= 0.017453292f / dt;
		return vector;
	}

	public override void DropItem()
	{
		base.DropItem();
	}

	[Tooltip("Renderer on the body to disable when spawning the projectile")]
	[SerializeField]
	private Renderer _renderer;

	[Tooltip("Prefab in the Global object pool to spawn when throwing")]
	[SerializeField]
	private GameObject _projectilePrefab;

	[Tooltip("Minimum velocity of the hand required to launch the projectile")]
	[SerializeField]
	private float minThrowSpeed;

	private static Camera _playerView;

	private static PhotonEvent gLaunchRPC;

	private CallLimiterWithCooldown m_spamCheck = new CallLimiterWithCooldown(5f, 4, 1f);

	private static readonly int kProjectileEvent = StaticHash.Compute("PaperPlaneThrowable".GetStaticHash(), "LaunchProjectileLocal".GetStaticHash());

	private static object[] gEventArgs = new object[5];

	private static RaiseEventOptions gRaiseOpts;

	[SerializeField]
	private string _throwableID;

	private int? _throwableIdHash;

	[Space]
	private Vector3 _lastWorldPos;

	private Quaternion _lastWorldRot;

	[Space]
	private Vector3 _itemWorldVel;

	private Vector3 _itemWorldAngVel;
}
