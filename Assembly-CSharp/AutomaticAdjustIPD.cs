using System;
using UnityEngine;
using UnityEngine.XR;

public class AutomaticAdjustIPD : MonoBehaviour, IGorillaSliceableSimple
{
	public void OnEnable()
	{
		GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
	}

	public void OnDisable()
	{
		GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
	}

	public void SliceUpdate()
	{
		if (!this.headset.isValid)
		{
			this.headset = InputDevices.GetDeviceAtXRNode(XRNode.Head);
		}
		if (this.headset.isValid && this.headset.TryGetFeatureValue(CommonUsages.leftEyePosition, out this.leftEyePosition) && this.headset.TryGetFeatureValue(CommonUsages.rightEyePosition, out this.rightEyePosition))
		{
			this.currentIPD = (this.rightEyePosition - this.leftEyePosition).magnitude;
			if (Mathf.Abs(this.lastIPD - this.currentIPD) < 0.01f)
			{
				return;
			}
			this.lastIPD = this.currentIPD;
			for (int i = 0; i < this.adjustXScaleObjects.Length; i++)
			{
				Transform transform = this.adjustXScaleObjects[i];
				if (!transform)
				{
					return;
				}
				transform.localScale = new Vector3(Mathf.LerpUnclamped(1f, 1.12f, (this.currentIPD - 0.058f) / 0.0050000027f), 1f, 1f);
			}
		}
	}

	public InputDevice headset;

	public float currentIPD;

	public Vector3 leftEyePosition;

	public Vector3 rightEyePosition;

	public bool testOverride;

	public Transform[] adjustXScaleObjects;

	public float sizeAt58mm = 1f;

	public float sizeAt63mm = 1.12f;

	public float lastIPD;
}
