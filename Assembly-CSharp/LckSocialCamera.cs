using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Fusion;
using GorillaExtensions;
using GorillaTag;
using Liv.Lck.GorillaTag;
using Photon.Pun;
using UnityEngine;

[NetworkBehaviourWeaved(1)]
public class LckSocialCamera : NetworkComponent, IGorillaSliceableSimple
{
	[Networked]
	[NetworkedWeaved(0, 1)]
	private unsafe ref LckSocialCamera.CameraData _networkedData
	{
		get
		{
			if (this.Ptr == null)
			{
				throw new InvalidOperationException("Error when accessing LckSocialCamera._networkedData. Networked properties can only be accessed when Spawned() has been called.");
			}
			return ref *(LckSocialCamera.CameraData*)(this.Ptr + 0);
		}
	}

	private LckSocialCamera.CameraState currentState
	{
		get
		{
			return this._localData.currentState;
		}
		set
		{
			this._localData.currentState = value;
			if (base.IsLocallyOwned)
			{
				this.CoconutCamera.SetVisualsActive(false);
				this.CoconutCamera.SetRecordingState(false);
				return;
			}
			this.CoconutCamera.SetVisualsActive(this.visible);
			this.CoconutCamera.SetRecordingState(this.recording);
		}
	}

	private static bool GetFlag(LckSocialCamera.CameraState cameraState, LckSocialCamera.CameraState flag)
	{
		return (cameraState & flag) == flag;
	}

	private static LckSocialCamera.CameraState SetFlag(LckSocialCamera.CameraState cameraState, LckSocialCamera.CameraState flag, bool value)
	{
		if (value)
		{
			cameraState |= flag;
		}
		else
		{
			cameraState &= ~flag;
		}
		return cameraState;
	}

	public bool visible
	{
		get
		{
			return LckSocialCamera.GetFlag(this.currentState, LckSocialCamera.CameraState.Visible);
		}
		set
		{
			this.currentState = LckSocialCamera.SetFlag(this.currentState, LckSocialCamera.CameraState.Visible, value);
		}
	}

	public bool recording
	{
		get
		{
			return LckSocialCamera.GetFlag(this.currentState, LckSocialCamera.CameraState.Recording);
		}
		set
		{
			this.currentState = LckSocialCamera.SetFlag(this.currentState, LckSocialCamera.CameraState.Recording, value);
		}
	}

	public unsafe override void WriteDataFusion()
	{
		*this._networkedData = new LckSocialCamera.CameraData(this._localData.currentState);
	}

	public override void ReadDataFusion()
	{
		if (this.m_isCorrupted)
		{
			return;
		}
		this.ReadDataShared(this._networkedData.currentState);
	}

	protected override void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		stream.SendNext(this.currentState);
	}

	protected override void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (info.Sender != info.photonView.Owner || this.m_isCorrupted)
		{
			return;
		}
		LckSocialCamera.CameraState cameraState = (LckSocialCamera.CameraState)stream.ReceiveNext();
		this.ReadDataShared(cameraState);
	}

	protected override void Awake()
	{
		base.Awake();
		if (this.m_rigNetworkController.IsNull())
		{
			this.m_rigNetworkController = base.GetComponentInParent<VRRigSerializer>();
		}
		if (this.m_rigNetworkController.IsNull())
		{
			return;
		}
		ListProcessor<InAction<RigContainer, PhotonMessageInfoWrapped>> succesfullSpawnEvent = this.m_rigNetworkController.SuccesfullSpawnEvent;
		InAction<RigContainer, PhotonMessageInfoWrapped> inAction = new InAction<RigContainer, PhotonMessageInfoWrapped>(this.OnSuccesfullSpawn);
		succesfullSpawnEvent.Add(in inAction);
	}

	private void OnDestroy()
	{
		NetworkBehaviourUtils.InternalOnDestroy(this);
		if (this.m_lckDelegateRegistered)
		{
			LckSocialCameraManager.OnManagerSpawned = (Action<LckSocialCameraManager>)Delegate.Remove(LckSocialCameraManager.OnManagerSpawned, new Action<LckSocialCameraManager>(this.OnManagerSpawned));
		}
	}

	protected override void Start()
	{
	}

	private void OnSuccesfullSpawn(in RigContainer rig, in PhotonMessageInfoWrapped info)
	{
		this._vrrig = rig.Rig;
		LCKSocialCameraFollower lckcoconutCamera = rig.LCKCoconutCamera;
		this._scaleTransform = lckcoconutCamera.ScaleTransform;
		this.CoconutCamera = lckcoconutCamera.CoconutCamera;
		this._visualObjects = lckcoconutCamera.VisualObjects;
		this.m_coconutCamera = lckcoconutCamera;
		this.m_isCorrupted = false;
		if (this._vrrig.isOfflineVRRig)
		{
			LckSocialCameraManager instance = LckSocialCameraManager.Instance;
			if (instance != null)
			{
				instance.SetLckSocialCamera(this);
			}
			else
			{
				LckSocialCameraManager.OnManagerSpawned = (Action<LckSocialCameraManager>)Delegate.Combine(LckSocialCameraManager.OnManagerSpawned, new Action<LckSocialCameraManager>(this.OnManagerSpawned));
				this.m_lckDelegateRegistered = true;
			}
		}
		else
		{
			lckcoconutCamera.SetNetworkController(this);
		}
		this.visible = this.visible;
	}

	private void StoreRigReference()
	{
		RigContainer rigContainer;
		if (base.Owner != null && !base.Owner.IsNull && VRRigCache.Instance.TryGetVrrig(base.Owner, out rigContainer))
		{
			this._vrrig = rigContainer.Rig;
		}
	}

	public void SliceUpdate()
	{
		if (this._vrrig.IsNull())
		{
			return;
		}
		this.CoconutCamera.transform.localScale = Vector3.one * this._vrrig.scaleFactor;
	}

	public new void OnEnable()
	{
		NetworkBehaviourUtils.InternalOnEnable(this);
		base.OnEnable();
		GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
	}

	public new void OnDisable()
	{
		NetworkBehaviourUtils.InternalOnDisable(this);
		base.OnDisable();
		GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
		if (this.m_isCorrupted)
		{
			return;
		}
		if (this.m_coconutCamera.IsNotNull())
		{
			this.m_coconutCamera.RemoveNetworkController(this);
		}
		this._scaleTransform = null;
		this._visualObjects = null;
		this.CoconutCamera = null;
	}

	private void OnManagerSpawned(LckSocialCameraManager manager)
	{
		manager.SetLckSocialCamera(this);
	}

	private void ReadDataShared(LckSocialCamera.CameraState newState)
	{
		this.currentState = newState;
	}

	public void TurnOff()
	{
		this.m_isCorrupted = true;
		base.gameObject.SetActive(false);
	}

	[WeaverGenerated]
	public unsafe override void CopyBackingFieldsToState(bool A_1)
	{
		base.CopyBackingFieldsToState(A_1);
		*this._networkedData = this.__networkedData;
	}

	[WeaverGenerated]
	public unsafe override void CopyStateToBackingFields()
	{
		base.CopyStateToBackingFields();
		this.__networkedData = *this._networkedData;
	}

	[SerializeField]
	private Transform _scaleTransform;

	[SerializeField]
	public CoconutCamera CoconutCamera;

	[SerializeField]
	private List<GameObject> _visualObjects;

	[SerializeField]
	private VRRig _vrrig;

	[SerializeField]
	private VRRigSerializer m_rigNetworkController;

	private LCKSocialCameraFollower m_coconutCamera;

	private bool m_isCorrupted = true;

	private bool m_lckDelegateRegistered;

	private LckSocialCamera.CameraDataLocal _localData;

	[WeaverGenerated]
	[DefaultForProperty("_networkedData", 0, 1)]
	[DrawIf("IsEditorWritable", true, CompareOperator.Equal, DrawIfMode.ReadOnly)]
	private LckSocialCamera.CameraData __networkedData;

	private enum CameraState
	{
		Empty,
		Visible,
		Recording
	}

	[NetworkStructWeaved(1)]
	[StructLayout(LayoutKind.Explicit, Size = 4)]
	private struct CameraData : INetworkStruct
	{
		public CameraData(LckSocialCamera.CameraState currentState)
		{
			this.currentState = currentState;
		}

		[FieldOffset(0)]
		public LckSocialCamera.CameraState currentState;
	}

	private struct CameraDataLocal
	{
		public LckSocialCamera.CameraState currentState;
	}
}
