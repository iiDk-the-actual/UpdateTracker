using System;
using GorillaLocomotion;
using Liv.Lck;
using Liv.Lck.GorillaTag;
using UnityEngine;
using UnityEngine.XR;

public class LckBodyCameraSpawner : MonoBehaviourTick
{
	public void SetFollowTransform(Transform transform)
	{
		this._followTransform = transform;
	}

	public TabletSpawnInstance tabletSpawnInstance
	{
		get
		{
			return this._tabletSpawnInstance;
		}
	}

	public static event LckBodyCameraSpawner.CameraStateDelegate OnCameraStateChange;

	public LckBodyCameraSpawner.CameraState cameraState
	{
		get
		{
			return this._cameraState;
		}
		set
		{
			switch (value)
			{
			case LckBodyCameraSpawner.CameraState.CameraDisabled:
				this.cameraPosition = LckBodyCameraSpawner.CameraPosition.NotVisible;
				this._tabletSpawnInstance.uiVisible = false;
				this._tabletSpawnInstance.cameraActive = false;
				this.ResetCameraModel();
				this.cameraVisible = false;
				this._shouldMoveCameraToNeck = false;
				break;
			case LckBodyCameraSpawner.CameraState.CameraOnNeck:
				this.cameraPosition = LckBodyCameraSpawner.CameraPosition.CameraDefault;
				this._tabletSpawnInstance.uiVisible = false;
				this._tabletSpawnInstance.cameraActive = true;
				this.ResetCameraModel();
				if (Application.platform == RuntimePlatform.Android)
				{
					this.SetPreviewActive(false);
				}
				this.cameraVisible = true;
				this._shouldMoveCameraToNeck = false;
				this._dummyTablet.SetTabletIsSpawned(false);
				break;
			case LckBodyCameraSpawner.CameraState.CameraSpawned:
				this.cameraPosition = LckBodyCameraSpawner.CameraPosition.CameraDefault;
				this._tabletSpawnInstance.uiVisible = true;
				this._tabletSpawnInstance.cameraActive = true;
				if (Application.platform == RuntimePlatform.Android)
				{
					this.SetPreviewActive(true);
				}
				this.ResetCameraModel();
				this.cameraVisible = true;
				this._shouldMoveCameraToNeck = false;
				this._dummyTablet.SetTabletIsSpawned(true);
				break;
			}
			this._cameraState = value;
			LckBodyCameraSpawner.CameraStateDelegate onCameraStateChange = LckBodyCameraSpawner.OnCameraStateChange;
			if (onCameraStateChange == null)
			{
				return;
			}
			onCameraStateChange(this._cameraState);
		}
	}

	private void SetPreviewActive(bool isActive)
	{
		LckResult<LckService> service = LckService.GetService();
		if (!service.Success)
		{
			Debug.LogError("LCK Could not get Service" + service.Error.ToString());
			return;
		}
		LckService result = service.Result;
		if (result == null)
		{
			return;
		}
		result.SetPreviewActive(isActive);
	}

	public LckBodyCameraSpawner.CameraPosition cameraPosition
	{
		get
		{
			return this._cameraPosition;
		}
		set
		{
			if (this._cameraModelTransform != null && this._cameraPosition != value)
			{
				switch (value)
				{
				case LckBodyCameraSpawner.CameraPosition.CameraDefault:
					this.ChangeCameraModelParent(this._cameraPositionDefault);
					this._cameraPosition = LckBodyCameraSpawner.CameraPosition.CameraDefault;
					return;
				case LckBodyCameraSpawner.CameraPosition.CameraSlingshot:
					this.ChangeCameraModelParent(this._cameraPositionSlingshot);
					this._cameraPosition = LckBodyCameraSpawner.CameraPosition.CameraSlingshot;
					break;
				case LckBodyCameraSpawner.CameraPosition.NotVisible:
					break;
				default:
					return;
				}
			}
		}
	}

	private bool cameraVisible
	{
		get
		{
			return this._cameraModelTransform.gameObject.activeSelf;
		}
		set
		{
			this._cameraModelTransform.gameObject.SetActive(value);
			this._cameraStrapRenderer.enabled = value;
		}
	}

	private void Awake()
	{
		this._tabletSpawnInstance = new TabletSpawnInstance(this._cameraSpawnPrefab, this._cameraSpawnParentTransform);
	}

	private new void OnEnable()
	{
		base.OnEnable();
		this.InitCameraStrap();
		this.cameraState = LckBodyCameraSpawner.CameraState.CameraDisabled;
		this.cameraPosition = LckBodyCameraSpawner.CameraPosition.CameraDefault;
		if (this._tabletSpawnInstance.Controller != null)
		{
			this._previousMode = this._tabletSpawnInstance.Controller.CurrentCameraMode;
		}
		ZoneManagement.OnZoneChange += this.OnZoneChanged;
	}

	private new void OnDisable()
	{
		base.OnDisable();
		ZoneManagement.OnZoneChange -= this.OnZoneChanged;
	}

	public override void Tick()
	{
		if (this._followTransform != null && base.transform.parent != null)
		{
			Matrix4x4 localToWorldMatrix = base.transform.parent.localToWorldMatrix;
			Vector3 vector = localToWorldMatrix.MultiplyPoint(this._followTransform.localPosition + this._followTransform.localRotation * new Vector3(0f, -0.05f, 0.1f));
			Quaternion quaternion = Quaternion.LookRotation(localToWorldMatrix.MultiplyVector(this._followTransform.localRotation * Vector3.forward), localToWorldMatrix.MultiplyVector(this._followTransform.localRotation * Vector3.up));
			base.transform.SetPositionAndRotation(vector, quaternion);
		}
		LckBodyCameraSpawner.CameraState cameraState = this._cameraState;
		if (cameraState != LckBodyCameraSpawner.CameraState.CameraOnNeck)
		{
			if (cameraState == LckBodyCameraSpawner.CameraState.CameraSpawned)
			{
				this.UpdateCameraStrap();
				if (this._cameraModelGrabbable.isGrabbed)
				{
					GorillaGrabber grabber = this._cameraModelGrabbable.grabber;
					Transform transform = grabber.transform;
					if (this.ShouldSpawnCamera(transform))
					{
						this.SpawnCamera(grabber, transform);
					}
				}
				else
				{
					this.ResetCameraModel();
				}
				if (this._tabletSpawnInstance.isSpawned)
				{
					Transform transform3;
					if (this._tabletSpawnInstance.directGrabbable.isGrabbed)
					{
						GorillaGrabber grabber2 = this._tabletSpawnInstance.directGrabbable.grabber;
						Transform transform2 = grabber2.transform;
						if (!this.ShouldSpawnCamera(transform2))
						{
							this.cameraState = LckBodyCameraSpawner.CameraState.CameraOnNeck;
							this._cameraModelGrabbable.target.SetPositionAndRotation(transform2.position, transform2.rotation * Quaternion.Euler(this._chestSpawnRotationOffset.x, this._chestSpawnRotationOffset.y, this._chestSpawnRotationOffset.z));
							this._tabletSpawnInstance.directGrabbable.ForceRelease();
							this._tabletSpawnInstance.SetParent(this._cameraModelTransform);
							this._tabletSpawnInstance.ResetLocalPose();
							this._cameraModelGrabbable.ForceGrab(grabber2);
							this._cameraModelGrabbable.onReleased += this.OnCameraModelReleased;
							this._previousMode = this._tabletSpawnInstance.Controller.CurrentCameraMode;
							if (this._previousMode == CameraMode.Selfie)
							{
								this._tabletSpawnInstance.Controller.SetCameraMode(CameraMode.FirstPerson);
							}
						}
					}
					else if (this._shouldMoveCameraToNeck && GtTag.TryGetTransform(GtTagType.HMD, out transform3) && Vector3.SqrMagnitude(transform3.position - this.tabletSpawnInstance.position) >= this._snapToNeckDistance * this._snapToNeckDistance)
					{
						this.cameraState = LckBodyCameraSpawner.CameraState.CameraOnNeck;
						this._tabletSpawnInstance.SetParent(this._cameraModelTransform);
						this._tabletSpawnInstance.ResetLocalPose();
						this._shouldMoveCameraToNeck = false;
					}
				}
			}
		}
		else
		{
			this.UpdateCameraStrap();
			if (this._cameraModelGrabbable.isGrabbed)
			{
				GorillaGrabber grabber3 = this._cameraModelGrabbable.grabber;
				Transform transform4 = grabber3.transform;
				if (this.ShouldSpawnCamera(transform4))
				{
					this.SpawnCamera(grabber3, transform4);
				}
			}
			else
			{
				this.ResetCameraModel();
			}
		}
		if (!this.IsSlingshotActiveInHierarchy())
		{
			this.cameraPosition = LckBodyCameraSpawner.CameraPosition.CameraDefault;
			return;
		}
		this.cameraPosition = LckBodyCameraSpawner.CameraPosition.CameraSlingshot;
	}

	private void OnZoneChanged(ZoneData[] zones)
	{
		if (!this._tabletSpawnInstance.isSpawned || this._tabletSpawnInstance.directGrabbable.isGrabbed)
		{
			return;
		}
		if (Vector3.Distance(this._tabletSpawnInstance.Controller.transform.position, base.transform.position) > 6f)
		{
			this.ManuallySetCameraOnNeck();
		}
	}

	private void OnDestroy()
	{
		this._tabletSpawnInstance.Dispose();
	}

	public void ManuallySetCameraOnNeck()
	{
		if (this.cameraState == LckBodyCameraSpawner.CameraState.CameraOnNeck)
		{
			return;
		}
		if (this._tabletSpawnInstance.isSpawned)
		{
			this.cameraState = LckBodyCameraSpawner.CameraState.CameraOnNeck;
			this._tabletSpawnInstance.SetParent(this._cameraModelTransform);
			this._tabletSpawnInstance.ResetLocalPose();
			this._shouldMoveCameraToNeck = false;
			this._previousMode = this._tabletSpawnInstance.Controller.CurrentCameraMode;
			if (this._previousMode == CameraMode.Selfie)
			{
				this._tabletSpawnInstance.Controller.SetCameraMode(CameraMode.FirstPerson);
			}
		}
	}

	private void OnCameraModelReleased()
	{
		this._cameraModelGrabbable.onReleased -= this.OnCameraModelReleased;
		this.ResetCameraModel();
	}

	public void SpawnCamera(GorillaGrabber overrideGorillaGrabber, Transform transform)
	{
		if (!this._tabletSpawnInstance.isSpawned)
		{
			this._tabletSpawnInstance.SpawnCamera();
		}
		if (this._previousMode == CameraMode.Selfie)
		{
			this._tabletSpawnInstance.Controller.SetCameraMode(CameraMode.Selfie);
			this._previousMode = CameraMode.Selfie;
		}
		this.cameraState = LckBodyCameraSpawner.CameraState.CameraSpawned;
		this._cameraModelGrabbable.ForceRelease();
		this._tabletSpawnInstance.ResetParent();
		Vector3 vector = Vector3.zero;
		Vector3 vector2 = Vector3.zero;
		vector2 = this._rotationOffsetWindows;
		XRNode xrNode = overrideGorillaGrabber.XrNode;
		if (xrNode != XRNode.LeftHand)
		{
			if (xrNode == XRNode.RightHand)
			{
				vector = this._rightHandSpawnOffsetWindows;
				vector2.z = -12f;
			}
		}
		else
		{
			vector = this._leftHandSpawnOffsetWindows;
			vector2.z = 12f;
		}
		if (!GTPlayer.Instance.IsDefaultScale)
		{
			vector *= 0.06f;
		}
		vector = transform.rotation * vector;
		this._tabletSpawnInstance.SetPositionAndRotation(transform.position + vector, transform.rotation * Quaternion.Euler(vector2));
		this._tabletSpawnInstance.directGrabbable.ForceGrab(overrideGorillaGrabber);
		this._tabletSpawnInstance.SetLocalScale(Vector3.one);
	}

	private bool ShouldSpawnCamera(Transform gorillaGrabberTransform)
	{
		Matrix4x4 worldToLocalMatrix = base.transform.worldToLocalMatrix;
		Vector3 vector = worldToLocalMatrix.MultiplyPoint(this._cameraModelOriginTransform.position);
		Vector3 vector2 = worldToLocalMatrix.MultiplyPoint(gorillaGrabberTransform.position);
		return Vector3.SqrMagnitude(vector - vector2) >= this._activateDistance * this._activateDistance;
	}

	private void ChangeCameraModelParent(Transform transform)
	{
		if (this._cameraModelTransform != null)
		{
			this._cameraModelGrabbable.SetOriginalTargetParent(transform);
			if (!this._cameraModelGrabbable.isGrabbed)
			{
				this._cameraModelTransform.transform.parent = transform;
				this._cameraModelTransform.transform.localPosition = Vector3.zero;
			}
		}
	}

	private void InitCameraStrap()
	{
		this._cameraStrapRenderer.positionCount = this._cameraStrapPoints.Length;
		this._cameraStrapPositions = new Vector3[this._cameraStrapPoints.Length];
	}

	private void UpdateCameraStrap()
	{
		for (int i = 0; i < this._cameraStrapPoints.Length; i++)
		{
			this._cameraStrapPositions[i] = this._cameraStrapPoints[i].position;
		}
		this._cameraStrapRenderer.SetPositions(this._cameraStrapPositions);
		Vector3 lossyScale = base.transform.lossyScale;
		float num = (lossyScale.x + lossyScale.y + lossyScale.z) * 0.3333333f;
		this._cameraStrapRenderer.widthMultiplier = num * 0.02f;
		Color color = ((this.cameraState == LckBodyCameraSpawner.CameraState.CameraSpawned) ? this._ghostColor : this._normalColor);
		this._cameraStrapRenderer.startColor = color;
		this._cameraStrapRenderer.endColor = color;
	}

	private void ResetCameraModel()
	{
		this._cameraModelTransform.localPosition = Vector3.zero;
		this._cameraModelTransform.localRotation = Quaternion.identity;
	}

	private VRRig GetLocalRig()
	{
		if (this._localRig == null)
		{
			this._localRig = VRRigCache.Instance.localRig.Rig;
		}
		return this._localRig;
	}

	private bool IsSlingshotHeldInHand(out bool leftHand, out bool rightHand)
	{
		VRRig localRig = this.GetLocalRig();
		if (localRig == null)
		{
			leftHand = false;
			rightHand = false;
			return false;
		}
		leftHand = localRig.projectileWeapon.InLeftHand();
		rightHand = localRig.projectileWeapon.InRightHand();
		return localRig.projectileWeapon.InHand();
	}

	private bool IsSlingshotActiveInHierarchy()
	{
		VRRig localRig = this.GetLocalRig();
		return !(localRig == null) && !(localRig.projectileWeapon == null) && localRig.projectileWeapon.gameObject.activeInHierarchy;
	}

	[SerializeField]
	private GameObject _cameraSpawnPrefab;

	[SerializeField]
	private Transform _cameraSpawnParentTransform;

	[SerializeField]
	private Transform _cameraModelOriginTransform;

	[SerializeField]
	private Transform _cameraModelTransform;

	[SerializeField]
	private LckDirectGrabbable _cameraModelGrabbable;

	[SerializeField]
	private Transform _cameraPositionDefault;

	[SerializeField]
	private Transform _cameraPositionSlingshot;

	private Vector3 _chestSpawnRotationOffset = new Vector3(90f, 0f, 0f);

	private Vector3 _rightHandSpawnOffsetAndroid = new Vector3(-0.265f, 0.02f, -0.065f);

	private Vector3 _leftHandSpawnOffsetAndroid = new Vector3(0.245f, 0.022f, -0.12f);

	private Vector3 _rotationOffsetAndroid = new Vector3(-90f, 60f, 125f);

	private Vector3 _rotationOffsetWindows = new Vector3(-70f, -180f, 0f);

	private Vector3 _rightHandSpawnOffsetWindows = new Vector3(-0.23f, -0.035f, -0.225f);

	private Vector3 _leftHandSpawnOffsetWindows = new Vector3(0.23f, -0.035f, -0.225f);

	[SerializeField]
	private float _activateDistance = 0.25f;

	[SerializeField]
	private float _snapToNeckDistance = 15f;

	[SerializeField]
	private LineRenderer _cameraStrapRenderer;

	[SerializeField]
	private Transform[] _cameraStrapPoints;

	[SerializeField]
	private Color _normalColor = Color.red;

	[SerializeField]
	private Color _ghostColor = Color.gray;

	[SerializeField]
	private GtDummyTablet _dummyTablet;

	private Transform _followTransform;

	private Vector3[] _cameraStrapPositions;

	private TabletSpawnInstance _tabletSpawnInstance;

	private VRRig _localRig;

	private bool _shouldMoveCameraToNeck;

	private CameraMode _previousMode;

	private LckBodyCameraSpawner.CameraState _cameraState;

	private LckBodyCameraSpawner.CameraPosition _cameraPosition;

	public enum CameraState
	{
		CameraDisabled,
		CameraOnNeck,
		CameraSpawned
	}

	public enum CameraPosition
	{
		CameraDefault,
		CameraSlingshot,
		NotVisible
	}

	public delegate void CameraStateDelegate(LckBodyCameraSpawner.CameraState state);
}
