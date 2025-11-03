using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class PrimaryButtonWatcher : MonoBehaviour
{
	private void Awake()
	{
		if (this.primaryButtonPress == null)
		{
			this.primaryButtonPress = new PrimaryButtonEvent();
		}
		this.devicesWithPrimaryButton = new List<InputDevice>();
	}

	private void OnEnable()
	{
		List<InputDevice> list = new List<InputDevice>();
		InputDevices.GetDevices(list);
		foreach (InputDevice inputDevice in list)
		{
			this.InputDevices_deviceConnected(inputDevice);
		}
		InputDevices.deviceConnected += this.InputDevices_deviceConnected;
		InputDevices.deviceDisconnected += this.InputDevices_deviceDisconnected;
	}

	private void OnDisable()
	{
		InputDevices.deviceConnected -= this.InputDevices_deviceConnected;
		InputDevices.deviceDisconnected -= this.InputDevices_deviceDisconnected;
		this.devicesWithPrimaryButton.Clear();
	}

	private void InputDevices_deviceConnected(InputDevice device)
	{
		bool flag;
		if (device.TryGetFeatureValue(CommonUsages.primaryButton, out flag))
		{
			this.devicesWithPrimaryButton.Add(device);
		}
	}

	private void InputDevices_deviceDisconnected(InputDevice device)
	{
		if (this.devicesWithPrimaryButton.Contains(device))
		{
			this.devicesWithPrimaryButton.Remove(device);
		}
	}

	private void Update()
	{
		bool flag = false;
		foreach (InputDevice inputDevice in this.devicesWithPrimaryButton)
		{
			bool flag2 = false;
			flag = (inputDevice.TryGetFeatureValue(CommonUsages.primaryButton, out flag2) && flag2) || flag;
		}
		if (flag != this.lastButtonState)
		{
			this.primaryButtonPress.Invoke(flag);
			this.lastButtonState = flag;
		}
	}

	public PrimaryButtonEvent primaryButtonPress;

	private bool lastButtonState;

	private List<InputDevice> devicesWithPrimaryButton;
}
