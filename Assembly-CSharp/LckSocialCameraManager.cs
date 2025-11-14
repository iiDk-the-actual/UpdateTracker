using System;
using Liv.Lck;
using Liv.Lck.GorillaTag;
using UnityEngine;

public class LckSocialCameraManager : MonoBehaviour
{
	public LckDirectGrabbable lckDirectGrabbable
	{
		get
		{
			return this._lckDirectGrabbable;
		}
	}

	public static LckSocialCameraManager Instance
	{
		get
		{
			return LckSocialCameraManager._instance;
		}
	}

	public void SetForceHidden(bool hidden)
	{
		this._forceHidden = hidden;
	}

	private void Awake()
	{
		this.SetManagerInstance();
		this._lckCamera = this._gtLckController.GetActiveCamera();
	}

	public void SetLckSocialCococamCamera(LckSocialCamera socialCamera)
	{
		this._socialCameraCococamInstance = socialCamera;
	}

	public void SetLckSocialTabletCamera(LckSocialCamera socialCameraTablet)
	{
		this._socialCameraTabletInstance = socialCameraTablet;
	}

	private void SetManagerInstance()
	{
		LckSocialCameraManager._instance = this;
		Action<LckSocialCameraManager> onManagerSpawned = LckSocialCameraManager.OnManagerSpawned;
		if (onManagerSpawned == null)
		{
			return;
		}
		onManagerSpawned(this);
	}

	private void OnEnable()
	{
		LckResult<LckService> service = LckService.GetService();
		if (service.Result != null)
		{
			service.Result.OnRecordingStarted += this.OnRecordingStarted;
			service.Result.OnStreamingStarted += this.OnRecordingStarted;
			service.Result.OnRecordingStopped += this.OnRecordingStopped;
			service.Result.OnStreamingStopped += this.OnRecordingStopped;
		}
		LckBodyCameraSpawner.OnCameraStateChange += this.OnBodyCameraStateChanged;
		this._gtLckController.OnCameraModeChanged += this.OnCameraModeChanged;
	}

	private void OnBodyCameraStateChanged(LckBodyCameraSpawner.CameraState state)
	{
		if (this._socialCameraTabletInstance == null)
		{
			return;
		}
		if (this._forceHidden)
		{
			this._socialCameraTabletInstance.visible = false;
			this._socialCameraCococamInstance.visible = false;
			return;
		}
		switch (state)
		{
		case LckBodyCameraSpawner.CameraState.CameraDisabled:
			this._socialCameraTabletInstance.visible = false;
			this._socialCameraCococamInstance.visible = false;
			this._socialCameraTabletInstance.IsOnNeck = false;
			return;
		case LckBodyCameraSpawner.CameraState.CameraOnNeck:
			this._socialCameraTabletInstance.visible = true;
			this._socialCameraTabletInstance.IsOnNeck = true;
			return;
		case LckBodyCameraSpawner.CameraState.CameraSpawned:
			this._socialCameraTabletInstance.visible = true;
			this._socialCameraTabletInstance.IsOnNeck = false;
			if (this._lckActiveCameraMode == CameraMode.ThirdPerson)
			{
				this._socialCameraCococamInstance.visible = true;
			}
			return;
		default:
			return;
		}
	}

	private void Update()
	{
		if (this._socialCameraCococamInstance != null && this._socialCameraTabletInstance != null && this._lckCamera != null)
		{
			Transform transform = this._lckCamera.transform;
			this._socialCameraCococamInstance.transform.position = transform.position;
			this._socialCameraCococamInstance.transform.rotation = transform.rotation;
			this._socialCameraTabletInstance.transform.position = base.transform.position;
			this._socialCameraTabletInstance.transform.rotation = base.transform.rotation;
			Camera main = Camera.main;
			if (main != null)
			{
				this._lckCamera.nearClipPlane = main.nearClipPlane;
				this._lckCamera.farClipPlane = main.farClipPlane;
			}
		}
		if (this.CoconutCamera.gameObject.activeSelf)
		{
			CameraMode lckActiveCameraMode = this._lckActiveCameraMode;
			if (lckActiveCameraMode != CameraMode.Selfie)
			{
				if (lckActiveCameraMode - CameraMode.ThirdPerson <= 1)
				{
					this.CoconutCamera.SetVisualsActive(this.cameraActive);
				}
				else
				{
					this.CoconutCamera.SetVisualsActive(false);
				}
			}
			else
			{
				this.CoconutCamera.SetVisualsActive(false);
			}
			this.CoconutCamera.SetRecordingState(this._recording);
		}
	}

	private void OnDisable()
	{
		LckResult<LckService> service = LckService.GetService();
		if (service.Result != null)
		{
			service.Result.OnRecordingStarted -= this.OnRecordingStarted;
			service.Result.OnRecordingStopped -= this.OnRecordingStopped;
			service.Result.OnStreamingStopped -= this.OnRecordingStopped;
			service.Result.OnStreamingStopped -= this.OnRecordingStopped;
		}
		LckBodyCameraSpawner.OnCameraStateChange -= this.OnBodyCameraStateChanged;
		this._gtLckController.OnCameraModeChanged -= this.OnCameraModeChanged;
	}

	public bool cameraActive
	{
		get
		{
			return this._localCameras.activeSelf;
		}
		set
		{
			this._localCameras.SetActive(value);
			if (!value)
			{
				this._gtLckController.StopRecording();
			}
		}
	}

	public bool uiVisible
	{
		get
		{
			return this._localUi.activeSelf;
		}
		set
		{
			this._localUi.SetActive(value);
		}
	}

	private void OnRecordingStarted(LckResult result)
	{
		this._recording = result.Success;
		if (this._socialCameraCococamInstance != null && this._socialCameraTabletInstance != null)
		{
			this._socialCameraCococamInstance.recording = result.Success;
			this._socialCameraTabletInstance.recording = result.Success;
		}
	}

	private void OnRecordingStopped(LckResult result)
	{
		this._recording = false;
		if (this._socialCameraCococamInstance != null && this._socialCameraTabletInstance != null)
		{
			this._socialCameraCococamInstance.recording = false;
			this._socialCameraTabletInstance.recording = false;
		}
	}

	private void OnCameraModeChanged(CameraMode mode, ILckCamera lckCamera)
	{
		this._lckCamera = lckCamera.GetCameraComponent();
		this._lckActiveCameraMode = mode;
		if (this._socialCameraCococamInstance == null || this._socialCameraTabletInstance == null)
		{
			return;
		}
		if (this._forceHidden)
		{
			this._socialCameraTabletInstance.visible = false;
			this._socialCameraCococamInstance.visible = false;
			return;
		}
		switch (this._lckActiveCameraMode)
		{
		case CameraMode.Selfie:
			if (this._socialCameraCococamInstance.visible)
			{
				this._socialCameraCococamInstance.visible = false;
				return;
			}
			break;
		case CameraMode.FirstPerson:
			if (this._socialCameraCococamInstance.visible)
			{
				this._socialCameraCococamInstance.visible = false;
				return;
			}
			break;
		case CameraMode.ThirdPerson:
			if (!this._socialCameraCococamInstance.visible)
			{
				this._socialCameraCococamInstance.visible = true;
				return;
			}
			break;
		case CameraMode.Drone:
			this._socialCameraCococamInstance.visible = !this._forceHidden && this.cameraActive;
			this._socialCameraTabletInstance.visible = this.cameraActive;
			return;
		default:
			this._socialCameraCococamInstance.visible = this.cameraActive;
			this._socialCameraTabletInstance.visible = this.cameraActive;
			break;
		}
	}

	[SerializeField]
	private GameObject _localUi;

	[SerializeField]
	private GameObject _localCameras;

	[SerializeField]
	private GTLckController _gtLckController;

	[SerializeField]
	private LckDirectGrabbable _lckDirectGrabbable;

	[SerializeField]
	public CoconutCamera CoconutCamera;

	private LckSocialCamera _socialCameraCococamInstance;

	private LckSocialCamera _socialCameraTabletInstance;

	private Camera _lckCamera;

	private CameraMode _lckActiveCameraMode;

	[OnEnterPlay_SetNull]
	private static LckSocialCameraManager _instance;

	public static Action<LckSocialCameraManager> OnManagerSpawned;

	private bool _recording;

	private bool _forceHidden;
}
