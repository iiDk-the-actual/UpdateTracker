using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using GorillaTag;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;

public class ControllerInputPoller : MonoBehaviour
{
	[DebugReadout]
	public bool leftIndexPressed
	{
		get
		{
			return this._leftIndexPressed;
		}
	}

	[DebugReadout]
	public bool leftIndexReleased
	{
		get
		{
			return this._leftIndexReleased;
		}
	}

	[DebugReadout]
	public bool rightIndexPressed
	{
		get
		{
			return this._rightIndexPressed;
		}
	}

	[DebugReadout]
	public bool rightIndexReleased
	{
		get
		{
			return this._rightIndexReleased;
		}
	}

	[DebugReadout]
	public bool leftIndexPressedThisFrame
	{
		get
		{
			return this._leftIndexPressedThisFrame;
		}
	}

	[DebugReadout]
	public bool leftIndexReleasedThisFrame
	{
		get
		{
			return this._leftIndexReleasedThisFrame;
		}
	}

	[DebugReadout]
	public bool rightIndexPressedThisFrame
	{
		get
		{
			return this._rightIndexPressedThisFrame;
		}
	}

	[DebugReadout]
	public bool rightIndexReleasedThisFrame
	{
		get
		{
			return this._rightIndexReleasedThisFrame;
		}
	}

	[DebugReadout]
	public Vector3 leftVelocity
	{
		get
		{
			return this._leftVelocity;
		}
	}

	[DebugReadout]
	public Vector3 rightVelocity
	{
		get
		{
			return this._rightVelocity;
		}
	}

	[DebugReadout]
	public Vector3 leftAngularVelocity
	{
		get
		{
			return this._leftAngularVelocity;
		}
	}

	[DebugReadout]
	public Vector3 rightAngularVelocity
	{
		get
		{
			return this._rightAngularVelocity;
		}
	}

	public GorillaControllerType controllerType { get; private set; }

	private void Awake()
	{
		if (ControllerInputPoller.instance == null)
		{
			ControllerInputPoller.instance = this;
			return;
		}
		if (ControllerInputPoller.instance != this)
		{
			Object.Destroy(base.gameObject);
		}
	}

	public static void AddUpdateCallback(Action callback)
	{
		if (!ControllerInputPoller.instance.didModifyOnUpdate)
		{
			ControllerInputPoller.instance.onUpdateNext.Clear();
			ControllerInputPoller.instance.onUpdateNext.AddRange(ControllerInputPoller.instance.onUpdate);
			ControllerInputPoller.instance.didModifyOnUpdate = true;
		}
		ControllerInputPoller.instance.onUpdateNext.Add(callback);
	}

	public static void RemoveUpdateCallback(Action callback)
	{
		if (!ControllerInputPoller.instance.didModifyOnUpdate)
		{
			ControllerInputPoller.instance.onUpdateNext.Clear();
			ControllerInputPoller.instance.onUpdateNext.AddRange(ControllerInputPoller.instance.onUpdate);
			ControllerInputPoller.instance.didModifyOnUpdate = true;
		}
		ControllerInputPoller.instance.onUpdateNext.Remove(callback);
	}

	public void LateUpdate()
	{
		if (!this.leftControllerDevice.isValid)
		{
			this.leftControllerDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
			if (this.leftControllerDevice.isValid)
			{
				this.controllerType = GorillaControllerType.OCULUS_DEFAULT;
				if (this.leftControllerDevice.name.ToLower().Contains("knuckles"))
				{
					this.controllerType = GorillaControllerType.INDEX;
				}
				Debug.Log(string.Format("Found left controller: {0} ControllerType: {1}", this.leftControllerDevice.name, this.controllerType));
			}
		}
		if (!this.rightControllerDevice.isValid)
		{
			this.rightControllerDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
		}
		if (!this.headDevice.isValid)
		{
			this.headDevice = InputDevices.GetDeviceAtXRNode(XRNode.CenterEye);
		}
		InputDevice inputDevice = this.leftControllerDevice;
		InputDevice inputDevice2 = this.rightControllerDevice;
		InputDevice inputDevice3 = this.headDevice;
		this.leftControllerDevice.TryGetFeatureValue(CommonUsages.primaryButton, out this.leftControllerPrimaryButton);
		this.leftControllerDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out this.leftControllerSecondaryButton);
		this.leftControllerDevice.TryGetFeatureValue(CommonUsages.primaryTouch, out this.leftControllerPrimaryButtonTouch);
		this.leftControllerDevice.TryGetFeatureValue(CommonUsages.secondaryTouch, out this.leftControllerSecondaryButtonTouch);
		this.leftControllerDevice.TryGetFeatureValue(CommonUsages.grip, out this.leftControllerGripFloat);
		this.leftControllerDevice.TryGetFeatureValue(CommonUsages.trigger, out this.leftControllerIndexFloat);
		this.leftControllerDevice.TryGetFeatureValue(CommonUsages.devicePosition, out this.leftControllerPosition);
		this.leftControllerDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out this.leftControllerRotation);
		this.leftControllerDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out this.leftControllerPrimary2DAxis);
		this.leftControllerDevice.TryGetFeatureValue(CommonUsages.triggerButton, out this.leftControllerTriggerButton);
		this.rightControllerDevice.TryGetFeatureValue(CommonUsages.primaryButton, out this.rightControllerPrimaryButton);
		this.rightControllerDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out this.rightControllerSecondaryButton);
		this.rightControllerDevice.TryGetFeatureValue(CommonUsages.primaryTouch, out this.rightControllerPrimaryButtonTouch);
		this.rightControllerDevice.TryGetFeatureValue(CommonUsages.secondaryTouch, out this.rightControllerSecondaryButtonTouch);
		this.rightControllerDevice.TryGetFeatureValue(CommonUsages.grip, out this.rightControllerGripFloat);
		this.rightControllerDevice.TryGetFeatureValue(CommonUsages.trigger, out this.rightControllerIndexFloat);
		this.rightControllerDevice.TryGetFeatureValue(CommonUsages.devicePosition, out this.rightControllerPosition);
		this.rightControllerDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out this.rightControllerRotation);
		this.rightControllerDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out this.rightControllerPrimary2DAxis);
		this.rightControllerDevice.TryGetFeatureValue(CommonUsages.triggerButton, out this.rightControllerTriggerButton);
		this.leftControllerPrimaryButton = SteamVR_Actions.gorillaTag_LeftPrimaryClick.GetState(SteamVR_Input_Sources.LeftHand);
		this.leftControllerSecondaryButton = SteamVR_Actions.gorillaTag_LeftSecondaryClick.GetState(SteamVR_Input_Sources.LeftHand);
		this.leftControllerPrimaryButtonTouch = SteamVR_Actions.gorillaTag_LeftPrimaryTouch.GetState(SteamVR_Input_Sources.LeftHand);
		this.leftControllerSecondaryButtonTouch = SteamVR_Actions.gorillaTag_LeftSecondaryTouch.GetState(SteamVR_Input_Sources.LeftHand);
		this.leftControllerGripFloat = SteamVR_Actions.gorillaTag_LeftGripFloat.GetAxis(SteamVR_Input_Sources.LeftHand);
		this.leftControllerIndexFloat = SteamVR_Actions.gorillaTag_LeftTriggerFloat.GetAxis(SteamVR_Input_Sources.LeftHand);
		this.leftControllerTriggerButton = SteamVR_Actions.gorillaTag_LeftTriggerClick.GetState(SteamVR_Input_Sources.LeftHand);
		this.leftControllerPrimary2DAxis = SteamVR_Actions.gorillaTag_LeftJoystick2DAxis.GetAxis(SteamVR_Input_Sources.LeftHand);
		this.rightControllerPrimaryButton = SteamVR_Actions.gorillaTag_RightPrimaryClick.GetState(SteamVR_Input_Sources.RightHand);
		this.rightControllerSecondaryButton = SteamVR_Actions.gorillaTag_RightSecondaryClick.GetState(SteamVR_Input_Sources.RightHand);
		this.rightControllerPrimaryButtonTouch = SteamVR_Actions.gorillaTag_RightPrimaryTouch.GetState(SteamVR_Input_Sources.RightHand);
		this.rightControllerSecondaryButtonTouch = SteamVR_Actions.gorillaTag_RightSecondaryTouch.GetState(SteamVR_Input_Sources.RightHand);
		this.rightControllerGripFloat = SteamVR_Actions.gorillaTag_RightGripFloat.GetAxis(SteamVR_Input_Sources.RightHand);
		this.rightControllerIndexFloat = SteamVR_Actions.gorillaTag_RightTriggerFloat.GetAxis(SteamVR_Input_Sources.RightHand);
		this.rightControllerTriggerButton = SteamVR_Actions.gorillaTag_RightTriggerClick.GetState(SteamVR_Input_Sources.RightHand);
		this.rightControllerPrimary2DAxis = SteamVR_Actions.gorillaTag_RightJoystick2DAxis.GetAxis(SteamVR_Input_Sources.RightHand);
		this.headDevice.TryGetFeatureValue(CommonUsages.devicePosition, out this.headPosition);
		this.headDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out this.headRotation);
		this.CalculateGrabState(this.leftControllerIndexFloat, ref this._leftIndexPressed, ref this._leftIndexReleased, out this._leftIndexPressedThisFrame, out this._leftIndexReleasedThisFrame, 0.75f, 0.65f);
		this.CalculateGrabState(this.rightControllerIndexFloat, ref this._rightIndexPressed, ref this._rightIndexReleased, out this._rightIndexPressedThisFrame, out this._rightIndexReleasedThisFrame, 0.75f, 0.65f);
		if (this.controllerType == GorillaControllerType.OCULUS_DEFAULT)
		{
			this.CalculateGrabState(this.leftControllerGripFloat, ref this.leftGrab, ref this.leftGrabRelease, out this.leftGrabMomentary, out this.leftGrabReleaseMomentary, 0.75f, 0.65f);
			this.CalculateGrabState(this.rightControllerGripFloat, ref this.rightGrab, ref this.rightGrabRelease, out this.rightGrabMomentary, out this.rightGrabReleaseMomentary, 0.75f, 0.65f);
		}
		else if (this.controllerType == GorillaControllerType.INDEX)
		{
			this.CalculateGrabState(this.leftControllerGripFloat, ref this.leftGrab, ref this.leftGrabRelease, out this.leftGrabMomentary, out this.leftGrabReleaseMomentary, 0.1f, 0.01f);
			this.CalculateGrabState(this.rightControllerGripFloat, ref this.rightGrab, ref this.rightGrabRelease, out this.rightGrabMomentary, out this.rightGrabReleaseMomentary, 0.1f, 0.01f);
		}
		this.leftControllerDevice.TryGetFeatureValue(CommonUsages.deviceVelocity, out this._leftVelocity);
		this.leftControllerDevice.TryGetFeatureValue(CommonUsages.deviceAngularVelocity, out this._leftAngularVelocity);
		this.rightControllerDevice.TryGetFeatureValue(CommonUsages.deviceVelocity, out this._rightVelocity);
		this.rightControllerDevice.TryGetFeatureValue(CommonUsages.deviceAngularVelocity, out this._rightAngularVelocity);
		this._UpdatePressFlags();
		if (this.didModifyOnUpdate)
		{
			List<Action> list = this.onUpdateNext;
			List<Action> list2 = this.onUpdate;
			this.onUpdate = list;
			this.onUpdateNext = list2;
			this.didModifyOnUpdate = false;
		}
		foreach (Action action in this.onUpdate)
		{
			action();
		}
	}

	private void CalculateGrabState(float grabValue, ref bool grab, ref bool grabRelease, out bool grabMomentary, out bool grabReleaseMomentary, float grabThreshold, float grabReleaseThreshold)
	{
		bool flag = grabValue >= grabThreshold;
		bool flag2 = grabValue <= grabReleaseThreshold;
		grabMomentary = flag && !grab;
		grabReleaseMomentary = flag2 && !grabRelease;
		grab = flag;
		grabRelease = flag2;
	}

	public void RecalculateGrabState()
	{
		this.CalculateGrabState(this.leftControllerIndexFloat, ref this._leftIndexPressed, ref this._leftIndexReleased, out this._leftIndexPressedThisFrame, out this._leftIndexReleasedThisFrame, 0.75f, 0.65f);
		this.CalculateGrabState(this.rightControllerIndexFloat, ref this._rightIndexPressed, ref this._rightIndexReleased, out this._rightIndexPressedThisFrame, out this._rightIndexReleasedThisFrame, 0.75f, 0.65f);
		if (this.controllerType == GorillaControllerType.OCULUS_DEFAULT)
		{
			this.CalculateGrabState(this.leftControllerGripFloat, ref this.leftGrab, ref this.leftGrabRelease, out this.leftGrabMomentary, out this.leftGrabReleaseMomentary, 0.75f, 0.65f);
			this.CalculateGrabState(this.rightControllerGripFloat, ref this.rightGrab, ref this.rightGrabRelease, out this.rightGrabMomentary, out this.rightGrabReleaseMomentary, 0.75f, 0.65f);
			return;
		}
		if (this.controllerType == GorillaControllerType.INDEX)
		{
			this.CalculateGrabState(this.leftControllerGripFloat, ref this.leftGrab, ref this.leftGrabRelease, out this.leftGrabMomentary, out this.leftGrabReleaseMomentary, 0.1f, 0.01f);
			this.CalculateGrabState(this.rightControllerGripFloat, ref this.rightGrab, ref this.rightGrabRelease, out this.rightGrabMomentary, out this.rightGrabReleaseMomentary, 0.1f, 0.01f);
		}
	}

	public static bool GetIndexPressed(XRNode node)
	{
		if (node != XRNode.LeftHand)
		{
			return node == XRNode.RightHand && ControllerInputPoller.instance.rightIndexPressed;
		}
		return ControllerInputPoller.instance.leftIndexPressed;
	}

	public static bool GetIndexReleased(XRNode node)
	{
		if (node != XRNode.LeftHand)
		{
			return node == XRNode.RightHand && ControllerInputPoller.instance.rightIndexReleased;
		}
		return ControllerInputPoller.instance.leftIndexReleased;
	}

	public static bool GetIndexPressedThisFrame(XRNode node)
	{
		if (node != XRNode.LeftHand)
		{
			return node == XRNode.RightHand && ControllerInputPoller.instance.leftIndexPressedThisFrame;
		}
		return ControllerInputPoller.instance.leftIndexPressedThisFrame;
	}

	public static bool GetIndexReleasedThisFrame(XRNode node)
	{
		if (node != XRNode.LeftHand)
		{
			return node == XRNode.RightHand && ControllerInputPoller.instance.leftIndexReleasedThisFrame;
		}
		return ControllerInputPoller.instance.leftIndexReleasedThisFrame;
	}

	public static bool GetGrab(XRNode node)
	{
		if (node == XRNode.LeftHand)
		{
			return ControllerInputPoller.instance.leftGrab;
		}
		return node == XRNode.RightHand && ControllerInputPoller.instance.rightGrab;
	}

	public static bool GetGrabRelease(XRNode node)
	{
		if (node == XRNode.LeftHand)
		{
			return ControllerInputPoller.instance.leftGrabRelease;
		}
		return node == XRNode.RightHand && ControllerInputPoller.instance.rightGrabRelease;
	}

	public static bool GetGrabMomentary(XRNode node)
	{
		if (node == XRNode.LeftHand)
		{
			return ControllerInputPoller.instance.leftGrabMomentary;
		}
		return node == XRNode.RightHand && ControllerInputPoller.instance.rightGrabMomentary;
	}

	public static bool GetGrabReleaseMomentary(XRNode node)
	{
		if (node == XRNode.LeftHand)
		{
			return ControllerInputPoller.instance.leftGrabReleaseMomentary;
		}
		return node == XRNode.RightHand && ControllerInputPoller.instance.rightGrabReleaseMomentary;
	}

	public static Vector2 Primary2DAxis(XRNode node)
	{
		if (node == XRNode.LeftHand)
		{
			return ControllerInputPoller.instance.leftControllerPrimary2DAxis;
		}
		return ControllerInputPoller.instance.rightControllerPrimary2DAxis;
	}

	public static bool PrimaryButtonPress(XRNode node)
	{
		if (node == XRNode.LeftHand)
		{
			return ControllerInputPoller.instance.leftControllerPrimaryButton;
		}
		return node == XRNode.RightHand && ControllerInputPoller.instance.rightControllerPrimaryButton;
	}

	public static bool SecondaryButtonPress(XRNode node)
	{
		if (node == XRNode.LeftHand)
		{
			return ControllerInputPoller.instance.leftControllerSecondaryButton;
		}
		return node == XRNode.RightHand && ControllerInputPoller.instance.rightControllerSecondaryButton;
	}

	public static bool PrimaryButtonTouch(XRNode node)
	{
		if (node == XRNode.LeftHand)
		{
			return ControllerInputPoller.instance.leftControllerPrimaryButtonTouch;
		}
		return node == XRNode.RightHand && ControllerInputPoller.instance.rightControllerPrimaryButtonTouch;
	}

	public static bool SecondaryButtonTouch(XRNode node)
	{
		if (node == XRNode.LeftHand)
		{
			return ControllerInputPoller.instance.leftControllerSecondaryButtonTouch;
		}
		return node == XRNode.RightHand && ControllerInputPoller.instance.rightControllerSecondaryButtonTouch;
	}

	public static float GripFloat(XRNode node)
	{
		if (node == XRNode.LeftHand)
		{
			return ControllerInputPoller.instance.leftControllerGripFloat;
		}
		if (node == XRNode.RightHand)
		{
			return ControllerInputPoller.instance.rightControllerGripFloat;
		}
		return 0f;
	}

	public static float TriggerFloat(XRNode node)
	{
		if (node == XRNode.LeftHand)
		{
			return ControllerInputPoller.instance.leftControllerIndexFloat;
		}
		if (node == XRNode.RightHand)
		{
			return ControllerInputPoller.instance.rightControllerIndexFloat;
		}
		return 0f;
	}

	public static float TriggerTouch(XRNode node)
	{
		if (node == XRNode.LeftHand)
		{
			return ControllerInputPoller.instance.leftControllerIndexTouch;
		}
		if (node == XRNode.RightHand)
		{
			return ControllerInputPoller.instance.rightControllerIndexTouch;
		}
		return 0f;
	}

	public static Vector3 DevicePosition(XRNode node)
	{
		if (node == XRNode.Head)
		{
			return ControllerInputPoller.instance.headPosition;
		}
		if (node == XRNode.LeftHand)
		{
			return ControllerInputPoller.instance.leftControllerPosition;
		}
		if (node == XRNode.RightHand)
		{
			return ControllerInputPoller.instance.rightControllerPosition;
		}
		return Vector3.zero;
	}

	public static Quaternion DeviceRotation(XRNode node)
	{
		if (node == XRNode.Head)
		{
			return ControllerInputPoller.instance.headRotation;
		}
		if (node == XRNode.LeftHand)
		{
			return ControllerInputPoller.instance.leftControllerRotation;
		}
		if (node == XRNode.RightHand)
		{
			return ControllerInputPoller.instance.rightControllerRotation;
		}
		return Quaternion.identity;
	}

	public static Vector3 DeviceVelocity(XRNode node)
	{
		if (node == XRNode.LeftHand)
		{
			return ControllerInputPoller.instance.leftVelocity;
		}
		if (node == XRNode.RightHand)
		{
			return ControllerInputPoller.instance.rightVelocity;
		}
		return Vector3.zero;
	}

	public static Vector3 DeviceAngularVelocity(XRNode node)
	{
		if (node == XRNode.LeftHand)
		{
			return ControllerInputPoller.instance.leftAngularVelocity;
		}
		if (node == XRNode.RightHand)
		{
			return ControllerInputPoller.instance.rightAngularVelocity;
		}
		return Vector3.zero;
	}

	public static bool PositionValid(XRNode node)
	{
		if (node == XRNode.Head)
		{
			return ControllerInputPoller.instance.headDevice.isValid;
		}
		if (node == XRNode.LeftHand)
		{
			return ControllerInputPoller.instance.leftControllerDevice.isValid;
		}
		return node == XRNode.RightHand && ControllerInputPoller.instance.rightControllerDevice.isValid;
	}

	public static bool HasPressFlags(XRNode node, EControllerInputPressFlags inputStateFlags)
	{
		EControllerInputPressFlags inputStateFlags2 = ControllerInputPoller.GetInputStateFlags(node);
		return inputStateFlags != EControllerInputPressFlags.None && (inputStateFlags2 & inputStateFlags) == inputStateFlags;
	}

	public EControllerInputPressFlags leftPressFlags { get; private set; }

	public EControllerInputPressFlags rightPressFlags { get; private set; }

	public EControllerInputPressFlags leftPressFlagsLastFrame { get; private set; }

	public EControllerInputPressFlags rightPressFlagsLastFrame { get; private set; }

	public static EControllerInputPressFlags GetInputStateFlags(XRNode node)
	{
		if (node == XRNode.LeftHand)
		{
			return ControllerInputPoller.instance.leftPressFlags;
		}
		if (node != XRNode.RightHand)
		{
			return EControllerInputPressFlags.None;
		}
		return ControllerInputPoller.instance.rightPressFlags;
	}

	public static void AddCallbackOnPressStart(EControllerInputPressFlags flags, Action<EHandednessFlags> callback)
	{
		ControllerInputPoller._AddInputStateCallback(ref ControllerInputPoller._g_callbacks_onPressStart, flags, callback);
	}

	public static void AddCallbackOnPressEnd(EControllerInputPressFlags flags, Action<EHandednessFlags> callback)
	{
		ControllerInputPoller._AddInputStateCallback(ref ControllerInputPoller._g_callbacks_onPressEnd, flags, callback);
	}

	public static void AddCallbackOnPressUpdate(EControllerInputPressFlags flags, Action<EHandednessFlags> callback)
	{
		ControllerInputPoller._AddInputStateCallback(ref ControllerInputPoller._g_callbacks_onPressUpdate, flags, callback);
	}

	private static void _AddInputStateCallback(ref ControllerInputPoller._InputCallbacksCadenceInfo ref_callbacksInfo, EControllerInputPressFlags flags, Action<EHandednessFlags> callback)
	{
		if (callback == null || flags == EControllerInputPressFlags.None)
		{
			return;
		}
		if (ref_callbacksInfo.list.Capacity <= ref_callbacksInfo.list.Count)
		{
			ref_callbacksInfo.list.Capacity = ref_callbacksInfo.list.Count * 2;
		}
		ref_callbacksInfo.list.Add(new ControllerInputPoller._InputCallback(flags, callback));
	}

	public static void RemoveCallbackOnPressStart(Action<EHandednessFlags> callback)
	{
		ControllerInputPoller._RemoveInputStateCallback(ref ControllerInputPoller._g_callbacks_onPressStart, callback);
	}

	public static void RemoveCallbackOnPressEnd(Action<EHandednessFlags> callback)
	{
		ControllerInputPoller._RemoveInputStateCallback(ref ControllerInputPoller._g_callbacks_onPressEnd, callback);
	}

	public static void RemoveCallbackOnPressUpdate(Action<EHandednessFlags> callback)
	{
		ControllerInputPoller._RemoveInputStateCallback(ref ControllerInputPoller._g_callbacks_onPressUpdate, callback);
	}

	private static void _RemoveInputStateCallback(ref ControllerInputPoller._InputCallbacksCadenceInfo ref_callbacksInfo, Action<EHandednessFlags> callback)
	{
		if (callback == null)
		{
			return;
		}
		ref_callbacksInfo.list.RemoveAll((ControllerInputPoller._InputCallback sub) => sub.callback == callback);
	}

	private void _UpdatePressFlags()
	{
		this.leftPressFlagsLastFrame = this.leftPressFlags;
		this.leftPressFlags = (this.leftIndexPressed ? EControllerInputPressFlags.Index : EControllerInputPressFlags.None) | (this.leftGrab ? EControllerInputPressFlags.Grip : EControllerInputPressFlags.None) | (this.leftControllerPrimaryButton ? EControllerInputPressFlags.Primary : EControllerInputPressFlags.None) | (this.leftControllerSecondaryButton ? EControllerInputPressFlags.Secondary : EControllerInputPressFlags.None);
		this.rightPressFlagsLastFrame = this.rightPressFlags;
		this.rightPressFlags = (this.rightIndexPressed ? EControllerInputPressFlags.Index : EControllerInputPressFlags.None) | (this.rightGrab ? EControllerInputPressFlags.Grip : EControllerInputPressFlags.None) | (this.rightControllerPrimaryButton ? EControllerInputPressFlags.Primary : EControllerInputPressFlags.None) | (this.rightControllerSecondaryButton ? EControllerInputPressFlags.Secondary : EControllerInputPressFlags.None);
		ControllerInputPoller._UpdatePressFlags_Callbacks(ref ControllerInputPoller._g_callbacks_onPressStart, ControllerInputPoller._EPressCadence.Start, this.leftPressFlags, this.leftPressFlagsLastFrame, this.rightPressFlags, this.rightPressFlagsLastFrame);
		ControllerInputPoller._UpdatePressFlags_Callbacks(ref ControllerInputPoller._g_callbacks_onPressEnd, ControllerInputPoller._EPressCadence.End, this.leftPressFlags, this.leftPressFlagsLastFrame, this.rightPressFlags, this.rightPressFlagsLastFrame);
		ControllerInputPoller._UpdatePressFlags_Callbacks(ref ControllerInputPoller._g_callbacks_onPressUpdate, ControllerInputPoller._EPressCadence.Held, this.leftPressFlags, this.leftPressFlagsLastFrame, this.rightPressFlags, this.rightPressFlagsLastFrame);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void _UpdatePressFlags_Callbacks(ref ControllerInputPoller._InputCallbacksCadenceInfo callbacksInfo, ControllerInputPoller._EPressCadence cadence, EControllerInputPressFlags lFlags_now, EControllerInputPressFlags lFlags_old, EControllerInputPressFlags rFlags_now, EControllerInputPressFlags rFlags_old)
	{
		for (int i = 0; i < callbacksInfo.list.Count; i++)
		{
			EControllerInputPressFlags flags = callbacksInfo.list[i].flags;
			Action<EHandednessFlags> callback = callbacksInfo.list[i].callback;
			EHandednessFlags ehandednessFlags = ControllerInputPoller._IsHandContributingToPressCadence(EHandednessFlags.Left, cadence, flags, lFlags_now, lFlags_old) | ControllerInputPoller._IsHandContributingToPressCadence(EHandednessFlags.Right, cadence, flags, rFlags_now, rFlags_old);
			if (ehandednessFlags != EHandednessFlags.None && callback != null)
			{
				try
				{
					callbacksInfo.list[i].callback(ehandednessFlags);
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static EHandednessFlags _IsHandContributingToPressCadence(EHandednessFlags hand, ControllerInputPoller._EPressCadence pressCadence, EControllerInputPressFlags cbFlags, EControllerInputPressFlags flags_now, EControllerInputPressFlags flags_old)
	{
		if ((pressCadence != ControllerInputPoller._EPressCadence.Held || (cbFlags & flags_now) != cbFlags) && (pressCadence != ControllerInputPoller._EPressCadence.Start || (cbFlags & flags_now) != cbFlags || (cbFlags & flags_old) == cbFlags) && (pressCadence != ControllerInputPoller._EPressCadence.End || (cbFlags & flags_now) == cbFlags || (cbFlags & flags_old) != cbFlags))
		{
			return EHandednessFlags.None;
		}
		return hand;
	}

	public const int k_defaultExecutionOrder = -400;

	[OnEnterPlay_SetNull]
	public static volatile ControllerInputPoller instance;

	public float leftControllerIndexFloat;

	public float leftControllerGripFloat;

	public float rightControllerIndexFloat;

	public float rightControllerGripFloat;

	public float leftControllerIndexTouch;

	public float rightControllerIndexTouch;

	public float rightStickLRFloat;

	public Vector3 leftControllerPosition;

	public Vector3 rightControllerPosition;

	public Vector3 headPosition;

	public Quaternion leftControllerRotation;

	public Quaternion rightControllerRotation;

	public Quaternion headRotation;

	public InputDevice leftControllerDevice;

	public InputDevice rightControllerDevice;

	public InputDevice headDevice;

	public bool leftControllerPrimaryButton;

	public bool leftControllerSecondaryButton;

	public bool rightControllerPrimaryButton;

	public bool rightControllerSecondaryButton;

	public bool leftControllerPrimaryButtonTouch;

	public bool leftControllerSecondaryButtonTouch;

	public bool rightControllerPrimaryButtonTouch;

	public bool rightControllerSecondaryButtonTouch;

	public bool leftControllerTriggerButton;

	public bool rightControllerTriggerButton;

	public bool leftGrab;

	public bool leftGrabRelease;

	public bool rightGrab;

	public bool rightGrabRelease;

	public bool leftGrabMomentary;

	public bool leftGrabReleaseMomentary;

	public bool rightGrabMomentary;

	public bool rightGrabReleaseMomentary;

	private bool _leftIndexPressed;

	private bool _leftIndexReleased;

	private bool _rightIndexPressed;

	private bool _rightIndexReleased;

	private bool _leftIndexPressedThisFrame;

	private bool _leftIndexReleasedThisFrame;

	private bool _rightIndexPressedThisFrame;

	private bool _rightIndexReleasedThisFrame;

	private Vector3 _leftVelocity;

	private Vector3 _rightVelocity;

	private Vector3 _leftAngularVelocity;

	private Vector3 _rightAngularVelocity;

	public Vector2 leftControllerPrimary2DAxis;

	public Vector2 rightControllerPrimary2DAxis;

	private List<Action> onUpdate = new List<Action>();

	private List<Action> onUpdateNext = new List<Action>();

	private bool didModifyOnUpdate;

	private static ControllerInputPoller._InputCallbacksCadenceInfo _g_callbacks_onPressStart = new ControllerInputPoller._InputCallbacksCadenceInfo(32);

	private static ControllerInputPoller._InputCallbacksCadenceInfo _g_callbacks_onPressEnd = new ControllerInputPoller._InputCallbacksCadenceInfo(32);

	private static ControllerInputPoller._InputCallbacksCadenceInfo _g_callbacks_onPressUpdate = new ControllerInputPoller._InputCallbacksCadenceInfo(32);

	private enum _EPressCadence
	{
		Start,
		End,
		Held
	}

	private struct _InputCallback
	{
		public _InputCallback(EControllerInputPressFlags flags, Action<EHandednessFlags> callback)
		{
			this.flags = flags;
			this.callback = callback;
		}

		public readonly EControllerInputPressFlags flags;

		public readonly Action<EHandednessFlags> callback;
	}

	private struct _InputCallbacksCadenceInfo
	{
		public _InputCallbacksCadenceInfo(int initialCapacity)
		{
			this.list = new List<ControllerInputPoller._InputCallback>(initialCapacity);
		}

		public readonly List<ControllerInputPoller._InputCallback> list;
	}
}
