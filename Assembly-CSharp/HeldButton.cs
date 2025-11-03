using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HeldButton : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (!base.enabled)
		{
			return;
		}
		GorillaTriggerColliderHandIndicator componentInParent = other.GetComponentInParent<GorillaTriggerColliderHandIndicator>();
		if (componentInParent == null)
		{
			return;
		}
		if ((componentInParent.isLeftHand && !this.leftHandPressable) || (!componentInParent.isLeftHand && !this.rightHandPressable))
		{
			return;
		}
		if (!this.pendingPress || other != this.pendingPressCollider)
		{
			UnityEvent unityEvent = this.onStartPressingButton;
			if (unityEvent != null)
			{
				unityEvent.Invoke();
			}
			this.touchTime = Time.time;
			this.pendingPressCollider = other;
			this.pressingHand = componentInParent;
			this.pendingPress = true;
			this.SetOn(true);
			GorillaTagger.Instance.StartVibration(componentInParent.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);
		}
	}

	private void LateUpdate()
	{
		if (!this.pendingPress)
		{
			return;
		}
		if (this.touchTime < this.releaseTime && this.releaseTime + this.debounceTime < Time.time)
		{
			UnityEvent unityEvent = this.onStopPressingButton;
			if (unityEvent != null)
			{
				unityEvent.Invoke();
			}
			this.pendingPress = false;
			this.pendingPressCollider = null;
			this.pressingHand = null;
			this.SetOn(false);
			return;
		}
		if (this.touchTime + this.pressDuration < Time.time)
		{
			this.onPressButton.Invoke();
			if (this.pressingHand != null)
			{
				GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(67, this.pressingHand.isLeftHand, 0.1f);
				GorillaTagger.Instance.StartVibration(this.pressingHand.isLeftHand, GorillaTagger.Instance.tapHapticStrength, GorillaTagger.Instance.tapHapticDuration);
			}
			UnityEvent unityEvent2 = this.onStopPressingButton;
			if (unityEvent2 != null)
			{
				unityEvent2.Invoke();
			}
			this.pendingPress = false;
			this.pendingPressCollider = null;
			this.pressingHand = null;
			this.releaseTime = Time.time;
			this.SetOn(false);
			return;
		}
		if (this.touchTime > this.releaseTime && this.pressingHand != null)
		{
			GorillaTagger.Instance.StartVibration(this.pressingHand.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 4f, Time.fixedDeltaTime);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (this.pendingPress && this.pendingPressCollider == other)
		{
			this.releaseTime = Time.time;
			UnityEvent unityEvent = this.onStopPressingButton;
			if (unityEvent == null)
			{
				return;
			}
			unityEvent.Invoke();
		}
	}

	public void SetOn(bool inOn)
	{
		if (inOn == this.isOn)
		{
			return;
		}
		this.isOn = inOn;
		if (this.isOn)
		{
			this.buttonRenderer.material = this.pressedMaterial;
			if (this.myText != null)
			{
				this.myText.text = this.onText;
				return;
			}
		}
		else
		{
			this.buttonRenderer.material = this.unpressedMaterial;
			if (this.myText != null)
			{
				this.myText.text = this.offText;
			}
		}
	}

	public Material pressedMaterial;

	public Material unpressedMaterial;

	public MeshRenderer buttonRenderer;

	private bool isOn;

	public float debounceTime = 0.25f;

	public bool leftHandPressable;

	public bool rightHandPressable = true;

	public float pressDuration = 0.5f;

	public UnityEvent onStartPressingButton;

	public UnityEvent onStopPressingButton;

	public UnityEvent onPressButton;

	[TextArea]
	public string offText;

	[TextArea]
	public string onText;

	public Text myText;

	private float touchTime;

	private float releaseTime;

	private bool pendingPress;

	private Collider pendingPressCollider;

	private GorillaTriggerColliderHandIndicator pressingHand;
}
