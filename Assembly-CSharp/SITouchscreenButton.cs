using System;
using GorillaTag.Audio;
using UnityEngine;
using UnityEngine.Events;

public class SITouchscreenButton : MonoBehaviour, IClickable
{
	private bool IsUsable
	{
		get
		{
			if (!this._screenRegion)
			{
				return Time.time - this._enableTime >= 0.2f;
			}
			return !this._screenRegion.HasPressedButton;
		}
	}

	private void Awake()
	{
		ITouchScreenStation componentInParent = base.GetComponentInParent<ITouchScreenStation>();
		if (componentInParent != null)
		{
			this._screenRegion = componentInParent.ScreenRegion;
		}
	}

	private void OnEnable()
	{
		this._enableTime = Time.time;
	}

	private void OnTriggerEnter(Collider other)
	{
		GorillaTriggerColliderHandIndicator componentInParent = other.GetComponentInParent<GorillaTriggerColliderHandIndicator>();
		if (componentInParent)
		{
			this.PressButton();
			GorillaTagger.Instance.StartVibration(componentInParent.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);
		}
	}

	public void PressButton()
	{
		if (!this.IsUsable)
		{
			return;
		}
		if (this._screenRegion)
		{
			this._screenRegion.RegisterButtonPress();
		}
		this.buttonPressed.Invoke(this.buttonType, this.data, NetworkSystem.Instance.LocalPlayer.ActorNumber);
		if (this._pressSound != null)
		{
			GTAudioOneShot.Play(this._pressSound, base.transform.position, this._pressSoundVolume, 1f);
		}
	}

	public void Click(bool leftHand = false)
	{
		this.PressButton();
	}

	public SITouchscreenButton.SITouchscreenButtonType buttonType;

	public int data;

	[SerializeField]
	private AudioClip _pressSound;

	[SerializeField]
	private float _pressSoundVolume = 0.1f;

	public UnityEvent<SITouchscreenButton.SITouchscreenButtonType, int, int> buttonPressed;

	private SIScreenRegion _screenRegion;

	private const float DEBOUNCE_TIME = 0.2f;

	private float _enableTime;

	public enum SITouchscreenButtonType
	{
		Back,
		Next,
		Exit,
		Help,
		Select,
		Dispense,
		Research,
		Collect,
		Debug,
		PageSelect,
		Purchase,
		Confirm,
		Cancel,
		OverrideFailure,
		None
	}
}
