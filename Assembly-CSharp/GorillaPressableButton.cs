using System;
using GorillaExtensions;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.UI;

public class GorillaPressableButton : MonoBehaviour, IClickable
{
	public event Action<GorillaPressableButton, bool> onPressed;

	public virtual void Start()
	{
	}

	private void OnEnable()
	{
		LocalisationManager.RegisterOnLanguageChanged(new Action(this.RefreshText));
		this.RefreshText();
	}

	private void OnDisable()
	{
		LocalisationManager.UnregisterOnLanguageChanged(new Action(this.RefreshText));
	}

	protected virtual void RefreshText()
	{
		if (this._offLocalizedText == null || this._offLocalizedText.IsEmpty || this._onLocalizedText == null || this._onLocalizedText.IsEmpty)
		{
			return;
		}
		string text;
		if (!this.isOn)
		{
			text = this.offText;
			text = this._offLocalizedText.GetLocalizedString();
			if (string.IsNullOrEmpty(text))
			{
				Debug.LogError("[LOCALIZATION::GORILLA_PRESSABLE_BUTTON] Null or empty string returned for OFF localized text", this);
				text = this.offText;
			}
		}
		else
		{
			text = this.onText;
			text = this._onLocalizedText.GetLocalizedString();
			if (string.IsNullOrEmpty(text))
			{
				Debug.LogError("[LOCALIZATION::GORILLA_PRESSABLE_BUTTON] Null or empty string returned for ON localized text", this);
				text = this.onText;
			}
		}
		if (this._myTxtSet || this.myText.IsNotNull())
		{
			this.myText.text = text;
		}
		if (this._myTmpTxtSet || this.myTmpText.IsNotNull())
		{
			this.myTmpText.text = text;
		}
		if (this._myTmpTxt2Set || this.myTmpText2.IsNotNull())
		{
			this.myTmpText2.text = text;
		}
	}

	protected virtual void SetOffText(bool setMyText, bool setMyTmpText = false, bool setMyTmpText2 = false)
	{
		string localizedString = this.offText;
		if (this._offLocalizedText != null && !this._offLocalizedText.IsEmpty)
		{
			localizedString = this._offLocalizedText.GetLocalizedString();
			if (string.IsNullOrEmpty(localizedString))
			{
				Debug.LogError("[LOCALIZATION::GORILLA_PRESSABLE_BUTTON] Null or empty string returned for OFF localized text", this);
				localizedString = this.offText;
			}
		}
		this._myTxtSet = setMyText;
		this._myTmpTxtSet = setMyTmpText;
		this._myTmpTxt2Set = setMyTmpText2;
		if (setMyText)
		{
			this.myText.text = localizedString;
		}
		if (setMyTmpText)
		{
			this.myTmpText.text = localizedString;
		}
		if (setMyTmpText2)
		{
			this.myTmpText2.text = localizedString;
		}
	}

	protected virtual void SetOnText(bool setMyText, bool setMyTmpText = false, bool setMyTmpText2 = false)
	{
		string localizedString = this.onText;
		if (this._onLocalizedText != null && !this._onLocalizedText.IsEmpty)
		{
			localizedString = this._onLocalizedText.GetLocalizedString();
			if (string.IsNullOrEmpty(localizedString))
			{
				Debug.LogError("[LOCALIZATION::GORILLA_PRESSABLE_BUTTON] Null or empty string returned for ON localized text", this);
				localizedString = this.onText;
			}
		}
		this._myTxtSet = setMyText;
		this._myTmpTxtSet = setMyTmpText;
		this._myTmpTxt2Set = setMyTmpText2;
		if (setMyText)
		{
			this.myText.text = localizedString;
		}
		if (setMyTmpText)
		{
			this.myTmpText.text = localizedString;
		}
		if (setMyTmpText2)
		{
			this.myTmpText2.text = localizedString;
		}
	}

	protected void OnTriggerEnter(Collider collider)
	{
		if (!base.enabled)
		{
			return;
		}
		if (this.touchTime + this.debounceTime >= Time.time)
		{
			return;
		}
		GorillaTriggerColliderHandIndicator component = collider.gameObject.GetComponent<GorillaTriggerColliderHandIndicator>();
		if (!component)
		{
			return;
		}
		this.PressButton(component.isLeftHand);
	}

	private void PressButton(bool isLeftHand)
	{
		this.touchTime = Time.time;
		UnityEvent unityEvent = this.onPressButton;
		if (unityEvent != null)
		{
			unityEvent.Invoke();
		}
		Action<GorillaPressableButton, bool> action = this.onPressed;
		if (action != null)
		{
			action(this, isLeftHand);
		}
		this.ButtonActivation();
		this.ButtonActivationWithHand(isLeftHand);
		GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(this.pressButtonSoundIndex, isLeftHand, 0.05f);
		GorillaTagger.Instance.StartVibration(isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);
		if (NetworkSystem.Instance.InRoom && GorillaTagger.Instance.myVRRig != null)
		{
			GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.Others, new object[] { 67, isLeftHand, 0.05f });
		}
	}

	public void Click(bool leftHand = false)
	{
		this.PressButton(leftHand);
	}

	public virtual void UpdateColor()
	{
		this.UpdateColorWithState(this.isOn);
	}

	protected void UpdateColorWithState(bool state)
	{
		if (state)
		{
			this.buttonRenderer.material = this.pressedMaterial;
			this.SetOnText(this.myText.IsNotNull(), this.myTmpText.IsNotNull(), this.myTmpText2.IsNotNull());
			return;
		}
		this.buttonRenderer.material = this.unpressedMaterial;
		this.SetOffText(this.myText.IsNotNull(), this.myTmpText.IsNotNull(), this.myTmpText2.IsNotNull());
	}

	public virtual void ButtonActivation()
	{
	}

	public virtual void ButtonActivationWithHand(bool isLeftHand)
	{
	}

	public virtual void ResetState()
	{
		this.isOn = false;
		this.UpdateColor();
	}

	public void SetText(string newText)
	{
		if (this.myTmpText != null)
		{
			this.myTmpText.text = newText;
		}
		if (this.myTmpText2 != null)
		{
			this.myTmpText2.text = newText;
		}
		if (this.myText != null)
		{
			this.myText.text = newText;
		}
	}

	public Material pressedMaterial;

	public Material unpressedMaterial;

	public MeshRenderer buttonRenderer;

	public int pressButtonSoundIndex = 67;

	public bool isOn;

	public float debounceTime = 0.25f;

	public float touchTime;

	public bool testPress;

	public bool testHandLeft;

	[TextArea]
	public string offText;

	[SerializeField]
	private LocalizedString _offLocalizedText;

	[TextArea]
	public string onText;

	[SerializeField]
	private LocalizedString _onLocalizedText;

	[SerializeField]
	[Tooltip("Use this one when you can. Don't use MyText if you can help it!")]
	public TMP_Text myTmpText;

	[SerializeField]
	[Tooltip("Use this one when you can. Don't use MyText if you can help it!")]
	public TMP_Text myTmpText2;

	public Text myText;

	[Space]
	public UnityEvent onPressButton;

	protected bool _myTxtSet;

	protected bool _myTmpTxtSet;

	protected bool _myTmpTxt2Set;
}
