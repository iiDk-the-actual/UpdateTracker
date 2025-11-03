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

	public void SetLckSocialCamera(LckSocialCamera socialCamera)
	{
		this._socialCameraInstance = socialCamera;
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
			service.Result.OnRecordingStopped += this.OnRecordingStopped;
		}
		this._gtLckController.OnCameraModeChanged += this.OnCameraModeChanged;
	}

	private void Update()
	{
		if (this._socialCameraInstance != null)
		{
			if (this._lckCamera != null)
			{
				Transform transform = this._lckCamera.transform;
				this._socialCameraInstance.transform.position = transform.position;
				this._socialCameraInstance.transform.rotation = transform.rotation;
				Camera main = Camera.main;
				if (main != null)
				{
					this._lckCamera.nearClipPlane = main.nearClipPlane;
					this._lckCamera.farClipPlane = main.farClipPlane;
				}
			}
			CameraMode cameraMode = this._lckActiveCameraMode;
			if (cameraMode == CameraMode.Selfie || cameraMode - CameraMode.ThirdPerson <= 1)
			{
				this._socialCameraInstance.visible = !this._forceHidden && this.cameraActive;
			}
			else
			{
				this._socialCameraInstance.visible = false;
			}
			this._socialCameraInstance.recording = this._recording;
		}
		if (this.CoconutCamera.gameObject.activeSelf)
		{
			CameraMode cameraMode = this._lckActiveCameraMode;
			if (cameraMode != CameraMode.Selfie)
			{
				if (cameraMode - CameraMode.ThirdPerson <= 1)
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
		}
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
	}

	private void OnRecordingStopped(LckResult result)
	{
		this._recording = false;
	}

	private void OnCameraModeChanged(CameraMode mode, ILckCamera lckCamera)
	{
		this._lckCamera = lckCamera.GetCameraComponent();
		this._lckActiveCameraMode = mode;
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

	private LckSocialCamera _socialCameraInstance;

	private Camera _lckCamera;

	private CameraMode _lckActiveCameraMode;

	[OnEnterPlay_SetNull]
	private static LckSocialCameraManager _instance;

	public static Action<LckSocialCameraManager> OnManagerSpawned;

	private bool _recording;

	private bool _forceHidden;
}
