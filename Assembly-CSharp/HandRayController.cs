using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class HandRayController : MonoBehaviour
{
	public static HandRayController Instance
	{
		get
		{
			if (HandRayController.instance == null)
			{
				HandRayController.instance = Object.FindAnyObjectByType<HandRayController>();
				if (HandRayController.instance == null)
				{
					Debug.LogErrorFormat("[KID::UI::HAND_RAY_CONTROLLER] Not found in scene", Array.Empty<object>());
				}
			}
			return HandRayController.instance;
		}
	}

	private void Awake()
	{
		if (HandRayController.instance != null && HandRayController.instance != this)
		{
			Debug.LogErrorFormat(base.gameObject, "[KID::UI::HAND_RAY_CONTROLLER] Duplicate instance of HandRayController", Array.Empty<object>());
			Object.DestroyImmediate(this);
			return;
		}
		HandRayController.instance = this;
	}

	private void Start()
	{
		this._leftHandRay.attachTransform = (this._leftHandRay.rayOriginTransform = KIDHandReference.LeftHand.transform);
		this._rightHandRay.attachTransform = (this._rightHandRay.rayOriginTransform = KIDHandReference.RightHand.transform);
		this.DisableHandRays();
		this._activationCounter = 0;
	}

	private void OnDisable()
	{
		this.DisableHandRays();
	}

	public void EnableHandRays()
	{
		if (this._activationCounter == 0)
		{
			if (ControllerBehaviour.Instance)
			{
				ControllerBehaviour.Instance.OnAction += this.PostUpdate;
			}
			this.ToggleHands();
		}
		this._activationCounter++;
	}

	public void DisableHandRays()
	{
		this._activationCounter--;
		if (this._activationCounter == 0)
		{
			if (ControllerBehaviour.Instance)
			{
				ControllerBehaviour.Instance.OnAction -= this.PostUpdate;
			}
			this.HideHands();
		}
	}

	public void PulseActiveHandray(float vibrationStrength, float vibrationDuration)
	{
		if (this._activeHandRay == null)
		{
			return;
		}
		this._activeHandRay.SendHapticImpulse(vibrationStrength, vibrationDuration);
	}

	private void PostUpdate()
	{
		if (!this._hasInitialised)
		{
			return;
		}
		if (this.ActiveHand == HandRayController.HandSide.Left)
		{
			if (ControllerBehaviour.Instance.RightButtonDown)
			{
				this.ToggleHands();
			}
			return;
		}
		if (ControllerBehaviour.Instance.LeftButtonDown)
		{
			this.ToggleHands();
		}
	}

	private void ToggleRightHandRay(bool enabled)
	{
		Debug.LogFormat(string.Format("[KID::UI::HAND_RAY_CONTROLLER] RIGHT Hand is: {0}. Setting to: {1}", this._rightHandRay.gameObject.activeInHierarchy, enabled), Array.Empty<object>());
		this._rightHandRay.gameObject.SetActive(enabled);
		if (enabled)
		{
			this._activeHandRay = this._rightHandRay;
		}
	}

	private void ToggleLeftHandRay(bool enabled)
	{
		Debug.LogFormat(string.Format("[KID::UI::HAND_RAY_CONTROLLER] LEFT Hand is: {0}. Setting to: {1}", this._rightHandRay.gameObject.activeInHierarchy, enabled), Array.Empty<object>());
		this._leftHandRay.gameObject.SetActive(enabled);
		if (enabled)
		{
			this._activeHandRay = this._leftHandRay;
		}
	}

	private void InitialiseHands()
	{
		Debug.Log("[KID::UI::HAND_RAY_CONTROLLER] Initialising Hands");
		this.ToggleRightHandRay(this.ActiveHand == HandRayController.HandSide.Right);
		this.ToggleLeftHandRay(this.ActiveHand == HandRayController.HandSide.Left);
		this._hasInitialised = true;
	}

	private void ToggleHands()
	{
		if (!this._hasInitialised)
		{
			this.InitialiseHands();
			return;
		}
		HandRayController.HandSide handSide = ((this.ActiveHand == HandRayController.HandSide.Left) ? HandRayController.HandSide.Right : HandRayController.HandSide.Left);
		Debug.LogFormat(string.Concat(new string[]
		{
			"[KID::UI::HAND_RAY_CONTROLLER] Setting ActiveHand FROM: [",
			this.ActiveHand.ToString(),
			"] TO: [",
			handSide.ToString(),
			"]"
		}), Array.Empty<object>());
		this.ActiveHand = handSide;
		this.ToggleRightHandRay(handSide == HandRayController.HandSide.Right);
		this.ToggleLeftHandRay(handSide == HandRayController.HandSide.Left);
	}

	private void HideHands()
	{
		this.ToggleRightHandRay(false);
		this.ToggleLeftHandRay(false);
		this._hasInitialised = false;
		this._activeHandRay = null;
	}

	[OnEnterPlay_SetNull]
	private static HandRayController instance;

	[SerializeField]
	private XRRayInteractor _leftHandRay;

	[SerializeField]
	private XRRayInteractor _rightHandRay;

	private bool _hasInitialised;

	private HandRayController.HandSide ActiveHand = HandRayController.HandSide.Right;

	private XRRayInteractor _activeHandRay;

	private int _activationCounter;

	private enum HandSide
	{
		Left,
		Right
	}
}
