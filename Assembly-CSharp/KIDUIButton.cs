using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class KIDUIButton : Button, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	private XRUIInputModule InputModule
	{
		get
		{
			return EventSystem.current.currentInputModule as XRUIInputModule;
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (ControllerBehaviour.Instance)
		{
			ControllerBehaviour.Instance.OnAction += this.PostUpdate;
		}
	}

	private void PostUpdate()
	{
		if (!KIDUIButton._canTrigger)
		{
			KIDUIButton._canTrigger = !ControllerBehaviour.Instance.TriggerDown;
		}
		if (!base.interactable || !this.inside || !KIDUIButton._canTrigger)
		{
			return;
		}
		if (ControllerBehaviour.Instance && ControllerBehaviour.Instance.TriggerDown && !KIDUIButton._triggeredThisFrame)
		{
			string text = string.Concat(new string[]
			{
				"[",
				base.transform.parent.parent.parent.name,
				".",
				base.transform.parent.parent.name,
				".",
				base.transform.parent.name,
				".",
				base.transform.name,
				"]"
			});
			Debug.Log(string.Concat(new string[]
			{
				"[KID::UIBUTTON::DEBUG] ",
				text,
				" - STEAM - OnClick is pressed. Time: [",
				Time.time.ToString(),
				"]"
			}), this);
			Button.ButtonClickedEvent onClick = base.onClick;
			if (onClick != null)
			{
				onClick.Invoke();
			}
			KIDUIButton._triggeredThisFrame = true;
			KIDUIButton._canTrigger = false;
		}
	}

	private void LateUpdate()
	{
		if (KIDUIButton._triggeredThisFrame)
		{
			string text = string.Concat(new string[]
			{
				"[",
				base.transform.parent.parent.parent.name,
				".",
				base.transform.parent.parent.name,
				".",
				base.transform.parent.name,
				".",
				base.transform.name,
				"]"
			});
			Debug.Log(string.Concat(new string[]
			{
				"[KID::UIBUTTON::DEBUG] ",
				text,
				" - STEAM - OnLateUpdate triggered and Triggered Frame Reset. Time: [",
				Time.time.ToString(),
				"]"
			}), this);
		}
		KIDUIButton._triggeredThisFrame = false;
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
		this.inside = false;
	}

	public void ResetButton()
	{
		this.inside = false;
		KIDUIButton._triggeredThisFrame = false;
	}

	protected override void OnDisable()
	{
		this.FixStuckPressedState();
		if (ControllerBehaviour.Instance)
		{
			ControllerBehaviour.Instance.OnAction -= this.PostUpdate;
		}
	}

	private void FixStuckPressedState()
	{
		this.InstantClearState();
		this._buttonText.color = (base.interactable ? this._normalTextColor : this._disabledTextColor);
		this.inside = false;
		KIDUIButton._triggeredThisFrame = false;
	}

	protected override void DoStateTransition(Selectable.SelectionState state, bool instant)
	{
		base.DoStateTransition(state, instant);
		switch (state)
		{
		default:
			this._buttonText.color = this._normalTextColor;
			this.SetIcons(true, false);
			return;
		case Selectable.SelectionState.Highlighted:
			this._buttonText.color = this._highlightedTextColor;
			this.SetIcons(false, true);
			return;
		case Selectable.SelectionState.Pressed:
			this._buttonText.color = this._pressedTextColor;
			this.SetIcons(true, false);
			return;
		case Selectable.SelectionState.Selected:
			this._buttonText.color = this._selectedTextColor;
			this.SetIcons(true, false);
			return;
		case Selectable.SelectionState.Disabled:
			this._buttonText.color = this._disabledTextColor;
			this.SetIcons(true, false);
			return;
		}
	}

	private void SetIcons(bool normalEnabled, bool highlightedEnabled)
	{
		if (this._normalIcon == null || this._highlightedIcon == null)
		{
			return;
		}
		GameObject normalIcon = this._normalIcon;
		if (normalIcon != null)
		{
			normalIcon.SetActive(normalEnabled);
		}
		GameObject highlightedIcon = this._highlightedIcon;
		if (highlightedIcon == null)
		{
			return;
		}
		highlightedIcon.SetActive(highlightedEnabled);
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);
		this.inside = true;
		if (!this.IsInteractable() || !this.IsActive())
		{
			return;
		}
		KIDAudioManager instance = KIDAudioManager.Instance;
		if (instance != null)
		{
			instance.PlaySound(KIDAudioManager.KIDSoundType.Hover);
		}
		Debug.Log("[KID::UIBUTTON::KIDAudioManager] Hover played");
		XRRayInteractor xrrayInteractor = this.InputModule.GetInteractor(eventData.pointerId) as XRRayInteractor;
		if (!xrrayInteractor)
		{
			return;
		}
		xrrayInteractor.xrController.SendHapticImpulse(this._highlightedVibrationStrength, this._highlightedVibrationDuration);
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		base.OnPointerDown(eventData);
		this.inside = false;
		if (!this.IsInteractable() || !this.IsActive())
		{
			return;
		}
		KIDAudioManager instance = KIDAudioManager.Instance;
		if (instance != null)
		{
			instance.PlaySound(this.onClickSound);
		}
		XRRayInteractor xrrayInteractor = this.InputModule.GetInteractor(eventData.pointerId) as XRRayInteractor;
		if (!xrrayInteractor)
		{
			return;
		}
		xrrayInteractor.xrController.SendHapticImpulse(this._pressedVibrationStrength, this._pressedVibrationDuration);
	}

	public void SetText(string text)
	{
		this._buttonText.SetText(text);
	}

	public void SetFont(TMP_FontAsset font)
	{
		this._buttonText.font = font;
	}

	public string GetText()
	{
		return this._buttonText.text;
	}

	public void SetBorderImage(Sprite newImg)
	{
		this._borderImage.sprite = newImg;
	}

	[SerializeField]
	private Image _borderImage;

	[SerializeField]
	private RectTransform _fillImageRef;

	[SerializeField]
	private TMP_Text _buttonText;

	[Header("Transition States")]
	[Header("Normal")]
	[SerializeField]
	[ColorUsage(true, false)]
	private Color _normalBorderColor;

	[SerializeField]
	[ColorUsage(true, false)]
	private Color _normalTextColor;

	[SerializeField]
	private float _normalBorderSize;

	[Header("Highlighted")]
	[SerializeField]
	[ColorUsage(true, false)]
	private Color _highlightedBorderColor;

	[SerializeField]
	[ColorUsage(true, false)]
	private Color _highlightedTextColor;

	[SerializeField]
	private float _highlightedBorderSize;

	[SerializeField]
	private float _highlightedVibrationStrength = 0.1f;

	[SerializeField]
	private float _highlightedVibrationDuration = 0.1f;

	[Header("Pressed")]
	[SerializeField]
	[ColorUsage(true, false)]
	private Color _pressedBorderColor;

	[SerializeField]
	[ColorUsage(true, false)]
	private Color _pressedTextColor;

	[SerializeField]
	private float _pressedBorderSize;

	[SerializeField]
	private float _pressedVibrationStrength = 0.5f;

	[SerializeField]
	private float _pressedVibrationDuration = 0.1f;

	[Header("Selected")]
	[SerializeField]
	[ColorUsage(true, false)]
	private Color _selectedBorderColor;

	[SerializeField]
	[ColorUsage(true, false)]
	private Color _selectedTextColor;

	[SerializeField]
	private float _selectedBorderSize;

	[Header("Disabled")]
	[SerializeField]
	[ColorUsage(true, false)]
	private Color _disabledBorderColor;

	[SerializeField]
	[ColorUsage(true, false)]
	private Color _disabledTextColor;

	[SerializeField]
	private float _disabledBorderSize;

	[Header("Audio")]
	[SerializeField]
	private KIDAudioManager.KIDSoundType onClickSound;

	[Header("Icon Swap Settings")]
	[SerializeField]
	private GameObject _normalIcon;

	[SerializeField]
	private GameObject _highlightedIcon;

	[Header("Steam Settings")]
	[SerializeField]
	private UXSettings _cbUXSettings;

	private bool inside;

	private static bool _triggeredThisFrame = false;

	private static bool _canTrigger = true;
}
