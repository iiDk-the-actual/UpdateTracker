using System;
using System.Collections.Generic;
using GorillaExtensions;
using GorillaLocomotion;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;

public class PrivateUIRoom : MonoBehaviourTick
{
	private GTPlayer localPlayer
	{
		get
		{
			return GTPlayer.Instance;
		}
	}

	private void Awake()
	{
		if (PrivateUIRoom.instance == null)
		{
			PrivateUIRoom.instance = this;
			this.occluder.SetActive(false);
			this.leftHandObject.SetActive(false);
			this.rightHandObject.SetActive(false);
			this.ui = new List<Transform>();
			this.uiParents = new Dictionary<Transform, Transform>();
			this.backgroundDirectionPropertyID = Shader.PropertyToID(this.backgroundDirectionPropertyName);
			this._uiRoot = new GameObject("UIRoot").transform;
			this._uiRoot.parent = base.transform;
			return;
		}
		Object.Destroy(this);
	}

	private new void OnEnable()
	{
		base.OnEnable();
		SteamVR_Events.System(EVREventType.VREvent_InputFocusChanged).Listen(new UnityAction<VREvent_t>(this.ToggleHands));
	}

	private new void OnDisable()
	{
		base.OnDisable();
		SteamVR_Events.System(EVREventType.VREvent_InputFocusChanged).Remove(new UnityAction<VREvent_t>(this.ToggleHands));
	}

	private static bool FindShoulderCamera()
	{
		if (PrivateUIRoom._shoulderCameraReference.IsNotNull())
		{
			return true;
		}
		if (GorillaTagger.Instance.IsNull())
		{
			return false;
		}
		PrivateUIRoom._shoulderCameraReference = GorillaTagger.Instance.thirdPersonCamera.GetComponentInChildren<Camera>(true);
		if (PrivateUIRoom._shoulderCameraReference == null)
		{
			Debug.LogError("[PRIVATE_UI_ROOMS] Could not find Shoulder Camera");
			return false;
		}
		PrivateUIRoom._virtualCameraReference = PrivateUIRoom._shoulderCameraReference.GetComponentInChildren<CinemachineVirtualCamera>();
		return true;
	}

	private void ToggleHands(VREvent_t ev)
	{
		Debug.Log(string.Format("[PrivateUIRoom::ToggleHands] Toggling hands visibility. Event: {0} ({1})", ev.eventType, (EVREventType)ev.eventType));
		Debug.Log(string.Format("[PrivateUIRoom::ToggleHands] _handsShowing: {0}", PrivateUIRoom.instance.rightHandObject.activeSelf));
		if (PrivateUIRoom.instance.rightHandObject.activeSelf)
		{
			this.HideHands();
			return;
		}
		this.ShowHands();
	}

	private void HideHands()
	{
		Debug.Log("[PrivateUIRoom::OnSteamMenuShown] Steam menu shown, disabling hands.");
		PrivateUIRoom.instance.leftHandObject.SetActive(false);
		PrivateUIRoom.instance.rightHandObject.SetActive(false);
	}

	private void ShowHands()
	{
		Debug.Log("[PrivateUIRoom::OnSteamMenuShown] Steam menu hidden, re-enabling hands.");
		PrivateUIRoom.instance.leftHandObject.SetActive(true);
		PrivateUIRoom.instance.rightHandObject.SetActive(true);
	}

	private void ToggleLevelVisibility(bool levelShouldBeVisible)
	{
		Camera component = GorillaTagger.Instance.mainCamera.GetComponent<Camera>();
		if (levelShouldBeVisible)
		{
			component.cullingMask = this.savedCullingLayers;
			if (this.savedCullingLayersShoudlerCam != null)
			{
				PrivateUIRoom._shoulderCameraReference.cullingMask = this.savedCullingLayersShoudlerCam.Value;
				this.savedCullingLayersShoudlerCam = null;
				return;
			}
		}
		else
		{
			this.savedCullingLayers = component.cullingMask;
			component.cullingMask = this.visibleLayers;
			if (PrivateUIRoom.FindShoulderCamera())
			{
				this.savedCullingLayersShoudlerCam = new int?(PrivateUIRoom._shoulderCameraReference.cullingMask);
				PrivateUIRoom._shoulderCameraReference.cullingMask = this.visibleLayers;
				PrivateUIRoom._virtualCameraReference.enabled = false;
			}
		}
	}

	private static void StopOverlay()
	{
		PrivateUIRoom.instance.localPlayer.inOverlay = false;
		PrivateUIRoom.instance.inOverlay = false;
		PrivateUIRoom.instance.localPlayer.disableMovement = false;
		PrivateUIRoom.instance.localPlayer.InReportMenu = false;
		PrivateUIRoom.instance.ToggleLevelVisibility(true);
		PrivateUIRoom.instance.occluder.SetActive(false);
		PrivateUIRoom.instance.leftHandObject.SetActive(false);
		PrivateUIRoom.instance.rightHandObject.SetActive(false);
		PrivateUIRoom._virtualCameraReference.enabled = true;
		KIDAudioManager.Instance.SetKIDUIAudioActive(false);
		Debug.Log("[PrivateUIRoom::StopOverlay] Re-enabling Game Audio");
	}

	private void GetIdealScreenPositionRotation(out Vector3 position, out Quaternion rotation, out Vector3 scale)
	{
		GameObject mainCamera = GorillaTagger.Instance.mainCamera;
		rotation = Quaternion.Euler(0f, mainCamera.transform.eulerAngles.y, 0f);
		scale = this.localPlayer.turnParent.transform.localScale;
		position = mainCamera.transform.position + rotation * Vector3.zero * scale.x;
	}

	private static void AssignShoulderCameraToCanvases(Transform focus)
	{
		Debug.Log("[KID::PrivateUIRoom::CanvasCameraAssigner] setting up canvases with shoulder camera.");
		if (!PrivateUIRoom.FindShoulderCamera())
		{
			return;
		}
		Canvas componentInChildren = focus.GetComponentInChildren<Canvas>(true);
		if (componentInChildren != null)
		{
			componentInChildren.worldCamera = PrivateUIRoom._shoulderCameraReference;
			Debug.Log("[KID::PrivateUIRoom::CanvasCameraAssigner] Assigned shoulder camera to Canvas: " + componentInChildren.name);
			return;
		}
		Debug.LogError("[KID::PrivateUIRoom::CanvasCameraAssigner] No Canvas component found on this GameObject.");
	}

	public static void AddUI(Transform focus)
	{
		if (PrivateUIRoom.instance.ui.Contains(focus))
		{
			return;
		}
		PrivateUIRoom.AssignShoulderCameraToCanvases(focus);
		PrivateUIRoom.instance.uiParents.Add(focus, focus.parent);
		focus.gameObject.SetActive(false);
		focus.parent = PrivateUIRoom.instance._uiRoot;
		focus.localPosition = Vector3.zero;
		focus.localRotation = Quaternion.identity;
		PrivateUIRoom.instance.ui.Add(focus);
		if (PrivateUIRoom.instance.ui.Count == 1 && PrivateUIRoom.instance.focusTransform == null)
		{
			PrivateUIRoom.instance.focusTransform = PrivateUIRoom.instance.ui[0];
			PrivateUIRoom.instance.focusTransform.gameObject.SetActive(true);
			if (!PrivateUIRoom.instance.inOverlay)
			{
				PrivateUIRoom.StartOverlay();
			}
		}
		PrivateUIRoom.instance.UpdateUIPositionAndRotation();
	}

	public static void RemoveUI(Transform focus)
	{
		if (!PrivateUIRoom.instance.ui.Contains(focus))
		{
			return;
		}
		focus.gameObject.SetActive(false);
		PrivateUIRoom.instance.ui.Remove(focus);
		if (PrivateUIRoom.instance.focusTransform == focus)
		{
			PrivateUIRoom.instance.focusTransform = null;
		}
		if (PrivateUIRoom.instance.uiParents[focus] != null)
		{
			focus.parent = PrivateUIRoom.instance.uiParents[focus];
			PrivateUIRoom.instance.uiParents.Remove(focus);
		}
		else
		{
			Object.Destroy(focus.gameObject);
		}
		if (PrivateUIRoom.instance.ui.Count > 0)
		{
			PrivateUIRoom.instance.focusTransform = PrivateUIRoom.instance.ui[0];
			PrivateUIRoom.instance.focusTransform.gameObject.SetActive(true);
			return;
		}
		if (!PrivateUIRoom.instance.overlayForcedActive)
		{
			PrivateUIRoom.StopOverlay();
		}
	}

	public static void ForceStartOverlay()
	{
		if (PrivateUIRoom.instance == null)
		{
			return;
		}
		PrivateUIRoom.instance.overlayForcedActive = true;
		if (PrivateUIRoom.instance.inOverlay)
		{
			return;
		}
		PrivateUIRoom.StartOverlay();
	}

	public static void StopForcedOverlay()
	{
		if (PrivateUIRoom.instance == null)
		{
			return;
		}
		PrivateUIRoom.instance.overlayForcedActive = false;
		if (PrivateUIRoom.instance.ui.Count == 0 && PrivateUIRoom.instance.inOverlay)
		{
			PrivateUIRoom.StopOverlay();
		}
	}

	private static void StartOverlay()
	{
		Vector3 vector;
		Quaternion quaternion;
		Vector3 vector2;
		PrivateUIRoom.instance.GetIdealScreenPositionRotation(out vector, out quaternion, out vector2);
		PrivateUIRoom.instance.leftHandObject.transform.localScale = vector2;
		PrivateUIRoom.instance.rightHandObject.transform.localScale = vector2;
		PrivateUIRoom.instance.occluder.transform.localScale = vector2;
		PrivateUIRoom.instance.localPlayer.InReportMenu = true;
		PrivateUIRoom.instance.localPlayer.disableMovement = true;
		PrivateUIRoom.instance.occluder.SetActive(true);
		PrivateUIRoom.instance.rightHandObject.SetActive(true);
		PrivateUIRoom.instance.leftHandObject.SetActive(true);
		PrivateUIRoom.instance.ToggleLevelVisibility(false);
		PrivateUIRoom.instance.localPlayer.inOverlay = true;
		PrivateUIRoom.instance.inOverlay = true;
		KIDAudioManager.Instance.SetKIDUIAudioActive(true);
		Debug.Log("[PrivateUIRoom::StartOverlay] Muting Game Audio");
	}

	public override void Tick()
	{
		if (!this.localPlayer.InReportMenu)
		{
			return;
		}
		this.occluder.transform.position = GorillaTagger.Instance.mainCamera.transform.position;
		Transform controllerTransform = this.localPlayer.GetControllerTransform(true);
		Transform controllerTransform2 = this.localPlayer.GetControllerTransform(false);
		this.rightHandObject.transform.SetPositionAndRotation(controllerTransform2.position, controllerTransform2.rotation);
		this.leftHandObject.transform.SetPositionAndRotation(controllerTransform.position, controllerTransform.rotation);
		if (this.ShouldUpdateRotation())
		{
			this.UpdateUIPositionAndRotation();
			return;
		}
		if (this.ShouldUpdatePosition())
		{
			this.UpdateUIPosition();
		}
	}

	private bool ShouldUpdateRotation()
	{
		float magnitude = (GorillaTagger.Instance.mainCamera.transform.position - this.lastStablePosition).X_Z().magnitude;
		Quaternion quaternion = Quaternion.Euler(0f, GorillaTagger.Instance.mainCamera.transform.rotation.eulerAngles.y, 0f);
		float num = Quaternion.Angle(this.lastStableRotation, quaternion);
		return magnitude > this.lateralPlay || num >= this.rotationalPlay;
	}

	private bool ShouldUpdatePosition()
	{
		return Mathf.Abs(GorillaTagger.Instance.mainCamera.transform.position.y - this.lastStablePosition.y) > this.verticalPlay;
	}

	private void UpdateUIPositionAndRotation()
	{
		Transform transform = GorillaTagger.Instance.mainCamera.transform;
		this.lastStablePosition = transform.position;
		this.lastStableRotation = transform.rotation;
		Vector3 normalized = transform.forward.X_Z().normalized;
		this._uiRoot.SetPositionAndRotation(this.lastStablePosition + normalized * 0.02f, Quaternion.LookRotation(normalized));
		PrivateUIRoom._shoulderCameraReference.transform.position = this._uiRoot.position;
		PrivateUIRoom._shoulderCameraReference.transform.rotation = this._uiRoot.rotation;
		this.backgroundRenderer.material.SetVector(this.backgroundDirectionPropertyID, this.backgroundRenderer.transform.InverseTransformDirection(normalized));
	}

	private void UpdateUIPosition()
	{
		Transform transform = GorillaTagger.Instance.mainCamera.transform;
		this.lastStablePosition = transform.position;
		this._uiRoot.position = this.lastStablePosition + this.lastStableRotation * new Vector3(0f, 0f, 0.02f);
		PrivateUIRoom._shoulderCameraReference.transform.position = this._uiRoot.position;
	}

	public static bool GetInOverlay()
	{
		return !(PrivateUIRoom.instance == null) && PrivateUIRoom.instance.inOverlay;
	}

	[SerializeField]
	private GameObject occluder;

	[SerializeField]
	private LayerMask visibleLayers;

	[SerializeField]
	private GameObject leftHandObject;

	[SerializeField]
	private GameObject rightHandObject;

	[SerializeField]
	private MeshRenderer backgroundRenderer;

	[SerializeField]
	private string backgroundDirectionPropertyName = "_SpotDirection";

	private int backgroundDirectionPropertyID;

	private int savedCullingLayers;

	private Transform _uiRoot;

	private Transform focusTransform;

	private List<Transform> ui;

	private Dictionary<Transform, Transform> uiParents;

	private float _initialAudioVolume;

	private bool inOverlay;

	private bool overlayForcedActive;

	private static PrivateUIRoom instance;

	private Vector3 lastStablePosition;

	private Quaternion lastStableRotation;

	[SerializeField]
	private float verticalPlay = 0.1f;

	[SerializeField]
	private float lateralPlay = 0.5f;

	[SerializeField]
	private float rotationalPlay = 45f;

	private int? savedCullingLayersShoudlerCam;

	private static Camera _shoulderCameraReference;

	private static CinemachineVirtualCamera _virtualCameraReference;
}
