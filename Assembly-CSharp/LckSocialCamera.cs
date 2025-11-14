using System;
using System.Runtime.InteropServices;
using Fusion;
using GorillaExtensions;
using GorillaTag;
using Liv.Lck.Cosmetics;
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

	public LCKSocialCameraFollower SocialCameraFollower { get; private set; }

	public override void OnSpawned()
	{
		if (base.IsLocallyOwned)
		{
			this._localState = LckSocialCamera.CameraState.Empty;
			this.visible = false;
			this.recording = false;
			this.IsOnNeck = false;
			return;
		}
		if (base.Runner != null)
		{
			LckSocialCamera.CameraState currentState = this._networkedData.currentState;
			this.ApplyVisualState(currentState);
			this._previousRenderedState = currentState;
		}
	}

	public unsafe override void WriteDataFusion()
	{
		*this._networkedData = new LckSocialCamera.CameraData(this._localState);
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
		stream.SendNext(this._localState);
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

	private void ReadDataShared(LckSocialCamera.CameraState newState)
	{
		if (newState != this._previousRenderedState)
		{
			this.ApplyVisualState(newState);
			this._previousRenderedState = newState;
		}
	}

	public bool IsOnNeck
	{
		get
		{
			return LckSocialCamera.GetFlag(base.IsLocallyOwned ? this._localState : this._previousRenderedState, LckSocialCamera.CameraState.OnNeck);
		}
		set
		{
			if (base.IsLocallyOwned)
			{
				this._localState = LckSocialCamera.SetFlag(this._localState, LckSocialCamera.CameraState.OnNeck, value);
			}
		}
	}

	public bool visible
	{
		get
		{
			return LckSocialCamera.GetFlag(base.IsLocallyOwned ? this._localState : this._previousRenderedState, LckSocialCamera.CameraState.Visible);
		}
		set
		{
			if (base.IsLocallyOwned)
			{
				this._localState = LckSocialCamera.SetFlag(this._localState, LckSocialCamera.CameraState.Visible, value);
			}
		}
	}

	public bool recording
	{
		get
		{
			return LckSocialCamera.GetFlag(base.IsLocallyOwned ? this._localState : this._previousRenderedState, LckSocialCamera.CameraState.Recording);
		}
		set
		{
			if (base.IsLocallyOwned)
			{
				this._localState = LckSocialCamera.SetFlag(this._localState, LckSocialCamera.CameraState.Recording, value);
			}
		}
	}

	private void ApplyVisualState(LckSocialCamera.CameraState newState)
	{
		if (this.m_isCorrupted)
		{
			return;
		}
		bool flag = LckSocialCamera.GetFlag(newState, LckSocialCamera.CameraState.Visible);
		bool flag2 = LckSocialCamera.GetFlag(newState, LckSocialCamera.CameraState.Recording);
		bool flag3 = LckSocialCamera.GetFlag(newState, LckSocialCamera.CameraState.OnNeck);
		if (!base.IsLocallyOwned)
		{
			IGtCameraVisuals cameraVisuals = this.m_CameraVisuals;
			if (cameraVisuals != null)
			{
				cameraVisuals.SetNetworkedVisualsActive(flag);
			}
			IGtCameraVisuals cameraVisuals2 = this.m_CameraVisuals;
			if (cameraVisuals2 != null)
			{
				cameraVisuals2.SetRecordingState(flag2);
			}
			if (this.m_cameraType == LckSocialCamera.CameraType.Tablet)
			{
				if (flag3)
				{
					this.SocialCameraFollower.SetParentToRig();
					return;
				}
				this.SocialCameraFollower.SetParentNull();
			}
			return;
		}
		IGtCameraVisuals cameraVisuals3 = this.m_CameraVisuals;
		if (cameraVisuals3 != null)
		{
			cameraVisuals3.SetVisualsActive(false);
		}
		IGtCameraVisuals cameraVisuals4 = this.m_CameraVisuals;
		if (cameraVisuals4 == null)
		{
			return;
		}
		cameraVisuals4.SetRecordingState(false);
	}

	private static bool GetFlag(LckSocialCamera.CameraState currentState, LckSocialCamera.CameraState flag)
	{
		return currentState.HasFlag(flag);
	}

	private static LckSocialCamera.CameraState SetFlag(LckSocialCamera.CameraState currentState, LckSocialCamera.CameraState flag, bool shouldBeSet)
	{
		if (shouldBeSet)
		{
			return currentState | flag;
		}
		return currentState & ~flag;
	}

	protected override void Awake()
	{
		base.Awake();
		if (this.CameraVisuals != null && !this.CameraVisuals.TryGetComponent<IGtCameraVisuals>(out this.m_CameraVisuals))
		{
			Debug.LogError("LCK: LckSocialCamera failed to find IGtCameraVisuals component on CameraVisuals");
		}
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

	public unsafe void SetVisibility(bool isVisible)
	{
		if (!base.Object.HasInputAuthority)
		{
			return;
		}
		LckSocialCamera.CameraData cameraData = *this._networkedData;
		cameraData.currentState = LckSocialCamera.SetFlag(cameraData.currentState, LckSocialCamera.CameraState.Visible, isVisible);
		*this._networkedData = cameraData;
	}

	private void OnSuccesfullSpawn(in RigContainer rig, in PhotonMessageInfoWrapped info)
	{
		this._vrrig = rig.Rig;
		LCKSocialCameraFollower lcksocialCameraFollower = ((this.m_cameraType == LckSocialCamera.CameraType.Cococam) ? rig.LckCococamFollower : rig.LCKTabletFollower);
		this._scaleTransform = lcksocialCameraFollower.ScaleTransform;
		this.CameraVisuals = lcksocialCameraFollower.CameraVisualsRoot;
		this.m_CameraVisuals = this.CameraVisuals.GetComponent<IGtCameraVisuals>();
		if (!base.IsLocallyOwned && lcksocialCameraFollower.GetComponent<ILckCosmeticDependantPlayerIdSupplier>() != null)
		{
			lcksocialCameraFollower.GetComponent<ILckCosmeticDependantPlayerIdSupplier>().UpdatePlayerId();
		}
		this.SocialCameraFollower = lcksocialCameraFollower;
		this.m_isCorrupted = false;
		if (!this._vrrig.isOfflineVRRig)
		{
			lcksocialCameraFollower.SetNetworkController(this);
			return;
		}
		LckSocialCameraManager instance = LckSocialCameraManager.Instance;
		if (!(instance != null))
		{
			LckSocialCameraManager.OnManagerSpawned = (Action<LckSocialCameraManager>)Delegate.Combine(LckSocialCameraManager.OnManagerSpawned, new Action<LckSocialCameraManager>(this.OnManagerSpawned));
			this.m_lckDelegateRegistered = true;
			return;
		}
		LckSocialCamera.CameraType cameraType = this.m_cameraType;
		if (cameraType == LckSocialCamera.CameraType.Cococam)
		{
			instance.SetLckSocialCococamCamera(this);
			return;
		}
		if (cameraType != LckSocialCamera.CameraType.Tablet)
		{
			throw new ArgumentOutOfRangeException();
		}
		instance.SetLckSocialTabletCamera(this);
	}

	public void SliceUpdate()
	{
		if (this._vrrig.IsNull())
		{
			return;
		}
		this.CameraVisuals.transform.localScale = Vector3.one * this._vrrig.scaleFactor;
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
		if (this.SocialCameraFollower.IsNotNull())
		{
			this.SocialCameraFollower.RemoveNetworkController(this);
		}
		this._scaleTransform = null;
		this.CameraVisuals = null;
	}

	private void OnManagerSpawned(LckSocialCameraManager manager)
	{
		LckSocialCamera.CameraType cameraType = this.m_cameraType;
		if (cameraType == LckSocialCamera.CameraType.Cococam)
		{
			manager.SetLckSocialCococamCamera(this);
			return;
		}
		if (cameraType != LckSocialCamera.CameraType.Tablet)
		{
			throw new ArgumentOutOfRangeException();
		}
		manager.SetLckSocialTabletCamera(this);
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
	public GameObject CameraVisuals;

	[SerializeField]
	private VRRig _vrrig;

	[SerializeField]
	private VRRigSerializer m_rigNetworkController;

	[SerializeField]
	private LckSocialCamera.CameraType m_cameraType;

	private bool m_isCorrupted = true;

	private bool m_lckDelegateRegistered;

	private IGtCameraVisuals m_CameraVisuals;

	private LckSocialCamera.CameraState _localState;

	private LckSocialCamera.CameraState _previousRenderedState;

	[WeaverGenerated]
	[DefaultForProperty("_networkedData", 0, 1)]
	[DrawIf("IsEditorWritable", true, CompareOperator.Equal, DrawIfMode.ReadOnly)]
	private LckSocialCamera.CameraData __networkedData;

	private enum CameraState
	{
		Empty,
		Visible,
		Recording,
		OnNeck = 4
	}

	private enum CameraType
	{
		Cococam,
		Tablet
	}

	[NetworkStructWeaved(1)]
	[StructLayout(LayoutKind.Explicit, Size = 4)]
	private struct CameraData : INetworkStruct
	{
		public CameraData(LckSocialCamera.CameraState state)
		{
			this.currentState = state;
		}

		[FieldOffset(0)]
		public LckSocialCamera.CameraState currentState;
	}
}
